using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_BCEX_Trading
{
    //假定BCEX交易平台API中有以下函数或类似的函数
    public class CBCEXTradeAPI
    {
        //从项目方账号获取指定日期的当日总共获取ETH总数
        public static double GetProfit(DateTime dtTradeDateTime)
        {
            double dRet = 100;//For test
            //dRet = BCEX_API_GetAccountBalance("ETH",dtTradeDateTime,"0x75a83599de596cbc91a1821ffa618c40e22ac8ca");
            //dRet += BCEX_API_GetAccountBalance("ETH", dtTradeDateTime, "0xdaee41a3d8f9c1d98c76cc662668aed0aa35cf9d");
            //dRet += BCEX_API_GetAccountBalance("ETH", dtTradeDateTime, "0xa2e40528330e45f642281404ad50a5bb35635c5d");

            return dRet;
        }

        //获得所有MIT持有者的所有跟MIT相关的有效交易记录（请注意必须是有效的交易记录）
        public static List<MITUserTradeRecs> GetAllMITHoldersTradingRecords()
        {
            //For Test
            List<MITUserTradeRecs> lstHistoryRet = CSettings.lstHistoryUsers;
            List<MITUserTradeRecs> lstRet = CSettings.lstUsers;         

            var lst = CHelper.MergeTradeRecords(lstRet, lstHistoryRet);

            Console.WriteLine("++++++++++++ 合并交易数据 +++++++++++\r\n");
          
            foreach (var item in lst)
            {
                if (item.sUserID == "772415")
                {
                    Console.WriteLine();
                }
                Console.WriteLine("     用户: [" + item.sUserID +"]");
                foreach (var item2 in item.lstTradingRecs)
                {                  
                    Console.WriteLine(item2.ToString());
                }
            }
            Console.WriteLine();
            return lst;           
        }

        public static bool Transfer(string sFromAcct, string sToAcct, double dETH)
        {
            bool bRet = true;

            //bRet = BCEX_API_Transfer(sFromAcct, sToAcct, dETH);

            return bRet;
        }
    }
}
