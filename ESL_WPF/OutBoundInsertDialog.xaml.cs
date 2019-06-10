using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// InBoundInsertDialog.xaml 的互動邏輯
    /// </summary>
    public partial class OutBoundInsertDialog : Window
    {
        public OutBoundInsertDialog()
        {
            InitializeComponent();
        }

        private void OutBoundProductdataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
           //Console.WriteLine( e.Column.Header);
            if (e.Column.DisplayIndex==6)
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
        private void OutBoundProductdataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            
        }

        private void OutBoundProductdataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void OutBoundProductdataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {

        }

        private void OutBoundProductdataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            try {
                var cellInfo = OutBoundProductdataGrid.SelectedCells[3];

                var content = cellInfo.Column.GetCellContent(cellInfo.Item);
                //Console.WriteLine("content  " + content);
            }
            catch (Exception ex ) {

            }


        }

        private void ChkAll_Checked(object sender, RoutedEventArgs e)

        {

            //Console.WriteLine("pp");

        }

        private void Chk_Checked(object sender, RoutedEventArgs e)
        {

                OutBoundProductdataGrid.CurrentCell = new DataGridCellInfo(
                OutBoundProductdataGrid.Items[OutBoundProductdataGrid.SelectedIndex], OutBoundProductdataGrid.Columns[6]);
                OutBoundProductdataGrid.BeginEdit();



        }
        private void ChkAll_Unchecked(object sender, RoutedEventArgs e)

        {

            //Console.WriteLine("pWWp");

        }

        private void OutBoundInsert_Click(object sender, RoutedEventArgs e)
        {
            string OID = OutBoundID.Content.ToString();
            List<OutBoundDetailClass> OutBoundDetailClassList = new List<OutBoundDetailClass>();
            List<string> proID = new List<string>();

            foreach (OutBoundProductClass OClass in OutBoundProductdataGrid.ItemsSource)
            {
                //Console.WriteLine("OClass   check " + OClass.check);
                if (OClass.check)
                {
                    OutBoundDetailClass InsertODteailClass = new OutBoundDetailClass();
                    InsertODteailClass.productID = OClass.id;
                    InsertODteailClass.amount = Convert.ToInt32(OClass.get);
                    InsertODteailClass.state = 1;
                    InsertODteailClass.updateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    InsertODteailClass.outboundId = OID;
                    InsertODteailClass.remarks = OClass.remarks;
                    OutBoundDetailClassList.Add(InsertODteailClass);
                    if (InsertODteailClass.amount == 0)
                    {
                        MessageBox.Show("勾選項目未填寫數量。");
                        return;
                    }
                    proID.Add(OClass.id);
                }
            }

            OutBoundOrderClass InsertOOrderClass = new OutBoundOrderClass();
            InsertOOrderClass.id = OID;
            InsertOOrderClass.date = DateTime.Now.ToString("yyyy/MM/dd");
            InsertOOrderClass.time = DateTime.Now.ToString("HH:mm:ss");
            foreach (WorkerComboboxClass data in WorkerComboBox.Items)
            {
                if(WorkerComboBox.Text == data.name)
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
            bool result =  a.InsertOutBound(InsertOOrderClass);
            bool eslLightresult = a.eslLightONList(proID);

            if (result)
                this.Close();

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
    }
}
