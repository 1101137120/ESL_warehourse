
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Controls;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media.Imaging;

namespace ESL_WPF
{
    class ServerClass
    {
        public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 总是接受  
            return true;
        }

        public bool Login(string username, string password)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/Login");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string postBody = "username=" + username + "&password=" + password ;
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    
                    dynamic result = JsonConvert.DeserializeObject(responseStr);
                    //Console.WriteLine("responseStr" + result.status);
                    if (result.status == "success")
                        return true;
                }

            }
            return false;
        }

        public DataTable useHttpWebRequest_Get(string IP,string tableName)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(IP);
            request.Method = WebRequestMethods.Http.Get;
            request.ContentType = "application/json";
            DataTable DataTable = new DataTable();
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var temp = reader.ReadToEnd();
                        dynamic product = JsonConvert.DeserializeObject(temp);

                        //Console.WriteLine("respone");

                        if (tableName == "product")
                        {
                            DataGridCheckBoxColumn checkbox = new DataGridCheckBoxColumn();
                            DataTable.Columns.Add(new DataColumn("選取", typeof(bool)));
                            DataTable.Columns.Add("ESLID", typeof(string));
                            DataTable.Columns.Add("貨物料號條碼", typeof(string));
                            DataTable.Columns.Add("品名", typeof(string));
                            DataTable.Columns.Add("價格", typeof(int));
                            DataTable.Columns.Add("貨價區域", typeof(string));
                            DataTable.Columns.Add("庫存數量", typeof(int));
                            DataTable.Columns.Add("備註", typeof(string));
                            //Console.WriteLine("respone"+ temp);
                            for (int i = 0; i < product.response.Count; i++)
                            {
                                DataTable.Rows.Add(false, product.response[i].eslID, product.response[i].id, product.response[i].name, product.response[i].price, product.response[i].shelf, product.response[i].inStocknow, product.response[i].remarks);
                            }
                        }
                        else if (tableName == "outboundproduct")
                        {
                         
                         /*   DataGridCheckBoxColumn checkbox = new DataGridCheckBoxColumn();
                            DataTable.Columns.Add("ESLID", typeof(string));
                            DataTable.Columns.Add("貨物料號條碼", typeof(string));
                            DataTable.Columns.Add("品名", typeof(string));
                            DataTable.Columns.Add(new DataColumn("選取", typeof(bool)));
                            DataTable.Columns.Add("貨價區域", typeof(string));
                            DataTable.Columns.Add("庫存數量", typeof(int));
                            DataTable.Columns.Add("取貨", typeof(string));
                            DataTable.Columns.Add("備註", typeof(string));*/
                            //Console.WriteLine("respone" + temp);
                            for (int i = 0; i < product.response.Count; i++)
                            {
                                DataTable.Rows.Add(product.response[i].eslID, product.response[i].id, product.response[i].name, false , product.response[i].shelf, product.response[i].inStocknow,"", product.response[i].remarks);
                            }
                        }
                        else if(tableName == "worker")
                        {
                            DataTable.Columns.Add("id", typeof(int));
                            DataTable.Columns.Add("收發人員", typeof(string));
                            DataTable.Columns.Add("手機號碼", typeof(string));
                            DataTable.Columns.Add("email", typeof(string));
                            DataTable.Columns.Add("帳號", typeof(string));
                            DataTable.Columns.Add("密碼", typeof(string));
                            DataTable.Columns.Add("level", typeof(int));
                           

                            for (int i = 0; i < product.response.Count; i++)
                            {
                                DataTable.Rows.Add(product.response[i].id, product.response[i].name, product.response[i].phone, product.response[i].email, product.response[i].userName, product.response[i].password, product.response[i].level);
                            }
                        }
                        else if (tableName == "inbound")
                        {
                            DataTable.Columns.Add(new DataColumn("Selected", typeof(bool)));
                            DataTable.Columns.Add("派單單號", typeof(int));
                            DataTable.Columns.Add("派單時間", typeof(string));
                            DataTable.Columns.Add("收發人員", typeof(string));
                            DataTable.Columns.Add("貨物品項數量", typeof(string));
                            DataTable.Columns.Add("收發完成時間", typeof(string));
                            DataTable.Columns.Add("備註", typeof(string));



                            for (int i = 0; i < product.response.Count; i++)
                            {
                                string boundState = "";
                                if (product.response[i].state == 0)
                                {
                                    boundState = "未完成";
                                }
                                else if (product.response[i].state == 1)
                                {
                                    boundState = "處理中";
                                }
                                else if (product.response[i].state == 2)
                                {
                                    boundState = product.response[i].completeTime;
                                }
                                DataTable.Rows.Add(false,product.response[i].id, product.response[i].date + product.response[i].time, product.response[i].workerName, product.response[i].detailCount, boundState, product.response[i].remarks);
                            }
                        }
                        else if (tableName == "outbound")
                        {
                            DataTable.Columns.Add(new DataColumn("Selected", typeof(bool)));
                            DataTable.Columns.Add("派單單號", typeof(int));
                            DataTable.Columns.Add("派單時間", typeof(string));
                            DataTable.Columns.Add("收發人員", typeof(string));
                            DataTable.Columns.Add("貨物品項數量", typeof(string));
                            DataTable.Columns.Add("收發完成時間", typeof(string));
                            DataTable.Columns.Add("備註", typeof(string));



                            for (int i = 0; i < product.response.Count; i++)
                            {
                                string boundState = "";
                                if (product.response[i].state == 0)
                                {
                                    boundState = "未完成";
                                }
                                else if (product.response[i].state == 1)
                                {
                                    boundState = "處理中";
                                }
                                else if (product.response[i].state == 2)
                                {
                                    boundState = product.response[i].completeTime;
                                }
                                DataTable.Rows.Add(false,product.response[i].id, product.response[i].date + product.response[i].time, product.response[i].workerName, product.response[i].detailCount, boundState, product.response[i].remarks);
                            }
                        }
                    }

                  
                }
                else
                {
                   // this.dataGridView1.DataSource = null;
                }
                return DataTable;
            }
        }

        public List<OutBoundProductClass> GetProductData() {
            dynamic productData =  HttpRequestGet("https://api.ihoin.com/warehouse/Product");

            List<OutBoundProductClass> ProductList = new List<OutBoundProductClass>();
            for (int i = 0; i < productData.response.Count; i++)
            {
                OutBoundProductClass PClass = new OutBoundProductClass();
                PClass.eslID = productData.response[i].eslID;
                PClass.id = productData.response[i].id;
                PClass.name = productData.response[i].name;
                PClass.price = productData.response[i].price;
                PClass.shelf = productData.response[i].shelf;
                PClass.inStocknow = productData.response[i].inStocknow;
                PClass.check = false;
                PClass.get = "";
                PClass.remarks = productData.response[i].remarks;
                PClass.id = productData.response[i].id;

                ProductList.Add(PClass);
            }
            return ProductList;
        }

        public List<OutBoundProductClass> GetBoundProductData()
        {
            dynamic productData = HttpRequestGet("https://api.ihoin.com/warehouse/BoundProduct");

            List<OutBoundProductClass> ProductList = new List<OutBoundProductClass>();
            for (int i = 0; i < productData.response.Count; i++)
            {
                OutBoundProductClass PClass = new OutBoundProductClass();
                PClass.eslID = productData.response[i].eslID;
                PClass.id = productData.response[i].id;
                PClass.name = productData.response[i].name;
                PClass.price = productData.response[i].price;
                PClass.shelf = productData.response[i].shelf;
                PClass.inStocknow = productData.response[i].inStocknow;
                PClass.check = false;
                PClass.get = "";
                PClass.remarks = productData.response[i].remarks;
                PClass.id = productData.response[i].id;

                ProductList.Add(PClass);
            }
            return ProductList;
        }

        public List<OutBoundClass> GetOutBoundData()
        {
            dynamic outBoundData = HttpRequestGet("https://api.ihoin.com/warehouse/OutBoundOrder");

            List<OutBoundClass> OutBoundList = new List<OutBoundClass>();
            string str = System.IO.Directory.GetCurrentDirectory();
            BitmapImage errorImage = new BitmapImage(new Uri(str + "\\error.png"));
            for (int i = 0; i < outBoundData.response.Count; i++)
            {
                OutBoundClass OClass = new OutBoundClass();
                string boundState = "";

                if (outBoundData.response[i].state == 0)
                {
                    boundState = "錯誤";
                    OClass.image = errorImage;
                }
                else if (outBoundData.response[i].state == 1)
                {
                    boundState = "處理中";
                }
                else if (outBoundData.response[i].state == 2)
                {
                    boundState = string.Format("{0:G}", outBoundData.response[i].completeTime);
                    //Console.WriteLine("completetime  "+outBoundData.response[i].completeTime);
                }
                OClass.id = outBoundData.response[i].id;
                OClass.datetime = outBoundData.response[i].datetime;
                OClass.workerName = outBoundData.response[i].workerName;
                OClass.detailCount = outBoundData.response[i].detailCount;
                OClass.check = false;
                OClass.boundState =boundState;
                OClass.remarks = outBoundData.response[i].remarks;
                OClass.updateTime = outBoundData.response[i].updateTime;

                OutBoundList.Add(OClass);
            }
            return OutBoundList;
        }



        public List<InBoundClass> GetInBoundData()
        {
            dynamic InBoundData = HttpRequestGet("https://api.ihoin.com/warehouse/InBoundOrder");

            List<InBoundClass> InBoundList = new List<InBoundClass>();
            string str = System.IO.Directory.GetCurrentDirectory();
            BitmapImage errorImage = new BitmapImage(new Uri(str + "\\error.png"));

            for (int i = 0; i < InBoundData.response.Count; i++)
            {
                InBoundClass IClass = new InBoundClass();
                string boundState = "";
                if (InBoundData.response[i].state == 0)
                {
                    boundState = "錯誤";
                    IClass.image = errorImage;
                }
                else if (InBoundData.response[i].state == 1)
                {
                    boundState = "處理中";
                }
                else if (InBoundData.response[i].state == 2)
                {
                    boundState = string.Format("{0:G}", InBoundData.response[i].completeTime);
                    boundState = InBoundData.response[i].completeTime;
                }
                IClass.id = InBoundData.response[i].id;
                IClass.datetime = InBoundData.response[i].datetime;
                IClass.workerName = InBoundData.response[i].workerName;
                IClass.detailCount = InBoundData.response[i].detailCount;
                IClass.boundState = boundState;
                IClass.remarks = InBoundData.response[i].remarks;
                IClass.updateTime = InBoundData.response[i].updateTime;
                InBoundList.Add(IClass);
            }
            return InBoundList;
        }

        public List<WorkerClass> GetWorkerData()
        {
            dynamic workerData = HttpRequestGet("https://api.ihoin.com/warehouse/Worker");

            List<WorkerClass> WorkerList = new List<WorkerClass>();
            for (int i = 0; i < workerData.response.Count; i++)
            {
                WorkerClass WClass = new WorkerClass();
                WClass.id = workerData.response[i].id;
                WClass.name = workerData.response[i].name;
                WClass.phone = workerData.response[i].phone;
                WClass.check = false;
                WClass.email = workerData.response[i].email;
                WClass.userName = workerData.response[i].userName;
                WClass.password = workerData.response[i].password;
                WClass.level = workerData.response[i].mangeLevel;
                WClass.remarks = workerData.response[i].remarks;
                WorkerList.Add(WClass);
            }
            return WorkerList;
        }

        public List<EslClass> GetEslData()
        {
            dynamic eslData = HttpRequestGet("https://api.ihoin.com/warehouse/EslData");

            List<EslClass> EslList = new List<EslClass>();
            for (int i = 0; i < eslData.response.Count; i++)
            {
                EslClass EClass = new EslClass();
                EClass.id = eslData.response[i].id;
                EClass.APId = eslData.response[i].APIP;
                EClass.lightStatus = eslData.response[i].lightStatus;
                EClass.productID = eslData.response[i].product_id;
                EClass.remarks = eslData.response[i].remarks;
                EClass.check = false;
                EslList.Add(EClass);
            }
            return EslList;
        }

        public List<APClass> GetAPData()
        {
            dynamic APData = HttpRequestGet("https://api.ihoin.com/warehouse/APData");

            List<APClass> APList = new List<APClass>();
            for (int i = 0; i < APData.response.Count; i++)
            {
                APClass AClass = new APClass();
                AClass.id = APData.response[i].id;
                AClass.APIP = APData.response[i].ip;
                AClass.name = APData.response[i].Name;
                AClass.status = APData.response[i].status;
                AClass.check = false;
                APList.Add(AClass);
            }
            return APList;
        }



        public dynamic  HttpRequestGet(string IP)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(IP);
            request.Method = WebRequestMethods.Http.Get;
            request.ContentType = "application/json";
            DataTable DataTable = new DataTable();

            dynamic result;
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var temp = reader.ReadToEnd();
                        result = JsonConvert.DeserializeObject(temp);

                        //Console.WriteLine("respone "+ result);

                    }
                }
                else
                {
                    // this.dataGridView1.DataSource = null;
                    result = null;
                }
                return result;
            }
        }

        public List<WorkerComboboxClass> WorkerData() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/Worker");
            request.Method = WebRequestMethods.Http.Get;
            request.ContentType = "application/json";
            DataTable DataTable = new DataTable();
            List<WorkerComboboxClass> WorkerClass = new List<WorkerComboboxClass>();
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var temp = reader.ReadToEnd();
                        dynamic result = JsonConvert.DeserializeObject(temp);

                        for (int i = 0; i < result.response.Count; i++)
                        {
                            WorkerComboboxClass data = new WorkerComboboxClass();
                            data.id = result.response[i].id;
                            data.name = result.response[i].name;
                            WorkerClass.Add(data);
                        }
                            return WorkerClass;
                        //Console.WriteLine("respone");
                    }


                }
                else
                {
                    // this.dataGridView1.DataSource = null;

                    return WorkerClass;
                }
                
            }
        }

        public bool productInsertDB(string ID, string name, int price, string shelf, int stock, string remark) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/ProductInsert");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string postBody = "ID="+ ID + "&name="+name+"&price="+price+"&shelf="+shelf+"&stock="+stock+"&remark="+remark+"";
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr"+ responseStr);
                    dynamic result = JsonConvert.DeserializeObject(responseStr);
                    if (result.status == "success")
                        return true;
                }

            }
            return false;
        }


        public bool workerInsertDB(string name, string phone, string email, string acount, string password, string level, string remark)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/WorkerInsert");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string postBody = "name=" + name + "&phone=" + phone + "&email=" + email + "&acount=" + acount + "&pw=" + password + "&level=" + level + "&remark=" + remark + "";
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic result = JsonConvert.DeserializeObject(responseStr);
                    if (result.status == "success")
                        return true;
                }

            }
            return false;
        }


        public dynamic OutBoundEslCheck(List<OutBoundDetailClass> detail)
        {
            dynamic result = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/EslAPCheck");
            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 3000;
                var serializer = new JavaScriptSerializer();
                string postBody = serializer.Serialize(detail);
                //Console.WriteLine("gqqq " + postBody);
                byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(postBody);
                    streamWriter.Flush();
                }
                /*  using (Stream reqStream = request.GetRequestStream())
                  {
                      reqStream.Write(byteArray, 0, byteArray.Length);
                  }*/
                //發出Request
                string responseStr = "";
                using (WebResponse response = request.GetResponse())
                {

                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        responseStr = reader.ReadToEnd();
                        //Console.WriteLine("responseStr" + responseStr);
                        result = JsonConvert.DeserializeObject(responseStr);
                    }

                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("ERROR  " + ex);
            }

            return result;
        }

        public dynamic InBoundEslCheck(List<InBoundDetailClass> detail)
        {
            dynamic result = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/EslAPCheck");
            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 3000;
                var serializer = new JavaScriptSerializer();
                string postBody = serializer.Serialize(detail);
                //Console.WriteLine("gqqq " + postBody);
                byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(postBody);
                    streamWriter.Flush();
                }
                /*  using (Stream reqStream = request.GetRequestStream())
                  {
                      reqStream.Write(byteArray, 0, byteArray.Length);
                  }*/
                //發出Request
                string responseStr = "";
                
                using (WebResponse response = request.GetResponse())
                {

                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        responseStr = reader.ReadToEnd();
                        //Console.WriteLine("responseStr" + responseStr);
                        result = JsonConvert.DeserializeObject(responseStr);
                    }

                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("ERROR  " + ex);
            }

            return result;
        }

        public bool eslEmptyCheck(string id)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/EslEmptyCheck");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string postBody = "eslId=" + id.ToString();
         
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic result = JsonConvert.DeserializeObject(responseStr);
                    if (result.status == "success")
                        return true;
                }

            }
            return false;
        }
        public bool InsertOutBound(OutBoundOrderClass OOrder)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/OutBoundDetailsInsert");
            try {
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 3000;
                var serializer = new JavaScriptSerializer();
                string postBody = serializer.Serialize(OOrder);
                //Console.WriteLine("gqqq "+ postBody);
                byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(postBody);
                    streamWriter.Flush();
                }
                /*  using (Stream reqStream = request.GetRequestStream())
                  {
                      reqStream.Write(byteArray, 0, byteArray.Length);
                  }*/
                //發出Request
                string responseStr = "";
                using (WebResponse response = request.GetResponse())
                {

                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        responseStr = reader.ReadToEnd();
                        //Console.WriteLine("responseStr" + responseStr);
                        dynamic result = JsonConvert.DeserializeObject(responseStr);
                        if (result.status == "success")
                            return true;
                    }

                }
            }
            catch (Exception ex) {
                //Console.WriteLine("ERROR  "+ex);
            }
            
            return false;
        }

        public bool InsertInBound(InBoundOrderClass IOrder)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/InBoundDetailsInsert");
            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 3000;
                var serializer = new JavaScriptSerializer();
                string postBody = serializer.Serialize(IOrder);
                //Console.WriteLine("gqqq " + postBody);
                byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(postBody);
                    streamWriter.Flush();
                }
                /*  using (Stream reqStream = request.GetRequestStream())
                  {
                      reqStream.Write(byteArray, 0, byteArray.Length);
                  }*/
                //發出Request
                string responseStr = "";
                using (WebResponse response = request.GetResponse())
                {

                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        responseStr = reader.ReadToEnd();
                        //Console.WriteLine("responseStr" + responseStr);
                        dynamic result = JsonConvert.DeserializeObject(responseStr);
                        if (result.status == "success")
                            return true;
                    }

                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("ERROR  " + ex);
            }

            return false;
        }

        public bool InsertAP(List<APClass> ap)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/APDataInsert");
            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 3000;
                var serializer = new JavaScriptSerializer();
                string postBody = serializer.Serialize(ap);
                //Console.WriteLine("gqqq " + postBody);
                byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(postBody);
                    streamWriter.Flush();
                }
                /*  using (Stream reqStream = request.GetRequestStream())
                  {
                      reqStream.Write(byteArray, 0, byteArray.Length);
                  }*/
                //發出Request
                string responseStr = "";
                using (WebResponse response = request.GetResponse())
                {

                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        responseStr = reader.ReadToEnd();
                        //Console.WriteLine("responseStr" + responseStr);
                        dynamic result = JsonConvert.DeserializeObject(responseStr);
                        if (result.status == "success")
                            return true;
                    }

                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("ERROR  " + ex);
            }

            return false;
        }

        public bool EslResetAP(EslResetClass ResetClass)
        {
            //Console.WriteLine("ResetClass"+ ResetClass.eslData.Count);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/EslResetAP");
            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 3000;
                var serializer = new JavaScriptSerializer();
                string postBody = serializer.Serialize(ResetClass);
                //Console.WriteLine("gqqq " + postBody);
                byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(postBody);
                    streamWriter.Flush();
                }
                /*  using (Stream reqStream = request.GetRequestStream())
                  {
                      reqStream.Write(byteArray, 0, byteArray.Length);
                  }*/
                //發出Request
                string responseStr = "";
                using (WebResponse response = request.GetResponse())
                {

                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        responseStr = reader.ReadToEnd();
                        //Console.WriteLine("responseStr" + responseStr);
                        dynamic result = JsonConvert.DeserializeObject(responseStr);
                        if (result.status == "success")
                            return true;
                    }

                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("ERROR  " + ex);
            }

            return false;
        }

        public bool productUpdateDB(string productID, string eslData, string productName, int productPrice, string productShelf, int stock, string remark)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/ProductUpdate");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string postBody = "productID=" + productID + "&eslData=" + eslData + "&productName=" + productName + "&productPrice=" + productPrice + "&productShelf=" + productShelf + "&stock=" + stock + "&remark=" + remark + "";
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic result = JsonConvert.DeserializeObject(responseStr);
                    if (result.status == "success")
                        return true;
                }

            }
            return false;
        }

        public bool UpdateOutBound(OutBoundOrderClass OOrder)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/OutBoundDetailsUpdate");
            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 3000;
                var serializer = new JavaScriptSerializer();
                string postBody = serializer.Serialize(OOrder);
                //Console.WriteLine("gqqq " + postBody);
                byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(postBody);
                    streamWriter.Flush();
                }
                //發出Request
                string responseStr = "";
                using (WebResponse response = request.GetResponse())
                {

                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        responseStr = reader.ReadToEnd();
                        //Console.WriteLine("responseStr" + responseStr);
                        dynamic result = JsonConvert.DeserializeObject(responseStr);
                        if (result.status == "success")
                            return true;
                    }

                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("ERROR  " + ex);
            }

            return false;
        }

        public bool UpdateInBound(InBoundOrderClass IOrder)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/InBoundDetailsUpdate");
            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 3000;
                var serializer = new JavaScriptSerializer();
                string postBody = serializer.Serialize(IOrder);
                //Console.WriteLine("gqqq " + postBody);
                byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(postBody);
                    streamWriter.Flush();
                }
                //發出Request
                string responseStr = "";
                using (WebResponse response = request.GetResponse())
                {

                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        responseStr = reader.ReadToEnd();
                        //Console.WriteLine("responseStr" + responseStr);
                        dynamic result = JsonConvert.DeserializeObject(responseStr);
                        if (result.status == "success")
                            return true;
                    }

                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("ERROR  " + ex);
            }

            return false;
        }

        public bool productDeleteDB(List<string> ID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/ProductDelete");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string iddata = "";
            foreach (string idstring in ID)
            {
                if (iddata == "")
                    iddata = idstring;
                else
                    iddata = iddata + "," + idstring;

                //Console.WriteLine("iddata" + iddata + "idstring  "+ idstring);

            }
            //Console.WriteLine("iddata" + iddata);
            string postBody = "productID=["+ iddata + "]";
            //Console.WriteLine("postBody" + postBody);
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic result = JsonConvert.DeserializeObject(responseStr);
                    if (result.status == "success")
                        return true;
                }

            }
            return false;
        }


        public bool workerDeleteDB(List<string> ID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/WorkerDelete");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string iddata = "";
            foreach (string idstring in ID)
            {
                if (iddata == "")
                    iddata = idstring;
                else
                    iddata = iddata + "," + idstring;

                //Console.WriteLine("iddata" + iddata + "idstring  " + idstring);

            }
            //Console.WriteLine("iddata" + iddata);
            string postBody = "peopleID=[" + iddata + "]";
            //Console.WriteLine("postBody" + postBody);
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic result = JsonConvert.DeserializeObject(responseStr);
                    if (result.status == "success")
                        return true;
                }

            }
            return false;
        }


        public bool outBoundDeleteDB(List<string> ID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/OutBoundOrderDelete");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString

            string iddata = "";
            foreach (string idstring in ID)
            {
                if (iddata == "")
                    iddata = idstring;
                else
                    iddata = iddata + "," + idstring;

                //Console.WriteLine("iddata" + iddata + "idstring  " + idstring);

            }
            //Console.WriteLine("iddata" + iddata);
            string postBody = "orderID=[" + iddata + "]&&updateTime="+ DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic result = JsonConvert.DeserializeObject(responseStr);
                    if (result.status == "success")
                        return true;
                }

            }
            return false;
        }


        public List<OutBoundExportListClass> outBoundExportDB(List<string> ID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/OutBoundOrderExport");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString

            string iddata = "";
            foreach (string idstring in ID)
            {
                if (iddata == "")
                    iddata = idstring;
                else
                    iddata = iddata + "," + idstring;

                //Console.WriteLine("iddata" + iddata + "idstring  " + idstring);

            }
            //Console.WriteLine("iddata" + iddata);
            string postBody = "orderID=[" + iddata + "]&&updateTime=" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic exportData = JsonConvert.DeserializeObject(responseStr);
                    List<OutBoundExportListClass> OutBoundExport = new List<OutBoundExportListClass>();
                    for (int i = 0; i < exportData.response.Count; i++)
                    {
                        OutBoundExportListClass ELClass = new OutBoundExportListClass();
                        ELClass.id = exportData.response[i].id;
                        ELClass.datetime = exportData.response[i].datetime;
                        ELClass.workerName = exportData.response[i].workerName;
                        ELClass.orderRemarks = exportData.response[i].orderRemarks;
                        ELClass.orderState = exportData.response[i].orderState;
                        ELClass.ExportClass = new List<OutBoundExportClass>();
                        OutBoundExport.Add(ELClass);
                        for (int a = 0; a < exportData.dresponse.Count; a++)
                        {
                            if(exportData.dresponse[a].outboundId == exportData.response[i].id)
                            {
                                OutBoundExportClass EClass = new OutBoundExportClass();
                                EClass.outboundId = exportData.dresponse[a].outboundId;
                                EClass.detailState = exportData.dresponse[a].detailState;
                                EClass.productName = exportData.dresponse[a].productName;
                                EClass.eslID = exportData.dresponse[a].eslID;
                                EClass.detailRemarks = exportData.dresponse[a].detailRemarks;
                                EClass.amount = exportData.dresponse[a].amount;
                                OutBoundExport[i].ExportClass.Add(EClass);
                            }

                        }

                    }
                    return OutBoundExport;
                }

            }
        }

        public List<InBoundExportListClass> inBoundExportDB(List<string> ID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/InBoundOrderExport");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString

            string iddata = "";
            foreach (string idstring in ID)
            {
                if (iddata == "")
                    iddata = idstring;
                else
                    iddata = iddata + "," + idstring;

                //Console.WriteLine("iddata" + iddata + "idstring  " + idstring);

            }
            //Console.WriteLine("iddata" + iddata);
            string postBody = "orderID=[" + iddata + "]&&updateTime=" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic exportData = JsonConvert.DeserializeObject(responseStr);
                    List<InBoundExportListClass> InBoundExport = new List<InBoundExportListClass>();
                    for (int i = 0; i < exportData.response.Count; i++)
                    {
                        InBoundExportListClass ILClass = new InBoundExportListClass();
                        ILClass.id = exportData.response[i].id;
                        ILClass.datetime = exportData.response[i].datetime;
                        ILClass.workerName = exportData.response[i].workerName;
                        ILClass.orderRemarks = exportData.response[i].orderRemarks;
                        ILClass.orderState = exportData.response[i].orderState;
                        ILClass.ExportClass = new List<InBoundExportClass>();
                        InBoundExport.Add(ILClass);
                        for (int a = 0; a < exportData.dresponse.Count; a++)
                        {
                            if (exportData.dresponse[a].inboundId == exportData.response[i].id)
                            {
                                InBoundExportClass IClass = new InBoundExportClass();
                                IClass.inboundId = exportData.dresponse[a].outboundId;
                                IClass.detailState = exportData.dresponse[a].detailState;
                                IClass.productName = exportData.dresponse[a].productName;
                                IClass.eslID = exportData.dresponse[a].eslID;
                                IClass.detailRemarks = exportData.dresponse[a].detailRemarks;
                                IClass.amount = exportData.dresponse[a].amount;
                                InBoundExport[i].ExportClass.Add(IClass);
                            }

                        }

                    }
                    return InBoundExport;
                }

            }
        }


        public List<InBoundExportListClass> inBoundAutoExportDB(string ExportDay)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/InBoundOrderAutoExport");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString

            string postBody = "ExportDay=" + ExportDay + "&&updateTime=" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic exportData = JsonConvert.DeserializeObject(responseStr);
                    List<InBoundExportListClass> InBoundExport = new List<InBoundExportListClass>();
                    for (int i = 0; i < exportData.response.Count; i++)
                    {
                        InBoundExportListClass ILClass = new InBoundExportListClass();
                        ILClass.id = exportData.response[i].id;
                        ILClass.datetime = exportData.response[i].datetime;
                        ILClass.workerName = exportData.response[i].workerName;
                        ILClass.orderRemarks = exportData.response[i].orderRemarks;
                        ILClass.orderState = exportData.response[i].orderState;
                        ILClass.ExportClass = new List<InBoundExportClass>();
                        InBoundExport.Add(ILClass);
                        for (int a = 0; a < exportData.dresponse.Count; a++)
                        {
                            if (exportData.dresponse[a].inboundId == exportData.response[i].id)
                            {
                                InBoundExportClass IClass = new InBoundExportClass();
                                IClass.inboundId = exportData.dresponse[a].inboundId;
                                IClass.detailState = exportData.dresponse[a].detailState;
                                IClass.productName = exportData.dresponse[a].productName;
                                IClass.eslID = exportData.dresponse[a].eslID;
                                IClass.detailRemarks = exportData.dresponse[a].detailRemarks;
                                IClass.amount = exportData.dresponse[a].amount;
                                InBoundExport[i].ExportClass.Add(IClass);
                            }

                        }

                    }
                    return InBoundExport;
                }

            }
        }


        public List<OutBoundExportListClass> outBoundAutoExportDB(string ExportDay)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/OutBoundOrderAutoExport");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString

            string postBody = "ExportDay=" + ExportDay + "&&updateTime=" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                   // //Console.WriteLine("responseStr" + responseStr);
                    dynamic exportData = JsonConvert.DeserializeObject(responseStr);
                    List<OutBoundExportListClass> OutBoundExport = new List<OutBoundExportListClass>();
                    for (int i = 0; i < exportData.response.Count; i++)
                    {
                        OutBoundExportListClass OLClass = new OutBoundExportListClass();
                        OLClass.id = exportData.response[i].id;
                        OLClass.datetime = exportData.response[i].datetime;
                        OLClass.workerName = exportData.response[i].workerName;
                        OLClass.orderRemarks = exportData.response[i].orderRemarks;
                        OLClass.orderState = exportData.response[i].orderState;
                        OLClass.ExportClass = new List<OutBoundExportClass>();
                        OutBoundExport.Add(OLClass);
                        for (int a = 0; a < exportData.dresponse.Count; a++)
                        {
                            if (exportData.dresponse[a].outboundId == exportData.response[i].id)
                            {
                                OutBoundExportClass OClass = new OutBoundExportClass();
                                OClass.outboundId = exportData.dresponse[a].outboundId;
                                OClass.detailState = exportData.dresponse[a].detailState;
                                OClass.productName = exportData.dresponse[a].productName;
                                OClass.eslID = exportData.dresponse[a].eslID;
                                OClass.detailRemarks = exportData.dresponse[a].detailRemarks;
                                OClass.amount = exportData.dresponse[a].amount;
                                OutBoundExport[i].ExportClass.Add(OClass);
                            }

                        }

                    }
                    return OutBoundExport;
                }

            }
        }
        public bool inBoundDeleteDB(List<string> ID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/InBoundOrderDelete");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string iddata = "";
            foreach (string idstring in ID)
            {
                if (iddata == "")
                    iddata = idstring;
                else
                    iddata = iddata + "," + idstring;

                //Console.WriteLine("iddata" + iddata + "idstring  " + idstring);

            }
            string postBody = "orderID=[" + iddata + "]&&updateTime=" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic result = JsonConvert.DeserializeObject(responseStr);
                    if (result.status == "success")
                        return true;
                }

            }
            return false;
        }

        public  List<OutBoundDetailListClass> outBoundDetailsList(string outBoundID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/OutBoundDetails");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string postBody = "outBoundId=" + outBoundID.ToString();
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    
                    dynamic detailData = JsonConvert.DeserializeObject(responseStr);
                    string str = System.IO.Directory.GetCurrentDirectory();
                    BitmapImage errorImage = new BitmapImage(new Uri(str + "\\error.png"));
                    List<OutBoundDetailListClass> OutBoundDetailList = new List<OutBoundDetailListClass>();
                    for (int i = 0; i < detailData.response.Count; i++)
                    {
                        OutBoundDetailListClass DClass = new OutBoundDetailListClass();
                        DClass.eslID = detailData.response[i].eslID;
                        DClass.proID = detailData.response[i].proID;
                        DClass.name = detailData.response[i].name;
                        DClass.detailID = detailData.response[i].detailID;
                        if (detailData.response[i].state!=null&&detailData.response[i].state == 0)
                        {
                            DClass.image = errorImage;
                        }

                        if (detailData.response[i].state == 2)
                            DClass.state = detailData.response[i].updateTime;
                        else if (detailData.response[i].state == 1)
                            DClass.state = "處理中";
                        else if (detailData.response[i].state == 0)
                            DClass.state = "錯誤";
                        else
                            DClass.state = null;

                        DClass.shelf = detailData.response[i].shelf;
                        DClass.inStocknow = detailData.response[i].inStocknow;
                        DClass.amount = detailData.response[i].amount;
                        if(DClass.amount!=null)
                            DClass.check = true;
                        else
                            DClass.check = false;
                        DClass.detailRemarks = detailData.response[i].detailRemarks;
                        OutBoundDetailList.Add(DClass);
                    }
                    return OutBoundDetailList;
                }

            }
        }

        public List<InBoundDetailListClass> inBoundDetailsList(string inBoundID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/InBoundDetails");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string postBody = "inBoundId=" + inBoundID.ToString();
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic detailData = JsonConvert.DeserializeObject(responseStr);
                    string str = System.IO.Directory.GetCurrentDirectory();
                    BitmapImage errorImage = new BitmapImage(new Uri(str + "\\error.png"));
                    List<InBoundDetailListClass> InBoundDetailList = new List<InBoundDetailListClass>();
                    for (int i = 0; i < detailData.response.Count; i++)
                    {
                        InBoundDetailListClass DClass = new InBoundDetailListClass();
                        DClass.eslID = detailData.response[i].eslID;
                        DClass.proID = detailData.response[i].proID;
                        DClass.name = detailData.response[i].name;
                        DClass.detailID = detailData.response[i].detailID;
                        if (detailData.response[i].state!=null&&detailData.response[i].state == 0)
                        {
                            DClass.image = errorImage;
                        }
                        DClass.shelf = detailData.response[i].shelf;
                        if (detailData.response[i].state == 2)
                            DClass.state = detailData.response[i].updateTime;
                        else if(detailData.response[i].state == 1)
                            DClass.state = "處理中";
                        else if (detailData.response[i].state == 0)
                            DClass.state = "錯誤";
                        else
                            DClass.state = null;

                        DClass.inStocknow = detailData.response[i].inStocknow;
                        DClass.amount = detailData.response[i].amount;
                        if (DClass.amount != null)
                            DClass.check = true;
                        else
                            DClass.check = false;
                        DClass.detailRemarks = detailData.response[i].detailRemarks;
                        InBoundDetailList.Add(DClass);
                    }
                    return InBoundDetailList;
                }

            }
        }


        public void inBoundSolvedState(string inBoundID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/InBoundOrderStatus");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string postBody = "inBoundId=" + inBoundID.ToString();
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic detailData = JsonConvert.DeserializeObject(responseStr);

                }

            }
        }

        public void outBoundSolvedState(string outBoundID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/OutBoundOrderStatus");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string postBody = "outBoundId=" + outBoundID.ToString();
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic detailData = JsonConvert.DeserializeObject(responseStr);

                }

            }
        }


        public bool eslLightONList(List<string> ID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/EslLightUpdate");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string iddata = "";
            foreach (string idstring in ID)
            {
                if (iddata == "")
                    iddata = idstring;
                else
                    iddata = iddata + "," + idstring;

                //Console.WriteLine("iddata" + iddata + "idstring  " + idstring);

            }
            //Console.WriteLine("iddata" + iddata);
            string postBody = "proID=[" + iddata + "]";
            //Console.WriteLine("postBody" + postBody);
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic result = JsonConvert.DeserializeObject(responseStr);
                    if (result.status == "success")
                        return true;
                }

            }
            return false;
        }

        public List<EslClass> GetEslIsUpdateData()
        {
            //Console.WriteLine("GetEslIsUpdateData  ");
            dynamic EslData = HttpRequestGet("https://api.ihoin.com/warehouse/EslIsUpdateCheck");
            //Console.WriteLine("back  "+ EslData);
            List<EslClass> EslList = new List<EslClass>();
            if (EslData.status == "success")
            {
                for (int i = 0; i < EslData.response.Count; i++)
                {
                    EslClass EClass = new EslClass();
                    EClass.id = EslData.response[i].id;
                    EClass.APId = EslData.response[i].APId;
                    EClass.lightStatus = EslData.response[i].lightStatus;
                    EClass.productID = EslData.response[i].product_id;
                    EClass.remarks = EslData.response[i].remarks;
                    if (EslData.response[i].failCount!=null)
                        EClass.count = EslData.response[i].failCount;
                    else
                        EClass.count = 0;
                    EslList.Add(EClass);
                }
            }

            return EslList;
        }



        public bool eslLightComplete(string eslID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/EslLightCompleteUpdate");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string postBody = "eslID=" + eslID;
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic result = JsonConvert.DeserializeObject(responseStr);
                    if (result.status == "success")
                        return true;
                }

            }
            return false;
        }

        public bool eslLightFail(string eslID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ihoin.com/warehouse/EslLightUpdateFail");
            //Console.WriteLine("pppooooooooooo");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 3000;
            //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
            string postBody = "eslID=" + eslID;
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);//要發送的字串轉為byte[]

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            //發出Request
            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                    //Console.WriteLine("responseStr" + responseStr);
                    dynamic result = JsonConvert.DeserializeObject(responseStr);
                    if (result.status == "success")
                        return true;
                }

            }
            return false;
        }

    }
}