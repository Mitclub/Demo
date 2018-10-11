using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_BCEX_Trading
{
    public class CMITRefundProcessor
    {
        private IMITRefundAPI _mITRefundAPI = null;

        public CMITRefundProcessor(IMITRefundAPI iMITAPI)
        {
            _mITRefundAPI = iMITAPI;
        }

        public void Process(DateTime dtRefundDate)
        {

            //第1步：根据当前MIT数量余额计算出返利利率，有变动则更新配置表中的数据
            Console.WriteLine("第1步：根据当前MIT数量余额计算出返利利率，有变动则更新配置表中的数据");

            //第2步：获取项目方昨天的总获利ETH总数
            double dYesProfit = _mITRefundAPI.GetYesterdayTotalProfit(dtRefundDate);
            Console.WriteLine(string.Format("第2步：获取前24小时用户总兑换MIT的ETH数量:[{0}]\r\n", dYesProfit));
            if (dYesProfit <= 0)
            {
                Console.WriteLine(string.Format("警告：获取前24小时用户总兑换MIT的ETH数量为零，程序退出！\r\n"));
                return;
            } 

            //第3步：获取昨天向MIT持币者总共需要返ETH的数量
            double dTotalETHForMITHolder = _mITRefundAPI.GetTotalDayRefundForMITHolder(dYesProfit,CSettings.BaseRefundRate);
            Console.WriteLine(string.Format("第3步：获取前24小时向MIT持币者总共需要返ETH的数量:{0} * {1} = [{2}]\r\n", dYesProfit,(CSettings.BaseRefundRate), dTotalETHForMITHolder));

            //第4步: 保存和更新，MIT分红程序自身保存的交易数据
            var lstUsers = CBCEXTradeAPI.GetAllMITHoldersTradingRecords();
            _mITRefundAPI.UpdateMITHoldersTradingRecords(dtRefundDate, lstUsers);
            Console.WriteLine("第4步: 保存和更新，MIT分红程序自身保存的交易数据");

            //第5步：计算每个用户权重            
            var lstWeightUsers = _mITRefundAPI.ComputeWeigth(dtRefundDate);
            Console.WriteLine("第5步：计算每个用户权重");
            PrintWeight(lstWeightUsers);
            Console.WriteLine();          

            //第6步：计算总权重
            double dTotalWeight = _mITRefundAPI.CalcTotalWeight(lstWeightUsers);
            Console.WriteLine(string.Format("第6步: 计算总权重:[{0}]!", CHelper.Round(dTotalWeight)));
            if (dTotalWeight == 0)
            {
                Console.WriteLine(string.Format("警告： 总权重为零，程序退出！\r\n"));
                return;
            }
            
            //第7步：为每一个MIT持有者计算分红
            var lstRefundUsers = _mITRefundAPI.CalcRefundForMITUser(dTotalETHForMITHolder * CSettings.RefundPartARate,dTotalWeight,lstWeightUsers);
            Console.WriteLine(string.Format("第7步：为每一个MIT持有者计算分红(A部分={0}*{1}={2})", dTotalETHForMITHolder, CSettings.RefundPartARate, dTotalETHForMITHolder*CSettings.RefundPartARate));
            Print(lstRefundUsers);
            Console.WriteLine();

            //第8步：为昨日MIT交易者额外分红
            var lstWeightUsers2 = _mITRefundAPI.ComputeWeigth(dtRefundDate,2);
            dTotalWeight = _mITRefundAPI.CalcTotalWeight(lstWeightUsers2);
            if (dTotalWeight > 0)
            {
                var lstRefundUsers2 = _mITRefundAPI.CalcRefundForMITUser(dTotalETHForMITHolder * CSettings.RefundPartBRate, dTotalWeight, lstWeightUsers2);               
                Console.WriteLine(string.Format("第8步：为前24小时MIT交易者额外分红(B部分:{0}*{1}={2})", dTotalETHForMITHolder, CSettings.RefundPartBRate, dTotalETHForMITHolder * CSettings.RefundPartBRate));
                PrintWeight(lstWeightUsers2);
                Print(lstRefundUsers2);               
                lstRefundUsers = MergeList(lstRefundUsers, lstRefundUsers2);
            }            

            //第9步：向用户派发ETH
            bool bIsSuccess = _mITRefundAPI.TransferETHToMITHolder(CSettings.MITFromAccount, lstRefundUsers);

            Console.WriteLine();
            Console.WriteLine("**********************************第9步：统计A和B两部分的分红***************************");
            //按交易时间降序排序，最新的交易在最前面
            var lsttmp= lstRefundUsers.OrderByDescending<MITUserRefund, double>(x => x.dETH).ToList();
            Print(lsttmp);
            Console.WriteLine("****************************************************************************");            
        }

        private void Print(List<MITUserRefund> lst)
        {
            Console.WriteLine();
            double dEth = 0.0;
            foreach (var item in lst)
            {
                dEth += item.dETH;
                if (dEth <= 0.0000000000001) continue;
                Console.WriteLine(string.Format("用户[{0}]共获得[{1}]个ETH",item.sUserID, CHelper.Round(item.dETH)));
            }
            Console.WriteLine(string.Format("------------------------------------\r\n            总计[{0}]个ETH", CHelper.Round(dEth)));
            Console.WriteLine();
        }

        private void PrintWeight(List<MITUserWeightRecs> lst)
        {
            Console.WriteLine();
            foreach (var item in lst)
            {
                if (item.dWeight <= 0.0000000000001) continue;
                Console.WriteLine(string.Format("用户[{0}]:权重为[{1}]", item.sUserID, item.dWeight));
            }
            Console.WriteLine();
        }

        private List<MITUserRefund> MergeList(List<MITUserRefund> lst1, List<MITUserRefund> lst2)
        {
            foreach (var item in lst1)
            {
                foreach (var item2 in lst2)
                {
                    if (item.sUserID == item2.sUserID)
                    {
                        item.dETH += item2.dETH;
                        break;
                    }
                }
            }
            return lst1;
        }
    }
}
