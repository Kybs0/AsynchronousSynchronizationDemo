using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsynchronousSynchronizationDemo
{
    public class TestA
    {
        public TestA()
        {
            TestB.PushCompleted += TestB_PushCompleted;
        }

        ObservableCollection<string> _collection = new ObservableCollection<string>();
        private void TestB_PushCompleted(object sender, string item)
        {
            _collection.Add(item);
        }
    }

    public static class TestB
    {
        public static event EventHandler<string> PushCompleted;
    }
}
