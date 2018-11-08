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

    }


}
