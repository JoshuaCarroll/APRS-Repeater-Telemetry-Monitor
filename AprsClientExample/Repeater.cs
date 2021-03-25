using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;

namespace AprsRepeaterMonitor
{
    [Serializable]
    class Repeater : DynamicObject
    {
        public string name;
        public string serialNumber;
        public string telemetry1;
        public string telemetry2;
        public string telemetry3;
        public string telemetry4;
        public string telemetry5;
        public string binary;
        public DateTime lastUpdated;

        public Repeater(string Name, string SerialNumber, string Telemetry1, string Telemetry2, string Telemetry3, string Telemetry4, string Telemetry5, string Binary)
        {
            name = Name;
            serialNumber = SerialNumber;
            telemetry1 = Telemetry1;
            telemetry2 = Telemetry2;
            telemetry3 = Telemetry3;
            telemetry4 = Telemetry4;
            telemetry5 = Telemetry5;
            binary = Binary;
            lastUpdated = DateTime.Now;
        }
    }
}
