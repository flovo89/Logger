namespace Logger.Common.User
{
    public sealed class UserProfile
    {
        #region Instance Constructor/Destructor

        internal UserProfile (SystemUsers.USERPROFILE nativeUserProfile)
        {
            this.NativeUserProfile = nativeUserProfile;
        }

        #endregion




        #region Instance Properties/Indexer

        public string DefaultPath
        {
            get
            {
                return this.NativeUserProfile.lpDefaultPath;
            }
        }

        public string PolicyPath
        {
            get
            {
                return this.NativeUserProfile.lpPolicyPath;
            }
        }

        public string ProfilePath
        {
            get
            {
                return this.NativeUserProfile.lpProfilePath;
            }
        }

        public string ServerName
        {
            get
            {
                return this.NativeUserProfile.lpServerName;
            }
        }

        public string UserName
        {
            get
            {
                return this.NativeUserProfile.lpUserName;
            }
        }

        internal SystemUsers.USERPROFILE NativeUserProfile { get; }

        #endregion
    }
}
