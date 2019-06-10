
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ESL_WPF
{
    public class ProductClass
    {
        int id { get; set; }
        string eslID { get; set; }
        string name { get; set; }
        int price { get; set; }
        string shelf { get; set; }
        int inStocknow { get; set; }

    }

    public class OutBoundProductClass : INotifyPropertyChanged
    {
        public string eslID { get; set; }
        public string id { get; set; }
        public string name { get; set; }
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

        public int price { get; set; }

        public string shelf { get; set; }
        public int inStocknow { get; set; }
        public string get { get; set; }
        public string remarks { get; set; }


    }

}