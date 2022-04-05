using System;
using System.Net.Http;
using Meadow;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Gateway.WiFi;

namespace WiFiTest
{
    class Program
    {
        static WiFiTestApp app;

        public static async Task Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--exitOnDebug")
                return;

            // instantiate and run new meadow app
            app = new WiFiTestApp();
            await app.RunAsync()
                     .ConfigureAwait(false);

            Thread.Sleep(Timeout.Infinite);
        }
    }

    public class WiFiTestApp : App<F7Micro, WiFiTestApp>
    {
        private const string NetworkName = null;
        private const string NetworkPassword = null;

        public async Task RunAsync()
        {
            if (NetworkName == null || NetworkPassword == null)
                throw new Exception(
                    $"Please set the {NetworkName} and {NetworkPassword} before running");

            var rgbPwmLed = new RgbPwmLed(
                Device,
                Device.Pins.OnboardLedRed,
                Device.Pins.OnboardLedGreen,
                Device.Pins.OnboardLedBlue);


            try
            {
                rgbPwmLed.SetColor(Color.Blue);
                Console.WriteLine("Initialize WiFi adapter...");
                Device.WiFiAdapter.StartWiFiInterface();

                Console.WriteLine("Initialized WiFi adapter, scanning for networks.");
                foreach (var network in Device.WiFiAdapter.Scan())
                {
                    Console.WriteLine(
                        $"Found {network.Ssid}, signal strength: {network.SignalDbStrength}dB");
                }

                Console.WriteLine($"Connecting to {NetworkName}...");
                ConnectionStatus connectionStatus;
                while ((connectionStatus =
                            (await Device.WiFiAdapter.Connect(NetworkName, NetworkPassword)
                                         .ConfigureAwait(false)).ConnectionStatus)
                    != ConnectionStatus.Success)
                {
                    rgbPwmLed.SetColor(Color.Red);
                    Console.WriteLine($"WiFi connect failed with {connectionStatus}");
                    Thread.Sleep(1000);
                    rgbPwmLed.SetColor(Color.Yellow);
                }

                Console.WriteLine("Network connected...");
            }
            catch (Exception ex)
            {
                rgbPwmLed.SetColor(Color.Red);
                Console.WriteLine(ex);
            }

            while (true)
            {
                try
                {
                    rgbPwmLed.SetColor(Color.Blue);
                    Console.WriteLine("Getting the weather...");
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "curl");
                    var weather = await client.GetStringAsync("http://wttr.in/?format=%l%20%t%20%C")
                                              .ConfigureAwait(false);

                    Console.WriteLine(weather);
                    rgbPwmLed.SetColor(Color.Green);
                    await Task.Delay(5000)
                              .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    rgbPwmLed.SetColor(Color.Red);
                    Console.WriteLine(ex);
                }

                Console.WriteLine("Cycling...");
            }
        }
    }
}