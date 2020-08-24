using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace Win_ADS
{
    public class ADS
    {
        private static bool ConnectOneTime = false;
        private static AdsStream[] adsStreams;
        private static int adsIndex = 0;

        public static TcAdsClient Tcads { get; private set; }

        partial struct TData
        {
            public Type ClassType;
            public string PLCName;
            public string VariableName;
            public string PLCType;
            public object value;
            public object loadClass;
            public int streamsize;
        }

        /// <summary>
        /// 连接到beckhoff PLC
        /// </summary>
        /// <param name="adsAdress">Twincat Adress地址</param>
        /// <returns></returns>
        public static bool Connect(string adsAdress, int AdsstreamSize)
        {
            adsStreams = new AdsStream[AdsstreamSize];
            if (!ConnectOneTime)
            {
                Tcads = new TcAdsClient();
                Tcads.AdsNotification += new AdsNotificationEventHandler(ads_callback);
                try
                {
                    Tcads.Connect(adsAdress, 801);
                    ConnectOneTime = true;
                }
                catch
                {
                    ConnectOneTime = false;
                }
            }
            return ConnectOneTime;
        }

        private static void ads_callback(object sender, AdsNotificationEventArgs e)
        {
            e.DataStream.Position = e.Offset;
            TData data = (TData)e.UserData;
            PLCDataChanged(data, e.DataStream);
        }

        private static void PLCDataChanged(TData data, AdsStream adsStream)
        {
            string typeName = data.PLCType;
            BinaryReader binaryReader = new BinaryReader(adsStream);
            int streamsize = data.streamsize;
            int loopsize = streamsize;

            switch (typeName)
            {
                case "SByte":
                    loopsize = streamsize;
                    sbyte[] sbdata = new sbyte[loopsize];
                    for (int i = 0; i < loopsize; i++)
                    {
                        sbdata[i] = binaryReader.ReadSByte();
                    }
                    SetValueToViewModel(data, sbdata);
                    break;
                case "Byte":
                    streamsize = 1;
                    break;
                case "Boolean":
                    loopsize = streamsize;
                    bool[] bdata = new bool[loopsize];
                    for (int i = 0; i < loopsize; i++)
                    {
                        bdata[i] = binaryReader.ReadBoolean();
                    }
                    SetValueToViewModel(data, bdata);
                    break;
                case "Int16":
                    streamsize = 2;
                    break;
                case "UInt16":
                    streamsize = 2;
                    break;
                case "Single":
                    streamsize = 4;
                    break;
                case "Int32":
                    streamsize = 4;
                    break;
                case "UInt32":
                    streamsize = 4;
                    break;
                case "Int64":
                    streamsize = 8;
                    break;
                case "UInt64":
                    streamsize = 8;
                    break;
                case "Double":
                    streamsize = 8;
                    break;
                default:
                    streamsize = 4;
                    break;
            }
        }

        private static void AddSubValue(TData data)
        {
            adsStreams[adsIndex] = new AdsStream(data.streamsize);
            Tcads.AddDeviceNotification(data.PLCName, adsStreams[adsIndex], 0, data.streamsize, AdsTransMode.OnChange, 100, 0, data);
            adsIndex++;
        }


        private static void SetValueToViewModel<T>(TData data, T[] value)
        {
            string variableName = data.VariableName;
            Type classtype = data.ClassType;
            object calledClass = data.loadClass;
            PropertyInfo pinfo = classtype.GetProperty(variableName);
            if (pinfo != null)
            {
                //如果是单一变量
                if (value.Length == 1)
                    pinfo.SetValue(calledClass, value[0], null);
                //数组变量
                else
                    pinfo.SetValue(calledClass, value, null);
            }
        }

        /// <summary>
        /// 遍历类内的属性，把Sub开头的属性自动添加到订阅PLC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">class的名称，通常为调用方的this</param>
        public static void ForeachClassProperties<T>(T model)
        {
            TData x;
            Type t;
            x.ClassType = model.GetType();
            x.loadClass = model;
            x.PLCName = "";
            x.PLCType = "";
            x.value = null;
            x.streamsize = 0;

            PropertyInfo[] PropertyList = x.ClassType.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                x.VariableName = name;
                if (name.StartsWith("Sub"))
                {
                    object value = item.GetValue(model, null);
                    x.PLCName = "." + name.Split('_')[1];
                    try
                    {
                        ITcAdsSymbol info = Tcads.ReadSymbolInfo(x.PLCName);
                        //TODO,已经可以获取stream的长度，接下来要把changedEvent的逻辑写一下
                        x.streamsize = info.Size;
                        x.PLCType = item.PropertyType.Name.Split('[')[0];
                        AddSubValue(x);
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
