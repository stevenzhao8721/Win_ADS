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
        public static void WriteSingle(string PLCName, object value, bool AutoReset = false)
        {
            string plcname = "." + PLCName;
            try
            {
                ITcAdsSymbol5 info = (ITcAdsSymbol5)Tcads.ReadSymbolInfo(plcname);
                int handle = Tcads.CreateVariableHandle(plcname);
                switch (info.TypeName)
                {
                    case "SINT":
                        sbyte sbdata = Convert.ToSByte(value);
                        Tcads.WriteAny(handle, sbdata);
                        break;
                    case "BYTE":
                        byte bydata = Convert.ToByte(value);
                        Tcads.WriteAny(handle, bydata);
                        break;
                    case "BOOL":
                        bool bdata = Convert.ToBoolean(value);
                        Tcads.WriteAny(handle, bdata);
                        if (AutoReset)
                            TobeResetList.Add(plcname);
                        break;
                    case "INT":
                        short int16data = Convert.ToInt16(value);
                        Tcads.WriteAny(handle, int16data);
                        break;
                    case "UINT":
                        ushort uint16data = Convert.ToUInt16(value);
                        Tcads.WriteAny(handle, uint16data);
                        break;
                    case "REAL":
                        float floatdata = Convert.ToSingle(value);
                        Tcads.WriteAny(handle, floatdata);
                        break;
                    case "DINT":
                        int intdata = Convert.ToInt32(value);
                        Tcads.WriteAny(handle, intdata);
                        break;
                    case "UDINT":
                        uint uintdata = Convert.ToUInt32(value);
                        Tcads.WriteAny(handle, uintdata);
                        break;
                    case "LINT":
                        long longdata = Convert.ToInt64(value);
                        Tcads.WriteAny(handle, longdata);
                        break;
                    case "ULINT":
                        ulong ulongdata = Convert.ToUInt64(value);
                        Tcads.WriteAny(handle, ulongdata);
                        break;
                    case "LREAL":
                        double doubledata = Convert.ToDouble(value);
                        Tcads.WriteAny(handle, doubledata);
                        break;
                }
                Tcads.DeleteVariableHandle(handle);
            }
            catch (Exception ex)
            {
                ErrorFile.ErrorLog(ex.Message, ADS.Logfilepath);

            }
        }

        private static void WriteSingleprivate(string PLCName, bool value)
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

        public async static Task<bool> WriteArrayAsync(string PLCName, object[] value)
        {
            string plcname = "." + PLCName;
            return await Task.Run(() =>
            {
                try
                {
                    ITcAdsSymbol5 info = (ITcAdsSymbol5)Tcads.ReadSymbolInfo(plcname);
                    string plctype = info.TypeName.Split(' ')[3];
                    int handle = Tcads.CreateVariableHandle(plcname);
                    int length = value.Length;
                    int i = 0;
                    switch (plctype)
                    {
                        case "SINT":
                            sbyte[] sbdata = new sbyte[length];
                            foreach (object svalue in value)
                            {
                                sbdata[i] = Convert.ToSByte(svalue);
                                i++;
                            }
                            Tcads.WriteAny(handle, sbdata);
                            break;
                        case "BYTE":
                            byte[] bydata = new byte[length];
                            foreach (object svalue in value)
                            {
                                bydata[i] = Convert.ToByte(svalue);
                                i++;
                            }
                            Tcads.WriteAny(handle, bydata);
                            break;
                        case "BOOL":
                            bool[] bdata = new bool[length];
                            foreach (object svalue in value)
                            {
                                bdata[i] = Convert.ToBoolean(svalue);
                                i++;
                            }
                            Tcads.WriteAny(handle, bdata);
                            break;
                        case "INT":
                            short[] int16data = new short[length];
                            foreach (object svalue in value)
                            {
                                int16data[i] = Convert.ToInt16(svalue);
                                i++;
                            }
                            Tcads.WriteAny(handle, int16data);
                            break;
                        case "UINT":
                            ushort[] uint16data = new ushort[length];
                            foreach (object svalue in value)
                            {
                                uint16data[i] = Convert.ToUInt16(svalue);
                                i++;
                            }
                            Tcads.WriteAny(handle, uint16data);
                            break;
                        case "REAL":
                            float[] floatdata = new float[length];
                            foreach (object svalue in value)
                            {
                                floatdata[i] = Convert.ToSingle(svalue);
                                i++;
                            }
                            Tcads.WriteAny(handle, floatdata);
                            break;
                        case "DINT":
                            int[] intdata = new int[length];
                            foreach (object svalue in value)
                            {
                                intdata[i] = Convert.ToInt32(svalue);
                                i++;
                            }
                            Tcads.WriteAny(handle, intdata);
                            break;
                        case "UDINT":
                            uint[] uintdata = new uint[length];
                            foreach (object svalue in value)
                            {
                                uintdata[i] = Convert.ToUInt32(svalue);
                                i++;
                            }
                            Tcads.WriteAny(handle, uintdata);
                            break;
                        case "LINT":
                            long[] longdata = new long[length];
                            foreach (object svalue in value)
                            {
                                longdata[i] = Convert.ToInt64(svalue);
                                i++;
                            }
                            Tcads.WriteAny(handle, longdata);
                            break;
                        case "ULINT":
                            ulong[] ulongdata = new ulong[length];
                            foreach (object svalue in value)
                            {
                                ulongdata[i] = Convert.ToUInt64(svalue);
                                i++;
                            }
                            Tcads.WriteAny(handle, ulongdata);
                            break;
                        case "LREAL":
                            double[] doubledata = new double[length];
                            foreach (object svalue in value)
                            {
                                doubledata[i] = Convert.ToDouble(svalue);
                                i++;
                            }
                            Tcads.WriteAny(handle, doubledata);
                            break;
                    }
                    Tcads.DeleteVariableHandle(handle);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorFile.ErrorLog(ex.Message, ADS.Logfilepath);
                    return false;
                }
            });
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
