using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Text;
using System.Runtime.CompilerServices;
using System.Dynamic;

namespace AprsRepeaterMonitor
{
    [Serializable]
    class Repeaters
    {
        public ExpandoObject repeaters;
        private string JsonFileLocation;

        public Repeaters(string jsonFileLocation)
        {
            JsonFileLocation = jsonFileLocation;
            repeaters = new ExpandoObject();
            Load();
        }

        public void Add(Repeater repeater)
        {
            Add(repeater.name, repeater);
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(repeaters, Formatting.Indented);
            File.WriteAllTextAsync(JsonFileLocation, json);
        }

        private void Load()
        {
            try
            {
                string json = File.ReadAllText(JsonFileLocation);
                repeaters = JsonConvert.DeserializeObject<ExpandoObject>(json);
            }
            catch (Exception)
            {
                // Move on 
            }
        }

        private void Add(string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = repeaters as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }
}
