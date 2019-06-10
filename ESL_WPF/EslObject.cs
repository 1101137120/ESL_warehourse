using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

namespace ESL_WPF
{
    public class EslObject
    {
        public Socket workSocket = null;
        public SmcEsl mSmcEsl = new SmcEsl(null);
        public Stopwatch stopwatch = new Stopwatch();
    }

    public class ScanEslObject
    {
        public string mac = "null";
        public string battery = "null";
    }

    public class Page1
    {
        public int no;
        public string barcode;
        public string product_name;
        public string shelf;
        public int price;
        public int stock;
        public string bleAddress;
        public string remarks;
        public string updateState;
        public string apLink;
        public string actionName;
        public string eslSize;
        public int lightStatus;
    }
    public class ErrorEslObject
    {
        public string mac = "null";
        public int error = 0;
    }

}
