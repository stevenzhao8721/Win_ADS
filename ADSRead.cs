using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace Win_ADS
{
    public class ADSRead
    {
        static TcAdsClient Tcads = ADS.Tcads;
        public static T ReadSingle<T>(string PLCName)
        {
            string plcname = "." + PLCName;
            try
            {
                int handle = Tcads.CreateVariableHandle(plcname);
                T returnData = (T)Tcads.ReadAny(handle, typeof(T));
                Tcads.DeleteVariableHandle(handle);
                return returnData;
            }
            catch
            {
                return default(T);
            }

        }

        public static T[] ReadArray<T>(int Size, string PLCName)
        {
            try
            {
                T[] PLCvalue = new T[Size];
                for (int j = 0; j < Size; j++)
                {
                    string plcindex = "[" + j.ToString() + "]";
                    PLCvalue[j] = ReadSingle<T>(PLCName + plcindex);
                }

                return PLCvalue;
            }

            catch
            {
                return null;
            }
        }
    }
}
