using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Logger.Common.Comparison;
using Logger.Common.DataTypes;
using Logger.Common.ObjectModel.Exceptions;




namespace Logger.Common.IO.Ports
{
    [Serializable]
    public sealed class SerialPortInstance : IEquatable<SerialPortInstance>,
            IComparable,
            IComparable<SerialPortInstance>,
            ISerializable
    {
        #region Constants

        private const int SerialPortCheckBaudRate = 9600;

        private const string SerialPortNamePrefix = "COM";

        #endregion




        #region Static Methods

        public static int Compare(SerialPortInstance x, SerialPortInstance y)
        {
            return (ObjectComparer.Compare<SerialPortInstance>(x, y));
        }

        public static bool Equals(SerialPortInstance x, SerialPortInstance y)
        {
            return (ObjectComparer.Equals<SerialPortInstance>(x, y));
        }

        public static SerialPortInstance[] GetAvailablePorts()
        {
            SerialPortInstance[] presentPorts = SerialPortInstance.GetPresentPorts();

            List<SerialPortInstance> availablePorts = new List<SerialPortInstance>();

            foreach (SerialPortInstance presentPort in presentPorts)
            {
                if (presentPort.IsAvailable())
                {
                    availablePorts.Add(presentPort);
                }
            }

            return (availablePorts.ToArray());
        }

        public static SerialPortInstance[] GetPresentPorts()
        {
            string[] portNames = SerialPort.GetPortNames();

            List<SerialPortInstance> instances = new List<SerialPortInstance>();

            foreach (string portName in portNames)
            {
                instances.Add(new SerialPortInstance(portName));
            }

            return (instances.ToArray());
        }

        public static bool operator ==(SerialPortInstance x, SerialPortInstance y)
        {
            return (SerialPortInstance.Equals(x, y));
        }

        public static bool operator >(SerialPortInstance x, SerialPortInstance y)
        {
            return (SerialPortInstance.Compare(x, y) > 0);
        }

        public static bool operator >=(SerialPortInstance x, SerialPortInstance y)
        {
            return (SerialPortInstance.Compare(x, y) >= 0);
        }

        public static bool operator !=(SerialPortInstance x, SerialPortInstance y)
        {
            return (!SerialPortInstance.Equals(x, y));
        }

        public static bool operator <(SerialPortInstance x, SerialPortInstance y)
        {
            return (SerialPortInstance.Compare(x, y) < 0);
        }

        public static bool operator <=(SerialPortInstance x, SerialPortInstance y)
        {
            return (SerialPortInstance.Compare(x, y) <= 0);
        }

        public static SerialPortInstance Parse(string str)
        {
            SerialPortInstance candidate = null;

            if (!SerialPortInstance.TryParse(str, out candidate))
            {
                throw (new FormatException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.SerialPortInstance_InvalidParseFormat, str)));
            }

            return (candidate);
        }

        public static bool TryParse(string str, out SerialPortInstance instance)
        {
            if (str == null)
            {
                instance = null;
                return (false);
            }

            if (str.IsEmpty())
            {
                instance = null;
                return (false);
            }

            byte number = SerialPortInstance.NameToNumber(str.ToUpperInvariant());
            if (number == 0)
            {
                instance = null;
                return (false);
            }

            instance = new SerialPortInstance(number);
            return true;
        }

        private static byte NameToNumber(string name)
        {
            if (name == null)
            {
                throw (new ArgumentNullException(nameof(name)));
            }

            if (name.IsEmpty())
            {
                throw (new EmptyStringArgumentException(nameof(name)));
            }

            byte number = 0;
            if (!byte.TryParse(name.Replace(SerialPortInstance.SerialPortNamePrefix, string.Empty), NumberStyles.Any, CultureInfo.InvariantCulture, out number))
            {
                number = 0;
            }

            return number;
        }

        private static string NumberToName(byte number)
        {
            return (SerialPortInstance.SerialPortNamePrefix + number.ToString(CultureInfo.InvariantCulture));
        }

        #endregion




        #region Instance Constructor/Destructor

        public SerialPortInstance(byte portNumber)
        {
            if (portNumber == 0)
            {
                throw (new ArgumentOutOfRangeException(nameof(portNumber)));
            }

            this.PortNumber = portNumber;
            this.PortName = SerialPortInstance.NumberToName(this.PortNumber);
        }

        public SerialPortInstance(string portName)
        {
            if (portName == null)
            {
                throw (new ArgumentNullException(nameof(portName)));
            }

            if (portName.IsEmpty())
            {
                throw (new EmptyStringArgumentException(nameof(portName)));
            }

            this.PortName = portName.ToUpperInvariant();
            this.PortNumber = SerialPortInstance.NameToNumber(this.PortName);

            if (this.PortNumber == 0)
            {
                throw (new ArgumentOutOfRangeException(nameof(portName)));
            }
        }

        private SerialPortInstance(SerializationInfo info, StreamingContext context)
        {
            this.PortName = info.GetString(nameof(this.PortName));
            this.PortNumber = info.GetByte(nameof(this.PortNumber));
        }

        #endregion




        #region Instance Properties/Indexer

        public string PortName { get; }

        public byte PortNumber { get; }

        #endregion




        #region Instance Methods

        public bool IsAvailable()
        {
            try
            {
                using (SerialPort checkPort = new SerialPort(this.PortName, SerialPortInstance.SerialPortCheckBaudRate))
                {
                    checkPort.Open();
                    return (true);
                }
            }
            catch (InvalidOperationException)
            {
                return (false);
            }
            catch (IOException)
            {
                return (false);
            }
            catch (UnauthorizedAccessException)
            {
                return (false);
            }
        }

        #endregion




        #region Overrides

        public override bool Equals(object obj)
        {
            return (this.Equals(obj as SerialPortInstance));
        }

        public override int GetHashCode()
        {
            return (this.PortNumber);
        }

        public override string ToString()
        {
            return (this.PortName);
        }

        #endregion




        #region Interface: IComparable

        int IComparable.CompareTo(object obj)
        {
            return (this.CompareTo(obj as SerialPortInstance));
        }

        #endregion




        #region Interface: IComparable<SerialPortInstance>

        public int CompareTo(SerialPortInstance other)
        {
            if (other == null)
            {
                return (1);
            }

            return (this.PortNumber.CompareTo(other.PortNumber));
        }

        #endregion




        #region Interface: IEquatable<SerialPortInstance>

        public bool Equals(SerialPortInstance other)
        {
            if (other == null)
            {
                return (false);
            }

            return (this.PortNumber == other.PortNumber);
        }

        #endregion




        #region Interface: ISerializable

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw (new ArgumentNullException(nameof(info)));
            }

            info.AddValue(nameof(this.PortName), this.PortName);
            info.AddValue(nameof(this.PortNumber), this.PortNumber);
        }

        #endregion
    }
}
