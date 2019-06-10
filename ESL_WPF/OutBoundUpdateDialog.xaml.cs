using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    /// OutBoundUpdateDialog.xaml 的互動邏輯
    /// </summary>
    public partial class OutBoundUpdateDialog : Window
    {
        public OutBoundUpdateDialog()
        {
            InitializeComponent();
        }

        private void OutBoundUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string OID = OutBoundID.Content.ToString();
            List<OutBoundDetailClass> OutBoundDetailClassList = new List<OutBoundDetailClass>();


            foreach (OutBoundDetailListClass OClass in OutBoundProductdataGrid.ItemsSource)
            {
                //Console.WriteLine("OClass   check " + OClass.check);
                if (OClass.check)
                {
                    OutBoundDetailClass InsertODteailClass = new OutBoundDetailClass();
                    InsertODteailClass.productID = OClass.proID.ToString();
                    InsertODteailClass.amount = Convert.ToInt32(OClass.amount);
                    InsertODteailClass.state = 1;
                    InsertODteailClass.remarks = OClass.detailRemarks;
                    InsertODteailClass.updateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    InsertODteailClass.outboundId = OID;
                    OutBoundDetailClassList.Add(InsertODteailClass);
                }
            }

            OutBoundOrderClass InsertOOrderClass = new OutBoundOrderClass();
            InsertOOrderClass.id = OID;
            InsertOOrderClass.date = DateTime.Now.ToString("yyyy/MM/dd");
            InsertOOrderClass.time = DateTime.Now.ToString("HH:mm:ss");
            foreach (WorkerComboboxClass data in workerComboBox.Items)
            {
                if (workerComboBox.Text == data.name)
                    InsertOOrderClass.workerId = data.id;
            }

            if (InsertOOrderClass.workerId == 0 || OutBoundDetailClassList.Count == 0)
            {
                MessageBox.Show("派單人員不能為空，商品最少選擇一項。");
                return;
            }



            InsertOOrderClass.state = 1;
            InsertOOrderClass.updateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            InsertOOrderClass.detailData = OutBoundDetailClassList;
            ServerClass a = new ServerClass();

            dynamic checkResult = a.OutBoundEslCheck(OutBoundDetailClassList);
            if (checkResult.status == "fail")
            {
                MessageBox.Show(checkResult.message);
                return;
            }
            bool result = a.UpdateOutBound(InsertOOrderClass);


            OutBoundProductdataGrid.ItemsSource = a.outBoundDetailsList(OID);
            OutBoundProductdataGrid.Items.Refresh();

            if (result)
                this.Close();

        }

        private void ChkAll_Checked(object sender, RoutedEventArgs e)

        {

            //Console.WriteLine("pp");

        }

        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
            try {
                OutBoundProductdataGrid.CurrentCell = new DataGridCellInfo(
                OutBoundProductdataGrid.Items[OutBoundProductdataGrid.SelectedIndex], OutBoundProductdataGrid.Columns[6]);
                OutBoundProductdataGrid.BeginEdit();
            }
            catch (Exception ex) {
            }

        }
        private void ChkAll_Unchecked(object sender, RoutedEventArgs e)

        {

            //Console.WriteLine("pWWp");

        }

        private void OutBoundProductdataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //Console.WriteLine(e.Column.Header);
            if (e.Column.DisplayIndex == 6)
            {
                int a = 1;
                for (int i = 0; i < OutBoundProductdataGrid.Items.Count; i++)
                {
                    if (i == OutBoundProductdataGrid.SelectedIndex)
                    {
                        DataGridRow row = (DataGridRow)OutBoundProductdataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                        CheckBox checkBox = FindChild<CheckBox>(row, "Chk");
                        if (checkBox.IsChecked == false)
                        {
                            checkBox.IsChecked = true;
                            //some code
                        }
                    }

                }
            }
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
          where T : DependencyObject
        {
            // Confirm parent is valid.  
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child 
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree 
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.  
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search 
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name 
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found. 
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }

        private void OutBoundDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ServerClass a = new ServerClass();
            List<string> OutBoundIDList = new List<string>();
            OutBoundIDList.Add(OutBoundID.Content.ToString());
            a.outBoundDeleteDB(OutBoundIDList);
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string filterText = searchTextBox.Text;
            ICollectionView cv = CollectionViewSource.GetDefaultView(OutBoundProductdataGrid.ItemsSource);

            if (!string.IsNullOrEmpty(filterText))
            {
                cv.Filter = o =>
                {
                    /* change to get data row value */
                    OutBoundDetailListClass p = o as OutBoundDetailListClass;
                    return (p.name.ToUpper().StartsWith(filterText.ToUpper()));
                    /* end change to get data row value */
                };
            }
            else
            {
                cv.Filter = o =>
                {
                    /* change to get data row value */
                    OutBoundDetailListClass p = o as OutBoundDetailListClass;
                    return (p.name.ToUpper().StartsWith(""));
                    /* end change to get data row value */
                };
            }
        }

        public void TryGridRefresh()
        {
            string IDToFind = search.Content.ToString();

            if (OutBoundProductdataGrid.ItemsSource is DataView)
            {
                foreach (DataRow drv in OutBoundProductdataGrid.ItemsSource)
                    if ((string)drv["name"] == IDToFind)
                    {
                    
                        // This is the data row view record you want...

                    }
            }
        }
    }
}
