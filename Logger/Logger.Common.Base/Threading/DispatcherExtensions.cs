using System;
using System.Windows.Threading;




namespace Logger.Common.Base.Threading
{
    public static class DispatcherExtensions
    {
        #region Static Methods

        public static DispatcherTimer BeginInvokeDelayed (this Dispatcher dispatcher, int delay, DispatcherPriority priority, Delegate action, params object[] args)
        {
            return dispatcher.BeginInvokeDelayed(TimeSpan.FromMilliseconds(delay), priority, action, args);
        }

        public static DispatcherTimer BeginInvokeDelayed (this Dispatcher dispatcher, TimeSpan delay, DispatcherPriority priority, Delegate action, params object[] args)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            DispatcherTimer timer = new DispatcherTimer(delay, priority, (s, e) =>
            {
                DispatcherTimer timer2 = (DispatcherTimer)s;
                timer2.Stop();
                Tuple<Delegate, object[]> data = (Tuple<Delegate, object[]>)timer2.Tag;
                timer2.Dispatcher.Invoke(data.Item1, data.Item2);
            }, dispatcher);
            timer.Tag = new Tuple<Delegate, object[]>(action, args);
            timer.Start();
            return timer;
        }

        public static void DoEvents (this Dispatcher dispatcher)
        {
            dispatcher.DoEvents(DispatcherPriority.SystemIdle);
        }

        public static void DoEvents (this Dispatcher dispatcher, DispatcherPriority priority)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            dispatcher.Invoke(priority, new Action(() =>
            {
            }));
        }

        #endregion
    }
}
