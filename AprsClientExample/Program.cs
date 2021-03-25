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

            string Title = "AA5JC APRS Telemetry Monitor";
            Console.Title = Title;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Title + "\r\n");
            
            if (string.IsNullOrEmpty(settings.Callsign))
            {
                Console.WriteLine("Settings file not found. Creating new settings.json. Please answer the following questions to set up:");
                Console.WriteLine("\r\n\r\nYou need to login on the server with a callsign. Typically this would be your own callsign with a hyphen and number added to the end (ie AA1BB-1).");
                Console.Write("What callsign do you want to use to login to the APRS server? ");
                Console.ForegroundColor = ConsoleColor.White;
                settings.Callsign = Console.ReadLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\r\nNow you will need to enter the APRS passcode.. or not. Here's the thing, since this application will only read data (doesn't write anything to APRS) you don't really need to provide a passcode. If you want to generate your password, visit http://bit.ly/aprspasscode. If you want to login without a passcode, hit enter and we'll send \"-1\" as your passcode.");
                Console.Write("What is your passcode [-1]? ");
                Console.ForegroundColor = ConsoleColor.White;
                settings.Password = Console.ReadLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\r\nYou can find a list of APRS servers at http://aprs2.net.");
                Console.Write("To which APRS server do you want to connect [noam.aprs2.net]? ");
                Console.ForegroundColor = ConsoleColor.White;
                settings.ServerAddress = Console.ReadLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\r\nThe default client-defined filter port is 14580.");
                Console.Write("To what port do you want to connect [14580]? ");
                Console.ForegroundColor = ConsoleColor.White;
                string strPort = Console.ReadLine();
                if (!int.TryParse(strPort, out settings.ServerPort)) {
                    settings.ServerPort = 14580;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\r\nAPRS filters enable you to only receive messages that match a predetermined set of parameters. You can read all about them at aprs-is.net. When I want the telemetry for the W5AUU repeaters, I use a filter like this:");
                Console.WriteLine("\tb/W5AUU-* -t/poimqsunw");
                Console.Write("What APRS filter do you want to use? ");
                Console.ForegroundColor = ConsoleColor.White;
                settings.Filter = Console.ReadLine();

                settings.RegExForParsingTelemetryData = "([a-zA-Z0-9]{1,3}[0123456789][a-zA-Z0-9]{0,3}[a-zA-Z].*)>APTT4,.*:T\\#(...),(...),(...),(...),(...),(...),(........)";
                settings.OutputFile = "repeaterTelemetry.json";
                settings.Save();
            }

            Repeaters repeaters = new Repeaters(settings.OutputFile);

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
            Console.Write(String.Format(@"
    Server: {0}:{1}
    Filter: {2}
    Login callsign: {4}
    Login password: {5}
    Output file: {6}

    * Press CTRL-C to quit *


", 
    settings.ServerAddress, settings.ServerPort, settings.Filter, settings.RegExForParsingTelemetryData, settings.Callsign, settings.Password, settings.OutputFile));
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
