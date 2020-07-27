using System;
using System.Diagnostics;
using System.Threading;

namespace Nuclear.Server
{
    public class TickManager : IDisposable
    {
        public TickManager(int tickrate,Action onTick)
        {
            if (tickrate < 0)
                throw new ArgumentOutOfRangeException("Tick rate cannot be zero or negative");
            TickDelay = 1000/tickrate;
            AddToTick(onTick);
            Tick();
        }

        public void Dispose()
        {
            OnTick = null;
            Disposed = true;
        }

        public void AddToTick(Action onTick)
        {
            OnTick += onTick;
        }

        int sleepTime;

        void Tick()
        {
            Stopwatch watch = new Stopwatch();
            new Thread(() =>
            {
                while(!Disposed)
                {
                    watch.Start();
                    OnTick();
                    sleepTime = TickDelay - (int)watch.ElapsedMilliseconds;
                    watch.Stop();
                    watch.Reset();
                    if(sleepTime>0)
                        Thread.Sleep(sleepTime);
                }
                Console.WriteLine("Tick manager disposed");
            }).Start();
        }

        public void RemoveTick(Action onTick)
        {
            OnTick -= onTick;
        }

        bool Disposed = false;
        readonly int TickDelay;
        Action OnTick = new Action(() => { });
    }
}
