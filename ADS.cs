﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;
using LogFunc;

namespace Win_ADS
{
    public static class ADS
    {
        private static bool ConnectOneTime = false;
        private static AdsStream[] adsStreams;
        private static int adsIndex = 0;
        public delegate void ExternalPLCDataChangedEventHandler(TData data, AdsStream adsStream);
        public static event ExternalPLCDataChangedEventHandler ExternalPLCDataChangedEvent;
        private static void E_dataChanged(TData data, AdsStream adsStream) => ExternalPLCDataChangedEvent?.Invoke(data,adsStream);

        public static TcAdsClient Tcads { get; private set; }
        public static string Logfilepath { get => _logfilepath;}

        private static string _logfilepath;

        public struct TData
        {
            public Type ClassType;
            public string PLCName;
            public string VariableName;
            public string PLCType;
            public object value;
            public object loadClass;
            public int streamsize;
            public bool isExternal;
        }

        /// <summary>
        /// 连接到beckhoff PLC
        /// </summary>
        /// <param name="adsAdress">Twincat Adress地址</param>
        /// <param name="AdsstreamSize">预设stream流大小</param>
        /// <param name="logfilepath">日志文件路径</param>
        /// <returns></returns>
        public static bool Connect(string adsAdress, string logfilepath,int AdsstreamSize=100)
        {
            _logfilepath = logfilepath;
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
                catch(Exception ex)
                {
                    ConnectOneTime = false;
                    ErrorFile.ErrorLog(ex.Message, Logfilepath);
                }
            }
            return ConnectOneTime;
        }

        public static void Disconnect()
        {
            if(Tcads!=null)
            {
                Tcads.Dispose();
            }
        }

        private static void ads_callback(object sender, AdsNotificationEventArgs e)
        {
            e.DataStream.Position = e.Offset;
            TData data = (TData)e.UserData;
            PLCDataChanged(data, e.DataStream);
        }

        private static void PLCDataChanged(TData data, AdsStream adsStream)
        {
            if (!data.isExternal)
            {
                string typeName = data.PLCType;
                BinaryReader binaryReader = new BinaryReader(adsStream);
                int streamsize = data.streamsize;
                int loopsize = streamsize;
                switch (typeName)
                {
                    case "SINT":
                        loopsize = streamsize;
                        sbyte[] sbdata = new sbyte[loopsize];
                        for (int i = 0; i < loopsize; i++)
                        {
                            sbdata[i] = binaryReader.ReadSByte();
                        }
                        SetValueToViewModel(data, sbdata);
                        break;
                    case "BYTE":
                        loopsize = streamsize;
                        byte[] bydata = new byte[loopsize];
                        for (int i = 0; i < loopsize; i++)
                        {
                            bydata[i] = binaryReader.ReadByte();
                        }
                        SetValueToViewModel(data, bydata);
                        break;
                    case "BOOL":
                        loopsize = streamsize;
                        bool[] bdata = new bool[loopsize];
                        for (int i = 0; i < loopsize; i++)
                        {
                            bdata[i] = binaryReader.ReadBoolean();
                        }
                        SetValueToViewModel(data, bdata);
                        break;
                    case "INT":
                        loopsize = streamsize / 2;
                        short[] int16data = new Int16[loopsize];
                        for (int i = 0; i < loopsize; i++)
                        {
                            int16data[i] = binaryReader.ReadInt16();
                        }
                        SetValueToViewModel(data, int16data);
                        break;
                    case "UINT":
                        loopsize = streamsize / 2;
                        ushort[] uint16data = new ushort[loopsize];
                        for (int i = 0; i < loopsize; i++)
                        {
                            uint16data[i] = binaryReader.ReadUInt16();
                        }
                        SetValueToViewModel(data, uint16data);
                        break;
                    case "REAL":
                        loopsize = streamsize / 4;
                        float[] floatdata = new float[loopsize];
                        for (int i = 0; i < loopsize; i++)
                        {
                            floatdata[i] = binaryReader.ReadSingle();
                        }
                        SetValueToViewModel(data, floatdata);
                        break;
                    case "DINT":
                        loopsize = streamsize / 4;
                        int[] intdata = new int[loopsize];
                        for (int i = 0; i < loopsize; i++)
                        {
                            intdata[i] = binaryReader.ReadInt32();
                        }
                        SetValueToViewModel(data, intdata);
                        break;
                    case "UDINT":
                        loopsize = streamsize / 4;
                        uint[] uintdata = new uint[loopsize];
                        for (int i = 0; i < loopsize; i++)
                        {
                            uintdata[i] = binaryReader.ReadUInt32();
                        }
                        SetValueToViewModel(data, uintdata);
                        break;
                    case "LINT":
                        loopsize = streamsize / 8;
                        long[] longdata = new long[loopsize];
                        for (int i = 0; i < loopsize; i++)
                        {
                            longdata[i] = binaryReader.ReadInt64();
                        }
                        SetValueToViewModel(data, longdata);
                        break;
                    case "ULINT":
                        loopsize = streamsize / 8;
                        ulong[] ulongdata = new ulong[loopsize];
                        for (int i = 0; i < loopsize; i++)
                        {
                            ulongdata[i] = binaryReader.ReadUInt64();
                        }
                        SetValueToViewModel(data, ulongdata);
                        break;
                    case "LREAL":
                        loopsize = streamsize / 8;
                        double[] doubledata = new double[loopsize];
                        for (int i = 0; i < loopsize; i++)
                        {
                            doubledata[i] = binaryReader.ReadDouble();
                        }
                        SetValueToViewModel(data, doubledata);
                        break;
                    default:
                        streamsize = 4;
                        break;
                }
            }
            else
            {
                E_dataChanged(data, adsStream);
            }
        }

        private static void AddSubValue(TData data)
        {
            try
            {
                adsStreams[adsIndex] = new AdsStream(data.streamsize);
                Tcads.AddDeviceNotification(data.PLCName, adsStreams[adsIndex], 0, data.streamsize, AdsTransMode.OnChange, 100, 0, data);
                adsIndex++;
            }
            catch (Exception ex)
            {
                ErrorFile.ErrorLog(ex.Message, Logfilepath);
            }
        }


        private static void SetValueToViewModel<T>(TData data, T[] value)
        {
            try
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
            catch (Exception ex)
            {
                ErrorFile.ErrorLog(ex.Message, Logfilepath);
            }

        }

        /// <summary>
        /// 设置没有ViewModel属性的PLC变量订阅。一般为报警信息
        /// </summary>
        /// <param name="PlcName"></param>
        public static void SetExternalSubscription(string PlcName)
        {
            TData x;
            x.ClassType = default;
            x.loadClass = "";
            x.PLCName = "." + PlcName;
            x.PLCType = "";
            x.value = null;
            x.streamsize = 0;
            x.VariableName = "";
            x.isExternal = true;
            try
            {
                ITcAdsSymbol5 info = (ITcAdsSymbol5)Tcads.ReadSymbolInfo(x.PLCName);
                x.streamsize = info.Size;
                if (info.TypeName.StartsWith("ARRAY"))
                {
                    x.PLCType = info.TypeName.Split(' ')[3];
                }
                else
                {
                    x.PLCType = info.TypeName;
                }
                AddSubValue(x);
            }
            catch (Exception ex)
            {
                ErrorFile.ErrorLog(ex.Message, Logfilepath);
            }
        }

        /// <summary>
        /// 设置变量名与PLC变量不一致的订阅方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="PlcName"></param>
        /// <param name="VariableName"></param>
        /// <param name="model"></param>
        public static void SetSubscription_ManualPropertyNamed<T>(string PlcName, string VariableName,T model)
        {
            TData x;
            x.ClassType = model.GetType();
            x.loadClass = model;
            x.PLCName = "." + PlcName;
            x.PLCType = "";
            x.value = null;
            x.streamsize = 0;
            x.VariableName = VariableName;
            x.isExternal = false;
            try
            {
                ITcAdsSymbol5 info = (ITcAdsSymbol5)Tcads.ReadSymbolInfo(x.PLCName);
                x.streamsize = info.Size;
                if (info.TypeName.StartsWith("ARRAY"))
                {
                    x.PLCType = info.TypeName.Split(' ')[3];
                }
                else
                {
                    x.PLCType = info.TypeName;
                }
                AddSubValue(x);
            }
            catch (Exception ex)
            {
                ErrorFile.ErrorLog(ex.Message, Logfilepath);
            }

        }

        /// <summary>
        /// 变量名与PLC变量一致的订阅方法,遍历类内的属性,把Sub开头的属性自动添加到订阅PLC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">class的名称，通常为调用方的this</param>
        /// <returns></returns>
        public static async Task SetSubscription_AutoPropertyNamed<T>(T model)
        {
            TData x;
            x.ClassType = model.GetType();
            x.loadClass = model;
            x.PLCName = "";
            x.PLCType = "";
            x.value = null;
            x.streamsize = 0;
            x.VariableName = "";
            x.isExternal = false;
            await Task.Run(() =>
            {
                PropertyInfo[] PropertyList = x.ClassType.GetProperties();
                foreach (PropertyInfo item in PropertyList)
                {
                    string name = item.Name;
                    x.VariableName = name;
                    if (name.StartsWith("Sub"))
                    {
                        x.PLCName = "." + name.Split('_')[1];
                        try
                        {
                            ITcAdsSymbol5 info = (ITcAdsSymbol5)Tcads.ReadSymbolInfo(x.PLCName);
                            x.streamsize = info.Size;
                            if (info.TypeName.StartsWith("ARRAY"))
                            {
                                x.PLCType = info.TypeName.Split(' ')[3];
                            }
                            else
                            {
                                x.PLCType = info.TypeName;
                            }
                            AddSubValue(x);
                        }
                        catch (Exception ex)
                        {
                            ErrorFile.ErrorLog(ex.Message, Logfilepath);
                        }
                    }
                }
            });
            
        }
    }
}
