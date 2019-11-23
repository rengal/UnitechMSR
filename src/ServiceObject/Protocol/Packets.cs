using System.Linq;
using System.Text;

namespace com.iiko.unitech.Protocol
{
    public class Packet
    {
        public static byte Esc = 0x1B;
        public static byte Fs = 0x1C;
    }

    public class RequestPacket : Packet
    {
        public RequestType Command { get; private set; }

        public RequestPacket(RequestType command)
        {
            Command = command;
        }
    }

    public class ResponsePacket : Packet
    {
    }

    public class TestConnectionResponse : ResponsePacket
    {
        public byte Status { get; private set; }
        public static TestConnectionResponse TryParse(DeviceBuffer buffer, out int length)
        {
            length = 0;
            const int responseLength = 2;
            const byte expectedStatus = 0x79;
            if (buffer.Length < responseLength || buffer.Data[0] != Esc || buffer.Data[1] != expectedStatus)
                return null;
            length = responseLength;
            return new TestConnectionResponse()
            {
                Status = buffer.Data[1]
            };
        }
    }

    public class WaitCardRollResponse : ResponsePacket
    {
        private static byte[] waitCardRollSuffix = { 0x3F, Fs, Esc };
        private static byte[] track1Prefix = { 0x1B, 01 };
        private static byte[] track2Prefix = { 0x1B, 02 };
        private static byte[] track3Prefix = { 0x1B, 03 };
        public byte[] Track1 { get; private set; }
        public byte[] Track2 { get; private set; }
        public byte[] Track3 { get; private set; }
        public byte Status { get; private set; }

        public static WaitCardRollResponse TryParse(DeviceBuffer buffer, out int length)
        {
            const int minLength = 12;
            length = 0;

            if (buffer.Length < minLength)
                return null;

            var suffixPos = Utils.FindFirst(buffer.Data, waitCardRollSuffix);
            if (suffixPos < 0)
                return null;
            if (buffer.Length < suffixPos + waitCardRollSuffix.Length + 1)
                return null;
            var track1Pos = Utils.FindFirst(buffer.Data, track1Prefix);
            if (track1Pos < 0)
                return null;
            var track2Pos = Utils.FindFirst(buffer.Data, track2Prefix, track1Pos);
            if (track2Pos < 0)
                return null;
            var track3Pos = Utils.FindFirst(buffer.Data, track3Prefix, track2Pos);
            if (track3Pos < 0)
                return null;

            length = suffixPos + waitCardRollSuffix.Length + 1;
            return new WaitCardRollResponse
            {
                Track1 = buffer.Data
                    .Skip(track1Pos + track1Prefix.Length)
                    .Take(track2Pos - track1Pos - track1Prefix.Length)
                    .ToArray(),

                Track2 = buffer.Data
                    .Skip(track2Pos + track2Prefix.Length)
                    .Take(track3Pos - track2Pos - track2Prefix.Length)
                    .ToArray(),

                Track3 = buffer.Data
                    .Skip(track3Pos + track3Prefix.Length)
                    .Take(suffixPos - track3Pos - track3Prefix.Length)
                    .ToArray(),

                Status = buffer.Data[suffixPos + waitCardRollSuffix.Length]
            };
        }
    }

    public class DeviceModelResponse : ResponsePacket
    {
        public int Model { get; private set; }
        public static DeviceModelResponse TryParse(DeviceBuffer buffer, out int length)
        {
            length = 0;
            const byte successStatus = 0x53;
            const int responseLength = 3;
            if (buffer.Length < responseLength || buffer.Data[0] != Esc || buffer.Data[2] != successStatus)
                return null;
            length = responseLength;
            return new DeviceModelResponse()
            {
                Model = buffer.Data[1] - '0'
            };
        }
    }
    public class DeviceFirmwareResponse : ResponsePacket
    {
        public string Revision { get; private set; }
        public string VersionHi { get; private set; }
        public string VersionLo { get; private set; }
        public static DeviceFirmwareResponse TryParse(DeviceBuffer buffer, out int length)
        {
            length = 0;
            const int responseLength = 9;
            if (buffer.Length < responseLength || buffer.Data[0] != Esc || buffer.Data[1] != 'R' || buffer.Data[2] != 'E' || buffer.Data[3] != 'V' || buffer.Data[6] != '.')
                return null;
            var encoding = Encoding.ASCII;
            length = responseLength;
            return new DeviceFirmwareResponse()
            {
                Revision = encoding.GetString(new byte[] { buffer.Data[4] }),
                VersionHi = encoding.GetString(new byte[] { buffer.Data[5] }),
                VersionLo = encoding.GetString(new byte[] { buffer.Data[7], buffer.Data[8] })
            };
        }
    }
}
