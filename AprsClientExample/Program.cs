using AprsRepeaterMonitor;
using Skyhop.Aprs.Client;
using Skyhop.Aprs.Client.Models;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AprsClientExample
{
    class Program
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary> 
        /// <param name="args">The command-line arguments.</param>
        static async Task Main(string[] args)
        {
            Settings settings = new Settings(true);
            settings.Save();

            Repeaters repeaters = new Repeaters(settings.OutputFile);

            string Title = "AA5JC APRS Telemetry Monitor";
            Console.Title = Title;
            Config config = new Config();
            config.Callsign = settings.Callsign;
            config.Password = settings.Password;
            config.Uri = settings.ServerAddress;
            config.UseOgnAdditives = false;
            config.Port = settings.ServerPort;
            config.SoftwareName = Title;
            config.SoftwareVersion = "1.1_05";
            config.Filter = settings.Filter;

            Listener listener = new Listener(config);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(String.Format(@"{7}
    Loading settings.json ...
        Server: {0}:{1}
        Filter: {2}
        Regex for telemetry: {3}
        Login callsign: {4}
        Login password: {5}
        Output file: {6}

    * Press CTRL-C to quit *


", 
    settings.ServerAddress, settings.ServerPort, settings.Filter, settings.RegExForParsingTelemetryData, settings.Callsign, settings.Password, settings.OutputFile, Title
    ));
            Console.ResetColor();

            listener.Connected += (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" -- Connected");
                Console.ResetColor();
            };

            listener.Disconnected += (sender, e) => {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" -- Disconnected");
                Console.ResetColor();
            };

            listener.DataReceived += (sender, e) => {
                AprsMessage message = null;

                try {
                    message = PacketInfo.Parse(e.Data);
                } catch {
                    
                }

                if (message == null) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Data);
                    Console.ResetColor();
                } else {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(e.Data);

                    Regex regex = new Regex(settings.RegExForParsingTelemetryData);
                    if (regex.IsMatch(e.Data))
                    {
                        Match m = regex.Match(e.Data);
                        Repeater repeater = new Repeater(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value, m.Groups[5].Value, m.Groups[6].Value, m.Groups[7].Value, m.Groups[8].Value);
                        repeaters.Add(repeater);
                        repeaters.Save();
                    }

                    Console.ResetColor();
                }
            };

            //listener.PacketReceived += (sender, eventArgs) =>
            //{
            //    // Please note it is faster to use the `listener.DataReceived` event if you only want the raw data.
            //    Console.WriteLine(eventArgs.AprsMessage.RawData);
            //    w.Write($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} {eventArgs.AprsMessage.RawData}");
            //};

            bool interruptReceived = false;

            Console.CancelKeyPress += delegate {
                Console.WriteLine("** Interrupt received **");
                interruptReceived = true;
                System.Environment.Exit(0);
            };

            await listener.Open();

            while (Console.ReadKey().KeyChar != '^' && !interruptReceived)
            {
                // Just keep swimming, just keep swimming..
            }

            listener.Stop();
        }
    }
}
