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
        public string Password;
        public string ServerAddress;
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
    }
}
