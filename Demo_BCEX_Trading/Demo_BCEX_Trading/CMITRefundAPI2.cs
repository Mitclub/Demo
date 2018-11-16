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
		
        public List<MITUserWeightRecs> ComputeWeigth(DateTime dtRefundDate, int flag = 1)
        {
            var lstRet = new List<MITUserWeightRecs>();

            if (_lstWeightDetails.Count < 1) return lstRet;

            foreach (var item in _lstWeightDetails)
            {

                var userweight = new MITUserWeightRecs();
                userweight.sUserID = item.sUserID;
                double dWeight = 0.0;
                foreach (var item2 in item.lstTradingRecs)
                {
                    if (!IsValidTimeRange(dtRefundDate, item2.dtTradingTime,flag))                  
                    {
                        continue;
                    }
                    dWeight += item2.dAmount * item2.dPrice;
                }
                
                userweight.dWeight = CHelper.Round(dWeight);
                lstRet.Add(userweight);

               

            }

            return lstRet;
        }

        /// <summary>
        /// 
        /// 计算每个用户权重，交易日期必须小于分红日期，并且不包括分红日当日
        /// 
        /// </summary>
        /// <param name="dtRefundDate">分红日期</param>
        /// <param name="lstUsers">所有MIT持有者的交易记录</param>
        /// <returns></returns>
        public List<MITUserWeightRecs> CalcUserWeight(DateTime dtRefundDate, List<MITUserTradeRecs> lstUsers)
        {
            List<MITUserWeightRecs> lstRet = new List<MITUserWeightRecs>();            
            double dWeight = 0.0;

         

            foreach (var user in lstUsers)
            {
               

                var userweight = new MITUserWeightRecs();
                dWeight = GetWeightForOneUser(dtRefundDate, user);

                var wdetails = GetWeigthDetails(dtRefundDate, user, dWeight);
               
                userweight.sUserID = user.sUserID;
                userweight.dWeight = CHelper.Round(dWeight);

                lstRet.Add(userweight);

               
            }
            return lstRet;
        }

        public List<MITUserWeightRecs> GetYesterdayWeightUsers(DateTime dtRefundDate, List<MITUserTradeRecs> lstUsers)
        {
            List<MITUserWeightRecs> lstRet = new List<MITUserWeightRecs>();
            DateTime dtValid1 = dtRefundDate.AddDays(-1);
            DateTime dtValid2 = dtRefundDate.AddDays(-2);

            double dWeight = 0.0;

            foreach (var user in lstUsers)
            {
                var userweight = new MITUserWeightRecs();
                dWeight = GetWeightForOneUser(dtRefundDate, user, 2);

                userweight.sUserID = user.sUserID;
                userweight.dWeight = dWeight;

                lstRet.Add(userweight);
            }
            return lstRet;
        }

        //计算总权重
        public double CalcTotalWeight(List<MITUserWeightRecs> lstUsers)
        {
            double dRet = 0.0;
            foreach (var item in lstUsers)
            {
                dRet += item.dWeight;
            }
           
            if (dRet<=0)
            {
                return dRet;
            }
            string data = string.Empty;
            foreach (var item in lstUsers)
            {
                if (item.dWeight <= 0) continue;
                data = string.Format("     用户:[{0}],权重[{1}],权重占比[{2}%]", item.sUserID, item.dWeight, CHelper.Round(item.dWeight / dRet * 100));
                Console.WriteLine(data);
            }
            Console.WriteLine();
            return dRet;
        }
 //计算总权重
        public double CalcTotalWeight(List<MITUserWeightRecs> lstUsers)
        {
            double dRet = 0.0;
            foreach (var item in lstUsers)
            {
                dRet += item.dWeight;
            }
           
            if (dRet<=0)
            {
                return dRet;
            }
            string data = string.Empty;
            foreach (var item in lstUsers)
            {
                if (item.dWeight <= 0) continue;
                data = string.Format("     用户:[{0}],权重[{1}],权重占比[{2}%]", item.sUserID, item.dWeight, CHelper.Round(item.dWeight / dRet * 100));
                Console.WriteLine(data);
            }
            Console.WriteLine();
            return dRet;
        }

        /// <summary>
        /// 为每一个MIT持有者计算分红
        /// </summary>
        /// <param name="dRefundProfit">总分红的ETH数量</param>
        /// <param name="dTotalWeight">总权重</param>
        /// <param name="lstUsers">所有MIT持有者</param>
        /// <returns></returns>
        public List<MITUserRefund> CalcRefundForMITUser(double dRefundProfit, double dTotalWeight, List<MITUserWeightRecs> lstUsers)
        {
            List<MITUserRefund> lstRet = new List<MITUserRefund>();
            double dEth = 0.0;
            double dtotal = 0.0;
            double dweight = 0.0;
            double dper = 0.0;

            //var bd = new BCEXData();
            //var lst = bd.TestGetWeight();
            //bool isfound = false;

            List<MITUserWeightRecs> lsttmp = new List<MITUserWeightRecs>();
            foreach (var user in lstUsers)
            {
               
                if (user.dWeight <= 0.000000000001)
                {
                    continue;
                }
                dweight += user.dWeight;
                dper += (double)user.dWeight / (double)dTotalWeight;
                dEth = (double)((double)user.dWeight / (double)dTotalWeight) * (double)dRefundProfit;
                dtotal += dEth;
                if(dtotal >= dRefundProfit)
                {
                    Console.WriteLine();
                }
                lstRet.Add(new MITUserRefund(user.sUserID, dEth));                
               
            }

            //foreach (var item in lst)
            //{
            //    isfound = false;

            //    foreach (var item3 in lstRet)
            //    {
            //        if (item3.sUserID == item.Item1.Trim())
            //        {
            //            isfound = true;
            //            break;
            //        }
            //    }
            //    if (!isfound)
            //    {
            //        Console.WriteLine("CAN NOT FOUND :" + item.Item1 + " weight : " + item.Item2);
            //    }

            //}

            return lstRet;
        }


        /// <summary>
        /// 
        /// 向用户派发ETH
        /// 
        /// </summary>
        /// <param name="sAccount">项目方账号</param>
        /// <param name="lstUsers">待派发的用户列表</param>
        /// <returns></returns>
        public bool TransferETHToMITHolder(string sMITAcct, List<MITUserRefund> lstUsers)
        {
            bool bRet = true;

            foreach (var item in lstUsers)
            {
                if (item.dETH > 0)
                {
                    item.bIsTransferOK = CBCEXTradeAPI.Transfer(sMITAcct, item.sUserID, item.dETH);
                }

                if (!item.bIsTransferOK) //出错了
                {
                    //1. LOG
                    //2. 三次后转人工
                }
            }

            return bRet;
        }

        private MITUserTradeRecs FilteringWithFIFO(MITUserTradeRecs user, double dTotalMIT)
        {
            MITUserTradeRecs recs = new MITUserTradeRecs();
            double dRet = 0.0;

            //按交易时间降序排序，最新的交易在最前面
            var lstRecs = user.lstTradingRecs.OrderByDescending<TradingRecords, DateTime>(x => x.dtTradingTime).ToList();

            recs.sUserID = user.sUserID;
            recs.dBalance = dTotalMIT;

            //按照First In First Out的原则 
            for (int i = 0; i < lstRecs.Count; i++)
            {
                var tradeRecs = new TradingRecords();

                if (lstRecs[i].enuTradeType != TRADETYPE.BUY)
                {
                    continue;
                }
                var temp = dTotalMIT - lstRecs[i].dAmount;

                if (temp >= 0)
                {
                    dRet += lstRecs[i].dAmount * lstRecs[i].dPrice;

                    tradeRecs.dAmount = lstRecs[i].dAmount;
                    tradeRecs.dPrice = lstRecs[i].dPrice;
                    tradeRecs.RecID = lstRecs[i].RecID;
                    tradeRecs.enuTradeType = TRADETYPE.BUY;
                    tradeRecs.dtTradingTime = lstRecs[i].dtTradingTime;
                    recs.lstTradingRecs.Add(tradeRecs);
                }
                else
                {
                    dRet += dTotalMIT * lstRecs[i].dPrice;
                    if (dTotalMIT > 0)
                    {
                        tradeRecs.dAmount = dTotalMIT;
                        tradeRecs.dPrice = lstRecs[i].dPrice;
                        tradeRecs.RecID = lstRecs[i].RecID;
                        tradeRecs.enuTradeType = TRADETYPE.BUY;
                        tradeRecs.dtTradingTime = lstRecs[i].dtTradingTime;
                        recs.lstTradingRecs.Add(tradeRecs);
                    }
                    break;
                }
                dTotalMIT = temp;
                if (dTotalMIT <= 0)
                {
                    break;
                }
            }

            return recs;
        }



    }


}
