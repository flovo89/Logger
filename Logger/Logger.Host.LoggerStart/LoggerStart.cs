using System;

using Logger.Core.Hosting;




namespace Logger.Host.LoggerStart
{
    public static class LoggerStart
    {
        #region Static Methods

        [STAThread]
        public static int Main ()
        {
            Bootstrapper bootstrapper = new Bootstrapper();
            return bootstrapper.Run();
        }

        #endregion
    }
}
