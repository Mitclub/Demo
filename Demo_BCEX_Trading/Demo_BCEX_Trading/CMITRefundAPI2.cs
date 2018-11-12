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
    /// 上币第一天也要运行，即使不分币，也会产生计算权重的细节数据
    /// 
    /// </summary>
    public class CMITRefundAPI2 : IMITRefundAPI
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
        /// 获得浮动分红百分比，本函数实现交易模式中公式1和公式2
        /// 
        /// </summary>
        /// <param name="dYesterdayData">获得昨日MIT项目方获得ETH总数</param>
        /// <param name="dTheDayBeforeYesterdayData">获得前天MIT项目方获得ETH总数</param>
        /// <param name="dBaseFloatingRate">基准浮动百分比</param>
        /// <returns></returns>
        public double GetFloatingRate(double dYesterdayData, double dTheDayBeforeYesterdayData, double dBaseFloatingRate)
        {
            if (dYesterdayData <= 0 || dTheDayBeforeYesterdayData <= 0 || dYesterdayData == dTheDayBeforeYesterdayData)
            {
                return 0;
            }

            //公式 1：计算昨天和前天的交易额涨跌百分比
            double dRate = (dYesterdayData - dTheDayBeforeYesterdayData) / Math.Min(dYesterdayData, dTheDayBeforeYesterdayData);

            //公式 2：交易额涨跌百分比归一化公式
            double dMAX = CSettings.FloatRateMAX, dMIN = CSettings.FloatRateMIN;//应该从外部中获取
            double dFloatingRate = (Math.Min(Math.Abs(dRate), dMAX) - dMIN) / (dMAX - dMIN) * dBaseFloatingRate;
            if (dRate < 0)
            {
                dFloatingRate = 0 - dFloatingRate;
            }
            return CHelper.Round(dFloatingRate);
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
            double dDayRefundRate = dDayBaseRefundRate ;

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

        /// <summary>
        /// 
        /// 获取昨天向MIT持币者总共返ETH的数量
        /// 
        /// </summary>
        /// <param name="dYesterdayTotalProfit">昨天项目方总共获得的ETH数</param>
        /// <param name="dDayBaseRefundRate">基准分红百分比</param>
        /// <param name="dDayFloatingRate">根据交易额的起伏而计算的浮动浮动利率</param>
        /// <returns></returns>
        public double GetTotalDayRefundForMITHolder(double dYesterdayTotalProfit, double dDayBaseRefundRate, double dDayFloatingRate)
        {
            double dDayRefundRate = dDayBaseRefundRate + dDayFloatingRate;

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
