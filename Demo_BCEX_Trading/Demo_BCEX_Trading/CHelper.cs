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

}
    public enum TRADETYPE
    {
        NONE = -1,
        SELL = 0,
        BUY = 1
    }
}

