using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_BCEX_Trading
{
    public class CHelper
    {
        public static int GetInteger(double dData)
        {
            int iRet = 0;
            string strD = Math.Abs(dData).ToString();
            string[] strs = strD.Split('.');
            int.TryParse(strs[0], out iRet);
            return iRet;

        }

        public static bool IsValidTimeRange(DateTime dtRefundDate, DateTime dt, int flag = 1)
        {
            DateTime dtValid1 = dtRefundDate.AddDays(-1);
            DateTime dtValid2 = dtRefundDate.AddDays(-2);

            if (dt > dtRefundDate)
            {
                return false;
            }
            if ((dt > dtValid1 || dt <= dtValid2) && flag == 2)
            {
                return false;
            }

            return Is24Hours(dtRefundDate, dt);
        }

        public static bool Is24Hours(DateTime dtRefundDate, DateTime dt)
        {
            bool bret = false;

            TimeSpan ts = dtRefundDate - dt;

            if (ts.TotalHours >= 24)
            {
                return true;
            }
            return bret;
        }
        /// <summary>
        /// 
        /// 根据账号MIT数量余额计算返利利率
        /// 
        /// </summary>
        /// <param name="dOriginalNumber">账号起始总MIT数量</param>
        /// <param name="dCurrentNumber">当前已经销售MIT的数量</param>
        /// <param name="dStartRefundRate">起始返利基准利率</param>
        /// <returns></returns>
        public static double CalcRefundRate(double dOriginalNumber, double dCurrentNumber, double dStartRefundRate)
        {
            double dRet = GetInteger(dCurrentNumber) / GetInteger(dOriginalNumber * dStartRefundRate) * dStartRefundRate + dStartRefundRate;

            return dRet > 0.9 ? 0.9 : dRet;
        }
        public static bool IsSameDay(DateTime dt1, DateTime dt2)
        {
            if (dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day)
            {
                return true;
            }
            return false;
        }

        public static double MathRound(double data)
        {
            string s = string.Format("{0}", data);
            string temp = s;
            int pos = s.IndexOf('.');
            if (pos <= 0) return data;
            s = temp.Substring(pos+1);
            if (s.Length <= 8) return data;
            s = s.Substring(0, 8);
            double.TryParse(temp.Substring(0,pos+1) + s, out data);
            return data;

        }
        public static double Round(double data)
        {
            return MathRound(data);
        }

        public static string SerializeObject(RootObject lstUsers)
        {
            string sRet = string.Empty;

            sRet = JsonConvert.SerializeObject(lstUsers);

            return sRet;
        }

        public static void WriteToText(string txtContent, string txtPath)
        {
            using (FileStream fs = new FileStream(txtPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.BaseStream.Seek(0, SeekOrigin.End);
                    sw.WriteLine("{0}\n", txtContent);
                    sw.Flush();
                }
            }
        }

        public static List<MITUserTradeRecs> MergeTradeRecords(List<MITUserTradeRecs> lsttradeRecs, List<MITUserTradeRecs> lstHistorytradeRecs )
        {
            List<MITUserTradeRecs> lstret = new List<MITUserTradeRecs>();

            foreach (var item in lsttradeRecs)
            {
                var userrec = new MITUserTradeRecs();
                if(item.sUserID == "772415")
                {
                    Console.WriteLine();
                }
                userrec.sUserID = item.sUserID;
                userrec.dBalance = 0.0;
                foreach (var item3 in item.lstTradingRecs)
                {
                    userrec.lstTradingRecs.Add(item3);
                }
                foreach (var item2 in lstHistorytradeRecs)
                {
                    if (item.sUserID == item2.sUserID)
                    {
                        foreach (var item4 in item2.lstTradingRecs)
                        {
                             userrec.lstTradingRecs.Add(item4);
                        }                       
                    }
                }
                lstret.Add(userrec);
            }

            return lstret;
           // var dic = Convert(lsttradeRecs);
           // var historydic = Convert(lstHistorytradeRecs);
           // if (lstHistorytradeRecs.Count < 1)
           // {
           //     return lsttradeRecs;
           // }
           // foreach (KeyValuePair<string, Dictionary<DateTime, TradingRecords>> item in historydic)
           // {
           //     if (dic.ContainsKey(item.Key))
           //     {                    
           //         foreach (KeyValuePair<DateTime, TradingRecords> item2 in item.Value)
           //         {
           //             if (!dic[item.Key].ContainsKey(item2.Key))
           //             {
           //                 dic[item.Key].Add(item2.Key,item2.Value);
           //             }
           //             else
           //             {
           //                 dic[item.Key][item2.Key].dAmount += item2.Value.dAmount;
           //             }
           //         }
           //     }
           //     else
           //     {
           //         dic.Add(item.Key, item.Value);
           //     }
           // }
           
           //return Revert(dic);
        }
        public static Dictionary<string, Dictionary<DateTime, TradingRecords>> Convert(List<MITUserTradeRecs> lsttradeRecs)
        {
            var dic = new Dictionary<string, Dictionary<DateTime, TradingRecords>>();

            foreach (var item in lsttradeRecs)
            {                
                var dic2 = new Dictionary<DateTime, TradingRecords>();

                foreach (var item2 in item.lstTradingRecs)
                {
                    if(!dic2.ContainsKey(item2.dtTradingTime))
                    {
                        dic2.Add(item2.dtTradingTime, item2);
                    }
                    else
                    {
                        if (item2.enuTradeType == TRADETYPE.BUY)
                        {
                            dic2[item2.dtTradingTime].dAmount += item2.dAmount;
                        }
                        else
                        {
                            dic2[item2.dtTradingTime].dAmount -= item2.dAmount;
                            if (dic2[item2.dtTradingTime].dAmount <0)
                            {
                                dic2[item2.dtTradingTime].enuTradeType = TRADETYPE.SELL;
                                dic2[item2.dtTradingTime].dAmount = Math.Abs(dic2[item2.dtTradingTime].dAmount);
                            }
                        }
                    }
                }
                dic.Add(item.sUserID,dic2);
            }

            return dic;
        }

        public static List<MITUserTradeRecs> Revert(Dictionary<string, Dictionary<DateTime, TradingRecords>> dic)
        {
            var lstRet = new List<MITUserTradeRecs>();

            foreach (KeyValuePair<string, Dictionary<DateTime, TradingRecords>> item in dic)
            {
                var recs = new MITUserTradeRecs();

                recs.sUserID = item.Key;

                foreach (KeyValuePair<DateTime, TradingRecords> item2 in item.Value)
                {
                    recs.lstTradingRecs.Add(item2.Value);
                }
                lstRet.Add(recs);
            }           
            
            return lstRet;
        }
    }

    public class MITUserWeightRecs
    {
        public string sUserID;//可以就是用户账号或者映射的GUID （再hash或者对账号进行MD5，比较简单）
        public double dWeight = 0.0;
        public string formular;
    }

    public class MITUserRefund
    {
        public string sUserID;//可以就是用户账号或者映射的GUID （再hash或者对账号进行MD5，比较简单）
        public double dETH = 0.0;//返多少个ETH给该用户
        public DateTime dtTimeStamp;
        public bool bIsTransferOK = false;

        public MITUserRefund(string sID, double dEth)
        {
            sUserID = sID;
            dETH = dEth;
            dtTimeStamp = DateTime.Now;
        }
    }

    public class MITUserTradeRecs
    {
        public string sUserID;//可以就是用户账号或者映射的GUID （再hash或者对账号进行MD5，比较简单）
        public double dBalance = 0.0;//当前共持有多少个MIT
        public List<TradingRecords> lstTradingRecs = new List<TradingRecords>();
    }

    public class TradingRecords
    {
        public int RecID = 0;
        public TRADETYPE enuTradeType = TRADETYPE.NONE;
        public double dPrice = 0;
        public double dAmount = 0;
        public DateTime dtTradingTime;

        public Record Convert()
        {
            var rec = new Record();

            rec.RecID = RecID;
            rec.dAmount = CHelper.Round(dAmount);
            rec.dPrice = CHelper.Round(dPrice);
            rec.dtTradingTime = dtTradingTime.ToString();
            rec.Type = enuTradeType.ToString();

            return rec;
        }
        public override string ToString()
        {
            string s = string.Format("Price:[{0}], Amount:[{1}], TradeTime[{2}],TradeType:[{3}], RecID:[{4}]",
                dPrice,dAmount,dtTradingTime.ToString(), enuTradeType.ToString(), RecID);
            return s;
        }
    }

    public enum TRADETYPE
    {
        NONE = -1,
        SELL = 0,
        BUY = 1
    }
}
