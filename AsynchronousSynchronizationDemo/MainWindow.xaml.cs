using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AsynchronousSynchronizationDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region 异步转同步

        #region 异步转同步（PushFrame-Task）
        private void PushFrameTask_OnClick(object sender, RoutedEventArgs e)
        {
            AwaitByPushFrame(TestAsync());
            Debug.WriteLine("PushFrameTask_OnClick end");
        }
        private static async Task TestAsync()
        {
            //Debug.WriteLine("异步任务start……");
            await Task.Delay(2000);
            //Debug.WriteLine("异步任务end……");
        }
        public static void AwaitByPushFrame(Task task)
        {
            var frame = new DispatcherFrame();
            task.ContinueWith(t =>
            {
                frame.Continue = false;
            });
            Dispatcher.PushFrame(frame);
        }

        #endregion

        #region 异步转同步（PushFrame-TaskResult）

        private void PushFrameTaskResult_OnClick(object sender, RoutedEventArgs e)
        {
            var result = AwaitByPushFrame(TestWithResultAsync());
            Debug.WriteLine($"PushFrameTaskResult_OnClick end:{result}");
        }
        private static async Task<string> TestWithResultAsync()
        {
            Debug.WriteLine("1. 异步任务start……");
            await Task.Delay(2000);
            Debug.WriteLine("2. 异步任务end……");
            return "2秒以后";
        }

        public static TResult AwaitByPushFrame<TResult>(Task<TResult> task)
        {
            var frame = new DispatcherFrame();
            task.ContinueWith(t =>
            {
                frame.Continue = false;
            });
            Dispatcher.PushFrame(frame);
            return task.Result;
        }
        #endregion

        #region TaskCompleteSource

        private void TaskCompleteSourceButton_OnClick(object sender, RoutedEventArgs e)
        {
            var result = AwaitByTaskCompleteSource(TestWithResultAsync);
            Debug.WriteLine($"4. TaskCompleteSource_OnClick end:{result}");
        }

        private string AwaitByTaskCompleteSource(Func<Task<string>> func)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();
            var task1 = taskCompletionSource.Task;
            Task.Run(async () =>
            {
                var result = await func.Invoke();
                taskCompletionSource.SetResult(result);
            });
            var task1Result = task1.Result;
            Debug.WriteLine($"3. AwaitByTaskCompleteSource end:{task1Result}");
            return task1Result;
        }

        #endregion

        #endregion

        #region 同步转异步

        #region 同步转异步 Task

        public async Task DelayAsync()
        {
            await Task.Run(() => Delay());
        }

        private void Delay()
        {
        }

        #endregion

        #region 同步转异步 AutoResetEvent
        private async void AutoResetEventTask_OnClick(object sender, RoutedEventArgs e)
        {
            await ExecuteStoryboradAsync(new Storyboard() { Duration = new Duration(TimeSpan.FromSeconds(2)) });
            Debug.WriteLine("PushFrameTask_OnClick end");
        }
        /// <summary>
        /// 执行动画
        /// </summary>
        /// <param name="storyboard"></param>
        /// <returns></returns>
        public static async Task ExecuteStoryboradAsync(Storyboard storyboard)
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);

            Debug.WriteLine("Storyboard start");
            storyboard.Completed += OnStoryboardCompleted;
            storyboard.Begin();

            void OnStoryboardCompleted(object sender, EventArgs e)
            {
                Debug.WriteLine("Storyboard end");
                storyboard.Completed -= OnStoryboardCompleted;
                autoResetEvent.Set();
            }

            autoResetEvent.WaitOne();
        }
        #endregion

        #region 同步转异步 TaskCompletion
        private async void TaskCompleteSourceAwait_OnClick(object sender, RoutedEventArgs e)
        {
            bool isCompleted = await AwaitByTaskCompletionAsync(new Storyboard() { Duration = new Duration(TimeSpan.FromSeconds(2)) });
            Debug.WriteLine($"TaskCompleteSourceAwait_OnClick end:{isCompleted}");
        }
        /// <summary>
        /// 执行动画
        /// </summary>
        /// <param name="storyboard"></param>
        /// <returns></returns>
        public static async Task<bool> AwaitByTaskCompletionAsync(Storyboard storyboard)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            Debug.WriteLine("Storyboard start");

            storyboard.Completed += OnStoryboardCompleted;
            storyboard.Begin();

            void OnStoryboardCompleted(object sender, EventArgs e)
            {
                Debug.WriteLine("Storyboard end");
                storyboard.Completed -= OnStoryboardCompleted;
                taskCompletionSource.SetResult(true);
            }
            return await taskCompletionSource.Task;
        }
        #endregion

        #endregion

        #region 死锁

        #region 异步转同步-Task
        private void Task_OnClick(object sender, RoutedEventArgs e)
        {
            AwaitUsingTask(TestAsync());
            Debug.WriteLine("Task_OnClick end");
        }
        private void AwaitUsingTask(Task task)
        {
            task.Wait();
            //task.Result;
        }

        //原因：
        //wait线程锁：主执行线程调用子线程后挂起等待子线程结果，
        //子线程又需要切换到主线程或者等待主线程返回，
        //从而导致两个线程均处在阻塞状态（死锁）

        //产生条件
        //1. 调用 Wait() 或 Result 的代码位于 UI 线程； 
        //2. Task 的实际执行在其他线程

        //如何避免？
        //如果已经使用了async/wait，那尽量不要再使用Task.Wait()/Task.Result！
        //当然如果你是API开发，不想让你的代码被上层调用后，造成死锁。你可以添加.ConfigureAwait(false);

        #endregion

        #region 异步转同步-AutoResetEvent

        private void AwaitAutoResetEventFalse_OnClick(object sender, RoutedEventArgs e)
        {
            AwaitUsingAutoResetEvent(TestAsync());
            Debug.WriteLine("AwaitAutoResetEvent_OnClick end");
        }

        public void AwaitUsingAutoResetEvent(Task task)
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);

            task.ContinueWith(t =>
            {
                autoResetEvent.Set();
            });
            autoResetEvent.WaitOne();
        }

        #endregion

        #region AutoResetEvent

        private int index = 0;
        private async void AwaitAutoResetEventTrue_OnClick(object sender, RoutedEventArgs e)
        {
            await AwaitUsingAutoResetEvent(index++);
        }
        AutoResetEvent autoResetEvent = new AutoResetEvent(true);
        public async Task AwaitUsingAutoResetEvent(int ind)
        {
            autoResetEvent.WaitOne();
            Debug.WriteLine($"Task.Delay(3000) start{ind}");
            await Task.Delay(3000);
            Debug.WriteLine($"Task.Delay(3000) end{ind}");
            autoResetEvent.Set();
        }

        //WaitOne只能在子线程，如果放在主线程的话，会将整个线程终止.与Task类似

        #endregion

        #region TaskCompleteSource

        private void TaskCompleteSourceDead_OnClick(object sender, RoutedEventArgs e)
        {
            AwaitByTaskCompleteSource(TestAsync());
            Debug.WriteLine($"4. TaskCompleteSource_OnClick end");
        }
        private void AwaitByTaskCompleteSource(Task task)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            var taskFromSource = taskCompletionSource.Task;
            task.ContinueWith(action =>
            {
                taskCompletionSource.SetResult(true);
            });
            var task1Result = taskFromSource.Result;
            Debug.WriteLine($"3. AwaitByTaskCompleteSource end:{task1Result}");
        }

        #endregion

        #endregion

    }
}
