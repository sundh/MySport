using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.Dynamic;
using System.Xml.Schema;

namespace SPLib
{

    /// <summary>
    /// 串口助手通用类
    /// </summary>
    public class SeriePortHelper
    {
        private List<byte> buffer = new List<byte>();
        private byte[] ReceiveBytes = null;
        private byte[] RecData = null;  //除校验位的一帧数据
        private byte[] ptData = null;  //校验位

        private SerialPort serialPort = null;
        public SerialPort SerialPortObject { get { return serialPort; } }

        /// <summary>
        /// 获取计算机上可用的端口列表【只读】
        /// </summary>
        public string[] PortsNames
        {
            get
            {
                return System.IO.Ports.SerialPort.GetPortNames();
            }
        }
        private AlgorithmHelper algorithmHelper = null;
        public AlgorithmHelper AlgorithmHelperObject
        {
            get { return algorithmHelper; }
        }
        /// <summary>
        /// 构造函数初始化相关数据
        /// </summary>
        public SeriePortHelper()
        {
            this.serialPort = new SerialPort();
            this.algorithmHelper = new AlgorithmHelper();
            //串口基本参数初始化
            this.serialPort.BaudRate = 9600;
            this.serialPort.Parity = System.IO.Ports.Parity.None;//校验位默认为Noe
            this.serialPort.DataBits = 8;//数据位默认8位

        }
        /// <summary>
        /// 根据端口名称打开或关闭端口
        /// </summary>
        /// <param name="portName">端口名称</param>
        /// <param name="status">操作状态：1 表示打开 0 表示关闭</param>
        /// <returns>返回当前端口的打开状态  true 或 false</returns>
        public bool OpenSerialPort(string portName, int status)
        {
            if (status == 1)
            {
                this.serialPort.PortName = portName;//只有端口在没有开启的时候，才能设置名称
                this.serialPort.Open();
            }
            else
            {
                this.serialPort.Close();
            }
            return this.serialPort.IsOpen;  //返回串口打开的状态
        }

        /// <summary>
        /// 判断十六进制字符串hex是否正确
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        private bool IsIILegalHex(string hex)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(hex, @"([^A-Fa-f0-9]|\S+?)+");
        }

        public void SendData(string data, SendFormat format)
        {
            if (!this.serialPort.IsOpen)
            {
                throw new Exception("端口未打开！请打开相关端口！");

            }
            else
            {
                byte[] byteData;
                if (format == SendFormat.Hex)//如果是16进制
                {
                    if (this.IsIILegalHex(data))
                    {
                        byteData = algorithmHelper.From16ToBytes(data);//将16进制转换成byte[]数组   
                    }
                    else
                    {
                        throw new Exception("数据不是16进制格式！");
                    }

                }
                else
                {
                    byteData = algorithmHelper.StringToBytes(data, false);
                }
                this.serialPort.Write(byteData, 0, byteData.Length);//发送数据 (数据，从0开始

            }
        }
        /// <summary>
        /// 串口接收数据
        /// </summary>
        /// <returns></returns>
        public byte[] ReceiveData()
        {
            int n = serialPort.BytesToRead;
            byte[] buf = new byte[n];
            serialPort.Read(buf, 0, n);
            buffer.AddRange(buf);
            //判断完整性 返回数据为 01 03 04 00 C8 01 CD BB C8
            while (buffer.Count >= 3)
            {   //因为modbus 地址码  1字节  功能码 1字节  数据长度 2 字节   数据位n字节   校验位 2字节
                if (buffer[0] == 0x01)
                {
                    int len = buffer[2];
                    if (buffer.Count < len + 3 + 2)
                    {
                        break;
                    }
                    //得到完整帧
                    ReceiveBytes = new byte[len + 3 + 2];
                    RecData = new byte[len + 3]; //数据位+地址位1字节 功能码1字节 数据长度 1字节
                    if (buffer.Count >= len + 3 + 2)  //数据帧长度大于等于1帧数据的长度时，才进行数据处理
                    {
                        ptData = new byte[2];   //校验位2字节 
                        buffer.CopyTo(0, RecData, 0, len + 3);  //复制数据位
                        buffer.CopyTo(len + 3, ptData, 0, 2);// 复制校验位
                        string sHex = algorithmHelper.BytesTo16(ptData, Enum16Hex.None); //把校验位字节码转为字符串
                        string ptStr = AlgorithmHelper.CRC16(RecData); //计算校验位
                        if (CheckParity(sHex, ptStr))
                        {
                            buffer.CopyTo(0, ReceiveBytes, 0, (len + 3 + 2));
                        }
                        buffer.RemoveRange(0, ReceiveBytes.Length);
                    }
                }
                else
                {
                    buffer.RemoveAt(0);
                }
                return ReceiveBytes;
            }

            return null;


        }

        public bool CheckParity(string sParity, string targetParity)
        {
            if (sParity.ToUpper() == targetParity.ToUpper())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public enum SendFormat
        {
            Hex,  //16进制
            String  //字符串 
        }
    }
}
