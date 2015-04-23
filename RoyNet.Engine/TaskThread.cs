using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoyNet.Engine
{
    public class TaskThread
    {
        private Thread _thread;
        private CancellationTokenSource _threadToken;
        private readonly Action _onExecute;
        public bool IsRunning { get; private set; }
        public event EventHandler<EventArgs> Started;
        public event EventHandler<EventArgs> Stoped;
        public TaskThread(Action onExecute)
        {
            _onExecute = onExecute;
        }

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }
            _threadToken = new CancellationTokenSource();
            _thread = new Thread(Loop);
            _thread.Start();
        }

        public void Stop()
        {
            _threadToken.Cancel();
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
            _thread = null;
            _threadToken = null;
            IsRunning = false;
            OnStoped();
        }

        protected virtual void OnStarted()
        {
            var handler = Started;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnStoped()
        {
            var handler = Stoped;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
