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
    /// dialog.xaml 的互動邏輯
    /// </summary>
    public partial class dialog : Window
    {
        public dialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ServerClass a = new ServerClass();
            if (productName.Text == "" || productPrice.Text == "" || productShelf.Text == "" || productStock.Text == "")
            {
                MessageBox.Show("名稱、價格、貨架、庫存是否填寫");
                return;
            }
            bool result =  a.productInsertDB(productID.Text,productName.Text,Convert.ToInt32(productPrice.Text),productShelf.Text,Convert.ToInt32(productStock.Text),productRemark.Text);

            if (!result)
                MessageBox.Show("更新失敗");

            this.Close();
            

        }
    }
}
