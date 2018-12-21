using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_BCEX_Trading
{
    public interface IMITRefundAPI
    {
        double GetYesterdayTotalProfit(DateTime dtRefundDate);
        double GetTheDayBeforeYesterdayTotalProfit(DateTime dtRefundDate);
        double GetFloatingRate(double dYesterdayData, double dTheDayBeforeYesterdayData, double dBaseFloatingRate);
        double GetTotalDayRefundForMITHolder(double dYesterdayTotalProfit, double dDayBaseRefundRate, double dDayFloatingRate);
        double GetTotalDayRefundForMITHolder(double dYesterdayTotalProfit, double dDayBaseRefundRate);
        List<MITUserWeightRecs> CalcUserWeight(DateTime dtRefundDate, List<MITUserTradeRecs> lstUsers);
        List<MITUserWeightRecs> ComputeWeigth(DateTime dtRefundDate, int flag = 1);
        List<MITUserWeightRecs> GetYesterdayWeightUsers(DateTime dtRefundDate, List<MITUserTradeRecs> lstUsers);
        double CalcTotalWeight(List<MITUserWeightRecs> lstUsers);
        List<MITUserRefund> CalcRefundForMITUser(double dRefundProfit, double dTotalWeight, List<MITUserWeightRecs> lstUsers);
        bool TransferETHToMITHolder(string sMITAcct, List<MITUserRefund> lstUsers);

        bool UpdateMITHoldersTradingRecords(DateTime dtRefundDate, List<MITUserTradeRecs> lstUsers);
    }

}
