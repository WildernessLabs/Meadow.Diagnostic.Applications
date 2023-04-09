using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;

namespace I2CScanner
{
    public class MeadowApp : App<F7FeatherV2>
    {
        public override Task Run()
        {
            Console.WriteLine("Beginning I2C Validation.");
            var scanner = new I2CScanner(Device);
            scanner.VerifyAndScan();
            return base.Run();
        }

        public override Task Initialize()
        {
            return base.Initialize();
        }
    }
}
