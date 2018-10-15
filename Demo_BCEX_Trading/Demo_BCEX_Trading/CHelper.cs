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

}
    public enum TRADETYPE
    {
        NONE = -1,
        SELL = 0,
        BUY = 1
    }
}

