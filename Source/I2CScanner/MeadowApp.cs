using Meadow;
using Meadow.Devices;
using System;

namespace I2CScanner
{
    public class MeadowApp : App<F7FeatherV2>
    {
        public MeadowApp()
        {
            Console.WriteLine("Beginning I2C Validation.");
            var scanner = new I2CScanner(Device);
            scanner.VerifyAndScan();
        }
    }
}
