using System;
using System.Collections.Generic;
using System.Linq;
using Meadow.Devices;
using Meadow.Hardware;

namespace I2CScanner
{
    public class I2CScanner
    {
        private readonly F7Micro _device;
        private readonly IReadOnlyList<I2cBusSpeed> _speeds;

        /// <summary>
        /// Create a <see cref="I2CScanner"/> that will scan all <seealso cref="I2cBusSpeed"/>
        /// </summary>
        /// <param name="device">The <see cref="F7Micro"/> on which the application is running</param>
        public I2CScanner(F7Micro device)
        {
            _device = device;
            _speeds = new[] {I2cBusSpeed.Standard, I2cBusSpeed.Fast, I2cBusSpeed.FastPlus};
        }

        /// <summary>
        /// Create a <see cref="I2CScanner"/> that will scan the provided <see cref="IReadOnlyList{T}"/> of <see cref="I2cBusSpeed"/>
        /// </summary>
        /// <param name="device">The <see cref="F7Micro"/> on which the application is running</param>
        /// <param name="speeds">The <see cref="IReadOnlyList{T}"/> of <see cref="I2cBusSpeed"/> to scan</param>
        public I2CScanner(F7Micro device, IReadOnlyList<I2cBusSpeed> speeds)
        {
            _device = device;
            _speeds = speeds;
        }

        /// <summary>
        /// Run all verification tests
        /// </summary>
        public void VerifyAndScan()
        {
            if (!VerifyPins()) return;
            var results = ScanBusForDevices();
            foreach (var (speed, addresses) in results)
            {
                Console.WriteLine($"Found {addresses.Count} devices @ {(int)speed/1000}kHz: {string.Join(", ", addresses.Select(x => $"{x:X}"))}");
            }
        }

        /// <summary>
        /// Verify that the I2C Pins are properly pulled up
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating if the check succeeded or failed.</returns>
        public bool VerifyPins()
        {
            Console.WriteLine("Validating SCL and SDA pins");
            using (var scl = _device.CreateDigitalInputPort(_device.Pins.I2C_SCL, resistorMode: ResistorMode.PullDown))
            {
                if (scl.State == false)
                {
                    Console.WriteLine("SCL does not appear to have pull-up resistor.");
                    return false;
                }
            }
            using (var sda = _device.CreateDigitalInputPort(_device.Pins.I2C_SDA, resistorMode: ResistorMode.PullDown))
            {
                if (sda.State == false)
                {
                    Console.WriteLine("SDA does not appear to have pull-up resistor.");
                    return false;
                }
            }
            Console.WriteLine("SDA and SCL Validated Successfully.");
            return true;
        }

        /// <summary>
        /// Scan the I2C Bus for devices, once for each <seealso cref="I2cBusSpeed"/> indicated in the constructor
        /// </summary>
        /// <returns>A dictionary keyed on the speed of the bus during the scan containing a
        /// list of each address that responded on the bus.</returns>
        public IReadOnlyDictionary<I2cBusSpeed, IReadOnlyList<byte>> ScanBusForDevices()
        {
            var results = new Dictionary<I2cBusSpeed, IReadOnlyList<byte>>();
            foreach (var speed in _speeds)
            {
                try
                {
                    Console.WriteLine($"Scanning I2C Bus @ {(int)speed / 1000}kHz...");
                    var bus = _device.CreateI2cBus(speed);
                    results.Add(speed, ScanBusForDevices(bus));
                    Console.WriteLine("Scanning I2C Bus complete.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred while scanning I2C bus @ {speed}: {ex}");
                }
            }

            return results;
        }

        /// <summary>
        /// Scan the I2C Bus for devices
        /// </summary>
        /// <param name="bus">The <see cref="II2cBus"/> to scan.</param>
        /// <returns>A <see cref="IReadOnlyList{@byte}"/> of addresses responding on the bus.</returns>
        public IReadOnlyList<byte> ScanBusForDevices(II2cBus bus)
        {
            var validAddresses = new List<byte>(128);
            for (byte address = 0; address < 127; address++)
            {
                if (IsReservedAddress(address))
                    continue;
                try
                {
                    bus.ReadData(address, 1);
                    validAddresses.Add(address);
                }
                catch
                {
                    // The error isn't really important here as a missing device 
                    // looks exactly like bad wiring.
                }
            }

            return validAddresses;
        }

        private static bool IsReservedAddress(byte address)
        {
            if (address >= 0x00 && address <= 0x07)
                return true;
            if (address >= 0x78 && address <= 0x7F)
                return true;
            return false;
        }

    }
}
