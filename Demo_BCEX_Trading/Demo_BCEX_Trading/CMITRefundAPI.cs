﻿using System;
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
        /// <param name="dDayFloatingRate">根据交易额的起伏而计算的浮动浮动利率</param>
        /// <returns></returns>
        public double GetTotalDayRefundForMITHolder(double dYesterdayTotalProfit, double dDayBaseRefundRate,double dDayFloatingRate)
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
                dWeight = GetWeightForOneUser(dtRefundDate,user);

                userweight.sUserID = user.sUserID;
                userweight.dWeight = dWeight;

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
                dWeight = GetWeightForOneUser(dtRefundDate, user,2);

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
            Console.WriteLine(string.Format("第六步：计算总权重:[{0}]\r\n", dRet));
            string data = string.Empty;
            foreach (var item in lstUsers)
            {
                data = string.Format("     用户:[{0}],权重[{1}],权重占比[{2}%]",item.sUserID,item.dWeight, CHelper.Round(item.dWeight/dRet*100));
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
        public List<MITUserRefund> CalcRefundForMITUser(double dRefundProfit,double dTotalWeight, List<MITUserWeightRecs> lstUsers)
        {
            List<MITUserRefund> lstRet = new List<MITUserRefund>();
            double dEth = 0.0;
            foreach (var user in lstUsers)
            {
                dEth = CHelper.Round(user.dWeight / dTotalWeight * dRefundProfit);
                lstRet.Add(new MITUserRefund(user.sUserID,dEth));               
            }

            //验证用户获得的ETH和今天需要分红的数量
            dEth = 0.0;
            foreach (var item in lstRet)
            {
                dEth += item.dETH;
            }
            if (dEth < dRefundProfit)
            {
                //多出来的这一点要怎么处理？？
                //Console.WriteLine("分红差值： " + (dRefundProfit - dEth).ToString());
            }
            else if (dEth > dRefundProfit)
            {
               //不应该发生
               //throw exception
            }
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

        /// <summary>
        /// 
        /// 保存和更新，MIT分红程序自身保存的交易数据和产生报告
        /// 
        /// </summary>
        /// <param name="lstUsers">待派发的用户列表</param>
        /// <returns></returns>
        public bool UpdateMITHoldersTradingRecords(List<MITUserRefund> lstUsers)
        {
            bool bRet = true;

            foreach (var item in lstUsers)
            {
                if (item.dETH > 0)
                {
                    //CHelpe
                    //item.bIsTransferOK = CBCEXTradeAPI.Transfer(sMITAcct, item.sUserID, item.dETH);
                }
            }

            return bRet;
        }

        #region

        /// <summary>
        /// 
        /// 保存和更新，MIT分红程序自身保存的交易数据和产生报告
        /// 
        /// </summary>
        /// <param name="lstUsers">待派发的用户列表</param>
        /// <returns></returns>
        public bool UpdateMITHoldersTradingRecords(DateTime dtRefundDate, List<MITUserTradeRecs> lstUsers)
        {
            bool bRet = true;

           
            //return lstRet;

            var saveData = new RootObject();

            saveData.FetchDataTime = DateTime.Now.ToString();
            saveData.Nodesize = _lstWeightDetails.Count;
            saveData.Users = new List<User>();

            foreach (var item in _lstWeightDetails)
            {
                var usr = new User();
                usr.sUserID = item.sUserID;
                usr.RecsSize = item.lstTradingRecs.Count;
                usr.Records = new List<Record>();
                foreach (var item2 in item.lstTradingRecs)
                {
                    var rec = new Record();
                    usr.Records.Add(item2.Convert());
                }

                saveData.Users.Add(usr);
            }
            string newdata = CHelper.SerializeObject(saveData);
            string filename = CSettings.UserHistoryTransactionRecords.Substring(0, CSettings.UserHistoryTransactionRecords.LastIndexOf('.'));
            string date = string.Format("_{0}{1}{2}{3}{4}{5}", DateTime.Now.Year, DateTime.Now.Month,
                                 DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            filename += date + ".txt";

            if (File.Exists(Environment.CurrentDirectory + CSettings.UserHistoryTransactionRecords))
            {
                File.Move(Environment.CurrentDirectory + CSettings.UserHistoryTransactionRecords,
                    Environment.CurrentDirectory + filename);
            }

            CHelper.WriteToText(newdata, Environment.CurrentDirectory + CSettings.UserHistoryTransactionRecords);

            return bRet;
        }


        // 计算每个用户的权重
        /*
               
              时间        数量       价格
             06：00       +1         0.1   
             07：00       +3         0.5   
             08：00       +3         0.8   
             09：00       +3         1.0   
             10：00       -5         2.0   
             11：00       +5         3.0   
             12：00       +3         4.0   
             13：00       +1         5.0   
             14：00       -8         6.0   
             15：00       +1000       0   
             16：00       -200       12.0
             17：00       +200       15.0
             ----------------------------           
                          +1006

             200 × 15.0 = 3000
             806 × 0    = 0             
             -----------------
                        +3000

         * */
        //private double GetWeightForOneUser(DateTime dtRefundDate, MITUserTradeRecs user)
        //{
        //    double dRet = 0.0;

        //    //按交易时间降序排序，最新的交易在最前面
        //    var lstRecs = user.lstTradingRecs.OrderByDescending<TradingRecords, DateTime>(x=>x.dtTradingTime).ToList();

        //    //所有的交易时间必须小于此时间
        //    DateTime dtValid = dtRefundDate.AddDays(-1);

        //    //所持有的MIT
        //    double dTotalMIT = 0.0;
        //    foreach (var item in lstRecs)
        //    {
        //        if (item.dtTradingTime > dtValid)
        //        {
        //            continue;
        //        }
        //        if (item.enuTradeType == TRADETYPE.BUY)
        //        {
        //            dTotalMIT += item.dAmount;
        //        }
        //        else if (item.enuTradeType == TRADETYPE.SELL)
        //        {
        //            dTotalMIT -= item.dAmount;
        //        }                
        //    }

        //    if (dTotalMIT <= 0)
        //    {
        //        return dRet;
        //    }

        //    //按照First In First Out的原则 
        //    for (int i = 0; i < lstRecs.Count; i++)
        //    {
        //        if(lstRecs[i].enuTradeType != TRADETYPE.BUY)
        //        {
        //            continue;
        //        }
        //        var temp = dTotalMIT - lstRecs[i].dAmount;

        //        if (temp >= 0)
        //        {
        //            dRet += lstRecs[i].dAmount * lstRecs[i].dPrice;
        //        }
        //        else
        //        {
        //            dRet += dTotalMIT * lstRecs[i].dPrice;
        //            break;
        //        }
        //        dTotalMIT = temp;
        //    }

        //    return dRet;
        //}

        // 计算每个用户的权重        
        private double GetWeightForOneUser(DateTime dtRefundDate, MITUserTradeRecs user, int flag=1)
        {
            double dRet = 0.0;

            //按交易时间降序排序，最新的交易在最前面
            var lstRecs = user.lstTradingRecs.OrderByDescending<TradingRecords, DateTime>(x => x.dtTradingTime).ToList();

            //所有的交易时间必须小于此时间           
            DateTime dtValid1 = dtRefundDate.AddDays(-1);
            DateTime dtValid2 = dtRefundDate.AddDays(-2);

            //所持有的MIT
            double dTotalMIT = 0.0;
            foreach (var item in lstRecs)
            {
                if (item.dtTradingTime > dtValid1 || (item.dtTradingTime <= dtValid2 && flag == 2))
                {
                    continue;
                }
                if (item.enuTradeType == TRADETYPE.BUY)
                {
                    dTotalMIT += item.dAmount;
                }
                else if (item.enuTradeType == TRADETYPE.SELL)
                {
                    dTotalMIT -= item.dAmount;
                }
            }

            if (dTotalMIT <= 0)
            {
                return dRet;
            }

            //按照First In First Out的原则 
            for (int i = 0; i < lstRecs.Count; i++)
            {
                if (lstRecs[i].enuTradeType != TRADETYPE.BUY)
                {
                    continue;
                }
                var temp = dTotalMIT - lstRecs[i].dAmount;

                if (temp >= 0)
                {
                    dRet += lstRecs[i].dAmount * lstRecs[i].dPrice;
                }
                else
                {
                    dRet += dTotalMIT * lstRecs[i].dPrice;
                    break;
                }
                dTotalMIT = temp;
            }

            return dRet;
        }
        #endregion
    }


}
