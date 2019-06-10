
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace ESL_WPF
{
    public class OutBoundClass
    {
        public BitmapImage image { get; set; }
        public string id { get; set; }
        public string datetime { get; set; }
        public string workerName { get; set; }
        public int detailCount { get; set; }
        public bool check { get; set; }

        public string updateTime { get; set; }
        public string boundState { get; set; }
        public string remarks { get; set; }

    }

    public class OutBoundDetailClass
    {
        public string id { get; set; }
        public int amount { get; set; }
        public string productID { get; set; }
        public int state { get; set; }

        public string outboundId { get; set; }
        public string updateTime { get; set; }

        public string remarks { get; set; }

    }

    public class OutBoundOrderClass
    {
        public string id { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public int workerId { get; set; }

        public int state { get; set; }
        public string remarks { get; set; }
        public string updateTime { get; set; }
        public string completeTime { get; set; }

        public List<OutBoundDetailClass> detailData { get; set; }

    }

    public class OutBoundExportListClass
    {
        public string id { get; set; }
        public string datetime { get; set; }
        public string workerName { get; set; }
        public string orderRemarks { get; set; }
        public int orderState { get; set; }

        public List<OutBoundExportClass> ExportClass { get; set; }

    }

    public class OutBoundExportClass
    {
        public string outboundId { get; set; }
        public int detailState { get; set; }
        public string productName { get; set; }
        public string eslID { get; set; }

        public string detailRemarks { get; set; }
        public int amount { get; set; }

    }


    public class OutBoundDetailListClass : INotifyPropertyChanged
    {
        public string eslID { get; set; }
        public string proID { get; set; }
        public string name { get; set; }
        public string detailID { get; set; }
        public BitmapImage image { get; set; }

        private bool onOff;


        public bool check
        {
            get { return this.onOff; }

            set

            {
                onOff = value;
                NotifyPropertyChanged("check");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public string shelf { get; set; }
        public string state { get; set; }
        public int inStocknow { get; set; }
        public string amount { get; set; }
        public string detailRemarks { get; set; }

    }
}