using LogFunc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TwinCAT.Ads;

namespace Win_ADS
{
    public static class ADSWrite
    {
        static TcAdsClient Tcads = ADS.Tcads;
        private static List<string> TobeResetList = new List<string>();
        private static bool ReadyToReset = false;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="PLCName">用nameof(属性名),"前提是属性名和PLC变量名一致"</param>
        /// <param name="value"></param>
        /// <param name="AutoReset">是否自动复位,只对bool量有效</param>
        public static void WriteSingle<T>(string PLCName, T value,bool AutoReset = false)
        {
            string plcname = "." + PLCName;
            try
            {
                int handle = Tcads.CreateVariableHandle(plcname);
                Tcads.WriteAny(handle, value);
                Tcads.DeleteVariableHandle(handle);
                if(typeof(T)==typeof(bool) & AutoReset)
                    TobeResetList.Add(plcname);
            }
            catch (Exception ex)
            {
                ErrorFile.ErrorLog(ex.Message, ADS.Logfilepath);
                
            }
        }

        private static void WriteSingleprivate<T>(string PLCName, T value)
        {
            try
            {
                int handle = Tcads.CreateVariableHandle(PLCName);
                Tcads.WriteAny(handle, value);
                Tcads.DeleteVariableHandle(handle);
            }
            catch (Exception ex)
            {
                ErrorFile.ErrorLog(ex.Message, ADS.Logfilepath);
                
            }
        }

        public static void WriteArray<T>(string PLCName, T[] value)
        {
            string plcname = "." + PLCName;
            try
            {
                int handle = Tcads.CreateVariableHandle(plcname);
                Tcads.WriteAny(handle, value);
                Tcads.DeleteVariableHandle(handle);
            }
            catch (Exception ex)
            {
                ErrorFile.ErrorLog(ex.Message, ADS.Logfilepath);
                
            }
        }

        /// <summary>
        /// 设置自动复位使能和时间
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="settime"></param>
        public static void EnableAutoReset(bool enable = true, int settime = 1000)
        {
            Timer AutoResetSignal = new Timer(settime);
            AutoResetSignal.Enabled = true;
            AutoResetSignal.AutoReset = true;  
            if (enable)
            {
                AutoResetSignal.Start();
                AutoResetSignal.Elapsed += new ElapsedEventHandler(AutoReset);
            }
                
            else
            {
                AutoResetSignal.Stop();
                AutoResetSignal.Elapsed -= new ElapsedEventHandler(AutoReset);
            }
                
            
        }

        private static void AutoReset(object sender, ElapsedEventArgs e)
        {
            if (TobeResetList.Count > 0)
            {
                //下一扫描周期复位信号
                if (!ReadyToReset)
                {
                    ReadyToReset = true;
                    return;
                }
                if (ReadyToReset)
                {
                    foreach (string variableName in TobeResetList)
                    {
                        WriteSingleprivate(variableName, false);
                    }
                    TobeResetList.Clear();
                    ReadyToReset = false;
                }
            }
        }
    }
}
