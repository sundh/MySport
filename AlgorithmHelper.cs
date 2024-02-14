using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SPLib
{
    /// <summary>
    /// 16进制使用的隔离符枚举
    /// </summary>
    public enum Enum16Hex
    {

        None,  //无
        Blank, //空格
        OX,    //OX
        Ox     //ox
    }
    /// <summary>
    /// 计算进制助手类
    /// </summary>
    public class AlgorithmHelper
    {
        # region 十进制转十六进制
        public string From10To16(int d)
        {
            string hex = "";
            if (d < 16)
            {
                hex = BeginChange(d);
            }
            else
            {
                int c;
                int s = 0;
                int n = d;
                int tmp = d;
                while ( n>= 16) {
                    s++;
                    n = n / 16;
                }
                string[] m = new string[s];
                int i = 0;
                do
                {
                    c = d / 16;
                    m[i++] = BeginChange(d % 16);// 判断是否大于10，如果大于10，则转换为A-F的格式
                    d = c;
                } while (c >= 16);
                hex = BeginChange(d);//最后一位小于16的数的处理
                for (int j = m.Length - 1; j >= 0; j--) {
                    hex += m[j];
                }
            }
            return hex;
        }

        public ushort CalculateCRC16(byte[] data) {
            ushort crc = 0xFFFF;
            ushort polynomial = 0xA001;//CRC-16 polynomial
            foreach (byte b in data) {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++) {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (ushort)((crc << 1) ^ polynomial);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }
            return crc;
        }

        /// <summary>
        /// 获取CRC16校验码
        /// </summary>
        /// <param name="value">校验数据</param>
        /// <returns>校验码 </returns>
        /// <exception cref="ArgumentException">异常对象</exception>
        public static string CRC16(byte[] value) {
            ushort poly = 0xA001;
            ushort crcInit = 0xFFFF;
            if (value == null || !value.Any()) {
                throw new ArgumentException("生成CRC的入参有误！");
            }
            ushort crc = crcInit;
            for (int i = 0; i < value.Count(); i++) {
                crc = (ushort)(crc ^ (value[i]));
                for (int j = 0; j < 8; j++) {
                    crc = (crc & 1) == 1 ? (ushort)(crc >> 1 ^ poly) : (ushort)(crc >> 1);
                }
            }
            string crcStr = Convert.ToString(crc, 16); //字节码转16进制字符串
            if (crcStr.Length < 2) { crcStr = "000" + crcStr; }
            else if (crcStr.Length < 3) { crcStr = "00" + crcStr; }
            else if (crcStr.Length < 4) { crcStr = "0" + crcStr; }
           
            return crcStr.Substring(2, 2).ToUpper() + crcStr.Substring(0, 2).ToUpper(); //高低位互换 

        }

        public string BeginChange(int d) {
            string hex = "";
            switch (d)
            {
                case 10:
                    hex = "A";break;
                case 11:
                    hex = "B"; break;
                case 12:    
                    hex = "C"; break;   
                case 13:        
                        hex = "D"; break;
                case 14:    
                    hex = "E"; break;   
                case 15:    
                     hex = "F"; break;
                default: hex = d.ToString(); break;
            }
            return hex;
        }
        #endregion
        //把16进制隔离符转换成实际的字符串 
        private string AddSplitString(Enum16Hex enum16) { 
          switch(enum16)
            {
                case Enum16Hex.None:
                    return "";
                case Enum16Hex.Ox:
                    return "0x";
                case Enum16Hex.OX:
                    return "0x";
                case Enum16Hex.Blank:
                    return " ";
                default: return "";

            }
        }

        /// <summary>
        /// 去掉16进制字符串中的隔离符【如:" ","0x","0X"】
        /// </summary>
        /// <param name="inString"></param>
        /// <returns></returns>
        private string DeleteSplitString(string inString) {
            string outString = "";
            string[] delArrary = {" ","0x","0X" };
            if (inString.Contains(" ") || inString.Contains("0X") || inString.Contains("0x")){
                string[] str = inString.Split(delArrary, System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < str.Length; i++)
                {
                    outString += str[i].ToString();
                }
                return outString;
            } else
            {
                return inString;
            }
           
        }
        /// <summary>
        /// 字符串转换成16进制
        /// </summary>
        /// <param name="inString">要转换的字符串</param>
        /// <param name="enum16">分隔符（枚举类型）None //无 Blank//空格 OX //OX Ox//ox
        /// </param>
        /// <returns></returns>
        public string StringTo16(string inString, Enum16Hex enum16) {
            string outString = "";
            byte[] bytes = Encoding.Default.GetBytes(inString);
            for (int i = 0; i < bytes.Length; i++) {
                int strInt = Convert.ToInt16(bytes[i] - '\0');
                string s = strInt.ToString("X");
                if (s.Length == 1) {
                    s = "0" + s;
                }
                s = s + AddSplitString(enum16);
                outString += s;
            }
            return outString;
           
        
        }
        /// <summary>
        /// 字符串转换成byte[]
        /// </summary>
        /// <param name="inString"></param>
        /// <returns></returns>
        private byte[] StringToBytes(string inString) {
            inString = StringTo16(inString, Enum16Hex.None);//把字符串转换成16进制数
            return From16ToBytes(inString);//把16进制数转换成Byte
        }
        /// <summary>
        /// 把16进制字符串转换成byte[]
        /// </summary>
        /// <param name="inString"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] From16ToBytes(string inString) {
            inString = DeleteSplitString(inString);//去掉16进制中的隔离符
            byte[] stringByte = new byte[inString.Length / 2];
            for (int a = 0, b = 0; a < inString.Length; a = a + 2, b++)
            {
                try
                {
                    string str = inString.Substring(a, 2);
                    stringByte[b] = (byte)Convert.ToInt16(str, 16);
                }
                catch (Exception ex) {
                    throw new Exception("输入的数据格式不是纯16进制数！参考错误信息："+ex.Message);
                }
            }
            return stringByte;
        }
        /// <summary>
        /// 把16进制字符串转换成英文数字和汉字混合格式
        /// </summary>
        /// <param name="inString"></param>
        /// <returns></returns>
        public string From16ToString(string inString) { 
          inString=DeleteSplitString(inString);
            return Encoding.Default.GetString(From16ToBytes(inString));
        
        }
        /// <summary>
        /// 把byte[]转换成string
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="enum16"></param>
        /// <returns></returns>
        public string BytesToString(byte[] bytes, Enum16Hex enum16) {
            return From16ToString(BytesTo16(bytes,enum16));
        
        }



        public string BytesTo16(byte[] bytes, Enum16Hex enum16) {
            string outString = "";
            for (int i = 0; i < bytes.Length; i++) {
                if (bytes[i].ToString("X").Length < 2) //16进制前面填充0
                {
                    outString += "0" + bytes[i].ToString("X") + AddSplitString(enum16);
                }
                else
                {
                    outString += bytes[i].ToString("X") + AddSplitString(enum16);
                }
            }
            return outString;
        }
        /// <summary>
        /// 把byte[] 直接转换成字符串，直接以2进制形式显示出来
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="enum16"></param>
        /// <returns></returns>
        public string BytesTo2String(byte[] bytes,Enum16Hex enum16) {
            string outString = "";
            for (int i = 0; i < bytes.Length; i++) {
                string tempString = Convert.ToString(bytes[i], 2);
                if (tempString.Length != 8)
                {
                    string add0 = "";
                    for (int j = 0; j < 8 - tempString.Length; j++)
                    {
                        add0 += "0";
                    }
                    outString += add0 + tempString + AddSplitString(enum16);
                }
                else {
                    outString += tempString + AddSplitString(enum16);
                }

            }
            return outString;
        }
        /// <summary>
        /// 把字符串转换成一个byte数组
        /// </summary>
        /// <param name="inString"></param>
        /// <param name="is16"></param>
        /// <returns></returns>
        public byte[] StringToBytes(string inString, bool is16) {
            if (is16)
            {
                return From16ToBytes(inString);
            }
            else
            { 
            return StringToBytes(inString);
            }
        }      
    }
}
