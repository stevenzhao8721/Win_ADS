using System;
using System.Collections.Generic;
using System.IO;
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
        public static T[] ReadArray<T>(string PLCName, int size)
        {
            string plcname = "." + PLCName;
            try
            {
                int handle = Tcads.CreateVariableHandle(plcname);
                T[] returnData = (T[])Tcads.ReadAny(handle, typeof(T[]), new int[] { size });
                Tcads.DeleteVariableHandle(handle);
                return returnData;
            }
            catch
            {
                return default;
            }

        }

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
                return default;
            }
        }
        /// <summary>
        /// 读取大数组数据，一般是曲线数据
        /// </summary>
        /// <param name="PLCName"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static float[] ReadArrayValue_Real(string PLCName, int size)
        {
            try
            {
                int handle = Tcads.CreateVariableHandle(PLCName);
                float[] Curvearray = new float[size];
                // AdsStream which gets the data
                AdsStream dataStream = new AdsStream(size * 4);
                BinaryReader binRead = new BinaryReader(dataStream);
                //read comlpete Array 
                Tcads.Read(handle, dataStream);

                dataStream.Position = 0;
                for (int i = 0; i < size; i++)
                {
                    //保留3位小数
                    Curvearray[i] = binRead.ReadSingle();
                }
                return Curvearray;
            }
            catch
            {
                return null;
            }
        }
    }
}
