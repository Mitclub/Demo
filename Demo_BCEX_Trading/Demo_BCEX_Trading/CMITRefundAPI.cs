using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Demo_BCEX_Trading
{
    /// <summary>
    /// 
    /// 本类按照Tony的思路来做，分红是按24小时为一个间隔，时间不累计，权重不计算时间因素？
    /// 
    /// </summary>
    public class CMITRefundAPI
    {
        private List<MITUserTradeRecs> _lstWeightDetails = new List<MITUserTradeRecs>();
        //获得指定分红日的昨日MIT项目方获得ETH总数
        public double GetYesterdayTotalProfit(DateTime dtRefundDate)
        {
#if DEBUG
            return CSettings.YesterdayTotalProfit;
#endif
            return CBCEXTradeAPI.GetProfit(dtRefundDate.AddDays(-1));
        }

        //获得指定分红日的前天MIT项目方获得ETH总数
        public double GetTheDayBeforeYesterdayTotalProfit(DateTime dtRefundDate)
        {
#if DEBUG
            return CSettings.TheDayBeforeYesterdayTotalProfit;
#endif
            return CBCEXTradeAPI.GetProfit(dtRefundDate.AddDays(-2));
        }

        /// <summary>
        /// 
        /// 获取昨天向MIT持币者总共返ETH的数量
        /// 
        /// </summary>
        /// <param name="dYesterdayTotalProfit">昨天项目方总共获得的ETH数</param>
        /// <param name="dDayBaseRefundRate">基准分红百分比</param>       
        /// <returns></returns>
        public double GetTotalDayRefundForMITHolder(double dYesterdayTotalProfit, double dDayBaseRefundRate)
        {
            double dDayRefundRate = dDayBaseRefundRate;

            //每天分红比例最大范围应该从配置文件中获取
            if (dDayRefundRate < CSettings.RefundRateMIN || dDayRefundRate > CSettings.RefundRateMAX)
            {
                //throw exception
                return -1;
            }

            if (dYesterdayTotalProfit <= 0.0)
            {
                //throw exception
                return -1;
            }

            return CHelper.Round(dYesterdayTotalProfit * dDayRefundRate);
        }
    }


}
