using System;
using System.IO.Ports;
using System.Linq;
using log4net;

namespace com.iiko.unitech.Protocol
{
    public class DeviceBuffer
    {
        private static readonly ILog Log = LogFactory.Instance.GetLogger(typeof(MsrDriver));

        public byte[] Data => data;

        public int Length { get; private set; }
        private byte[] data = new byte[0];

        public void AppendExistingData(SerialPort port)
        {
            try
            {
                var bytesToRead = port.BytesToRead;
                if (bytesToRead <= 0)
                    return;

                if (data.Length < Length + bytesToRead)
                    Array.Resize(ref data, Length + bytesToRead);
                var count = port.Read(data, Length, bytesToRead);
                Log.Debug($"< {Utils.ByteArrayToHexString(data, Length, count)} ({count} bytes)");
                Length += count;
            }
            catch (Exception e)
            {
                if (MsrDriver.IsSerialIoException(e))
                {
                    Log.Error($"Failed to read from port: {e.Message}");
                }
            }
        }

        public void Append(byte[] source)
        {
            if (data.Length < Length + source.Length)
                Array.Resize(ref data, Length + source.Length);

            Array.Copy(source, 0, data, Length, source.Length);
            Log.Debug($"< {Utils.ByteArrayToHexString(data, Length, source.Length)} ({source.Length} bytes)");
            Length += source.Length;
        }

        public void RemoveFirst(int count)
        {
            if (count <= 0)
                return;

            for (var i = 0; i < Length - count; i++)
                data[i] = data[i + count];
            data = data.Skip(count).ToArray();
            Length -= count;
        }

        public void Clear()
        {
            Length = 0;
        }
    }
}
