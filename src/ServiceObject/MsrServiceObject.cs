using System;
using log4net;
using Microsoft.PointOfService;
using Microsoft.PointOfService.BaseServiceObjects;

[assembly: PosAssembly("iiko")]

namespace com.iiko.unitech
{
    [ServiceObject(DeviceType.Scanner, "Unitech", "POS .Net Driver for Unitech MSR", 1, 14)]
    public class MsrServiceObject : ScannerBase
    {
        private static readonly ILog Log = LogFactory.Instance.GetLogger(typeof(MsrServiceObject));

        private string checkHealthText = "";
        private MsrDriver driver;

        public override string CheckHealth(HealthCheckLevel level)
        {
            Log.Info("Check Health");
            VerifyState(true, true);
            try
            {
                var testConnectionResult = driver.TestConnection();
                checkHealthText = $"Completed with status: 0x{testConnectionResult.Status:X02}";
            }
            catch (PosControlException e)
            {
                checkHealthText = e.Message;
                throw;
            }

            return checkHealthText;
        }

        public override DirectIOData DirectIO(int command, int data, object obj)
        {
            throw new NotImplementedException();
        }

        public override void Open()
        {
            Log.Info("Open");
            base.Open();
            checkHealthText = "Opened";
            Log.Info("Open - Ok");
        }

        public override void Close()
        {
            Log.Info("Close");
            base.Close();
            checkHealthText = "Closed";
            Log.Info("Close - Ok");
        }

        public override bool DeviceEnabled
        {
            // Device State checking done in base class
            get => base.DeviceEnabled;
            set
            {
                Log.Info($"Set_DeviceEnabled({value})");
                if (value == base.DeviceEnabled)
                    return;

                base.DeviceEnabled = value;
                try
                {
                    if (value)
                    {
                        driver = new MsrDriver(DevicePath);
                    }
                    driver.DeviceEnabled = value;

                    if(value)
                        driver.OnCardRolled += driver_OnCardRolled;
                    else
                        driver.OnCardRolled -= driver_OnCardRolled;
                    checkHealthText = value ? "Enabled" : "Disabled";
                    Log.Info($"Set_DeviceEnabled({value}) - Ok");
                }
                catch (PosControlException e)
                {
                    Log.Error($"Failed with error: {e.Message}");
                    base.DeviceEnabled = false;
                    checkHealthText = e.Message;
                    throw;
                }
            }
        }

        public override string CheckHealthText => checkHealthText;

        private void driver_OnCardRolled(object sender, byte[] scanData)
        {
            GoodScan(scanData);
        }
    }
}