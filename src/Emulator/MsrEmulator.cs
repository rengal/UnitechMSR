using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using com.iiko.unitech;
using com.iiko.unitech.Protocol;

namespace Emulator
{
    public class MsrEmulator : IDisposable
    {
        private enum CardRollMode
        {
            Idle,
            ReadRaw,
            Read
        }

        private readonly object gate = new object();
        private SerialPort port;
        private byte[] buffer = { };
        private int bufferLength;
        private readonly TimeSpan bufferTimeout = TimeSpan.FromMilliseconds(500);
        private CancellationTokenSource clearBufferCts;
        private CardRollMode cardRollMode;
        private bool emulateCardRoll;


        private static readonly byte[] TestConnectionResponse = { 0x1b, 0x79 };

        private static readonly byte[] CardRollResponseRaw =
        {
            0x1b, 0x73, 0x1b, 0x01, 0x00, 0x1b, 0x02, 0x1d, 0xd7, 0x38, 0x2b, 0x23, 0x21, 0xa8, 0x42, 0x18,
            0x59, 0x08, 0x0d, 0x42, 0x10, 0x85, 0x1f, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x1b, 0x03, 0x00, 0x3f, 0x1c, 0x1b, 0x30
        };

        private static readonly byte[] CardRollResponse =
        {
            0x1b, 0x73, 0x1b, 0x01, 0x1b, 0x2b, 0x1b, 0x02, 0x3b, 0x37, 0x37, 0x38, 0x3d, 0x32, 0x33, 0x30,
            0x35, 0x30, 0x30, 0x30, 0x31, 0x3d, 0x32, 0x32, 0x30, 0x35, 0x30, 0x30, 0x30, 0x30, 0x33, 0x3f,
            0x1b, 0x03, 0x1b, 0x2b, 0x3f, 0x1c, 0x1b, 0x30
        };

        private static readonly byte[] ModelResponse =
        {
            0x1B, 0x33, 0x53
        };

        private static readonly byte[] FirmwareResponse =
        {
            0x1B, 0x52, 0x45, 0x56, 0x55, 0x31, 0x2E, 0x30, 0x32
        };

        public void Start(string portName)
        {
            lock (gate)
            {
                port = new SerialPort(portName, 9600);
                port.Open();
                port.DataReceived += Port_DataReceived;
            }
        }

        public void EmulateCardRoll()
        {
            lock (gate)
            {
                emulateCardRoll = true;
                CheckCardRollResponse();
            }
        }

        private void CheckCardRollResponse()
        {
            if (!emulateCardRoll || cardRollMode == CardRollMode.Idle)
                return;
            port.Write(CardRollResponse, 0, CardRollResponse.Length);
            Console.WriteLine($"-> {Utils.ByteArrayToHexString(CardRollResponse, 0, CardRollResponse.Length)}");
            cardRollMode = CardRollMode.Idle;
            emulateCardRoll = false;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (gate)
            {
                var serialPort = (SerialPort)sender;
                var bytesToRead = serialPort.BytesToRead;
                if (buffer.Length < bufferLength + bytesToRead)
                    Array.Resize(ref buffer, bufferLength + bytesToRead);
                bufferLength += serialPort.Read(buffer, 0, bytesToRead);
                if (ProcessBuffer())
                    ClearBuffer();
                else
                    ScheduleClearBuffer(bufferTimeout);
            }
        }

        private void ScheduleClearBuffer(TimeSpan interval)
        {
            lock (gate)
            {
                clearBufferCts?.Cancel();
                clearBufferCts = new CancellationTokenSource();
                Task.Delay(interval).ContinueWith(_ =>
                {
                    lock (gate)
                    {
                        if (clearBufferCts.IsCancellationRequested)
                            return;
                        ClearBuffer();
                    }
                }, clearBufferCts.Token);
            }
        }

        private void ClearBuffer()
        {
            lock (gate)
            {
                clearBufferCts?.Cancel();
                bufferLength = 0;
            }
        }

        private bool ProcessBuffer()
        {
            lock (gate)
            {
                Console.WriteLine($"<-{Utils.ByteArrayToHexString(buffer, 0, bufferLength)}");
                if (bufferLength < 2)
                    return false;
                if (buffer[0] != Packet.Esc)
                    return false;
                if (buffer[1] == (byte)RequestType.Reset)
                {
                    Console.WriteLine("<- Reset");
                    cardRollMode = CardRollMode.Idle;
                }
                else if (buffer[1] == (byte)RequestType.TestConnection)
                {
                    Console.WriteLine("<- TestConnection");
                    port.Write(TestConnectionResponse, 0, TestConnectionResponse.Length);
                    Console.WriteLine($"-> {Utils.ByteArrayToHexString(TestConnectionResponse, 0, TestConnectionResponse.Length)}");
                }
                else if (buffer[1] == (byte)RequestType.StartRead)
                {
                    Console.WriteLine("<- StartRead");
                    cardRollMode = CardRollMode.Read;
                    CheckCardRollResponse();
                }
                else if (buffer[1] == (byte)RequestType.GetDeviceModel)
                {
                    Console.WriteLine("<- GetDeviceModel");
                    port.Write(ModelResponse, 0, ModelResponse.Length);
                    Console.WriteLine($"-> {Utils.ByteArrayToHexString(ModelResponse, 0, ModelResponse.Length)}");
                }
                else if (buffer[1] == (byte)RequestType.GetFirmware)
                {
                    Console.WriteLine("<- GetFirmware");
                    port.Write(FirmwareResponse, 0, FirmwareResponse.Length);
                    Console.WriteLine($"-> {Utils.ByteArrayToHexString(FirmwareResponse, 0, FirmwareResponse.Length)}");
                }
                else
                    return false;

                return true;
            }
        }

        public void Dispose()
        {
            lock (gate)
            {
                port?.Dispose();
            }
        }
    }
}