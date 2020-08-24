using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
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

        partial struct SubArraySet
        {
            public int streamsize;
            public string variableType;
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
                Tcads.AdsNotificationEx += new AdsNotificationExEventHandler(ads_callback);
                Tcads.AdsNotification += new AdsNotificationEventHandler(ads_array_callback);
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

        private static void ads_array_callback(object sender, AdsNotificationEventArgs e)
        {
            e.DataStream.Position = e.Offset;
            TData data = (TData)e.UserData;

        }

        private void ArrayDataSet(TData data,AdsStream adsStream)
        {
            string typeName = data.PLCType;
            BinaryReader binaryReader = new BinaryReader(adsStream);
            int streamsize = 1;
            switch (typeName)
            {
                case "SByte":
                    streamsize = 1;
                    break;
                case "Byte":
                    streamsize = 1;
                    break;
                case "Boolean":
                    streamsize = 1;
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

        private static void AddArrayValue(TData data)
        {
            adsStreams[adsIndex] = new AdsStream(data.streamsize);
            Tcads.AddDeviceNotification(data.PLCName, adsStreams[adsIndex], 0, data.streamsize, AdsTransMode.OnChange, 100, 0, data);
            adsIndex++;
        }


        private static int SetAdsStreamSize(string typeName)
        {
            int size = 1;
            switch (typeName)
            {
                case "SByte":
                    size = 1;
                    break;
                case "Byte":
                    size = 1;
                    break;
                case "Boolean":
                    size = 1;
                    break;
                case "Int16":
                    size = 2;
                    break;
                case "UInt16":
                    size = 2;
                    break;
                case "Single":
                    size = 4;
                    break;
                case "Int32":
                    size = 4;
                    break;
                case "UInt32":
                    size = 4;
                    break;
                case "Int64":
                    size = 8;
                    break;
                case "UInt64":
                    size = 8;
                    break;
                case "Double":
                    size = 8;
                    break;
                default:
                    size = 4;
                    break;
            }
            return size;
        }

        private void SetArrayValue()
        {

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
                    //如果是数组类型
                    if (item.PropertyType.IsArray)
                    {
                        ITcAdsSymbol info = Tcads.ReadSymbolInfo(x.PLCName);
                        //TODO,已经可以获取stream的长度，接下来要把changedEvent的逻辑写一下
                        int streamsize = info.Size;
                        x.PLCType = item.PropertyType.Name.Split('[')[0];
                        x.streamsize = SetAdsStreamSize(x.PLCType);
                        AddArrayValue(x);
                    }
                    //如果是单一变量
                    else
                    {
                        x.PLCType = GetPLCType(x.PLCName);
                        try
                        {
                            switch (x.PLCType)
                            {
                                case "BOOL":
                                    t = typeof(bool);
                                    break;
                                case "REAL":
                                    t = typeof(float);
                                    break;
                                case "UINT":
                                    t = typeof(ushort);
                                    break;
                                case "UDINT":
                                    t = typeof(uint);
                                    break;
                                case "USINT":
                                    t = typeof(byte);
                                    break;
                                default:
                                    t = typeof(object);
                                    break;
                            }
                            Addvalue(t, x);

                        }
                        catch
                        {

                        }
                    }
                }
            }
        }
        /// <summary>
        /// 订阅新的变量
        /// </summary>
        /// <param name="PlcName"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        private static void Addvalue(Type type, TData data)
        {
            Tcads.AddDeviceNotificationEx(data.PLCName, AdsTransMode.OnChange, 100, 0, data, type);
        }


        private static void ads_callback(object sender, AdsNotificationExEventArgs e)
        {
            try
            {
                TData td = (TData)e.UserData;
                td.value = e.Value;
                UpdateSignelVariable(td);
            }
            catch (Exception ex)
            {

            }
        }
        private static void UpdateSignelVariable(TData td)
        {
            string plctype = td.PLCType;
            string variableName = td.VariableName;
            Type type = td.ClassType;
            PropertyInfo pinfo = type.GetProperty(variableName);
            if (pinfo != null)
            {
                switch (plctype)
                {
                    case "BOOL":
                        bool Bvalue = Convert.ToBoolean(td.value);
                        pinfo.SetValue(td.loadClass, Bvalue, null);
                        break;
                    case "REAL":
                        float Fvalue = Convert.ToSingle(td.value);
                        pinfo.SetValue(td.loadClass, Fvalue, null);
                        break;
                    case "UINT":
                        ushort Uvalue = Convert.ToUInt16(td.value);
                        pinfo.SetValue(td.loadClass, Uvalue, null);
                        break;
                    case "UDINT":
                        uint UDvalue = Convert.ToUInt32(td.value);
                        pinfo.SetValue(td.loadClass, UDvalue, null);
                        break;
                    case "USINT":
                        byte USvalue = Convert.ToByte(td.value);
                        pinfo.SetValue(td.loadClass, USvalue, null);
                        break;
                }
            }
        }


        public static string GetPLCType(string plcname)
        {
            try
            {
                ITcAdsSymbol x = Tcads.ReadSymbolInfo(plcname);
                string type = x.Type.ToString();
                return type;
            }
            catch(Exception ex)
            {
                return "";
            }
        }

        public static void readtestArray(string plcname)
        {
            int handle = Tcads.CreateVariableHandle(plcname);
            AdsStream adsst = new AdsStream(20);
            BinaryReader bread = new BinaryReader(adsst);
            Tcads.Read(handle, adsst);
            bool[] x = new bool[20];
            for (int i = 0; i < 20; i++)
            {
                x[i] = bread.ReadBoolean();
            }
        }
    }
}
