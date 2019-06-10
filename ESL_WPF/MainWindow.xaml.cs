using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Media =  System.Windows.Media;

using System.ComponentModel;
using System.IO;
using System.Windows.Controls.Primitives;
using static ESL_WPF.Tools;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Drawing;
using System.Data;
using System.Windows.Media.Imaging;

using WebSocket = Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json;

namespace ESL_WPF
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Timers.Timer ExportDataTimer = new System.Timers.Timer();
        System.Timers.Timer EslLightCheckTimer = new System.Timers.Timer();
        System.Timers.Timer ConnectTimeOutTimer = new System.Timers.Timer();
        System.Timers.Timer ResetEslScanTimer = new System.Timers.Timer();
        delegate void APScanDataInvoker(EventArgs e);
        delegate void ReceiveDataInvoker(EventArgs e);
        Dictionary<string, EslObject> mDictSocket = new Dictionary<string, EslObject>();
        ElectronicPriceData mElectronicPriceData = new ElectronicPriceData();
        List<APClass> APList = new List<APClass>();
        List<Page1> productDataListUpdate = new List<Page1>();
        List<EslClass> resetAPList = new List<EslClass>();
        List<string> reEslId = new List<string>();
        int productIndex = 0;
        bool resetAPStatus = false;
        bool dtectionAPStatus = false;
        WebSocket.Socket socket = null;
        public MainWindow()
        {
            InitializeComponent();
            ServerClass a = new ServerClass();

            //productDataGrid.ItemsSource = a.useHttpWebRequest_Get("http://localhost:9004/ESL/v1/Product","product").DefaultView;
            productDataGrid.ItemsSource = a.GetProductData();

            /*var aaa = productDataGrid.Items.OfType<OutBoundProductClass>();
            List<OutBoundProductClass> d = new List<OutBoundProductClass>();
            d = aaa.ToList();*/
            
            workerDataGrid.ItemsSource = a.GetWorkerData();
            eslDataGrid.ItemsSource = a.GetEslData();
            inboundDataGrid.ItemsSource = a.GetInBoundData();
            apDataGrid.ItemsSource = a.GetAPData();
            outboundDataGrid.ItemsSource = a.GetOutBoundData();

            ExportDataTimer.Interval = 1 * 10 * 1000;
            ExportDataTimer.Elapsed += ExportDataTimer_Elapsed;

            EslLightCheckTimer.Interval = 1 * 10 * 1000;
            EslLightCheckTimer.Elapsed += EslLightCheckTimer_Elapsed;
           // EslLightCheckTimer.Start();

            ConnectTimeOutTimer.Interval = 1 * 20 * 1000;
            ConnectTimeOutTimer.Elapsed += ConnectTimeOutTimer_Elapsed;


            ResetEslScanTimer.Interval = 1 * 10 * 1000;
            ResetEslScanTimer.Elapsed += ResetEslScanTimer_Elapsed;
            //  workerDataGrid.ItemsSource = a.useHttpWebRequest_Get("http://localhost:9004/ESL/v1/Worker", "worker").DefaultView;
            //  inboundDataGrid.ItemsSource = a.useHttpWebRequest_Get("http://localhost:9004/ESL/v1/InBoundOrder", "inbound").DefaultView;
            //  outboundDataGrid.ItemsSource = a.useHttpWebRequest_Get("http://localhost:9004/ESL/v1/OutBoundOrder", "outbound").DefaultView;

            Tools tool = new Tools();
            tool.onApScanEvent += new EventHandler(AP_Scan);
            tool.SNC_GetAP_Info();
            string str = System.IO.Directory.GetCurrentDirectory();
            BitmapImage errorImage = new BitmapImage(new Uri(str + "\\error.png"));

            socket = WebSocket.IO.Socket("https://api.ihoin.com/");//实例化对象
            socket.On(WebSocket.Socket.EVENT_CONNECT, () =>       //监听链接
            {
              //  //Console.WriteLine("链接成功");
               // socket.Emit("hi", "ccdcd");
                socket.On("OErrorMessage", (data) =>      
                {
                    dynamic boundData = JsonConvert.DeserializeObject(data.ToString());
                    //Console.WriteLine("boundData   " + boundData);
                    OutBoundClass boundClass = new OutBoundClass { id = boundData.id, boundState = boundData.state, updateTime = boundData.updateTime };
                        foreach (OutBoundClass OClass in outboundDataGrid.ItemsSource)
                        {
                           // //Console.WriteLine("boundData   " + boundData.id);
                            if (OClass.id == boundClass.id)
                            {
                                 OClass.updateTime = boundClass.updateTime;

                                if (boundClass.boundState == "0")
                                    OClass.image = errorImage;
                                else
                                    OClass.image = null;
                            }
                        }

                });

                socket.On("IErrorMessage", (data) =>
                {
                    dynamic boundData = JsonConvert.DeserializeObject(data.ToString());
                    //Console.WriteLine("boundData   " + boundData);
                    InBoundClass boundClass = new InBoundClass { id = boundData.id, boundState = boundData.state, updateTime = boundData.updateTime };
                        foreach (InBoundClass IClass in inboundDataGrid.ItemsSource)
                        {
                          //  //Console.WriteLine("boundData   " + boundData.id);
                            if (IClass.id == boundClass.id)
                            {
                                IClass.updateTime = boundClass.updateTime;

                                if(boundClass.boundState =="0")
                                    IClass.image = errorImage;
                                else
                                    IClass.image = null;
                            }
                        }




                });

               /* socket.Emit("outbounderror", "{'id': '79156234108','state': 0}");
                socket.Emit("outbounderror", "{'id': '10265398147','state': 1}");
                socket.Emit("inbounderror", "{'id': '27394105681','state': 0}");*/
                socket.On("eslUpdate", (data) =>
                {
                    ////Console.WriteLine(data);

                    // List<EslClass> EslList = a.GetEslIsUpdateData();
                    if (productDataListUpdate.Count != 0)
                        return;

                    if (data.ToString()==null)
                        return;

                    dynamic EslData = JsonConvert.DeserializeObject(data.ToString());
                    //Console.WriteLine("result  "+ EslData);
                    List<EslClass> EslList = new List<EslClass>();
                    for (int i = 0; i < EslData.Count; i++)
                    {
                        EslClass EClass = new EslClass();
                        EClass.id = EslData[i].id;
                        EClass.APId = EslData[i].APId;
                        EClass.lightStatus = EslData[i].lightStatus;
                        EClass.productID = EslData[i].product_id;
                        EslList.Add(EClass);
                    }

                    
                    if (EslList.Count != 0)
                    {
                        foreach (EslClass esldata in EslList)
                        {
                            Page1 page = new Page1();
                            page.bleAddress = esldata.id;
                            page.apLink = esldata.APId;
                            page.actionName = "light";
                            page.lightStatus = esldata.lightStatus;
                            page.remarks = esldata.lightStatus.ToString();
                            productDataListUpdate.Add(page);
                        }


                    }

                    if (productDataListUpdate.Count != 0)
                    {
                            productIndex = 0;
                            foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                            {
                                //   if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                                //  {
                                kvp.Value.mSmcEsl.stopScanBleDevice();
                                //  }
                            }

                            Thread.Sleep(100);
                            foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                            {
                                if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                                {
                                    kvp.Value.mSmcEsl.ConnectBleDevice(productDataListUpdate[productIndex].bleAddress);
                                    ConnectTimeOutTimer.Start();
                                }
                            }
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                tbMessageBox.Text = productDataListUpdate[productIndex].bleAddress + "  嘗試連線中請稍候... \r\n";
                            }));

                    }
                });
            });
            socket.Connect();
            
            
         /*   socket.On(WebSocket.Socket.EVENT_DISCONNECT, () => {
                //Console.WriteLine("段線成功");
                socket.Close();
            });*/

        }
        

        private void ExportDataTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            string tempPath = System.IO.Path.GetTempPath();
            bool fi = File.Exists(tempPath + "EslTemp.tmp");
            if (fi)
            {
                ServerClass a = new ServerClass();
                StreamReader TempStreamReader = new StreamReader(tempPath + "EslTemp.tmp");
                String line = TempStreamReader.ReadToEnd();
                string[] words = line.Split('\n');

                TempStreamReader.Close();
                List<InBoundExportListClass> ExportData = a.inBoundAutoExportDB(words[0].Trim());
                List<OutBoundExportListClass> ExportDataO = a.outBoundAutoExportDB(words[0].Trim());
                if (ExportData.Count != 0)
                {
                    StreamWriter csvStreamWriter = new StreamWriter(words[1].Trim() + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "InBound"+".csv", true, System.Text.Encoding.UTF8);

                    string delimiter = ",";
                    string strHeader = "";
                    string strOrder = "";
                    int maxCount = 0;
                    strHeader += "" + delimiter;
                    strOrder += "" + delimiter;
                    for (int i = 0; i < ExportData.Count; i++)
                    {


                        strHeader += "ID" + delimiter + "時間" + delimiter + "派遣員" + delimiter + "備註" + delimiter + "狀態" + delimiter;
                        strHeader += "" + delimiter + "" + delimiter;


                        strOrder += ExportData[i].id + delimiter + ExportData[i].datetime + delimiter + ExportData[i].workerName + delimiter + ExportData[i].orderRemarks + delimiter + ExportData[i].orderState + delimiter;
                        strOrder += "" + delimiter + "" + delimiter;

                        if (maxCount < ExportData[i].ExportClass.Count)
                        {
                            maxCount = ExportData[i].ExportClass.Count;
                        }
                    }
                    csvStreamWriter.WriteLine(strHeader);
                    csvStreamWriter.WriteLine(strOrder);
                    csvStreamWriter.WriteLine();

                    for (int i = 0; i < maxCount; i++)
                    {

                        string strDetail = "";
                        strDetail += "" + delimiter;
                        string strDetailHeader = "";
                        strDetailHeader += "" + delimiter;
                        for (int x = 0; x < ExportData.Count; x++)
                        {
                            if (i == 0)
                            {

                                strDetailHeader += "品名" + delimiter + "ESLID" + delimiter + "明細狀態" + delimiter + "備註" + delimiter + "數量" + delimiter;
                                strDetailHeader += "" + delimiter + "" + delimiter;

                            }
                            if (i < ExportData[x].ExportClass.Count)
                            {
                                strDetail += ExportData[x].ExportClass[i].productName + delimiter + ExportData[x].ExportClass[i].eslID + delimiter + ExportData[x].ExportClass[i].detailState + delimiter + ExportData[x].ExportClass[i].detailRemarks + delimiter + ExportData[x].ExportClass[i].amount + delimiter;
                                strDetail += "" + delimiter + "" + delimiter;
                            }

                        }

                        if (i == 0)
                        {
                            csvStreamWriter.WriteLine(strDetailHeader);
                        }

                        csvStreamWriter.WriteLine(strDetail);

                    }

                    csvStreamWriter.Close();
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        inboundDataGrid.ItemsSource = a.GetInBoundData();
                    }));

                }

                if (ExportDataO.Count != 0)
                {
                    StreamWriter csvStreamWriter = new StreamWriter(words[1].Trim() + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss")+"OutBound" + ".csv", true, System.Text.Encoding.UTF8);

                    string delimiter = ",";
                    string strHeader = "";
                    string strOrder = "";
                    int maxCount = 0;
                    strHeader += "" + delimiter;
                    strOrder += "" + delimiter;
                    for (int i = 0; i < ExportDataO.Count; i++)
                    {


                        strHeader += "ID" + delimiter + "時間" + delimiter + "派遣員" + delimiter + "備註" + delimiter + "狀態" + delimiter;
                        strHeader += "" + delimiter + "" + delimiter;


                        strOrder += ExportDataO[i].id + delimiter + ExportDataO[i].datetime + delimiter + ExportDataO[i].workerName + delimiter + ExportDataO[i].orderRemarks + delimiter + ExportDataO[i].orderState + delimiter;
                        strOrder += "" + delimiter + "" + delimiter;

                        if (maxCount < ExportDataO[i].ExportClass.Count)
                        {
                            maxCount = ExportDataO[i].ExportClass.Count;
                        }
                    }
                    csvStreamWriter.WriteLine(strHeader);
                    csvStreamWriter.WriteLine(strOrder);
                    csvStreamWriter.WriteLine();

                    for (int i = 0; i < maxCount; i++)
                    {

                        string strDetail = "";
                        strDetail += "" + delimiter;
                        string strDetailHeader = "";
                        strDetailHeader += "" + delimiter;
                        for (int x = 0; x < ExportDataO.Count; x++)
                        {
                            if (i == 0)
                            {

                                strDetailHeader += "品名" + delimiter + "ESLID" + delimiter + "明細狀態" + delimiter + "備註" + delimiter + "數量" + delimiter;
                                strDetailHeader += "" + delimiter + "" + delimiter;

                            }
                            if (i < ExportDataO[x].ExportClass.Count)
                            {
                                strDetail += ExportDataO[x].ExportClass[i].productName + delimiter + ExportDataO[x].ExportClass[i].eslID + delimiter + ExportDataO[x].ExportClass[i].detailState + delimiter + ExportDataO[x].ExportClass[i].detailRemarks + delimiter + ExportDataO[x].ExportClass[i].amount + delimiter;
                                strDetail += "" + delimiter + "" + delimiter;
                            }

                        }

                        if (i == 0)
                        {
                            csvStreamWriter.WriteLine(strDetailHeader);
                        }

                        csvStreamWriter.WriteLine(strDetail);

                    }

                    csvStreamWriter.Close();
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        outboundDataGrid.ItemsSource = a.GetOutBoundData();
                    }));

                }
            }

        }


        private void EslLightCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ServerClass a = new ServerClass();
            List <EslClass> EslList = a.GetEslIsUpdateData();

            bool isAction = false;
            if (productDataListUpdate.Count != 0)
                isAction = true;


            if (EslList.Count!=0)
            {
                EslLightCheckTimer.Stop();
                foreach (EslClass data in EslList)
                {
                    Page1 page = new Page1();
                    page.bleAddress = data.id;
                    page.apLink = data.APId;
                    page.actionName = "light";
                    page.lightStatus = data.lightStatus;
                    page.remarks = data.lightStatus.ToString();
                    productDataListUpdate.Add(page);
                }


            }

            if (productDataListUpdate.Count != 0)
            {
                if (!isAction)
                {
                    productIndex = 0;
                    foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                    {
                        //   if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                        //  {
                        kvp.Value.mSmcEsl.stopScanBleDevice();
                        //  }
                    }

                    Thread.Sleep(100);
                    foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                    {
                        if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                        {
                            kvp.Value.mSmcEsl.ConnectBleDevice(productDataListUpdate[productIndex].bleAddress);
                            ConnectTimeOutTimer.Start();
                        }
                    }
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        tbMessageBox.Text = productDataListUpdate[productIndex].bleAddress + "  嘗試連線中請稍候... \r\n";
                    }));
                }
               
            }
        }

        private void ConnectTimeOutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ConnectTimeOutTimer.Stop();
            productDataListUpdate[productIndex].updateState = "更新失敗";
            ServerClass a = new ServerClass();
            a.eslLightFail(productDataListUpdate[productIndex].bleAddress);
            foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
            {
                if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                {
                    productIndex++;
                    kvp.Value.mSmcEsl.DisConnectBleDevice();
                }
            }


            
      /*      if (productDataListUpdate.Count > productIndex)
            {
                foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                {
                    if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                    {
                        kvp.Value.mSmcEsl.ConnectBleDevice(productDataListUpdate[productIndex].bleAddress);
                        ConnectTimeOutTimer.Start();
                    }
                }
            }
            else
            {
                MessageBox.Show("更新結束");
                productIndex = 0;
                ServerClass a = new ServerClass();
                this.Dispatcher.Invoke((Action)(() =>
                {
                    productDataGrid.ItemsSource = a.GetProductData();
                    productDataGrid.Items.Refresh();
                }));
                productDataListUpdate.Clear();
                EslLightCheckTimer.Start();
            }*/
        }

        private void ResetEslScanTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
            {
                kvp.Value.mSmcEsl.stopScanBleDevice();
            }
            ResetEslScanTimer.Stop();
            resetAPStatus = false;
            EslResetClass EslReset = new EslResetClass();
            EslReset.eslId = new List<string>();
            EslReset.eslData = new List<EslClass>();
            EslReset.eslId = reEslId;
            EslReset.eslData = resetAPList;
            ServerClass a = new ServerClass();
            a.EslResetAP(EslReset);

            this.Dispatcher.Invoke((Action)(() =>
            {
                resetESLAPLinkButton.Foreground = Media.Brushes.Black;
                resetESLAPLinkButton.Content = "重製配對";
                eslDataGrid.ItemsSource = a.GetEslData();
                eslDataGrid.Items.Refresh();
            }));



        }

        private void InsertProductButton_Click(object sender, RoutedEventArgs e)
        {
            dialog dddd = new dialog();
            dddd.Closing += Dddd_Closing;
            dddd.ShowDialog();
                
                
        }

        private void Dddd_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Console.WriteLine("IDDDDDDDDDDDDDDDDDDDDDDDDD");
            ServerClass a = new ServerClass();
            productDataGrid.ItemsSource = a.GetProductData();
            productDataGrid.Items.Refresh();
            // throw new NotImplementedException();
        }

        private void ProductDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result =  MessageBox.Show("確定刪除商品?", "商品刪除", MessageBoxButton.YesNo);

            List<string> productID = new List<string>();
            if (result == MessageBoxResult.Yes)
            {

                foreach (OutBoundProductClass PClass in productDataGrid.ItemsSource)
                {
                    //Console.WriteLine("OClass   check " + PClass.check + PClass.id);
                    if (PClass.check)
                    {
                        productID.Add(PClass.id);
                    }
                }

                ServerClass a = new ServerClass();
                bool resultDB = a.productDeleteDB(productID);
                if (resultDB)
                {
                    productDataGrid.ItemsSource = a.GetProductData();
                    productDataGrid.Items.Refresh();
                }


            }

        }


        private void ChkAll_Checked(object sender, RoutedEventArgs e)

        {

            //Console.WriteLine("pp");    

        }

        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
           /* productDataGrid.CurrentCell = new DataGridCellInfo(
            productDataGrid.Items[productDataGrid.SelectedIndex], productDataGrid.Columns[0]);
            productDataGrid.BeginEdit();*/

        }
        private void ChkAll_Unchecked(object sender, RoutedEventArgs e)

        {

            //Console.WriteLine("pWWp");

        }

        private void OutBoundButton_Click(object sender, RoutedEventArgs e)
        {
            ServerClass a = new ServerClass();
            OutBoundInsertDialog OutBoundInsert = new OutBoundInsertDialog();
            OutBoundInsert.Closing += OutBoundUpdate_Closing;

            //Console.WriteLine("DS");
                OutBoundInsert.OutBoundProductdataGrid.ItemsSource = a.GetBoundProductData();
                OutBoundInsert.OutBoundProductdataGrid.Items.Refresh();
                List<WorkerComboboxClass> ssss = a.WorkerData();

                foreach (WorkerComboboxClass data in ssss)
                {
                    OutBoundInsert.WorkerComboBox.Items.Add(data);
                }

                 OutBoundInsert.OutBoundID.Content = randomNum();

            OutBoundInsert.ShowDialog();

        }

        private string randomNum()
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());

            List<int> listLinq = new List<int>(Enumerable.Range(1, 10));
            listLinq = listLinq.OrderBy(num => rand.Next()).ToList<int>();
            string result="";
            for (int i = 0; i < 10; i++)
            {
                result = result + listLinq[i].ToString();
            }
            return result;
        }

        private void UpdateBoundButton_Click(object sender, RoutedEventArgs e)
        {
            ServerClass a = new ServerClass();
            if (tabControl1.SelectedIndex == 0)
            {
                OutBoundUpdateDialog OutBoundUpdate = new OutBoundUpdateDialog();
                OutBoundUpdate.Closing += OutBoundUpdate_Closing;
                foreach (OutBoundClass OClass in outboundDataGrid.ItemsSource)
                {
                    if (OClass.check)
                    {
                        //Console.WriteLine("DS");
                        OutBoundUpdate.OutBoundProductdataGrid.ItemsSource = a.outBoundDetailsList(OClass.id);
                        OutBoundUpdate.OutBoundProductdataGrid.Items.Refresh();
                        OutBoundUpdate.OutBoundID.Content = OClass.id;
                        List<WorkerComboboxClass> ssss = a.WorkerData();
                        int i = 0;
                        foreach (WorkerComboboxClass data in ssss)
                        {
                            OutBoundUpdate.workerComboBox.Items.Add(data);
                            if (OClass.workerName == data.name)
                            {
                                OutBoundUpdate.workerComboBox.SelectedIndex = i;
                            }
                            i++;
                        }
                        OutBoundUpdate.ShowDialog();
                        break;
                    }
                }
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                InBoundUpdateDialog InBoundUpdate = new InBoundUpdateDialog();
                InBoundUpdate.Closing += InBoundUpdate_Closing;
                foreach (InBoundClass IClass in inboundDataGrid.ItemsSource)
                {
                    if (IClass.check)
                    {
                        //Console.WriteLine("DS");
                        InBoundUpdate.InBoundProductdataGrid.ItemsSource = a.inBoundDetailsList(IClass.id);
                        InBoundUpdate.InBoundProductdataGrid.Items.Refresh();
                        InBoundUpdate.InBoundID.Content = IClass.id;
                        List<WorkerComboboxClass> ssss = a.WorkerData();
                        int i = 0;
                        foreach (WorkerComboboxClass data in ssss)
                        {
                            InBoundUpdate.workerComboBox.Items.Add(data);
                            if (IClass.workerName == data.name)
                            {
                                InBoundUpdate.workerComboBox.SelectedIndex = i;
                            }
                            i++;
                        }
                        InBoundUpdate.ShowDialog();
                        break;
                    }
                }
            }
            
        }

        private void OutBoundUpdate_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Console.WriteLine("IDDDDDDDDDDDDDDDDDDDDDDDDD");
            ServerClass a = new ServerClass();
            outboundDataGrid.ItemsSource = a.GetOutBoundData();
            outboundDataGrid.Items.Refresh();
            // throw new NotImplementedException();
        }

        private void InBoundUpdate_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Console.WriteLine("IDDDDDDDDDDDDDDDDDDDDDDDDD");
            ServerClass a = new ServerClass();
            inboundDataGrid.ItemsSource = a.GetInBoundData();
            inboundDataGrid.Items.Refresh();
            // throw new NotImplementedException();
        }

        private void OutChk_Checked(object sender, RoutedEventArgs e)
        {
               /* int a = 1;
                for (int i = 0; i < outboundDataGrid.Items.Count; i++)
                {
                if (i == outboundDataGrid.SelectedIndex)
                {
                    DataGridRow row = (DataGridRow)outboundDataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                    CheckBox checkBox = FindChild<CheckBox>(row, "Chk");
                    checkBox.IsChecked = true;

                }
                else
                {
                    DataGridRow row = (DataGridRow)outboundDataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                    CheckBox checkBox = FindChild<CheckBox>(row, "Chk");
                    checkBox.IsChecked = false;

                }

                }*/
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent is valid.  
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = Media.VisualTreeHelper.GetChild(parent, i);
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

        private void image_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Image ig = sender as System.Windows.Controls.Image;

            if (ig != null)
            {
                if (tabControl1.SelectedIndex == 0)
                {
                    ImageDialog IDialog = new ImageDialog();
                    int no = 0;
                    foreach (OutBoundClass OClass in outboundDataGrid.ItemsSource)
                    {
                        if (no == outboundDataGrid.SelectedIndex)
                        {
                            IDialog.Id = OClass.id;
                            IDialog.action = "OutBound";
                            IDialog.ErrorTime.Content = OClass.updateTime;
                            IDialog.ReturnWorkerLabel.Content = OClass.workerName;
                            IDialog.Closing += IOutBoundDialog_Closing;
                            IDialog.Show();
                        }
                        no++;
                    }

                    
                }
                else if (tabControl1.SelectedIndex == 1)
                {
                    ImageDialog IDialog = new ImageDialog();
                    int no = 0;
                    foreach (InBoundClass IClass in inboundDataGrid.ItemsSource)
                    {
                        if (no == inboundDataGrid.SelectedIndex)
                        {
                            IDialog.Id = IClass.id;
                            IDialog.action = "InBound";
                            IDialog.ErrorTime.Content = IClass.updateTime;
                            IDialog.ReturnWorkerLabel.Content = IClass.workerName;
                            IDialog.Closing += IInBoundDialog_Closing;
                            IDialog.Show();
                        }
                        no++;
                    }

                    
                }
            }

        }

        private void IOutBoundDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ServerClass a = new ServerClass();
            outboundDataGrid.ItemsSource=a.GetOutBoundData();
            outboundDataGrid.Items.Refresh();
        }

        private void IInBoundDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ServerClass a = new ServerClass();
            inboundDataGrid.ItemsSource = a.GetInBoundData();
            inboundDataGrid.Items.Refresh();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                string filterText = BoundSearchTextBox.Text;
                ICollectionView cv = CollectionViewSource.GetDefaultView(outboundDataGrid.ItemsSource);

                if (!string.IsNullOrEmpty(filterText))
                {
                    cv.Filter = o =>
                    {
                        /* change to get data row value */
                        OutBoundClass p = o as OutBoundClass;
                        return (p.id.ToString().ToUpper().StartsWith(filterText.ToUpper()));
                        /* end change to get data row value */
                    };
                }
                else
                {
                    cv.Filter = o =>
                    {
                        /* change to get data row value */
                        OutBoundClass p = o as OutBoundClass;
                        return (p.id.ToString().ToUpper().StartsWith(""));
                        /* end change to get data row value */
                    };
                }
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                string filterText = BoundSearchTextBox.Text;
                ICollectionView cv = CollectionViewSource.GetDefaultView(inboundDataGrid.ItemsSource);

                if (!string.IsNullOrEmpty(filterText))
                {
                    cv.Filter = o =>
                    {
                        /* change to get data row value */
                        InBoundClass p = o as InBoundClass;
                        return (p.id.ToString().ToUpper().StartsWith(filterText.ToUpper()));
                        /* end change to get data row value */
                    };
                }
                else
                {
                    cv.Filter = o =>
                    {
                        /* change to get data row value */
                        InBoundClass p = o as InBoundClass;
                        return (p.id.ToString().ToUpper().StartsWith(""));
                        /* end change to get data row value */

                    };
                }
            }
        }

        private void TabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BoundSearchTextBox.Text = null;
            if (tabControl1.SelectedIndex == 0)
            {
                ICollectionView cv = CollectionViewSource.GetDefaultView(outboundDataGrid.ItemsSource);
                cv.Filter = o =>
                {
                    /* change to get data row value */
                    OutBoundClass p = o as OutBoundClass;
                    return (p.id.ToString().ToUpper().StartsWith(""));
                    /* end change to get data row value */

                };
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                ICollectionView cv = CollectionViewSource.GetDefaultView(inboundDataGrid.ItemsSource);
                cv.Filter = o =>
                {
                    /* change to get data row value */
                    InBoundClass p = o as InBoundClass;
                    return (p.id.ToString().ToUpper().StartsWith(""));
                    /* end change to get data row value */

                };
            }


        }

        private void InBoundButton_Click(object sender, RoutedEventArgs e)
        {
            ServerClass a = new ServerClass();
            InBoundInsertDialog InBoundInsert = new InBoundInsertDialog();
            InBoundInsert.Closing += InBoundUpdate_Closing;


            //Console.WriteLine("DS");
            InBoundInsert.InBoundProductdataGrid.ItemsSource = a.GetBoundProductData();
            InBoundInsert.InBoundProductdataGrid.Items.Refresh();
            List<WorkerComboboxClass> ssss = a.WorkerData();

            foreach (WorkerComboboxClass data in ssss)
            {
                InBoundInsert.WorkerComboBox.Items.Add(data);
            }

            InBoundInsert.InBoundID.Content = randomNum();

            InBoundInsert.ShowDialog();
        }

        private void DeleteBoundButton_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {

                ServerClass a = new ServerClass();
                List<string> OutBoundIDList = new List<string>();

                foreach (OutBoundClass OClass in outboundDataGrid.ItemsSource)
                {
                    if (OClass.check)
                    {
                        OutBoundIDList.Add(OClass.id);
                    }
                }
                
                bool result = a.outBoundDeleteDB(OutBoundIDList);
                if (result)
                {
                    outboundDataGrid.ItemsSource = a.GetOutBoundData();
                    outboundDataGrid.Items.Refresh();
                }
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                ServerClass a = new ServerClass();
                List<string> InBoundIDList = new List<string>();

                foreach (InBoundClass IClass in inboundDataGrid.ItemsSource)
                {
                    if (IClass.check)
                    {
                        InBoundIDList.Add(IClass.id);
                    }
                }

                bool result = a.inBoundDeleteDB(InBoundIDList);

                if (result)
                {
                    inboundDataGrid.ItemsSource = a.GetInBoundData();
                    inboundDataGrid.Items.Refresh();
                }
            }
        }

        private void ExportAndDeleteButton_Click(object sender, RoutedEventArgs e)
        {

            List<string> BoundIDList = new List<string>();
            if (tabControl1.SelectedIndex == 0)
            {

                ServerClass a = new ServerClass();
                

                foreach (OutBoundClass OClass in outboundDataGrid.ItemsSource)
                {
                    if (OClass.check)
                    {
                        BoundIDList.Add(OClass.id);
                    }
                }

                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "Csv|*.csv";
                saveFileDialog.Title = "Save an Csv File";
                saveFileDialog.FileName = "importData";
                // //Console.WriteLine(AdListTimely.Max().AdData.Count);
                saveFileDialog.RestoreDirectory = true;

                Nullable<bool> result = saveFileDialog.ShowDialog();
                if (result == true)
                {
                    StreamWriter csvStreamWriter = new StreamWriter(saveFileDialog.FileName, true, System.Text.Encoding.UTF8);

                    //output header data
                    List<OutBoundExportListClass> ExportData = a.outBoundExportDB(BoundIDList);

                    string delimiter = ",";
                    string strHeader = "";
                    string strOrder = "";
                    int maxCount = 0;
                    strHeader += "" + delimiter;
                    strOrder += "" + delimiter;
                    for (int i = 0; i < ExportData.Count; i++)
                    {


                        strHeader += "ID" + delimiter + "時間" + delimiter + "派遣員" + delimiter + "備註" + delimiter + "狀態" + delimiter;
                        strHeader += "" + delimiter + "" + delimiter;


                        strOrder += ExportData[i].id + delimiter + ExportData[i].datetime + delimiter + ExportData[i].workerName + delimiter + ExportData[i].orderRemarks + delimiter + ExportData[i].orderState + delimiter;
                        strOrder += "" + delimiter + "" + delimiter;

                        if (maxCount < ExportData[i].ExportClass.Count)
                        {
                            maxCount = ExportData[i].ExportClass.Count;
                        }
                    }
                    csvStreamWriter.WriteLine(strHeader);
                    csvStreamWriter.WriteLine(strOrder);
                    csvStreamWriter.WriteLine();

                    for (int i = 0; i < maxCount; i++)
                    {

                        string strDetail = "";
                        strDetail += "" + delimiter;
                        string strDetailHeader = "";
                        strDetailHeader += "" + delimiter;
                        for (int x = 0; x < ExportData.Count; x++)
                        {
                            if (i == 0)
                            {

                                strDetailHeader += "品名" + delimiter + "ESLID" + delimiter + "明細狀態" + delimiter + "備註" + delimiter + "數量" + delimiter;
                                strDetailHeader += "" + delimiter + "" + delimiter;

                            }
                            if (i < ExportData[x].ExportClass.Count)
                            {
                                strDetail += ExportData[x].ExportClass[i].productName + delimiter + ExportData[x].ExportClass[i].eslID + delimiter + ExportData[x].ExportClass[i].detailState + delimiter + ExportData[x].ExportClass[i].detailRemarks + delimiter + ExportData[x].ExportClass[i].amount + delimiter;
                                strDetail += "" + delimiter + "" + delimiter;
                            }

                        }

                        if (i == 0)
                        {
                            csvStreamWriter.WriteLine(strDetailHeader);
                        }

                        csvStreamWriter.WriteLine(strDetail);

                    }

                    csvStreamWriter.Close();

                    outboundDataGrid.ItemsSource = a.GetOutBoundData();

                }

            }
            else if (tabControl1.SelectedIndex == 1)
            {
                ServerClass a = new ServerClass();


                foreach (InBoundClass IClass in inboundDataGrid.ItemsSource)
                {
                    if (IClass.check)
                    {
                        BoundIDList.Add(IClass.id);
                    }
                }

                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "Csv|*.csv";
                saveFileDialog.Title = "Save an Csv File";
                saveFileDialog.FileName = "importData";
                // //Console.WriteLine(AdListTimely.Max().AdData.Count);
                saveFileDialog.RestoreDirectory = true;

                Nullable<bool> result = saveFileDialog.ShowDialog();
                if (result == true)
                {
                    StreamWriter csvStreamWriter = new StreamWriter(saveFileDialog.FileName, true, System.Text.Encoding.UTF8);

                    //output header data
                    List<InBoundExportListClass> ExportData = a.inBoundExportDB(BoundIDList);

                    string delimiter = ",";
                    string strHeader = "";
                    string strOrder = "";
                    int maxCount = 0;
                    strHeader += "" + delimiter;
                    strOrder += "" + delimiter;
                    for (int i = 0; i < ExportData.Count; i++)
                    {


                        strHeader += "ID" + delimiter + "時間" + delimiter + "派遣員" + delimiter + "備註" + delimiter + "狀態" + delimiter;
                        strHeader += "" + delimiter + "" + delimiter;


                        strOrder += ExportData[i].id + delimiter + ExportData[i].datetime + delimiter + ExportData[i].workerName + delimiter + ExportData[i].orderRemarks + delimiter + ExportData[i].orderState + delimiter;
                        strOrder += "" + delimiter + "" + delimiter;

                        if (maxCount < ExportData[i].ExportClass.Count)
                        {
                            maxCount = ExportData[i].ExportClass.Count;
                        }
                    }
                    csvStreamWriter.WriteLine(strHeader);
                    csvStreamWriter.WriteLine(strOrder);
                    csvStreamWriter.WriteLine();

                    for (int i = 0; i < maxCount; i++)
                    {

                        string strDetail = "";
                        strDetail += "" + delimiter;
                        string strDetailHeader = "";
                        strDetailHeader += "" + delimiter;
                        for (int x = 0; x < ExportData.Count; x++)
                        {
                            if (i == 0)
                            {

                                strDetailHeader += "品名" + delimiter + "ESLID" + delimiter + "明細狀態" + delimiter + "備註" + delimiter + "數量" + delimiter;
                                strDetailHeader += "" + delimiter + "" + delimiter;

                            }
                            if (i < ExportData[x].ExportClass.Count) {
                                strDetail += ExportData[x].ExportClass[i].productName + delimiter + ExportData[x].ExportClass[i].eslID + delimiter + ExportData[x].ExportClass[i].detailState + delimiter + ExportData[x].ExportClass[i].detailRemarks + delimiter + ExportData[x].ExportClass[i].amount + delimiter;
                                strDetail += "" + delimiter + "" + delimiter;
                            }
                        }

                        if (i == 0)
                        {
                            csvStreamWriter.WriteLine(strDetailHeader);
                        }

                        csvStreamWriter.WriteLine(strDetail);

                    }

                    csvStreamWriter.Close();

                    inboundDataGrid.ItemsSource = a.GetInBoundData();

                }
            }
            

        }

        private void ExportSetButton_Click(object sender, RoutedEventArgs e)
        {
            AutoExportSetDialog AutoExportSet = new AutoExportSetDialog();

            string tempPath = System.IO.Path.GetTempPath();
            bool fi = File.Exists(tempPath + "EslTemp.tmp");
            if (fi)
            {
                StreamReader TempStreamReader = new StreamReader(tempPath + "EslTemp.tmp");
                String line = TempStreamReader.ReadToEnd();
                string[] words = line.Split('\n');
                int selectIndex = 0;
                bool isCustom = true;
                foreach (ComboBoxItem CmbItem in AutoExportSet.ExportDayCombobox.Items)
                {
                    if (CmbItem.Content.ToString() == words[0].Trim())
                    {
                        AutoExportSet.ExportDayCombobox.SelectedIndex = selectIndex;
                        isCustom = false;
                    }
                    selectIndex++;
                }

                if (isCustom)
                {
                    AutoExportSet.ExportDayCombobox.SelectedIndex = 6;
                    AutoExportSet.CustomDay.Text = words[0].Trim();
                }

                AutoExportSet.ExportPathTextbox.Text = words[1];
                TempStreamReader.Close();
            }
            AutoExportSet.ShowDialog();


        }

        private void AutoExportCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)(autoExportCheckBox.IsChecked))
                ExportDataTimer.Start();
            else
                ExportDataTimer.Stop();
        }

        private void WorkerInsertButton_Click(object sender, RoutedEventArgs e)
        {
            WorkerInsertDialog WorkerDialog = new WorkerInsertDialog();
            WorkerDialog.Closing += WorkerDialog_Closing;
            WorkerDialog.ShowDialog();
        }

        private void WorkerDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ServerClass a = new ServerClass();
            workerDataGrid.ItemsSource = a.GetWorkerData();
            workerDataGrid.Items.Refresh();
            // throw new NotImplementedException();
        }

        private void WorkerDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("確定刪除會員?", "會員刪除", MessageBoxButton.YesNo);

            List<string> peopleID = new List<string>();
            if (result == MessageBoxResult.Yes)
            {

                foreach (WorkerClass WClass in workerDataGrid.ItemsSource)
                {
                    if (WClass.check)
                    {
                        peopleID.Add(WClass.id);
                    }
                }

                ServerClass a = new ServerClass();
                bool resultDB = a.workerDeleteDB(peopleID);
                if (resultDB)
                {
                    workerDataGrid.ItemsSource = a.GetWorkerData();
                    workerDataGrid.Items.Refresh();
                }


            }
        }

        private void BarcodeTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.A && e.Key <= Key.Z) || e.Key > Key.D0 || e.Key < Key.D9)
            {
                ////Console.WriteLine("ININKeyPress" + e.KeyChar);
            }
            else
            {
                BarcodeState.Content = "输入法请切换英文。";
                BarcodeState.Foreground = Media.Brushes.Red;
                return;
            }

            if (e.Key == Key.Enter)
            {

                CheckSyntaxAndReport();
            }
        }

        string dataTemp = "";
        private void CheckSyntaxAndReport()
        {
            
            string decoded = BarcodeTextBox.Text.ToUpper();
            if (decoded.Length >= 12)
            {


                decimal number3 = 0;
                Boolean canConvert = decimal.TryParse(decoded, out number3);
                //Console.WriteLine("canConvert  :"+ canConvert);

                if (canConvert)
                {
                    int rowIndex = 0;
                    foreach (OutBoundProductClass PClass in productDataGrid.ItemsSource)
                    {
                        if (PClass.id == decoded)
                        {
                            if (dataTemp == "")
                            {
                                //Console.WriteLine("INININININ");
                                dataTemp = decoded;
;                               BarcodeState.Content = PClass.name + "掃描成功";
                                BarcodeTextBox.Text = "";
                                return;
                            }
                            decimal number2 = 0;
                            Boolean canConvertDataTemp = decimal.TryParse(dataTemp, out number2);
                            if (canConvertDataTemp)
                            {
                                dataTemp = PClass.id;
                                BarcodeState.Content = PClass.name + "掃描成功";
                            }
                            else
                            {
                                ServerClass a = new ServerClass();
                                DataGridCell cell = GetCell(rowIndex, 1, productDataGrid);
                                if (!a.eslEmptyCheck(cell.Content.ToString()))
                                {
                                     BarcodeTextBox.Text = "";
                                    MessageBox.Show("Esl Null");
                                    return;
                                }

                                PClass.eslID = dataTemp;
                                BarcodeState.Content = "配對成功";
                                

                                cell.Foreground = Media.Brushes.Green;
                                cell.Content = dataTemp;
                                cell.Focus();

                                /*productDataGrid.CurrentCell = new DataGridCellInfo(
                                productDataGrid.Items[rowIndex], productDataGrid.Columns[1]);
                                productDataGrid.BeginEdit();
                                */
                                BarcodeTextBox.Focus();
                                dataTemp = "";
                            }

                        }
                        rowIndex++;
                    }
                }
                else
                {

                    if (dataTemp == "")
                    {
                        dataTemp = decoded;
                        BarcodeState.Content = decoded + "掃描成功";
                        BarcodeTextBox.Text = "";
                        return;
                    }

                    decimal number2 = 0;
                    Boolean canConvertDataTemp = decimal.TryParse(dataTemp, out number2);
                    if (canConvertDataTemp)
                    {
                        int rowIndex = 0;
                        ServerClass a = new ServerClass();
                        DataGridCell cell = GetCell(rowIndex, 1, productDataGrid);
                        if (!a.eslEmptyCheck(cell.Content.ToString()))
                        {
                            BarcodeTextBox.Text = "";
                            MessageBox.Show("Esl Null");

                            return;
                        }
                        foreach (OutBoundProductClass PClass in productDataGrid.ItemsSource)
                        {
                            if (PClass.id == dataTemp)
                            { 
                                PClass.eslID = decoded;
                                BarcodeState.Content = "配對成功";
                                dataTemp = "";

                                

                                cell.Foreground = Media.Brushes.Green;

                                productDataGrid.CurrentCell = new DataGridCellInfo(
                                productDataGrid.Items[rowIndex], productDataGrid.Columns[1]);
                                productDataGrid.BeginEdit();

                                BarcodeTextBox.Focus();
                                // productDataGrid.Items.Refresh();
                                break;
                            }
                            rowIndex++;
                        }




                    }
                    else
                    {
                        dataTemp = decoded;
                        BarcodeState.Content = decoded + "掃描成功";
                    }
                }

            }

            BarcodeTextBox.Text = "";
        }


        public static T GetVisualChild<T>(Media.Visual parent) where T : Media.Visual
        {
            T child = default(T);
            int numVisuals = Media.VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < numVisuals; i++)
            {
                Media.Visual v = (Media.Visual)Media.VisualTreeHelper.GetChild(parent, i);
                child = v as T;

                if (child == null)
                    child = GetVisualChild<T>(v);
                else
                    break;
            }

            return child;
        }



        public DataGridCell GetCell(int rowIndex, int columnIndex, DataGrid dg)
        {
            DataGridRow row = dg.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            DataGridCellsPresenter p = GetVisualChild<DataGridCellsPresenter>(row);
            DataGridCell cell = p.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
            return cell;
        }

        private void APLinkButton_Click(object sender, RoutedEventArgs e)
        {
            APList.Clear();
            tbMessageBox.Text = "";
           // AP_ListBox.Items.Clear();
            Tools tool = new Tools();
            tool.onApScanEvent += new EventHandler(AP_Scan);
            tool.SNC_GetAP_Info();
        }

        private void AP_Scan(object sender, EventArgs e)
        {
            APScanDataInvoker stc = new APScanDataInvoker(ApScanReceiveData);
            Dispatcher.BeginInvoke(stc, e);
        }

        private void ApScanReceiveData(EventArgs e)
        {
            List<AP_Information> AP = (e as Tools.ApScanEventArgs).data;
            ClearSocket();
            Socket client = null;
            List<APClass> InsertAPList = new List<APClass>();
            foreach (AP_Information mAP_Information in AP)
            {
                tbMessageBox.AppendText("IP = " + mAP_Information.AP_IP + " Mac = " + mAP_Information.AP_MAC_Address + " Name = " + mAP_Information.AP_Name);
                tbMessageBox.AppendText("\n");

                APClass aClass = new APClass();
                aClass.APIP = mAP_Information.AP_IP;
                aClass.name = mAP_Information.AP_Name;
                InsertAPList.Add(aClass);
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  // TCP
                                                                                                       //client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); // UDP
                IPAddress ipAddress = IPAddress.Parse(mAP_Information.AP_IP);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 1200);
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                /*   AP_ListBox.Items.Add(mAP_Information.AP_IP);
                   AP_ListBox.SelectedIndex = 0;
                   AP_IP_Label.Text = AP_ListBox.SelectedItem.ToString();*/
            }

            ServerClass a = new ServerClass();
            a.InsertAP(InsertAPList);
            APList = a.GetAPData();


        }

        private void ClearSocket()
        {
            foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
            {
                kvp.Value.mSmcEsl = null;
                kvp.Value.workSocket.Close();
                kvp.Value.workSocket = null;
            }
            mDictSocket.Clear();
        }


        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;
                // Complete the connection.
                client.EndConnect(ar);
                SmcEsl aSmcEsl = new SmcEsl(client);
                aSmcEsl.onSMCEslReceiveEvent += new EventHandler(SMCEslReceiveEvent); //全資料回傳

                EslObject mEslObject = new EslObject();
                mEslObject.workSocket = client;
                mEslObject.mSmcEsl = aSmcEsl;
                mDictSocket.Add(client.RemoteEndPoint.ToString(), mEslObject);

                // aSmcEsl.stopScanBleDevice();
                aSmcEsl.DisConnectBleDevice();

                this.Dispatcher.Invoke((Action)(() =>
                {
                    tbMessageBox.Text = client.RemoteEndPoint.ToString() + "  連線成功 \n";
                }));
              
                // connectDone.Set();
            }
            catch (Exception e)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    tbMessageBox.Text = "AP 連線失敗，請檢查網路設定是否正確 \n";
                }));
                
            }
        }

        private void SMCEslReceiveEvent(object sender, EventArgs e)
        {
            ReceiveDataInvoker stc = new ReceiveDataInvoker(ReceiveData);
            Dispatcher.BeginInvoke(stc, e);
        }

        private void ReceiveData(EventArgs e)
        {
            int msgId = (e as SmcEsl.SMCEslReceiveEventArgs).msgId;
            bool status = (e as SmcEsl.SMCEslReceiveEventArgs).status;
            string deviceIP = (e as SmcEsl.SMCEslReceiveEventArgs).apIP;
            string data = (e as SmcEsl.SMCEslReceiveEventArgs).data;

            double battery = (e as SmcEsl.SMCEslReceiveEventArgs).battery;

            string str_data = "";
            //Console.WriteLine("BACKs");

            //掃描
            if (msgId == SmcEsl.msg_ScanDevice)
            {
                ScanUI(data, deviceIP, battery);
            }

            // 藍牙連線
            else if (msgId == SmcEsl.msg_ConnectEslDevice)
            {
                if (status)
                {
                    str_data = "連線成功";
                    //    tbMessageBox.SelectionColor = Color.FromArgb(60, 119, 119);
                    ConnectTimeOutTimer.Stop();
                    if (productDataListUpdate.Count > productIndex)
                    {
                        if (productDataListUpdate[productIndex].actionName == "pUpdate")
                        {
                           // Bitmap bmp = mElectronicPriceData.setPage1(productDataListUpdate[productIndex].bleAddress);
                            Bitmap bmp = mElectronicPriceData.setProductPage2(productDataListUpdate[productIndex]);
                            image.Source = BitmapToImageSource(bmp);
                            // mSmcEsl.TransformImageToData(bmp);
                            // mSmcEsl.WriteESLDataWithBle();
                            foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                            {
                                if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                                {
                                    kvp.Value.mSmcEsl.TransformImageToData(bmp);
                                    kvp.Value.mSmcEsl.WriteESLDataWithBle();
                                }
                            }
                        }
                        else if (productDataListUpdate[productIndex].actionName == "light")
                        {
                            foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                            {
                                if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                                {
                                    if (productDataListUpdate[productIndex].lightStatus == 1)
                                        kvp.Value.mSmcEsl.WriteLEDRData(1);
                                    else
                                        kvp.Value.mSmcEsl.CloseDRData();
                                }
                            }
                        }

                    }

                    
                }
                else
                {
                    str_data = "連線失敗";
                 
                }
            }
            // 藍牙斷線
            else if (msgId == SmcEsl.msg_DisconnectEslDevice)
            {
             
                if (status)
                {
                    str_data = "斷線成功";
                    //Console.WriteLine("WTF" + productDataListUpdate.Count +"  and  "+ productIndex);
                    if (productDataListUpdate.Count > productIndex)
                    {


                        foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                        {
                            if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                            {
                                kvp.Value.mSmcEsl.ConnectBleDevice(productDataListUpdate[productIndex].bleAddress);
                                ConnectTimeOutTimer.Start();
                            }
                        }
                        str_data = str_data +  "\n" + productDataListUpdate[productIndex].bleAddress + "嘗試連線中";
                    }
                    else
                    {
                        //MessageBox.Show("更新結束");
                        productIndex = 0;
                        ServerClass a = new ServerClass();
                        productDataGrid.ItemsSource = a.GetProductData();
                        productDataGrid.Items.Refresh();
                        eslDataGrid.ItemsSource = a.GetEslData();
                        eslDataGrid.Items.Refresh();
                        productDataListUpdate.Clear();
                       // EslLightCheckTimer.Start();
                    }
                }
                else
                {

                    str_data = "斷線失敗";

                }
            }
            // 取得藍牙名稱
            else if (msgId == SmcEsl.msg_ReadEslName)
            {
                str_data = "Device Name : " + data;
            }
            // 寫入設備名稱
            else if (msgId == SmcEsl.msg_WriteEslName)
            {
                if (status)
                {
                    str_data = "Esl 名稱更新成功";
                }
                else
                {
                    str_data = "Esl 名稱更新失敗";
                }
            }
            // 寫入ESL資料
            else if (msgId == SmcEsl.msg_WriteEslData)
            {
                if (status)
                {
                    //    //Console.WriteLine("資料寫入成功");
                    str_data = "資料寫入成功";
                }
                else
                {
                    str_data = "資料寫入失敗";
                }
            }
            // 寫入ESL資料，全部寫完
            else if (msgId == SmcEsl.msg_WriteEslDataFinish)
            {

              
                str_data = "全部資料寫入成功";
                
                foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                {
                    if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                    {
                        kvp.Value.mSmcEsl.DisConnectBleDevice();
                    }
                }
                ServerClass a = new ServerClass();
                a.productUpdateDB(productDataListUpdate[productIndex].barcode, productDataListUpdate[productIndex].bleAddress, productDataListUpdate[productIndex].product_name, productDataListUpdate[productIndex].price, productDataListUpdate[productIndex].shelf, productDataListUpdate[productIndex].stock, productDataListUpdate[productIndex].remarks);
                //Console.WriteLine("NO " + productDataListUpdate[productIndex].no);
                productDataListUpdate[productIndex].updateState = "更新完成";
                productIndex++;

            }
            // 寫入AP Beacon Data
            else if (msgId == SmcEsl.msg_WriteBeacon)
            {
                if (status)
                {
                    str_data = "Beacon更新成功";
                    // tbBeaconCount
                   

                }
                else
                {
                    str_data = "Beacon更新失敗";
                }
            }

            // ---------  ESL  版本 -------
            else if (msgId == SmcEsl.msg_ReadEslVersion)
            {
                if (status)
                {
                    str_data = "ESL 版本 = " + data;
                }
                else
                {
                    str_data = "ESL 版本讀取錯誤";
                }
            }
            // ---------  ESL  電壓 -------
            else if (msgId == SmcEsl.msg_ReadEslBattery)
            {
                if (status)
                {
                    str_data = "Esl電池電壓 = " + data + " V";
                }
                else
                {
                    str_data = "Esl電池電壓讀取失敗";
                }
            }
            // ---------  ESL  製造資料 -------
            else if (msgId == SmcEsl.msg_ReadManufactureData)
            {
                if (status)
                {
                    str_data = "製造資料 = " + data;
                }
                else
                {
                    str_data = "製造資料讀取錯誤";
                }
            }
            // ---------  ESL  版本 -------
            else if (msgId == SmcEsl.msg_WriteManufactureData)
            {
                if (status)
                {
                    str_data = "ESL 版本寫入成功";
                }
                else
                {
                    str_data = "ESL 版本寫入失敗";
                }
            }
            else if (msgId == SmcEsl.msg_SetLED_ESL)
            {
                if (status)
                {
                    str_data = "ESL LED設定成功";
                    foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                    {
                        if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                        {
                            kvp.Value.mSmcEsl.DisConnectBleDevice();
                        }
                    }
                    ServerClass a = new ServerClass();
                    a.eslLightComplete(productDataListUpdate[productIndex].bleAddress);
                    productDataListUpdate[productIndex].updateState = "更新完成";
                    productIndex++;
                }
                else
                {
                    str_data = "ESL LED設定失敗";
                }
            }

            // ---------  AP  寫入Buffer -------
            else if (msgId == SmcEsl.msg_WriteESLDataBuffer)
            {
                if (status)
                {

                        str_data = "寫入 AP Buffer 完成";
                }
                else
                {
                    str_data = "寫入 AP Buffer 失敗";
                }
            }

            // ---------  AP  更新ESL -------
            else if (msgId == SmcEsl.msg_UpdataESLDataFromBuffer)
            {
                //

                if (status)
                {
                   

                }
                else
                {

                        str_data = "AP 更新 ESL 失敗";

                }



            }
            // ---------  AP  設定時間 -------
            else if (msgId == SmcEsl.msg_SetRTCTime)
            {
                if (status)
                {
                    str_data = "AP 設定時間 完成";
                }
                else
                {
                    str_data = "AP 設定時間 失敗";
                }
            }
            // ---------  AP  取得時間 -------
            else if (msgId == SmcEsl.msg_GetRTCTime)
            {

                string yy = data.Substring(0, 2);
                string MM = data.Substring(2, 2);
                string dd = data.Substring(4, 2);
                string ww = data.Substring(6, 2);
                string HH = data.Substring(8, 2);
                string mm = data.Substring(10, 2);
                string ss = data.Substring(12, 2);
                str_data = yy + "/" + MM + "/" + dd + "  星期:" + ww + "  " + HH + ":" + mm + ":" + ss;

            }
            // ---------  AP  Beacon Time  -------
            else if (msgId == SmcEsl.msg_SetBeaconTime)
            {
                if (status)
                {
                    str_data = "Beacon 設定時間 完成";
                }
                else
                {
                    str_data = "Beacon 設定時間 失敗";
                }
            }

            //----------- 2018/03/23
            // ---------  AP  設定時間 -------
            else if (msgId == SmcEsl.msg_SetCustomerID_AP)
            {
                if (status)
                {
                    str_data = "AP 客戶ID設定 成功";
                }
                else
                {
                    str_data = "AP 客戶ID設定 失敗";
                }
            }
            else if (msgId == SmcEsl.msg_SetCustomerID_ESL)
            {
                if (status)
                {
                    str_data = "ESL 客戶ID設定 成功";
                }
                else
                {
                    str_data = "ESL 客戶ID設定 失敗";
                }
            }
            else if (msgId == SmcEsl.msg_ReadEslType)
            {
                if (data.Substring(2, 2).Equals("00"))
                {
                    str_data = "2.13吋";
                }
                else if (data.Substring(2, 2).Equals("01"))
                {
                    str_data = "2.9吋";
                }
                else if (data.Substring(2, 2).Equals("02"))
                {
                    str_data = "4.2吋";
                }
            }
            // 寫入ESL資料
            else if (msgId == SmcEsl.msg_WriteEslData2)
            {
                if (status)
                {
                    //    //Console.WriteLine("資料寫入成功");
                  
                    str_data = "資料寫入成功";
                }
                else
                {
                    str_data = "資料寫入失敗";
                }
            }
            // 寫入ESL資料，全部寫完
            else if (msgId == SmcEsl.msg_WriteEslDataFinish2)
            {
    
                str_data = "全部資料寫入成功";
               
            }



            if (str_data.Equals("斷線成功") || str_data.Equals("斷線失敗") || str_data.Equals("連線失敗") || str_data.Contains("AP 更新 ESL 失敗"))
            {
                

            }
            else if (str_data.Equals("連線成功"))
            {
               
            }
            else
            {
              
            }



            if (str_data.Equals("資料寫入成功") || msgId == SmcEsl.msg_ScanDevice)
            {

            }
            else
            {
                //Console.WriteLine("IIIIIIIIIIIO");
                tbMessageBox.AppendText(Environment.NewLine + deviceIP + "  Time = " + DateTime.Now.ToString("HH:mm:ss") + " =>" + str_data);
                tbMessageBox.SelectionStart = tbMessageBox.Text.Length;
            }



        }


        private void ScanUI(string data, string deviceIP, double battery)
        {
            string RssiS = "";
            string sizes = "0";
            //Console.WriteLine("Count:"+data.Length+"  data: "+data);

            if (data.Length == 14)
            {
                ////Console.WriteLine("ddddddddd");
                RssiS = data.Substring(data.Length - 2, 2);
                data = data.Substring(0, data.Length - 2);
            }
            else if (data.Length == 16)
            {
                ////Console.WriteLine("dddddfafaf");
                RssiS = data.Substring(data.Length - 4, 2);
                sizes = data.Substring(data.Length - 2, 2);
                data = data.Substring(0, 12);
            }
            else
            {
                ////Console.WriteLine("dddddfafaf");
                RssiS = data.Substring(data.Length - 4, 2);
                sizes = data.Substring(data.Length - 2, 2);
                data = data.Substring(0, 12);

            }
            //Console.WriteLine("RSSI"+RssiS);


            if (resetAPStatus)
            {
                bool isNull = false;
                foreach (EslClass EslData in resetAPList)
                {
                    if (EslData.id == data)
                    {
                        isNull = true;
                        foreach (APClass aClass in APList)
                        {
                            if (deviceIP.Contains(aClass.APIP))
                            {
                                EslData.APId = aClass.id.ToString();
                            }
                        }

                        EslData.battery = battery;
                        EslData.rssi = (int)Convert.ToByte(RssiS, 16);
                        break;
                    }
                }
                if (!isNull)
                {
                    EslClass scanData = new EslClass();
                    foreach (APClass aClass in APList)
                    {
                        if (deviceIP.Contains(aClass.APIP))
                        {
                            scanData.APId = aClass.id.ToString();
                        }
                    }

                    scanData.battery = battery;
                    scanData.id = data;
                    scanData.lightStatus = 0;
                    scanData.rssi = (int)Convert.ToByte(RssiS, 16);
                    ResetEslScanTimer.Stop();
                    ResetEslScanTimer.Start();
                    resetAPList.Add(scanData);
                }

            }
            else if (dtectionAPStatus)
            {
                int no = 0;
                foreach (EslClass EClass in eslDataGrid.ItemsSource) {
                    if (EClass.id == data)
                    {
                        Console.WriteLine(data+"ddds   "+ (int)Convert.ToByte(RssiS, 16));
                        EClass.rssi = (int)Convert.ToByte(RssiS, 16);
                        DataGridCell cell = GetCell(no,5, eslDataGrid);
                        cell.Content = (int)Convert.ToByte(RssiS, 16);
                    }
                    no++;
                }
            }
        }

        private void ESLUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            //ConnectBleTimeOut.Stop();
            //ScanTimer.Stop();
            // ScanBleButton.Text = "Start Scan Ble";
            // ScanBleButton.ForeColor = Color.Black;
            // scan = false;
            int rowIndex = 0;
            productDataListUpdate.Clear();
            

            foreach (OutBoundProductClass PClass in productDataGrid.ItemsSource)
            {
                for (int i = 0; i < productDataGrid.Columns.Count; i++)
                {
                    //Console.WriteLine(GetCell(rowIndex, i, productDataGrid).Foreground +"   "+ rowIndex + "  "+i);
                    if (GetCell(rowIndex, i, productDataGrid).Foreground == Media.Brushes.Green)
                    {
                        Page1 productData = new Page1();
                        productData.no = rowIndex;
                        foreach (EslClass eClass in eslDataGrid.ItemsSource)
                        {
                            if (eClass.id == PClass.eslID)
                            {
                                productData.apLink = eClass.APId;
                            }
                        }
                        
                        productData.bleAddress = PClass.eslID;

                        if (productData.apLink == "" || productData.bleAddress == "")
                        {
                            if (productData.bleAddress == "")
                            {
                                MessageBox.Show(PClass.name+"未綁定ESL");
                                return;
                            }
                            if (productData.apLink == "")
                            {
                                MessageBox.Show(productData.bleAddress + "AP未配對");
                                return;
                            }
                        }
                        productData.barcode = PClass.id;
                        productData.product_name = PClass.name;
                        productData.price = PClass.price;
                        productData.stock = PClass.inStocknow;
                        productData.remarks = PClass.remarks;
                        productData.shelf = PClass.shelf;
                        productData.actionName = "pUpdate";
                        productDataListUpdate.Add(productData);


                        if (productData.apLink==null)
                        {
                            MessageBox.Show(productData.bleAddress+"未配對AP");
                            ServerClass a = new ServerClass();
                            productDataGrid.ItemsSource = a.GetProductData();
                            productDataGrid.Items.Refresh();
                            return;
                        }

                        break;

                    }

                }
                rowIndex++;
            }

            if (productDataListUpdate.Count != 0)
            {
                productIndex = 0;
                foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                {
                 //   if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                  //  {
                        kvp.Value.mSmcEsl.stopScanBleDevice();
                   //  }
                }

                Thread.Sleep(100);
                foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                {
                    if (kvp.Key.Contains(productDataListUpdate[productIndex].apLink))
                    {
                        kvp.Value.mSmcEsl.ConnectBleDevice(productDataListUpdate[productIndex].bleAddress);
                        ConnectTimeOutTimer.Start();
                    }
                }
                tbMessageBox.Text = productDataListUpdate[productIndex].bleAddress + "  嘗試連線中請稍候... \r\n";
            }
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void ESLButton_Click(object sender, RoutedEventArgs e)
        {
            APGrid.Visibility = Visibility.Collapsed;
            ESLGrid.Visibility = Visibility.Visible;
        }

        private void APButton_Click(object sender, RoutedEventArgs e)
        {
            APGrid.Visibility = Visibility.Visible;
            ESLGrid.Visibility = Visibility.Collapsed;
        }

        private void ResetESLAPLinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (resetESLAPLinkButton.Content.ToString() == "重製配對")
            {
                foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                {
                    kvp.Value.mSmcEsl.startScanBleDevice();
                }
                resetAPList.Clear();
                reEslId.Clear();
                ResetEslScanTimer.Start();
                resetAPStatus = true;
                foreach (EslClass EClass in eslDataGrid.ItemsSource)
                {
                    EClass.battery = 0;
                    EClass.rssi = 0;
                    EClass.APId = "";
                    resetAPList.Add(EClass);
                    reEslId.Add(EClass.id);
                }
                resetESLAPLinkButton.Foreground = Media.Brushes.Red;
                resetESLAPLinkButton.Content = "停止配對";
            }
            else
            {
                foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                {
                    kvp.Value.mSmcEsl.stopScanBleDevice();
                }
                ResetEslScanTimer.Stop();
                resetAPStatus = false;

                resetESLAPLinkButton.Foreground = Media.Brushes.Black;
                resetESLAPLinkButton.Content = "重製配對";

                EslResetClass EslReset = new EslResetClass();
                EslReset.eslId = new List<string>();
                EslReset.eslData = new List<EslClass>();
                EslReset.eslId = reEslId;
                EslReset.eslData = resetAPList;
                ServerClass a = new ServerClass();
                a.EslResetAP(EslReset);
                eslDataGrid.ItemsSource = a.GetEslData();
                eslDataGrid.Items.Refresh();
            }


        }

        private void ProductDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //Console.WriteLine("ProductDataGrid_CellEditEnding");



            if (e.EditAction == DataGridEditAction.Commit)
            {
                /*  if (e.Column.DisplayIndex == 1)
                  {
                      ServerClass a = new ServerClass();
                      int no = 0;
                      foreach (OutBoundProductClass PClass in productDataGrid.ItemsSource)
                      {
                          //Console.WriteLine("no " + no + "  productDataGrid.SelectedIndex:" + e.Row.GetIndex());
                          if (no == e.Row.GetIndex())
                          {
                              bool  eslIsNull = a.eslEmptyCheck(PClass.eslID);
                              if (!eslIsNull)
                              {
                                  PClass.eslID = "";
                                  MessageBox.Show("Esl   Null");
                                  return;
                              }
                              break;
                          }
                          no++;
                      }
                  }*/


                //Console.WriteLine("e.Column.DisplayIndex " + e.Column.DisplayIndex + "  e.Row.GetIndex():" + e.Row.GetIndex());
                DataGridCell cell = GetCell(e.Row.GetIndex(), e.Column.DisplayIndex, productDataGrid);
                /*   if (e.Column.DisplayIndex == 1)
                   {
                       ServerClass a = new ServerClass();

                       int no = 0;

                       foreach (OutBoundProductClass PClass in productDataGrid.ItemsSource)
                       {
                           if (no == e.Row.GetIndex())
                           {
                               bool eslIsNull = a.eslEmptyCheck(PClass.eslID);
                               if (!eslIsNull)
                               {
                                   PClass.eslID = null;
                                   cell.Content = DBNull.Value;
                                   MessageBox.Show("Esl   Null");
                               }
                               else
                               {
                                   cell.Foreground = Media.Brushes.Green;
                               }
                               break;
                           }
                           no++;
                       }
                   }
                   else
                   {
                       cell.Foreground = Media.Brushes.Green;
                   }*/
                cell.Foreground = Media.Brushes.Green;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            socket.Disconnect();    
        }

        private void ProductDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Page1 Page = new Page1();
            int no = 0;
            foreach (OutBoundProductClass PClass in productDataGrid.ItemsSource)
            {
                //Console.WriteLine("no "+ no+ "  productDataGrid.SelectedIndex:"+ productDataGrid.SelectedIndex);
                if (no == productDataGrid.SelectedIndex)
                {
                    Page.bleAddress = PClass.eslID;
                    Page.barcode = PClass.id;
                    Page.product_name = PClass.name;
                    Page.shelf = PClass.shelf;
                    break;
                }
                no++;
            }
            if (Page.barcode != null)
            {
                //Console.WriteLine("barcode");
                Bitmap bmp = mElectronicPriceData.setProductPage2(Page);
                image.Source = BitmapToImageSource(bmp);
            }

        }

        private void Produt_Search_Click(object sender, RoutedEventArgs e)
        {
            string filterText = searchTextBox.Text;
            ICollectionView cv = CollectionViewSource.GetDefaultView(productDataGrid.ItemsSource);

            if (!string.IsNullOrEmpty(filterText))
            {
                cv.Filter = o =>
                {
                    /* change to get data row value */
                    OutBoundProductClass p = o as OutBoundProductClass;
                    return (p.name.ToUpper().StartsWith(filterText.ToUpper()));
                    /* end change to get data row value */
                };
            }
            else
            {
                cv.Filter = o =>
                {
                    /* change to get data row value */
                    OutBoundProductClass p = o as OutBoundProductClass;
                    return (p.name.ToUpper().StartsWith(""));
                    /* end change to get data row value */
                };
            }
        }


        private void OrderDataGrid(string col, DataGrid dg)
        {
            ICollectionView v = CollectionViewSource.GetDefaultView(dg.ItemsSource);
            v.SortDescriptions.Clear();
            ListSortDirection d = new ListSortDirection();
            if (dg.ColumnFromDisplayIndex(0).SortDirection == ListSortDirection.Ascending)
                d = ListSortDirection.Descending;
            else
                d = ListSortDirection.Ascending;

            v.SortDescriptions.Add(new SortDescription(col, d));
            v.Refresh();
            dg.ColumnFromDisplayIndex(0).SortDirection = d;
        }
        private void OrderWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                OrderDataGrid("workerName", outboundDataGrid);
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                OrderDataGrid("workerName", inboundDataGrid);
            }

        }

        private void OrderIDButton_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                OrderDataGrid("id", outboundDataGrid);
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                OrderDataGrid("id", inboundDataGrid);
            }
        }

        private void TbMessageBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbMessageBox.ScrollToEnd();
        }

        private void ESLDetection_Click(object sender, RoutedEventArgs e)
        {
            if (ESLDetection.Content.ToString() == "檢測")
            {
                foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                {
                    kvp.Value.mSmcEsl.startScanBleDevice();

                }
                dtectionAPStatus = true;
                ESLDetection.Foreground = Media.Brushes.Red;
                ESLDetection.Content = "停止檢測";
            }
            else
            {
                foreach (KeyValuePair<string, EslObject> kvp in mDictSocket)
                {
                    kvp.Value.mSmcEsl.stopScanBleDevice();
                }
                dtectionAPStatus = false;

                ESLDetection.Foreground = Media.Brushes.Black;
                ESLDetection.Content = "檢測";

            }
        }
    }
}


