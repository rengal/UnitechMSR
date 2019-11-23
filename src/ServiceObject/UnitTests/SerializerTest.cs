using System.Linq;
using com.iiko.unitech;
using com.iiko.unitech.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace com.iiko.unitech.UnitTests
{
    public class Serializer
    {
        [Test]
        public void TestCardRoll()
        {
            byte[] source =
            {
                0x1b, 0x73, 0x1b, 0x01, 0x1b, 0x2b, 0x1b, 0x02, 0x3b, 0x37, 0x37, 0x38, 0x3d, 0x32, 0x33, 0x30,
                0x35, 0x30, 0x30, 0x30, 0x31, 0x3d, 0x32, 0x32, 0x30, 0x35, 0x30, 0x30, 0x30, 0x30, 0x33, 0x3f,
                0x1b, 0x03, 0x1b, 0x2b, 0x3f, 0x1c, 0x1b, 0x30
            };
            var buffer = new DeviceBuffer();
            buffer.Append(source);
            var cardRollData =
                Protocol.Serializer.DeserializeNext(RequestType.StartRead, buffer) as WaitCardRollResponse;
            Assert.NotNull(cardRollData);
        }

        [Test]
        public void TestCardRollIncomplete()
        {

            byte[] source =
            {
                0x1b, 0x73, 0x1b, 0x01, 0x1b, 0x2b, 0x1b, 0x02, 0x3b, 0x37, 0x37, 0x38, 0x3d, 0x32, 0x33, 0x30,
                0x35, 0x30, 0x30, 0x30, 0x31, 0x3d, 0x32, 0x32, 0x30, 0x35, 0x30, 0x30, 0x30, 0x30, 0x33, 0x3f,
                0x1b, 0x03, 0x1b, 0x2b, 0x3f, 0x1c, 0x1b, 0x30
            };
            for (int count = 0; count <= source.Length; count++)
            {
                var buffer = new DeviceBuffer();
                buffer.Append(source.Take(count).ToArray());
                var cardRollData =
                    Protocol.Serializer.DeserializeNext(RequestType.StartRead, buffer) as WaitCardRollResponse;
                if (count < source.Length)
                    Assert.Null(cardRollData);
                else
                    Assert.NotNull(cardRollData);
            }
        }

        [Test]
        public void TestCombined()
        {
            byte[] source =
            {
                0x1b, 0x73, 0x1b, 0x01, 0x1b, 0x2b, 0x1b, 0x02, 0x3b, 0x37, 0x37, 0x38, 0x3d, 0x32, 0x33, 0x30,
                0x35, 0x30, 0x30, 0x30, 0x31, 0x3d, 0x32, 0x32, 0x30, 0x35, 0x30, 0x30, 0x30, 0x30, 0x33, 0x3f,
                0x1b, 0x03, 0x1b, 0x2b, 0x3f, 0x1c, 0x1b, 0x30,
                0x1b, 0x52, 0x45, 0x56, 0x55, 0x31, 0x2e, 0x30, 0x32
            };
            var buffer = new DeviceBuffer();
            buffer.Append(source);
            var cardRollData =
                Protocol.Serializer.DeserializeNext(RequestType.StartRead, buffer) as WaitCardRollResponse;
            Assert.NotNull(cardRollData);
            var firmwareInfo =
                Protocol.Serializer.DeserializeNext(RequestType.GetFirmware, buffer) as DeviceFirmwareResponse;
            Assert.NotNull(firmwareInfo);
            Assert.AreEqual("U", firmwareInfo.Revision);
            Assert.AreEqual("1", firmwareInfo.VersionHi);
            Assert.AreEqual("02", firmwareInfo.VersionLo);
        }
    }
}
