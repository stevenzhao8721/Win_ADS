using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace Win_ADS
{
    public class ADS
    {
        private static bool ConnectOneTime = false;

        public static TcAdsClient Tcads { get; private set; }

        partial struct TData
        {
            public Type ClassType;
            public string PLCName;
            public string VariableName;
            public string PLCType;
            public object value;
            public object loadClass;
        }

        /// <summary>
        /// 连接到beckhoff PLC
        /// </summary>
        /// <param name="adsAdress">Twincat Adress地址</param>
        /// <returns></returns>
        public static bool Connect(string adsAdress)
        {
            if(!ConnectOneTime)
            {
                Tcads = new TcAdsClient();
                Tcads.AdsNotificationEx += new AdsNotificationExEventHandler(ads_callback);
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
            PropertyInfo[] PropertyList = x.ClassType.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                try
                {
                    string name = item.Name;
                    x.VariableName = name;
                    object value = item.GetValue(model, null);
                    x.PLCName = "." + name.Split('_')[1];
                    x.PLCType = GetPLCType(x.PLCName);
                    //如果是订阅型变量
                    if (name.StartsWith("Sub"))
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
                        Addvalue(x.PLCName, t, x);
                    }
                }
                catch
                {

                }
            }
        }
        /// <summary>
        /// 订阅新的变量
        /// </summary>
        /// <param name="PlcName"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        private static void Addvalue(string PlcName, Type type, TData data)
        {
            Tcads.AddDeviceNotificationEx(PlcName, AdsTransMode.OnChange, 100, 0, data, type);
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


        private static string GetPLCType(string plcname)
        {
            try
            {
                ITcAdsSymbol x = Tcads.ReadSymbolInfo(plcname);
                string type = x.Type.ToString();
                return type;
            }
            catch
            {
                return "";
            }
        }
    }
}
