using AprsRepeaterMonitor;
using Skyhop.Aprs.Client;
using Skyhop.Aprs.Client.Models;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Settings file not found. Creating new settings.json. Please answer the following questions to set up:");
                Console.ResetColor();

                settings.Callsign = Settings.Ask("Callsign");
                settings.Password = Settings.Ask("Password");
                settings.ServerAddress = Settings.Ask("ServerAddress");

                string strPort = Settings.Ask("ServerPort");
                if (!int.TryParse(strPort, out settings.ServerPort)) {
                    settings.ServerPort = 14580;
                }

                settings.Filter = Settings.Ask("Filter");

                settings.RegExForParsingTelemetryData = "([a-zA-Z0-9]{1,3}[0123456789][a-zA-Z0-9]{0,3}[a-zA-Z].*)>APTT4,.*:T\\#(...),(...),(...),(...),(...),(...),(........)";
                settings.OutputFile = "repeaterTelemetry.json";
                settings.Save();
            }

            ReadSettingsAndListen:

            Repeaters repeaters = new Repeaters(settings.OutputFile);

            Config config = new Config();
            config.Callsign = settings.Callsign;
            config.Password = settings.Password;
            config.Uri = settings.ServerAddress;
            config.UseOgnAdditives = false;
            config.Port = settings.ServerPort;
            config.SoftwareName = Console.Title;
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

    * Press ESC for settings, CTRL-C to quit *


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

            bool interruptReceived = false;

            Console.CancelKeyPress += delegate {
                Console.WriteLine("** Interrupt received **");
                interruptReceived = true;
                System.Environment.Exit(0);
            };

            await listener.Open();

            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                // Just keep swimming, just keep swimming..
            }

            listener.Stop();

        ShowMenu:

            Console.WriteLine("\r\n");
            Console.WriteLine(@" /--------------------------------------------------------------------------\ ");
            Console.WriteLine(@" |" + Console.Title.PadBoth(75));
            Console.WriteLine(@" |" + "MENU".PadBoth(75));
            Console.WriteLine(@" | ");
            Console.WriteLine(" |     1. Change callsign");
            Console.WriteLine(" |     2. Change password");
            Console.WriteLine(" |     3. Change server address");
            Console.WriteLine(" |     4. Change server port");
            Console.WriteLine(" |     5. Change APRS filter");
            Console.WriteLine(" |     6. Change regex for parsing telemetry");
            Console.WriteLine(" |     7. Change output file");
            Console.WriteLine(" |     S. Save and restart program");
            Console.WriteLine(" |     Q. Quit program");
            Console.WriteLine(@" | ");
            Console.WriteLine(@" \--------------------------------------------------------------------------/ ");
            Console.WriteLine("");
            Console.Write("> ");

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                    settings.Callsign = Settings.Ask("Callsign");
                    break;
                case ConsoleKey.D2:
                    settings.Password = Settings.Ask("Password");
                    break;
                case ConsoleKey.D3:
                    settings.ServerAddress = Settings.Ask("ServerAddress");
                    break;
                case ConsoleKey.D4:
                    string strPort = Settings.Ask("ServerPort");
                    if (!int.TryParse(strPort, out settings.ServerPort))
                    {
                        settings.ServerPort = 14580;
                    }
                    break;
                case ConsoleKey.D5:
                    settings.Filter = Settings.Ask("Filter");
                    break;
                case ConsoleKey.D6:
                    settings.RegExForParsingTelemetryData = Settings.Ask("RegExForParsingTelemetryData");
                    break;
                case ConsoleKey.D7:
                    settings.OutputFile = Settings.Ask("OutputFile");
                    break;
                case ConsoleKey.S:
                    settings.Save();
                    goto ReadSettingsAndListen;
                    //break;
                case ConsoleKey.Q:
                    System.Environment.Exit(0);
                    break;
                default:
                    break;
            }

            goto ShowMenu;
        }
    }
}
