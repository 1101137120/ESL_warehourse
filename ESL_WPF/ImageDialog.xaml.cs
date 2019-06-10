using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ESL_WPF
{
    /// <summary>
    /// ImageDialog.xaml 的互動邏輯
    /// </summary>
    public partial class ImageDialog : Window
    {
        public string Id;
        public string action;
        public ImageDialog()
        {
            InitializeComponent();
        }

        private void SolvedButton_Click(object sender, RoutedEventArgs e)
        {
            ServerClass a = new ServerClass();
            if (action == "OutBound")
                a.outBoundSolvedState(Id);
            else if(action == "InBound")
                a.inBoundSolvedState(Id);

            this.Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
