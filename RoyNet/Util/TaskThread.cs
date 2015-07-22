using System;
using System.Threading;

namespace RoyNet.Util
{
    public class TaskThread
    {
        private Thread _thread;
        private CancellationTokenSource _threadToken;
        private readonly Action _onExecute;
        public bool IsRunning { get; private set; }
        public event EventHandler<EventArgs> Started;
        public string Name { get; private set; }
        //public event EventHandler<EventArgs> Stoped;
        public TaskThread(string name, Action onExecute)
        {
            _onExecute = onExecute;
            Name = name;
        }

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }
            _threadToken = new CancellationTokenSource();
            _thread = new Thread(Loop);
            _thread.Name = Name;
            _thread.Start();
        }

        public void Stop()
        {
            _threadToken.Cancel();
            _thread.Join();
            _thread = null;
            _threadToken = null;
            IsRunning = false;
        }

        private void Loop()
        {
            IsRunning = true;
            OnStarted();
            while (!_threadToken.IsCancellationRequested)
            {
                _onExecute();
                Thread.Sleep(1);
            }
            //OnStoped();
        }

        protected virtual void OnStarted()
        {
            var handler = Started;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        //protected virtual void OnStoped()
        //{
        //    var handler = Stoped;
        //    if (handler != null) handler(this, EventArgs.Empty);
        //}
    }
}
