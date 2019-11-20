using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Emulator
{
    public class MsrEmulator : IDisposable
    {
        private readonly object gate = new object();
        private SerialPort port;
        private byte[] buffer;
        private int bufferLength;
        private readonly TimeSpan bufferTimeout = TimeSpan.FromMilliseconds(500);
        private CancellationTokenSource clearBufferCts;
        private bool trackRequested;
        private bool emulateCardRoll;

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
            if (emulateCardRoll && trackRequested)
            {
                port.Write(CardRollResponse, 0, CardRollResponse.Length);
                emulateCardRoll = false;
                trackRequested = false;
            }
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
                clearBufferCts?.Cancel();
                clearBufferCts = new CancellationTokenSource();
                Task.Delay(bufferTimeout).ContinueWith(_ =>
                {
                    lock (gate)
                    {
                        if (clearBufferCts.IsCancellationRequested)
                            return;
                        ClearBuffer();
                    }
                }, clearBufferCts.Token);
                ProcessBuffer();
            }
        }

        private void ClearBuffer()
        {
            lock (gate)
            {
                bufferLength = 0;
            }
        }

        private void ProcessBuffer()
        {
            lock (gate)
            {
                if (bufferLength == 2)
                {
                    if (buffer[0] == 0x1B && buffer[1] == 0x74)
                        port.Write(ModelResponse, 0, ModelResponse.Length);
                    else if (buffer[0] == 0x1B && buffer[1] == 0x72)
                    {
                        trackRequested = true;
                        CheckCardRollResponse();
                    }
                    ClearBuffer();
                }
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
