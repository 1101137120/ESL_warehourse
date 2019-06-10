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
    /// EslSystemLogin.xaml 的互動邏輯
    /// </summary>
    public partial class EslSystemLogin : Window
    {
        public EslSystemLogin()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {


            ServerClass a = new ServerClass();
            bool result = a.Login(username.Text, passwordBox.Password);
            //Console.WriteLine("---------result  "+ result);
            if (result)
            {
                //Console.WriteLine("---------DS");
                MainWindow mainWin = new MainWindow();
                //Console.WriteLine("---------ss");
                mainWin.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("帳密錯誤");
            }
        }
    }
}
