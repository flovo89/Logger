using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

using Logger.Common.Base.User;

using Microsoft.Win32;




namespace Logger.Common.Base.Runtime
{
    public static class UniqueIdentification
    {
        #region Static Methods

        public static Guid GetLocalMachineId ()
        {
            string installDate = null;
            try
            {
                using (RegistryKey installDateKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    int installDateValue = (int)installDateKey.GetValue("InstallDate", 0);
                    installDate = installDateValue.ToString("D", CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                installDate = null;
            }

            string macAddress = null;
            try
            {
                int macAddressValue = 0;
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    if (networkInterface.NetworkInterfaceType.ToString().ToUpperInvariant().Contains("ETHERNET"))
                    {
                        PhysicalAddress physicalAddress = networkInterface.GetPhysicalAddress();
                        byte[] physicalAddressBytes = physicalAddress.GetAddressBytes();
                        foreach (byte physicalAddressByte in physicalAddressBytes)
                        {
                            macAddressValue = unchecked( ( macAddressValue + 1 ) * physicalAddressByte );
                        }
                    }
                }
                macAddress = macAddressValue.ToString("D", CultureInfo.InvariantCulture);
            }
            catch
            {
                macAddress = null;
            }

            StringBuilder fallbackInfo = new StringBuilder();
            fallbackInfo.AppendLine(Environment.SystemDirectory);
            fallbackInfo.AppendLine(Environment.ProcessorCount.ToString("D", CultureInfo.InvariantCulture));
            fallbackInfo.AppendLine(Environment.Is64BitOperatingSystem.ToString(CultureInfo.InvariantCulture));

            string indicator = "0";
            string inputString = "0";
            if (installDate != null)
            {
                indicator = "1";
                inputString = installDate;
            }
            else if (macAddress != null)
            {
                indicator = "2";
                inputString = macAddress;
            }
            else
            {
                indicator = "3";
                inputString = fallbackInfo.ToString();
            }
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);

            byte[] outputBytes = null;
            using (MD5 hasher = MD5.Create())
            {
                outputBytes = hasher.ComputeHash(inputBytes);
            }
            Guid outputGuid = new Guid(outputBytes);
            outputGuid = Guid.Parse(indicator + outputGuid.ToString("N").Substring(1));

            return outputGuid;
        }

        public static Guid GetNetworkDomainId ()
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(SystemUsers.NetworkDomain);

            byte[] outputGuidBytes = null;

            using (MD5 hasher = MD5.Create())
            {
                outputGuidBytes = hasher.ComputeHash(nameBytes);
            }

            Guid outputGuid = new Guid(outputGuidBytes);

            return outputGuid;
        }

        #endregion
    }
}
