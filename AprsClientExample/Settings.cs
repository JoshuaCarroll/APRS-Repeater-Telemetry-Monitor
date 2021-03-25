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
    }
}
