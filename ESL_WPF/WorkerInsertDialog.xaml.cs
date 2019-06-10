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
    /// WorkerInsertDialog.xaml 的互動邏輯
    /// </summary>
    public partial class WorkerInsertDialog : Window
    {
        public WorkerInsertDialog()
        {
            InitializeComponent();
        }

        private void WorkerInsertButton_Click(object sender, RoutedEventArgs e)
        {
            ServerClass a = new ServerClass();
            bool result = a.workerInsertDB(WorkerNameTextBox.Text,WorkerPhoneTextBox.Text,WorkerMailTextBox.Text,WorkerUserNameTextBox.Text,WorkerPasswordTextBox.Text,WorkerLevelComboBox.Text,WorkerRemarkTextBox.Text);
            if (result)
                this.Close();
        }
    }
}
