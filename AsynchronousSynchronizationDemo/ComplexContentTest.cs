using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AsynchronousSynchronizationDemo
{
    class ContentTest
    {
        private ShowingInfo _showingInfo = null;
        private Thread _otherThread;
        public ContentTest()
        {
            _otherThread = new Thread(t =>
              {

              });
            _otherThread.Start();
        }
        public void Hide()
        {
            //if (_showingInfo.ShowType == "OtherThread")
            //{
            //    _otherThread.BeginInvoke(() =>
            //    {
            //        _showingInfo.ShowingElement.Visibility = Visibility.Collapsed;
            //    });
            //}
            //else if (_showingInfo.ShowType == "CurrentThread")
            //{
            //    Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        _showingInfo.ShowingElement.Visibility = Visibility.Collapsed;
            //    });
            //}
        }
    }

    public class ShowingInfo
    {
        public string ShowType { get; set; }
        public FrameworkElement ShowingElement { get; set; }
    }
}
