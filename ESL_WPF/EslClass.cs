
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ESL_WPF
{
    class EslClass
    {

        public string id { get; set; }
        public bool check { get; set; }
        public string APId { get; set; }
        public int rssi { get; set; }
        public double battery { get; set; }
        public int lightStatus { get; set; }
        public string productID { get; set; }
        public int count { get; set; }

        public string remarks { get; set; }

    }

    class EslResetClass
    {
        public List<string> eslId { get; set; }

        public List<EslClass> eslData { get; set; }

    }

}