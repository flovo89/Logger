using System;
using System.ComponentModel;
using System.Globalization;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;

using Logger.Common.DataTypes;
using Logger.Common.ObjectModel.Exceptions;
using Logger.Common.Runtime;




// ReSharper disable ConditionIsAlwaysTrueOrFalse




namespace Logger.Common.User
{
    public static class SystemUsers
    {
        #region Constants

        private const int Logon32LogonInteractive = 2;

        private const int Logon32ProviderDefault = 0;

        #endregion




        #region Static Properties/Indexer

        public static string CurrentDomain
        {
            get
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent(false))
                {
                    if (identity == null)
                    {
                        return null;
                    }

                    string domain = null;
                    string username = null;
                    SystemUsers.ExtractDomainAndUserName(identity.Name, true, out domain, out username);

                    return domain;
                }
            }
        }

        public static WindowsIdentity CurrentIdentity
        {
            get
            {
                return WindowsIdentity.GetCurrent(false);
            }
        }

        public static IntPtr CurrentToken
        {
            get
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent(false))
                {
                    if (identity == null)
                    {
                        return IntPtr.Zero;
                    }

                    return identity.Token;
                }
            }
        }

        public static string CurrentUser
        {
            get
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent(false))
                {
                    if (identity == null)
                    {
                        return null;
                    }

                    string domain = null;
                    string username = null;
                    SystemUsers.ExtractDomainAndUserName(identity.Name, true, out domain, out username);

                    return username;
                }
            }
        }

        public static bool IsCurrentAdministrator
        {
            get
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent(false))
                {
                    if (identity == null)
                    {
                        return false;
                    }

                    WindowsPrincipal principal = new WindowsPrincipal(identity);

                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
        }

        public static bool IsCurrentSystem
        {
            get
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent(false))
                {
                    if (identity == null)
                    {
                        return true;
                    }

                    return identity.IsSystem;
                }
            }
        }

        public static string LocalDomain
        {
            get
            {
                return Environment.MachineName;
            }
        }

        public static string NetworkDomain
        {
            get
            {
                using (ManagementObject mgmtObj = new ManagementObject("Win32_ComputerSystem.Name='" + SystemUsers.LocalDomain + "'"))
                {
                    mgmtObj.Get();

                    string networkDomain = mgmtObj["domain"].ToString();
                    networkDomain = SystemUsers.ResolveDomain(networkDomain);

                    return networkDomain;
                }
            }
        }

        #endregion




        #region Static Methods

        public static void CloseLogonToken (IntPtr token)
        {
            if (token == IntPtr.Zero)
            {
                return;
            }

            SystemUsers.CloseHandle(token);
        }

        public static IntPtr CreateLogonToken (string username, string password, string domain, out int errorCode)
        {
            if (username == null)
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (username.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(username));
            }

            domain = domain ?? string.Empty;

            IntPtr token = IntPtr.Zero;

            username = SystemUsers.ResolveUserName(username);
            domain = SystemUsers.ResolveDomain(domain);

            bool returnValue = SystemUsers.LogonUser(username, domain, password, SystemUsers.Logon32LogonInteractive, SystemUsers.Logon32ProviderDefault, ref token);

            if (!returnValue || ( token == IntPtr.Zero ))
            {
                errorCode = WindowsApi.GetLastErrorCode();

                return IntPtr.Zero;
            }

            errorCode = 0;
            return token;
        }

        public static void ExtractDomainAndUserName (string user, bool resolve, out string domain, out string username)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            domain = null;
            username = null;

            int index = user.IndexOf('\\');

            if (index == -1)
            {
                username = user;
            }
            else
            {
                if (index == 0)
                {
                    domain = string.Empty;
                }
                else
                {
                    domain = user.Substring(0, index);
                }

                if (index >= user.Length - 1)
                {
                    username = string.Empty;
                }
                else
                {
                    username = user.Substring(index + 1);
                }
            }

            if (resolve)
            {
                username = username == null ? null : SystemUsers.ResolveUserName(username);
                domain = domain == null ? null : SystemUsers.ResolveDomain(domain);
            }
        }

        public static CultureInfo GetCurrentUserCulture ()
        {
            return new CultureInfo(SystemUsers.GetUserDefaultUILanguage(), true);
        }

        public static CultureInfo GetUserCulture (WindowsIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (SystemUsers.CurrentIdentity.User.Equals(identity.User))
            {
                return SystemUsers.GetCurrentUserCulture();
            }

            WindowsIdentity tempIdentity = null;
            WindowsImpersonationContext tempImpersonationContext = null;
            UserProfile tempUserProfile = null;

            try
            {
                SystemUsers.Impersonate(identity.Token, true, out tempIdentity, out tempImpersonationContext, out tempUserProfile);

                return SystemUsers.GetCurrentUserCulture();
            }
            finally
            {
                SystemUsers.Unimpersonate(identity.Token, tempIdentity, tempImpersonationContext, tempUserProfile);
            }
        }

        public static void Impersonate (IntPtr token, bool loadUserProfile, out WindowsIdentity identity, out WindowsImpersonationContext impersonationContext, out UserProfile userProfile)
        {
            bool success = false;

            identity = null;
            impersonationContext = null;
            userProfile = null;

            try
            {
                identity = new WindowsIdentity(token);
                impersonationContext = identity.Impersonate();

                if (loadUserProfile)
                {
                    USERPROFILE profileInfo = new USERPROFILE();
                    profileInfo.dwSize = Marshal.SizeOf(profileInfo);
                    profileInfo.lpUserName = identity.Name;
                    profileInfo.dwFlags = 1;

                    bool loadSuccess = SystemUsers.LoadUserProfile(token, ref profileInfo);
                    if (!loadSuccess)
                    {
                        int errorCode = WindowsApi.GetLastErrorCode();
                        string errorMessage = WindowsApi.GetErrorMessage(errorCode);

                        throw new Win32Exception(errorCode, errorMessage);
                    }

                    userProfile = new UserProfile(profileInfo);
                }

                success = true;
            }
            finally
            {
                if (!success)
                {
                    SystemUsers.Unimpersonate(token, identity, impersonationContext, userProfile);
                }
            }
        }

        public static string ResolveDomain (string domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException(nameof(domain));
            }

            if (domain.IsEmpty() || string.Equals(domain.Trim(), ".", StringComparison.InvariantCultureIgnoreCase))
            {
                return SystemUsers.LocalDomain;
            }

            return domain;
        }

        public static string ResolveUserName (string username)
        {
            if (username == null)
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (username.IsEmpty() || string.Equals(username.Trim(), ".", StringComparison.InvariantCultureIgnoreCase))
            {
                //Do not use CurrentUser because it would call ResolveUserName again
                return Environment.UserName;
            }

            return username;
        }

        public static void Unimpersonate (IntPtr token, WindowsIdentity identity, WindowsImpersonationContext impersonationContext, UserProfile userProfile)
        {
            if (userProfile != null)
            {
                SystemUsers.UnloadUserProfile(token, userProfile.NativeUserProfile.hProfile);
            }

            if (impersonationContext != null)
            {
                impersonationContext.Undo();
                impersonationContext.Dispose();
            }

            if (identity != null)
            {
                identity.Dispose();
            }
        }

        [DllImport ("kernel32.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool CloseHandle (IntPtr handle);

        [DllImport ("Kernel32.dll", SetLastError = false)]
        private static extern ushort GetUserDefaultUILanguage ();

        [DllImport ("userenv.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool LoadUserProfile (IntPtr hToken, ref USERPROFILE lpProfileInfo);

        [DllImport ("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool LogonUser (string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport ("userenv.dll", SetLastError = false)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool UnloadUserProfile (IntPtr hToken, IntPtr hProfileInfo);

        #endregion




        #region Type: USERPROFILE

        // ReSharper disable InconsistentNaming
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        [StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct USERPROFILE
        {
            public int dwSize;

            public int dwFlags;

            public string lpUserName;

            public string lpProfilePath;

            public string lpDefaultPath;

            public string lpServerName;

            public string lpPolicyPath;

            public IntPtr hProfile;
        }




        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore InconsistentNaming

        #endregion
    }
}
