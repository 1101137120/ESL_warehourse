using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ESL_WPF
{
     class WorkerComboboxClass
    {
        public int id { get; set; }
        public string name { get; set; }

        public override string ToString()
        {
            return name;
        }
    }

    public class WorkerClass : INotifyPropertyChanged
    {
        public string id { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }

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

        public string userName { get; set; }
        public string password { get; set; }
        public int level { get; set; }
        public string remarks { get; set; }
    }
}