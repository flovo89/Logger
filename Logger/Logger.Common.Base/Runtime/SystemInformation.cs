using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

using Logger.Common.DataTypes;
using Logger.Common.IO.Endianess;
using Logger.Common.IO.Files;
using Logger.Common.IO.Ports;
using Logger.Common.User;
using Logger.Common.Windows;




namespace Logger.Common.Runtime
{
    public static class SystemInformation
    {
        #region Constants

        private const string SystemInfoArguments = "/report \"[%tempfile%]\" /categories all";

        private const string SystemInfoExecutable = "msinfo32";

        #endregion




        #region Static Methods

        public static string CreateReport ()
        {
            return SystemInformation.CreateReport(false);
        }

        public static void CreateReport (TextWriter writer)
        {
            SystemInformation.CreateReport(writer, false);
        }

        public static string CreateReport (bool includeSystemInformationDump)
        {
            StringBuilder sb = new StringBuilder();

            using (StringWriter w = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                SystemInformation.CreateReport(w, includeSystemInformationDump);
            }

            return sb.ToString();
        }

        public static void CreateReport (TextWriter writer, bool includeSystemInformationDump)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            try
            {
                SystemInformation.CreateReportInternal(writer, includeSystemInformationDump);
            }
            catch (Exception exception)
            {
                string exceptionMessage = exception.ToDetailedString();

                writer.WriteLine();
                writer.WriteLine(exceptionMessage);
            }
        }

        public static string CreateSystemInformationDump ()
        {
            StringBuilder sb = new StringBuilder();

            using (StringWriter w = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                SystemInformation.CreateSystemInformationDump(w);
            }

            return sb.ToString();
        }

        public static void CreateSystemInformationDump (TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            FilePath tempFile = FilePath.GetTempFile();

            ProcessStartInfo startInfo = new ProcessStartInfo(SystemInformation.SystemInfoExecutable, SystemInformation.SystemInfoArguments.Replace("[%tempfile%]", tempFile.Path));

            startInfo.CreateNoWindow = true;
            startInfo.ErrorDialog = false;
            startInfo.UseShellExecute = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;

            writer.WriteLine("Command line: " + startInfo.FileName);
            writer.WriteLine("Arguments:    " + startInfo.Arguments);
            writer.WriteLine();

            try
            {
                Process process = Process.Start(startInfo);
                process.WaitForExit();
                string dump = tempFile.ReadText();
                tempFile.TryDelete();

                dump = dump.IsEmpty() ? null : dump.Trim();

                writer.WriteLine(dump ?? "[System information dump not available]");
            }
            catch (Exception exception)
            {
                string exceptionMessage = exception.ToDetailedString();

                writer.WriteLine();
                writer.WriteLine(exceptionMessage);
            }
        }

        private static string AssemblyToString (Assembly assembly)
        {
            if (assembly == null)
            {
                return "[null]";
            }

            StringBuilder str = new StringBuilder();

            str.Append(assembly.FullName.PadRight(150));
            str.Append(( "Dynamic: " + assembly.IsDynamic ).PadRight(20));
            if (!assembly.IsDynamic)
            {
                str.Append(( "GAC: " + assembly.GlobalAssemblyCache ).PadRight(20));
                str.Append(( "Host context: " + assembly.HostContext.ToString(CultureInfo.InvariantCulture) ).PadRight(30));
                str.Append(( "Fully trusted: " + assembly.IsFullyTrusted ).PadRight(30));
                str.Append(( "Security rule set: " + assembly.SecurityRuleSet ).PadRight(40));
                str.Append(( "Image runtime version: " + assembly.ImageRuntimeVersion ).PadRight(40));
                str.Append("Location: " + ( assembly.Location ?? "[null]" ));
            }

            return str.ToString().Trim();
        }

        private static void CreateReportInternal (TextWriter writer, bool includeSystemInformationDump)
        {
            DateTime now = DateTime.Now;
            writer.WriteLine("Date and time information");
            writer.WriteLine("-------------------------");
            writer.WriteLine("Time:                    " + now.ToTechnical('-'));
            writer.WriteLine("Local time:              " + now.ToLocalTime().ToTechnical('-'));
            writer.WriteLine("UTC time:                " + now.ToUniversalTime().ToTechnical('-'));
            writer.WriteLine("Day of week:             " + now.DayOfWeek);
            writer.WriteLine("Day of month:            " + now.Day.ToString(CultureInfo.InvariantCulture));
            writer.WriteLine("Day of year:             " + now.DayOfYear.ToString(CultureInfo.InvariantCulture));
            writer.WriteLine("Is daylight saving time: " + now.IsDaylightSavingTime());
            writer.WriteLine();

            TimeZone timeZone = TimeZone.CurrentTimeZone;
            writer.WriteLine("Time zone information");
            writer.WriteLine("---------------------");
            writer.WriteLine("Daylight saving name: " + timeZone.DaylightName);
            writer.WriteLine("Time zone:            " + timeZone.StandardName);
            writer.WriteLine();

            writer.WriteLine("Environment information");
            writer.WriteLine("-----------------------");
            writer.WriteLine("Machine name:                     " + Environment.MachineName);
            writer.WriteLine("OS version:                       " + Environment.OSVersion);
            writer.WriteLine("CLR version (Environment):        " + Environment.Version.ToString(4));
            writer.WriteLine("CLR version (RuntimeEnvironment): " + RuntimeEnvironment.GetSystemVersion());
            writer.WriteLine("Is 64 bit OS:                     " + Environment.Is64BitOperatingSystem);
            writer.WriteLine("Is 64 bit process:                " + Environment.Is64BitProcess);
            writer.WriteLine("Endianess:                        " + EndianConverter.LocalMachine);
            writer.WriteLine("Default encoding:                 " + Encoding.Default.WebName);
            writer.WriteLine("Current culture:                  " + CultureInfo.CurrentCulture);
            writer.WriteLine("Current UI culture:               " + CultureInfo.CurrentUICulture);
            writer.WriteLine("Installed UI culture:             " + CultureInfo.InstalledUICulture);
            writer.WriteLine("Has shutdown started:             " + Environment.HasShutdownStarted);
            writer.WriteLine("Exit code:                        " + Environment.ExitCode.ToString(CultureInfo.InvariantCulture));
            writer.WriteLine("Tick count:                       " + Environment.TickCount.ToString(CultureInfo.InvariantCulture) + " ms");
            writer.WriteLine("System configuration file:        " + RuntimeEnvironment.SystemConfigurationFile);
            writer.WriteLine("Command line:                     " + CommandLine.ProcessCommandLine);
            writer.WriteLine();

            Process currentProcess = Process.GetCurrentProcess();
            writer.WriteLine("Process information");
            writer.WriteLine("-------------------");
            writer.WriteLine("Process name:              " + currentProcess.ProcessName);
            writer.WriteLine("Process ID:                " + currentProcess.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteLine("Process handle:            " + currentProcess.Handle.ToInt64().ToString(CultureInfo.InvariantCulture));
            writer.WriteLine("Priority class:            " + currentProcess.PriorityClass);
            writer.WriteLine("Base priority:             " + currentProcess.BasePriority.ToString(CultureInfo.InvariantCulture));
            writer.WriteLine("Is priority boost enabled: " + currentProcess.PriorityBoostEnabled);
            writer.WriteLine("Is responding:             " + currentProcess.Responding);
            writer.WriteLine("Thread count:              " + currentProcess.Threads.Count.ToString(CultureInfo.InvariantCulture));
            writer.WriteLine("Handle count:              " + currentProcess.HandleCount.ToString(CultureInfo.InvariantCulture));
            writer.WriteLine("Start time:                " + currentProcess.StartTime.ToTechnical('-'));
            writer.WriteLine("Session ID:                " + currentProcess.SessionId.ToString(CultureInfo.InvariantCulture));
            writer.WriteLine();

            WindowsIdentity currentIdentity = SystemUsers.CurrentIdentity;
            writer.WriteLine("User and logon information");
            writer.WriteLine("--------------------------");
            writer.WriteLine("Domain (Environment): " + Environment.UserDomainName);
            writer.WriteLine("Domain (SystemUsers): " + SystemUsers.CurrentDomain);
            writer.WriteLine("Name (Environment):   " + Environment.UserName);
            writer.WriteLine("Name (SystemUsers):   " + SystemUsers.CurrentUser);
            writer.WriteLine("Is interactive:       " + Environment.UserInteractive);
            writer.WriteLine("Is administrator:     " + SystemUsers.IsCurrentAdministrator);
            writer.WriteLine("Is anonymous:         " + currentIdentity.IsAnonymous);
            writer.WriteLine("Is authenticated:     " + currentIdentity.IsAuthenticated);
            writer.WriteLine("Is guest:             " + currentIdentity.IsGuest);
            writer.WriteLine("Is system:            " + currentIdentity.IsSystem);
            writer.WriteLine("Authentication type:  " + currentIdentity.AuthenticationType);
            writer.WriteLine("Impersonation level:  " + currentIdentity.ImpersonationLevel);
            writer.WriteLine("User SID:             " + currentIdentity.User);
            writer.WriteLine("Local domain:         " + SystemUsers.LocalDomain);
            writer.WriteLine("Network domain:       " + SystemUsers.NetworkDomain);
            writer.WriteLine("Token:                " + SystemUsers.CurrentToken.ToInt64().ToString(CultureInfo.InvariantCulture));
            writer.WriteLine();

            writer.WriteLine("Unique identification information");
            writer.WriteLine("---------------------------------");
            writer.WriteLine("Local machine ID:  " + UniqueIdentification.GetLocalMachineId().ToString("N"));
            writer.WriteLine("Network domain ID: " + UniqueIdentification.GetNetworkDomainId().ToString("N"));
            writer.WriteLine();

            writer.WriteLine("System properties");
            writer.WriteLine("-----------------");
            PropertyInfo[] properties = typeof(System.Windows.Forms.SystemInformation).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (PropertyInfo property in properties)
            {
                try
                {
                    object value = property.GetValue(null, null);
                    writer.WriteLine("System property: " + property.Name.PadRight(50, ' ') + " = " + value);
                }
                catch
                {
                }
            }
            writer.WriteLine();

            writer.WriteLine("Screen informations");
            writer.WriteLine("-------------------");
            writer.WriteLine("Primary Screen:   " + Screen.PrimaryScreen.DeviceName);
            foreach (Screen screen in Screen.AllScreens)
            {
                writer.WriteLine("Available screen: " + SystemInformation.ScreenToString(screen));
            }
            writer.WriteLine();

            writer.WriteLine("Processor informations");
            writer.WriteLine("----------------------");
            writer.WriteLine("Processor count:    " + Environment.ProcessorCount.ToString(CultureInfo.InvariantCulture));
            writer.WriteLine("Processor affinity: " + currentProcess.ProcessorAffinity);
            writer.WriteLine("Total time:         " + ( (long)currentProcess.TotalProcessorTime.TotalMilliseconds ).ToString(CultureInfo.InvariantCulture) + " ms");
            writer.WriteLine("User time:          " + ( (long)currentProcess.UserProcessorTime.TotalMilliseconds ).ToString(CultureInfo.InvariantCulture) + " ms");
            writer.WriteLine("Privileged time:    " + ( (long)currentProcess.PrivilegedProcessorTime.TotalMilliseconds ).ToString(CultureInfo.InvariantCulture) + " ms");
            writer.WriteLine();

            writer.WriteLine("Memory informations");
            writer.WriteLine("-------------------");
            writer.WriteLine("Working set (current):    " + currentProcess.WorkingSet64.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " bytes");
            writer.WriteLine("Working set (peak):       " + currentProcess.PeakWorkingSet64.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " bytes");
            writer.WriteLine("Working set (maximum):    " + currentProcess.MaxWorkingSet.ToInt64().ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " bytes");
            writer.WriteLine("Working set (minimum):    " + currentProcess.MinWorkingSet.ToInt64().ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " bytes");
            writer.WriteLine("Virtual memory (current): " + currentProcess.VirtualMemorySize64.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " bytes");
            writer.WriteLine("Virtual memory (peak):    " + currentProcess.PeakVirtualMemorySize64.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " bytes");
            writer.WriteLine("Paged memory (current):   " + currentProcess.PagedMemorySize64.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " bytes");
            writer.WriteLine("Paged memory (peak):      " + currentProcess.PeakPagedMemorySize64.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " bytes");
            writer.WriteLine("Private memory:           " + currentProcess.PrivateMemorySize64.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " bytes");
            writer.WriteLine("Paged system memory:      " + currentProcess.PagedSystemMemorySize64.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " bytes");
            writer.WriteLine("Nonpaged system memory:   " + currentProcess.NonpagedSystemMemorySize64.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " bytes");
            writer.WriteLine();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            writer.WriteLine("Assembly informations");
            writer.WriteLine("---------------------");
            foreach (Assembly assembly in assemblies)
            {
                writer.WriteLine("Loaded assembly: " + SystemInformation.AssemblyToString(assembly));
            }
            writer.WriteLine();

            writer.WriteLine("Native module informations");
            writer.WriteLine("--------------------------");
            writer.WriteLine("Main module:   " + SystemInformation.ProcessModuleToString(currentProcess.MainModule));
            foreach (ProcessModule processModule in currentProcess.Modules)
            {
                writer.WriteLine("Loaded module: " + SystemInformation.ProcessModuleToString(processModule));
            }
            writer.WriteLine();

            IntPtr[] windows = SystemWindows.FindWindows();
            writer.WriteLine("Window informations");
            writer.WriteLine("-------------------");
            writer.WriteLine("Main window handle: " + currentProcess.MainWindowHandle.ToInt64().ToString(CultureInfo.InvariantCulture));
            writer.WriteLine("Main window title:  " + currentProcess.MainWindowTitle);
            foreach (IntPtr window in windows)
            {
                try
                {
                    writer.WriteLine("Available window:   " + window.ToInt64().ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " @ " + SystemWindows.GetProcess(window).Id.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " = " + ( SystemWindows.GetWindowTitle(window) ?? "[null]" ));
                }
                catch
                {
                }
            }
            writer.WriteLine();

            writer.WriteLine("Debugger informations");
            writer.WriteLine("---------------------");
            writer.WriteLine("Is attached: " + Debugger.IsAttached);
            writer.WriteLine("Is logging:  " + Debugger.IsLogging());
            writer.WriteLine();

            Array values = Enum.GetValues(typeof(Environment.SpecialFolder));
            writer.WriteLine("Directory informations");
            writer.WriteLine("----------------------");
            writer.WriteLine("Current directory:  " + Environment.CurrentDirectory);
            writer.WriteLine("System directory:   " + Environment.SystemDirectory);
            writer.WriteLine("Runtime directory:  " + RuntimeEnvironment.GetRuntimeDirectory());
            writer.WriteLine("Temp directory:     " + DirectoryPath.GetTempDirectory());
            foreach (int specialFolder in values)
            {
                try
                {
                    Environment.SpecialFolder folder = (Environment.SpecialFolder)specialFolder;
                    writer.WriteLine("Environment folder: " + folder.ToString().PadRight(30, ' ') + " = " + Environment.GetFolderPath(folder));
                }
                catch
                {
                }
            }
            writer.WriteLine();

            string[] drives = Environment.GetLogicalDrives();
            writer.WriteLine("Drive informations");
            writer.WriteLine("------------------");
            foreach (string drive in drives)
            {
                writer.WriteLine("Present drive: " + SystemInformation.DriveInfoToString(new DriveInfo(drive)));
            }
            writer.WriteLine();

            SerialPortInstance[] serialPorts = SerialPortInstance.GetPresentPorts();
            writer.WriteLine("Serial port informations");
            writer.WriteLine("------------------------");
            foreach (SerialPortInstance serialPort in serialPorts)
            {
                writer.WriteLine("Present port: " + serialPort);
            }
            writer.WriteLine();

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            writer.WriteLine("Network informations");
            writer.WriteLine("--------------------");
            writer.WriteLine("Is network available:      " + NetworkInterface.GetIsNetworkAvailable());
            foreach (NetworkInterface interf in interfaces)
            {
                writer.WriteLine("Present network interface: " + SystemInformation.NetworkInterfaceToString(interf));
            }
            writer.WriteLine();

            Process[] processes = Process.GetProcesses();
            writer.WriteLine("Running processes");
            writer.WriteLine("-----------------");
            foreach (Process process in processes)
            {
                writer.WriteLine("Running process: " + process.Id.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' ') + " = " + process.ProcessName);
            }
            writer.WriteLine();

            writer.WriteLine("Available environment variables");
            writer.WriteLine("-------------------------------");
            IDictionary userEnvironmentVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);
            foreach (DictionaryEntry userEnvironmentVariable in userEnvironmentVariables)
            {
                writer.WriteLine("User variable:    " + userEnvironmentVariable.Key.ToString().PadRight(30, ' ') + " = " + userEnvironmentVariable.Value);
            }
            IDictionary processEnvironmentVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
            foreach (DictionaryEntry processEnvironmentVariable in processEnvironmentVariables)
            {
                writer.WriteLine("Process variable: " + processEnvironmentVariable.Key.ToString().PadRight(30, ' ') + " = " + processEnvironmentVariable.Value);
            }
            IDictionary machineEnvironmentVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
            foreach (DictionaryEntry machineEnvironmentVariable in machineEnvironmentVariables)
            {
                writer.WriteLine("Machine variable: " + machineEnvironmentVariable.Key.ToString().PadRight(30, ' ') + " = " + machineEnvironmentVariable.Value);
            }
            writer.WriteLine();

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            writer.WriteLine("Available cultures");
            writer.WriteLine("------------------");
            writer.WriteLine("Invariant culture:    " + SystemInformation.CultureInfoToString(CultureInfo.InvariantCulture));
            foreach (CultureInfo culture in cultures)
            {
                writer.WriteLine("Available culture:    " + SystemInformation.CultureInfoToString(culture));
            }
            writer.WriteLine();

            EncodingInfo[] encodings = Encoding.GetEncodings();
            writer.WriteLine("Available encodings");
            writer.WriteLine("-------------------");
            foreach (EncodingInfo encoding in encodings)
            {
                writer.WriteLine("Available encoding: " + SystemInformation.EncodingToString(encoding.GetEncoding()));
            }
            writer.WriteLine();

            if (includeSystemInformationDump)
            {
                writer.WriteLine("System Information Report dump");
                writer.WriteLine("------------------------------");
                writer.WriteLine();
                SystemInformation.CreateSystemInformationDump(writer);
            }
        }

        private static string CultureInfoToString (CultureInfo culture)
        {
            StringBuilder str = new StringBuilder();

            str.Append(( culture.Name.IsEmpty() ? "[null]" : culture.Name ).PadRight(30));
            str.Append(( "ISO (2): " + culture.TwoLetterISOLanguageName ).PadRight(20));
            str.Append(( "ISO (3): " + culture.ThreeLetterISOLanguageName ).PadRight(20));
            str.Append(( "WIN (3): " + culture.ThreeLetterWindowsLanguageName ).PadRight(20));
            str.Append(( "IETF: " + culture.IetfLanguageTag ).PadRight(25));
            str.Append(( "LCID: " + culture.LCID.ToString(CultureInfo.InvariantCulture) ).PadRight(20));
            str.Append(( "Keyboard layout ID: " + culture.KeyboardLayoutId.ToString(CultureInfo.InvariantCulture) ).PadRight(30));
            str.Append(( "Culture types: " + culture.CultureTypes ).PadRight(100));
            str.Append(( "User override: " + culture.UseUserOverride ).PadRight(30));
            str.Append(( "Neutral: " + culture.IsNeutralCulture ).PadRight(30));
            str.Append(( "Read-only: " + culture.IsReadOnly ).PadRight(30));
            str.Append("Parent: " + culture.Parent.Name.PadRight(30));
            str.Append(( "Used calendar: " + culture.Calendar ).PadRight(80));
            str.Append(( "English name: " + culture.EnglishName ).PadRight(80));
            str.Append(( "Display name: " + culture.DisplayName ).PadRight(80));
            str.Append(( "Native name: " + culture.NativeName ).PadRight(80));

            return str.ToString().Trim();
        }

        private static string DriveInfoToString (DriveInfo drive)
        {
            StringBuilder str = new StringBuilder();

            str.Append(drive.Name.PadRight(10));
            str.Append(( "Ready: " + drive.IsReady ).PadRight(20));
            str.Append(( "Root: " + drive.RootDirectory ).PadRight(20));

            StringBuilder str2 = new StringBuilder();

            try
            {
                str2.Append(( "Type: " + drive.DriveType ).PadRight(40));
                str2.Append(( "Format: " + drive.DriveFormat ).PadRight(40));
                str2.Append(( "Label: " + drive.VolumeLabel ).PadRight(40));
                str2.Append(( "Available free: " + drive.AvailableFreeSpace.ToString(CultureInfo.InvariantCulture).PadLeft(16) + " bytes" ).PadRight(40));
                str2.Append(( "Total free: " + drive.TotalFreeSpace.ToString(CultureInfo.InvariantCulture).PadLeft(16) + " bytes" ).PadRight(40));
                str2.Append(( "Total size: " + drive.TotalSize.ToString(CultureInfo.InvariantCulture).PadLeft(16) + " bytes" ).PadRight(40));


                str.Append(str2);
            }
            catch
            {
                str.Append("[Drive not ready for further information]");
            }

            return str.ToString().Trim();
        }

        private static string EncodingToString (Encoding encoding)
        {
            StringBuilder str = new StringBuilder();

            str.Append(encoding.WebName.PadRight(40));
            str.Append(( "Body name: " + encoding.BodyName ).PadRight(40));
            str.Append(( "Header name: " + encoding.HeaderName ).PadRight(40));
            str.Append(( "Code page: " + encoding.CodePage ).PadRight(40));
            str.Append(( "Windows code page: " + encoding.WindowsCodePage ).PadRight(40));
            str.Append(( "Browser display: " + encoding.IsBrowserDisplay ).PadRight(30));
            str.Append(( "Browser save: " + encoding.IsBrowserSave ).PadRight(30));
            str.Append(( "Mail+News display: " + encoding.IsMailNewsDisplay ).PadRight(30));
            str.Append(( "Mail+News save: " + encoding.IsMailNewsSave ).PadRight(30));
            str.Append(( "Read-only: " + encoding.IsReadOnly ).PadRight(30));
            str.Append(( "Single byte: " + encoding.IsSingleByte ).PadRight(30));
            str.Append(( "Always normalized: " + encoding.IsAlwaysNormalized() ).PadRight(30));
            str.Append("Name: " + encoding.EncodingName);

            return str.ToString().Trim();
        }

        private static string NetworkInterfaceToString (NetworkInterface intf)
        {
            StringBuilder str = new StringBuilder();

            str.Append(intf.Id.PadRight(50));
            str.Append(( "Interface type: " + intf.NetworkInterfaceType ).PadRight(40));
            str.Append(( "Operational status: " + intf.OperationalStatus ).PadRight(30));
            str.Append(( "Speed: " + intf.Speed.ToString(CultureInfo.InvariantCulture).PadLeft(10) ).PadRight(30));
            str.Append(( "Receive-only: " + intf.IsReceiveOnly ).PadRight(30));
            str.Append(( "Supports multicast: " + intf.SupportsMulticast ).PadRight(30));
            str.Append(( "Supports IPv4: " + intf.Supports(NetworkInterfaceComponent.IPv4) ).PadRight(30));
            str.Append(( "Supports IPv6" + intf.Supports(NetworkInterfaceComponent.IPv6) ).PadRight(30));
            str.Append(( "Physical address: " + intf.GetPhysicalAddress() ).PadRight(50));

            IPInterfaceProperties ip = intf.GetIPProperties();

            str.Append(( "DNS suffix: " + ( ip.DnsSuffix ?? "[null]" ) ).PadRight(40));
            str.Append(( "DNS enabled: " + ip.IsDnsEnabled ).PadRight(30));
            str.Append(( "Dynamic DNS: " + ip.IsDynamicDnsEnabled ).PadRight(30));
            str.Append(( "Interface name: " + intf.Name ).PadRight(100));
            str.Append(( "Interface description: " + intf.Description ).PadRight(120));

            bool appended = false;

            foreach (UnicastIPAddressInformation unicastIpAddressInformation in ip.UnicastAddresses)
            {
                if (appended)
                {
                    str.Append(", ");
                }
                str.Append("Unicast=" + unicastIpAddressInformation.Address);
                appended = true;
            }

            foreach (MulticastIPAddressInformation multicastIpAddressInformation in ip.MulticastAddresses)
            {
                if (appended)
                {
                    str.Append(", ");
                }
                str.Append("Multicast=" + multicastIpAddressInformation.Address);
                appended = true;
            }

            foreach (IPAddressInformation ipAddressInformation in ip.AnycastAddresses)
            {
                if (appended)
                {
                    str.Append(", ");
                }
                str.Append("Anycast=" + ipAddressInformation.Address);
                appended = true;
            }

            foreach (IPAddress dhcpServerAddress in ip.DhcpServerAddresses)
            {
                if (appended)
                {
                    str.Append(", ");
                }
                str.Append("DHCP=" + dhcpServerAddress);
                appended = true;
            }

            foreach (IPAddress winsServerAddress in ip.WinsServersAddresses)
            {
                if (appended)
                {
                    str.Append(", ");
                }
                str.Append("WINS=" + winsServerAddress);
                appended = true;
            }

            foreach (IPAddress dnsAddress in ip.DnsAddresses)
            {
                if (appended)
                {
                    str.Append(", ");
                }
                str.Append("DNS=" + dnsAddress);
                appended = true;
            }

            foreach (GatewayIPAddressInformation gatewayIpAddressInformation in ip.GatewayAddresses)
            {
                if (appended)
                {
                    str.Append(", ");
                }
                str.Append("Gateway=" + gatewayIpAddressInformation.Address);
                appended = true;
            }

            return str.ToString().Trim();
        }

        private static string ProcessModuleToString (ProcessModule module)
        {
            StringBuilder str = new StringBuilder();

            str.Append(module.ModuleName.PadRight(80));
            str.Append(( "File version: " + module.FileVersionInfo.FileVersion ).PadRight(80));
            str.Append(( "Product version: " + module.FileVersionInfo.ProductVersion ).PadRight(50));
            str.Append(( "Memory size: " + module.ModuleMemorySize.ToString(CultureInfo.InvariantCulture) + " bytes" ).PadRight(50));
            str.Append(( "Base address: " + module.BaseAddress.ToInt64().ToString(CultureInfo.InvariantCulture) ).PadRight(40));
            str.Append(( "Entry point address: " + module.EntryPointAddress.ToInt64().ToString(CultureInfo.InvariantCulture) ).PadRight(40));
            str.Append("File name: " + module.FileName);

            return str.ToString().Trim();
        }

        private static string ScreenToString (Screen screen)
        {
            StringBuilder str = new StringBuilder();

            str.Append(screen.DeviceName.PadRight(20));
            str.Append(( "Bits per pixel: " + screen.BitsPerPixel ).PadRight(30));
            str.Append(( "Bounds: " + screen.Bounds ).PadRight(50));
            str.Append(( "Working area: " + screen.WorkingArea ).PadRight(50));

            return str.ToString().Trim();
        }

        #endregion
    }
}
