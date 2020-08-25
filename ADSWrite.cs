using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace Win_ADS
{
    public static class ADSWrite
    {
        static TcAdsClient Tcads = ADS.Tcads;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="PLCName">用nameof(属性名),"前提是属性名和PLC变量名一致"</param>
        /// <param name="value"></param>
        public static void WriteSingle<T>(string PLCName, T value)
        {
            string plcname = "." + PLCName;
            try
            {
                int handle = Tcads.CreateVariableHandle(plcname);
                Tcads.WriteAny(handle, value);
                Tcads.DeleteVariableHandle(handle);
            }
            catch
            {                
            }
        }

        public static void WriteArray<T>(string PLCName, T[] value)
        {
            string plcname = "." + PLCName;
            Task.Run(() =>
            {
                try
                {
                    int handle = Tcads.CreateVariableHandle(plcname);
                    Tcads.WriteAny(handle, value);
                    Tcads.DeleteVariableHandle(handle);
                }
                catch
                {
                }
            });
            
        }
    }
}
