using System;
using com.iiko.unitech.Resourses;
using Microsoft.PointOfService;

[assembly: PosAssembly("iiko")]

namespace com.iiko.unitech
{
    [ServiceObject(DeviceType.Scanner, "Unitech", "POS .Net Driver for Unitech MSR", 1, 14)]
    public class MsrServiceObject : Scanner
    {
        public override string CheckHealth(HealthCheckLevel level)
        {
            return LocalResources.Success;
        }

        public override void Claim(int timeout)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override void Open()
        {
            throw new NotImplementedException();
        }

        public override DirectIOData DirectIO(int command, int data, object obj)
        {
            throw new NotImplementedException();
        }

        public override void Release()
        {
            throw new NotImplementedException();
        }

        public override void ResetStatistics()
        {
            throw new NotImplementedException();
        }

        public override void ResetStatistics(StatisticCategories statistics)
        {
            throw new NotImplementedException();
        }

        public override void ResetStatistics(string[] statistics)
        {
            throw new NotImplementedException();
        }

        public override void ResetStatistic(string statistic)
        {
            throw new NotImplementedException();
        }

        public override string RetrieveStatistics()
        {
            throw new NotImplementedException();
        }

        public override string RetrieveStatistics(StatisticCategories statistics)
        {
            throw new NotImplementedException();
        }

        public override string RetrieveStatistics(string[] statistics)
        {
            throw new NotImplementedException();
        }

        public override string RetrieveStatistic(string statistic)
        {
            throw new NotImplementedException();
        }

        public override void UpdateStatistic(string name, object value)
        {
            throw new NotImplementedException();
        }

        public override void UpdateStatistics(Statistic[] statistics)
        {
            throw new NotImplementedException();
        }

        public override void UpdateStatistics(StatisticCategories statistics, object value)
        {
            throw new NotImplementedException();
        }

        public override PowerReporting CapPowerReporting => PowerReporting.None;

        public override bool CapStatisticsReporting => false;
        public override bool CapUpdateStatistics => false;
        public override string CheckHealthText => "";
        public override bool Claimed => false;
        public override string DeviceDescription => LocalResources.DeviceDescription;
        public override bool DeviceEnabled { get; set; }
        public override string DeviceName { get; }
        public override bool FreezeEvents { get; set; }
        public override PowerNotification PowerNotify { get; set; }
        public override PowerState PowerState { get; }
        public override string ServiceObjectDescription { get; }
        public override ControlState State { get; }
        public override event DirectIOEventHandler DirectIOEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        public override void ClearInput()
        {
            throw new NotImplementedException();
        }

        public override int DataCount { get; }
        public override bool DataEventEnabled { get; set; }
        public override bool AutoDisable { get; set; }
        public override bool DecodeData { get; set; }
        public override byte[] ScanData { get; }
        public override byte[] ScanDataLabel { get; }
        public override BarCodeSymbology ScanDataType { get; }
        public override event DataEventHandler DataEvent;
        public override event DeviceErrorEventHandler ErrorEvent;
    }
   
}