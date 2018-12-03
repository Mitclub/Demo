using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_BCEX_Trading
{

    public class CSettings
    {
        #region public Members

        public static DateTime RefundDate;
        public static int RefundDateRange;
        public static double FloatRate;
        public static double RefundPartARate;
        public static double RefundPartBRate;
        public static double BaseRefundRate;
        public static double FloatRateMAX;
        public static double FloatRateMIN;
        public static double RefundRateMAX;
        public static double RefundRateMIN;
        public static string MITAccount;
        public static string MITFromAccount;

        //Test only
        public static double YesterdayTotalProfit;
        public static double TheDayBeforeYesterdayTotalProfit;
        public static string UserTransactionRecords;
        public static string UserHistoryTransactionRecords;
        public static string UserDataPath;
        public static string UserHistoryDataPath;

        public static List<MITUserTradeRecs> lstUsers = new List<MITUserTradeRecs>();
        public static List<MITUserTradeRecs> lstHistoryUsers = new List<MITUserTradeRecs>();
        #endregion

        public static void Init()
        {
           

            if (!DateTime.TryParse(ConfigurationManager.AppSettings["RefundDate"], out RefundDate))
            {
                RefundDate = DateTime.Now;
            }
            
            if (!int.TryParse(ConfigurationManager.AppSettings["RefundDateRange"], out RefundDateRange))
            {
                RefundDateRange = 0;
            }

            if (!double.TryParse(ConfigurationManager.AppSettings["FloatRate"], out FloatRate))
            {
                FloatRate = 0.1;
            }

            if (!double.TryParse(ConfigurationManager.AppSettings["RefundPartARate"], out RefundPartARate))
            {
                RefundPartARate = 0.6;
            }

            if (!double.TryParse(ConfigurationManager.AppSettings["RefundPartBRate"], out RefundPartBRate))
            {
                RefundPartBRate = 0.4;
            }

            if (!double.TryParse(ConfigurationManager.AppSettings["BaseRefundRate"], out BaseRefundRate))
            {
                BaseRefundRate = 0.5;
            }

            if (!double.TryParse(ConfigurationManager.AppSettings["FloatRateMAX"], out FloatRateMAX))
            {
                FloatRateMAX = 100;
            }

            if (!double.TryParse(ConfigurationManager.AppSettings["FloatRateMIN"], out FloatRateMIN))
            {
                FloatRateMIN = 0;
            }

            if (!double.TryParse(ConfigurationManager.AppSettings["RefundRateMAX"], out RefundRateMAX))
            {
                RefundRateMAX = 1;
            }

            if (!double.TryParse(ConfigurationManager.AppSettings["RefundRateMIN"], out RefundRateMIN))
            {
                RefundRateMIN = 0.2;
            }

            MITAccount = ConfigurationManager.AppSettings["MITAccount"];

            MITFromAccount = ConfigurationManager.AppSettings["MITFromAccount"];

            #region Print

            Console.WriteLine("------------------------------------初始化-----------------------------------\r\n");
            Console.WriteLine(string.Format("执行分红的时间: [{0}]", RefundDate.ToString()));

            Console.WriteLine(string.Format("基准浮动利率: [{0}]", FloatRate));

            Console.WriteLine(string.Format("基准分红利率: [{0}]", BaseRefundRate));

            Console.WriteLine(string.Format("浮动利率归一化最大值: [{0}]", FloatRateMAX));

            Console.WriteLine(string.Format("浮动利率归一化最小值: [{0}]", FloatRateMIN));

            Console.WriteLine(string.Format("分红利率最大值: [{0}]", RefundRateMAX));

            Console.WriteLine(string.Format("分红利率最小值: [{0}]", RefundRateMIN));

            Console.WriteLine(string.Format("当天分红分两个部分-A 给所有持币用户: [{0}]", RefundPartARate));

            Console.WriteLine(string.Format("当天分红分两个部分-B 给昨日交易用户额外分红: [{0}]", RefundPartBRate));

            Console.WriteLine(string.Format("项目方收入ETH的账号: [{0}]", MITAccount));

            Console.WriteLine(string.Format("项目方发送ETH的账号: [{0}]", MITFromAccount));
            #endregion

            if (!double.TryParse(ConfigurationManager.AppSettings["YesterdayTotalProfit"], out YesterdayTotalProfit))
            {
                YesterdayTotalProfit = 50;
            }

            if (!double.TryParse(ConfigurationManager.AppSettings["TheDayBeforeYesterdayTotalProfit"], out TheDayBeforeYesterdayTotalProfit))
            {
                TheDayBeforeYesterdayTotalProfit = 50;
            }

            UserTransactionRecords = ConfigurationManager.AppSettings["UserTransactionRecords"];
            UserHistoryTransactionRecords = ConfigurationManager.AppSettings["UserHistoryTransactionRecords"];

            String path = Environment.CurrentDirectory + UserTransactionRecords + "HistoryTradeRecords.txt";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            //Test only                
            Console.WriteLine("\r\n----------------------------------Test Only----------------------------------\r\n");
         
            Console.WriteLine(string.Format("获取用户交易记录的路径: [{0}]", UserTransactionRecords));

            Console.WriteLine();

           
        }

        public static void LoadData(int iDate)
        {
            try
            {
                lstUsers.Clear();
                Console.WriteLine("++++++++++++ 获取过去24小时用户所有交易数据 +++++++++++");
                UserDataPath = Environment.CurrentDirectory + UserTransactionRecords + "TradeRecords" + iDate.ToString() + ".txt";
                if (iDate == 100)
                {
                    BCEXData bd = new BCEXData();
                    bd.GetData(lstUsers);
                   
                }else
                {
                    GetUserTradeRecords(UserDataPath, lstUsers);
                }               
                
                Console.WriteLine();

                lstHistoryUsers.Clear();
                //获取用户所有历史权重交易数据
                Console.WriteLine("++++++++++++ 获取用户所有历史权重交易数据 +++++++++++");
                UserHistoryDataPath = Environment.CurrentDirectory + UserTransactionRecords + "HistoryTradeRecords.txt";
                GetUserTradeRecords(UserHistoryDataPath, lstHistoryUsers);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        private static void GetUserTradeRecords(string path, List<MITUserTradeRecs> lstUsers)
        {
            if (File.Exists(path))
            {
                string jsonstring = File.ReadAllText(path);
                var jobject = JsonConvert.DeserializeObject<RootObject>(jsonstring);

                foreach (var user in jobject.Users)
                {
                    var userRecs = new MITUserTradeRecs();
                    userRecs.sUserID = user.sUserID;
                    userRecs.dBalance = user.dBalance;
                    Console.WriteLine("++++++++++++ " + user.sUserID + " +++++++++++");
                    foreach (var item in user.Records)
                    {
                        var recs = item.Convert();
                        if (recs.dtTradingTime > CSettings.RefundDate)
                        {
                            return;
                        }
                        userRecs.lstTradingRecs.Add(recs);
                        Console.WriteLine(recs.ToString());
                    }
                    lstUsers.Add(userRecs);
                }
            }
        }

        private static void GetDir(string path)
        {
            string extName = ".txt";
            string[] dir = Directory.GetDirectories(path); //文件夹列表   
            DirectoryInfo fdir = new DirectoryInfo(path);
            FileInfo[] file = fdir.GetFiles();

            if (file.Length != 0 || dir.Length != 0) //当前目录文件或文件夹不为空                   
            {
                foreach (FileInfo f in file) //显示当前目录所有文件   
                {
                    if (extName.ToLower().IndexOf(f.Extension.ToLower()) >= 0)
                    {
                        string[] data = File.ReadAllLines(f.FullName);
                        string sID = f.ToString();
                        lstUsers.Add(GetUserRecs(sID.Substring(0, sID.LastIndexOf('.')), data));
                    }
                }
            }
        }

        private static MITUserTradeRecs GetUserRecs(string sID, string[] data)
        {
            var user = new MITUserTradeRecs();
            user.sUserID = sID;

            string tmp = string.Empty;
            for (int i = 0; i < data.Length; i++)
            {
                tmp = data[i].Trim();
                if (string.IsNullOrEmpty(tmp) || tmp.IndexOf("//") >= 0)
                {
                    continue;
                }
                var temp = tmp.Split(',');
                if (temp.Length != 3)
                {
                    continue;
                }

                var recs = new TradingRecords();

                if (!double.TryParse(temp[0], out recs.dAmount))
                {
                    // Console.WriteLine(string.Format("错误！ 解析数量[{0}]", temp[0]));
                    continue;
                }
                if (recs.dAmount < 0)
                {
                    recs.enuTradeType = TRADETYPE.SELL;
                    recs.dAmount = Math.Abs(recs.dAmount);
                }
                else
                {
                    recs.enuTradeType = TRADETYPE.BUY;
                }
                if (!double.TryParse(temp[1], out recs.dPrice))
                {
                    Console.WriteLine(string.Format("错误！ 解析价格[{0}]", temp[1]));
                }

                if (!DateTime.TryParse(temp[2], out recs.dtTradingTime))
                {
                    Console.WriteLine(string.Format("错误！ 解析时间[{0}]", temp[2]));
                }

                user.lstTradingRecs.Add(recs);
            }
            return user;
        }

    }


    public class Record
    {
        public int RecID { get; set; }
        public string Type { get; set; }
        public double dPrice { get; set; }
        public double dAmount { get; set; }
        public string dtTradingTime { get; set; }

        public TradingRecords Convert()
        {
            var rec = new TradingRecords();

            rec.RecID = RecID;
            rec.dAmount = dAmount;
            rec.dPrice = dPrice;
            DateTime.TryParse(dtTradingTime, out rec.dtTradingTime);

            if (Type.Trim().ToUpper() == "BUY")
            {
                rec.enuTradeType = TRADETYPE.BUY;
            }
            else
            {
                rec.enuTradeType = TRADETYPE.SELL;
            }
            return rec;
        }
    }
    public class User
    {
        public string sUserID { get; set; }
        public int RecsSize { get; set; }
        public double dBalance { get; set; }
        public List<Record> Records { get; set; }

    }

    public class RootObject
    {
        public string FetchDataTime { get; set; }
        public int Nodesize { get; set; }
        public List<User> Users { get; set; }
    }






}
