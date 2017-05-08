using System;
using System.IO.Ports;




namespace Logger.Common.IO.Ports
{
    public static class SerialPortExtensions
    {
        #region Static Methods

        public static SerialPortInstance GetSerialPortInstance (this SerialPort port)
        {
            if (port == null)
            {
                throw new ArgumentNullException(nameof(port));
            }

            return new SerialPortInstance(port.PortName);
        }

        #endregion
    }
}
