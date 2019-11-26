using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using com.iiko.unitech.Protocol;
using log4net;
using Microsoft.PointOfService;

namespace com.iiko.unitech
{
    public partial class MsrDriver
    {
        private static readonly ILog Log = LogFactory.Instance.GetLogger(typeof(MsrDriver));

        private bool enabled;
        private SerialPort port;
        private readonly object gate = new object();
        private readonly string portName;
        private readonly TimeSpan defaultTimeout = TimeSpan.FromMilliseconds(5000);
        private readonly TimeSpan pollInterval = TimeSpan.FromSeconds(5);
        private readonly TimeSpan bufferIdleTimeout = TimeSpan.FromSeconds(1);

        public event EventHandler<byte[]> OnCardRolled;

        //lock-protected fields
        private MsrCommandDto currentCommandDto;
        private readonly DeviceBuffer receiveBuffer = new DeviceBuffer();
        private CancellationTokenSource clearBufferCts;
        private CancellationTokenSource pollCts;

        public MsrDriver(string portName)
        {
            this.portName = portName;
        }

        private void WriteSerial(byte[] data)
        {
            lock (gate)
            {
                try
                {
                    Log.Debug($"-> {Utils.ByteArrayToHexString(data, 0, data.Length)} ({data.Length} bytes)");
                    port.Write(data, 0, data.Length);
                }
                catch (Exception e)
                {
                    if (IsSerialIoException(e))
                        throw new PosControlException(e.Message, ErrorCode.Failure);
                    throw;
                }
            }
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            lock (gate)
            {
                receiveBuffer.AppendExistingData(serialPort);
                ScheduleClearBuffer(bufferIdleTimeout);
            }

            ProcessDataReceivedAsync();
        }

        private void ScheduleClearBuffer(TimeSpan interval)
        {
            lock (gate)
            {
                clearBufferCts?.Cancel();
                clearBufferCts = new CancellationTokenSource();
                Task.Delay(interval, clearBufferCts.Token).ContinueWith(_=>
                {
                    lock (gate)
                    {
                        if (clearBufferCts.IsCancellationRequested)
                            return;
                        receiveBuffer.Clear();
                    }
                }, TaskContinuationOptions.NotOnCanceled);
            }
        }

        private void ExecCommand(RequestType requestType)
        {
            var requestPacket = new RequestPacket(requestType);
            WriteSerial(Serializer.Serialize(requestPacket));
        }

        /// <summary>
        /// Выполнить команду и получить ответ
        /// </summary>
        /// <param name="requestType">Тип запроса</param>
        /// <param name="timeout">таймаут ожидания ответа</param>
        /// <returns>Ответ от устройства</returns>
        private TResult ExecCommand<TResult>(RequestType requestType, TimeSpan timeout)
            where TResult : ResponsePacket
        {
            var requestPacket = new RequestPacket(requestType);
            ManualResetEvent completedEvent;
            lock (gate)
            {
                clearBufferCts?.Cancel();
                currentCommandDto = new MsrCommandDto(requestPacket);
                completedEvent = currentCommandDto.completedEvent;
            }

            WaitHandle[] waitHandles = {completedEvent};
            WriteSerial(Serializer.Serialize(currentCommandDto.request));
            var waitIndex = WaitHandle.WaitAny(waitHandles, timeout);
            lock (gate)
            {
                switch (waitIndex)
                {
                    case 0: //completed
                        var result = currentCommandDto.response as TResult;
                        currentCommandDto = null;
                        return result;
                    case WaitHandle.WaitTimeout: //timeout
                    {
                        currentCommandDto = null;
                        Log.Error("Command timed out");
                        if (receiveBuffer.Length > 0)
                            throw new PosControlException("Command failed", ErrorCode.Failure);
                        throw new PosControlException("Timeout", ErrorCode.Timeout);
                    }
                }
            }

            return null;
        }

        private void Reset()
        {
            Log.Info("Reset");
            ExecCommand(RequestType.Reset);
        }

        public TestConnectionResponse TestConnection()
        {
            Log.Info("TestConnection");
            return ExecCommand<TestConnectionResponse>(RequestType.TestConnection, defaultTimeout);
        }

        private void StartRead()
        {
            Log.Info("StartRead");
            ExecCommand(RequestType.StartRead);
        }

        private DeviceModelResponse GetDeviceModel()
        {
            Log.Info("GetDeviceModel");
            return ExecCommand<DeviceModelResponse>(RequestType.GetDeviceModel, defaultTimeout);
        }

        private DeviceFirmwareResponse GetFirmware()
        {
            Log.Info("GetFirmware");
            return ExecCommand<DeviceFirmwareResponse>(RequestType.GetFirmware, defaultTimeout);
        }

        public bool DeviceEnabled
        {
            get => enabled;
            set => SetDeviceEnabled(value);
        }

        private void SetDeviceEnabled(bool value)
        {
            if (enabled == value)
                return;
            if (value)
            {
                try
                {
                    OpenPort();
                    ReadProperties();
                }
                catch (PosControlException)
                {
                    ClosePort();
                    throw;
                }

                enabled = true;
            }
            else
            {
                enabled = false;
                ClosePort();
            }
            SchedulePoll();
        }

        private void SchedulePoll()
        {
            lock (gate)
            {
                pollCts?.Cancel();
                if (!enabled)
                    return;
                StartRead();
                pollCts = new CancellationTokenSource();
                var token = pollCts.Token;
                Task.Delay(pollInterval, pollCts.Token).ContinueWith(_ =>
                {
                    lock (gate)
                    {
                        if (token.IsCancellationRequested)
                            return;
                        SchedulePoll();
                    }
                }, TaskContinuationOptions.NotOnCanceled);
            }
        }

        private void OpenPort()
        {
            lock (gate)
            {
                try
                {
                    port = new SerialPort(portName, 9600);
                    port.Open();
                    port.DataReceived += SerialPortDataReceived;
                    Log.Info($"Port {portName} opened");
                }
                catch (Exception e)
                {
                    if (IsSerialIoException(e))
                    {
                        port?.Close();
                        port?.Dispose();
                        port = null;
                        Log.Error($"Failed to open port {portName}: {e.Message}");
                        throw new PosControlException($"Failed to open port {portName}: {e.Message}",
                            ErrorCode.Failure);
                    }

                    throw;
                }
            }
        }

        private void ClosePort()
        {
            lock (gate)
            {
                try
                {
                    if (port == null)
                        return;
                    port.DataReceived -= SerialPortDataReceived;
                    port?.Close();
                    port?.Dispose();
                    Log.Info($"Port {portName} closed");
                }
                catch (Exception e)
                {
                    if (IsSerialIoException(e))
                    {
                        port?.Close();
                        port?.Dispose();
                        port = null;
                        Log.Error($"Failed to close port {portName}: {e.Message}");
                        throw new PosControlException($"Failed to close port {portName}: {e.Message}",
                            ErrorCode.Failure);
                    }
                }
            }
        }

        private void ReadProperties()
        {
            TestConnection();
            var modelInfo = GetDeviceModel();
            if (modelInfo != null)
                Log.Info($"Device model: {modelInfo.Model}");
            var firmwareInfo = GetFirmware();
            if (firmwareInfo != null)
                Log.Info($"Device firmware: revision={firmwareInfo.Revision}, " +
                         $"version={firmwareInfo.VersionHi}.{firmwareInfo.VersionLo}");
        }

        #region Private Methods (Helpers)

        public static bool IsSerialIoException(Exception e)
        {
            return e is InvalidOperationException || e is ArgumentException || e is IOException ||
                   e is UnauthorizedAccessException;
        }

        #endregion
    }
}