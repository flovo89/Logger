namespace Logger.Core.Interfaces
{
    public static class SessionExitCodes
    {
        #region Constants

        public const int AnotherInstanceIsAlreadyRunning = -3;

        public const int LogoffSystem = 2;

        public const int Normal = 0;

        public const int RestartSystem = 3;

        public const int SessionEnding = 5;

        public const int ShutdownSystem = 4;

        public const int SystemRequirementsNotFulfilled = -2;

        #endregion
    }
}
