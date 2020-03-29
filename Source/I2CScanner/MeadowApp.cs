using System;
using Meadow;
using Meadow.Devices;

namespace I2CScanner
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public MeadowApp()
        {
            Console.WriteLine("Beginning I2C Validation.");
            var scanner = new I2CScanner(Device);
            scanner.VerifyAndScan();
        }
    }
}
