using System;
using System.Collections.Generic;
using System.IO;
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
    /// AutoExportSetDialog.xaml 的互動邏輯
    /// </summary>
    public partial class AutoExportSetDialog : Window
    {
        public AutoExportSetDialog()
        {
            InitializeComponent();
        }

        private void ExportDayCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExportDayCombobox.SelectedIndex == 6){
                CustomDay.Visibility = Visibility.Visible;
            }
            else {
                CustomDay.Visibility = Visibility.Collapsed;
            }
        }

        private void ExportPathButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folder  = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folder.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ExportPathTextbox.Text = folder.SelectedPath;

            }
        }

        private void AutoExportSetButton_Click(object sender, RoutedEventArgs e)
        {
            

                string tempPath = System.IO.Path.GetTempPath();

                StreamWriter TempStreamWriter = new StreamWriter(tempPath + "EslTemp.tmp", false, System.Text.Encoding.UTF8);

                string strExportDay = "";
                string strExportPath = "";
                if (ExportDayCombobox.SelectedIndex == 6)
                    strExportDay = CustomDay.Text;
                else
                    strExportDay = ExportDayCombobox.Text;

                

                strExportPath = ExportPathTextbox.Text;

                if (strExportDay==""|| strExportPath=="")
                {
                    MessageBox.Show("匯出時間或路徑不能為空值");
                    return;
                }

                TempStreamWriter.WriteLine(strExportDay);
                TempStreamWriter.WriteLine(strExportPath);
                TempStreamWriter.Close();
            this.Close();
        }
    }
}
