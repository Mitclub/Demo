using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_BCEX_Trading
{
    /// <summary>
    /// 模拟器，获取用户交易MIT的所有信息，开始计算分红
    /// </summary>
    public class CSimulator
    {
        public void Run()
        {
            IMITRefundAPI refund = new CMITRefundAPI2();

            CMITRefundProcessor processor = new CMITRefundProcessor(refund);

            if (CSettings.RefundDateRange > 0)
            {
                CSettings.RefundDate = CSettings.RefundDate.AddDays(-1);
                for (int i = 0; i < CSettings.RefundDateRange; i++)
                {
                    CSettings.RefundDate = CSettings.RefundDate.AddDays(1);
                    Console.WriteLine("\r\n--------------------------开始获取数据计算分红----执行时间：" + CSettings.RefundDate.ToString() + "-------------------------\r\n");
                   
                    CSettings.LoadData(i+1);                   
                    try
                    {
                        processor.Process(CSettings.RefundDate);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                }
            }
            else
            {
                if (CSettings.UserTransactionRecords.IndexOf("Test100") > 0)
                {
                    CSettings.LoadData(100);
                }
                else
                {
                    CSettings.LoadData(1);
                }
                
                processor.Process(CSettings.RefundDate);
            }
        }
    }
}
