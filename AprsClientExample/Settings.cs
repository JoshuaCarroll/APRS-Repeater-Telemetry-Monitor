using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AprsRepeaterMonitor
{
    public class Settings
    {
        public string Callsign;
        private string password;
        public string Password
        {
            get {
                return password;
            }
            set { 
                if (value.Trim() == string.Empty)
                {
                    password = "-1";
                }
                else
                {
                    password = value;
                }
            }
        }
        private string serverAddress;
        public string ServerAddress
        {
            get {
                return serverAddress;
            }
            set {
                if (value.Trim() == string.Empty)
                {
                    serverAddress = "noam.aprs2.net";
                }
                else
                {
                    serverAddress = value;
                }
            }
        }
        public int ServerPort;
        public string Filter;
        public string OutputFile;
        public string RegExForParsingTelemetryData;

        public Settings()
        {

        }

        public Settings(bool loadSaved)
        {
            if (loadSaved)
            {
                Load();
            }
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllTextAsync("settings.json", json);
        }
        
        public void Load()
        {
            try
            {
                string strJson = File.ReadAllText("settings.json");
                Settings settings = JsonConvert.DeserializeObject<Settings>(strJson);
                Callsign = settings.Callsign;
                Password = settings.Password;
                ServerAddress = settings.ServerAddress;
                ServerPort = settings.ServerPort;
                Filter = settings.Filter;
                OutputFile = settings.OutputFile;
                RegExForParsingTelemetryData = settings.RegExForParsingTelemetryData;
            }
            catch (System.IO.FileNotFoundException)
            {
                Password = "-1";
                ServerAddress = "205.209.228.93";
                ServerPort = 14580;
                OutputFile = "repeaterTelemetry.json";
                RegExForParsingTelemetryData = @"([a-zA-Z0-9]{1,3}[0123456789][a-zA-Z0-9]{0,3}[a-zA-Z].*)>APTT4,.*:T\\#(...),(...),(...),(...),(...),(...),(........)";
            }
        }

        public static string Ask(string settingName)
        {
            switch (settingName)
            {
                case "Callsign":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\r\n\r\nYou need to login on the server with a callsign. Typically this would be your own callsign with a hyphen and number added to the end (ie AA1BB-1).");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("What callsign do you want to use to login to the APRS server? ");
                    break;
                case "Password":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\r\nNow you will need to enter the APRS passcode.. or not. Here's the thing, since this application will only read data (doesn't write anything to APRS) you don't really need to provide a passcode. If you want to generate your password, visit http://bit.ly/aprspasscode. If you want to login without a passcode, hit enter and we'll send \"-1\" as your passcode.");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("What is your passcode [-1]? ");
                    break;
                case "ServerAddress":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\r\nYou can find a list of APRS servers at http://aprs2.net.");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("To which APRS server do you want to connect [noam.aprs2.net]? ");
                    break;
                case "ServerPort":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\r\nThe default client-defined filter port is 14580.");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("To what port do you want to connect [14580]? ");
                    break;
                case "Filter":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\r\nAPRS filters enable you to only receive messages that match a predetermined set of parameters. You can read all about them at aprs-is.net. When I want the telemetry for the W5AUU repeaters, I use a filter like this:");
                    Console.WriteLine("\tb/W5AUU-* -t/poimqsunw");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("What APRS filter do you want to use? ");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case "RegExForParsingTelemetryData":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\r\nThe default regular expresion for parsing APRS telemtry data is:");
                    Console.WriteLine(@"([a-zA-Z0-9]{1,3}[0123456789][a-zA-Z0-9]{0,3}[a-zA-Z].*)>APTT4,.*:T\\#(...),(...),(...),(...),(...),(...),(........)");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("What regular expression do you want to use? ");
                    break;
                case "OutputFile":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\r\nThe default output file location is:");
                    Console.WriteLine("repeaterTelemetry.json");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("What output file location do you want to use? ");
                    break;
                default:
                    return string.Empty;
            }

            Console.ResetColor();
            return Console.ReadLine();
        }
    }
}
