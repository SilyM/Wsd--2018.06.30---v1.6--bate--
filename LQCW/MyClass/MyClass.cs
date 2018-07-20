using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using INIFILE;
using System.IO;
using System.IO.Ports;


namespace _LQCW.MyClass
{
    class Myclass
        {
        #region DataGridView布局
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">采集端地址</param>
        /// <param name="b">通道号</param>
        /// <param name="c">缆号</param>
        /// <param name="d">芯片位置号</param>
        /// <param name="e">温度</param>
       
        #endregion

        #region 串口参数预置
        /// <summary>
        /// 串口参数
        /// </summary>
        /// <param name="Baudrate"></param>
        /// <param name="databits"></param>
        /// <param name="stop"></param>
        /// <param name="PARITY"></param>
        public void DecomSerf(ComboBox Baudrate, ComboBox databits, ComboBox stop, ComboBox PARITY)
        {
            //----------------------串口预置--------------------------//
            switch (Profile.G_BAUDRATE)
            {
                case "300":
                    Baudrate.SelectedIndex = 0;
                    break;
                case "600":
                    Baudrate.SelectedIndex = 1;
                    break;
                case "1200":
                    Baudrate.SelectedIndex = 2;
                    break;
                case "2400":
                    Baudrate.SelectedIndex = 3;
                    break;
                case "4800":
                    Baudrate.SelectedIndex = 4;
                    break;
                case "9600":
                    Baudrate.SelectedIndex = 5;
                    break;
                case "19200":
                    Baudrate.SelectedIndex = 6;
                    break;
                case "38400":
                    Baudrate.SelectedIndex = 7;
                    break;
                case "115200":
                    Baudrate.SelectedIndex = 8;
                    break;
                default:
                    {
                        MessageBox.Show("波特率参数错误！");
                        return;
                    }
            }
            switch (Profile.G_DATABITS)
            {
                case "5":
                    databits.SelectedIndex = 0;
                    break;
                case "6":
                    databits.SelectedIndex = 1;
                    break;
                case "7":
                    databits.SelectedIndex = 2;
                    break;
                case "8":
                    databits.SelectedIndex = 3;
                    break;
                default:
                    {
                        MessageBox.Show("数据位参数错误！");
                        return;
                    }

            }

            switch (Profile.G_STOP)
            {
                case "1":
                    stop.SelectedIndex = 0;
                    break;
                case "1.5":
                    stop.SelectedIndex = 1;
                    break;
                case "2":
                    stop.SelectedIndex = 2;
                    break;
                default:
                    {
                        MessageBox.Show("停止位参数错误！");
                        return;
                    }

            }
            switch (Profile.G_PARITY)
            {
                case "wu":
                    PARITY.SelectedIndex = 0;
                    break;
                case "ODD":
                    PARITY.SelectedIndex = 1;
                    break;
                case "EVEN":
                    PARITY.SelectedIndex = 2;
                    break;
                default:
                    {
                        MessageBox.Show("停止位参数错误！");
                        return;
                    }

            }
        }
        #endregion

        #region 获取串口
        public void COMCHECK(ComboBox com)
        {
            string[] strCOM = SerialPort.GetPortNames(); //check COM
            if (strCOM == null)
            {
                MessageBox.Show("本机没有串口！");
                return;
            }

            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
            {
                com.Items.Add(s);
            }
        }
        #endregion

        #region 数据保存至本地
        public void Save_file(string aa, string times,string xinxi)
        {
            DateTime dt = DateTime.Now;
            Directory.CreateDirectory(@"C:\" + "LQCW");
            StreamWriter SW = new StreamWriter(@"C:\" + "LQCW" + "\\" + "_" + xinxi + ".txt", true, Encoding.UTF8);
            SW.Write(dt + " " + aa + "\r\n\r\n");
            SW.Flush();
            SW.Close();
        }
        #endregion

        #region buffer 数据缓存机制


        #endregion

        #region CRC校验plus
        private static ushort[] crc16_table = new ushort[256]
       {
    0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280, 0xC241,
    0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1, 0xC481, 0x0440,
    0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00, 0xCFC1, 0xCE81, 0x0E40,
    0x0A00, 0xCAC1, 0xCB81, 0x0B40, 0xC901, 0x09C0, 0x0880, 0xC841,
    0xD801, 0x18C0, 0x1980, 0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40,
    0x1E00, 0xDEC1, 0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41,
    0x1400, 0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641,
    0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081, 0x1040,
    0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1, 0xF281, 0x3240,
    0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501, 0x35C0, 0x3480, 0xF441,
    0x3C00, 0xFCC1, 0xFD81, 0x3D40, 0xFF01, 0x3FC0, 0x3E80, 0xFE41,
    0xFA01, 0x3AC0, 0x3B80, 0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840,
    0x2800, 0xE8C1, 0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41,
    0xEE01, 0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40,
    0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681, 0x2640,
    0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0, 0x2080, 0xE041,
    0xA001, 0x60C0, 0x6180, 0xA141, 0x6300, 0xA3C1, 0xA281, 0x6240,
    0x6600, 0xA6C1, 0xA781, 0x6740, 0xA501, 0x65C0, 0x6480, 0xA441,
    0x6C00, 0xACC1, 0xAD81, 0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41,
    0xAA01, 0x6AC0, 0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840,
    0x7800, 0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41,
    0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81, 0x7C40,
    0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1, 0xB681, 0x7640,
    0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101, 0x71C0, 0x7080, 0xB041,
    0x5000, 0x90C1, 0x9181, 0x5140, 0x9301, 0x53C0, 0x5280, 0x9241,
    0x9601, 0x56C0, 0x5780, 0x9741, 0x5500, 0x95C1, 0x9481, 0x5440,
    0x9C01, 0x5CC0, 0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40,
    0x5A00, 0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841,
    0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81, 0x4A40,
    0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0, 0x4C80, 0x8C41,
    0x4400, 0x84C1, 0x8581, 0x4540, 0x8701, 0x47C0, 0x4680, 0x8641,
    0x8201, 0x42C0, 0x4380, 0x8341, 0x4100, 0x81C1, 0x8081, 0x4040
       };




        private static int crc16_byte(int crc, byte data)
        {
            return (crc >> 8) ^ crc16_table[(crc ^ data) & 0xff];
        }

        public  byte[] CRC16(byte[] buffer, int len)
        {
            int crc = 0;
            for (int i = 0; i < len; i++)
            {
                crc = crc16_byte(crc, buffer[i]);
            }
            byte[] returnVal = new byte[2];
            returnVal = BitConverter.GetBytes(crc);
            return returnVal;
        }

        #endregion

        #region CRC 校验

        //public byte[]  CRC16(byte[] data)
        //{
        //    byte[] returnVal = new byte[2];
        //    byte CRC16Lo, CRC16Hi, CL, CH, SaveHi, SaveLo;
        //    int i, Flag;
        //    CRC16Lo = 0xFF;
        //    CRC16Hi = 0xFF;
        //    CL = 0x00;
        //    CH = 0x00;
        //    for (i = 0; i < data.Length; i++)
        //    {
        //        CRC16Lo = (byte)(CRC16Lo ^ data[i]);//每一个数据与CRC寄存器进行异或
        //        for (Flag = 0; Flag <= 7; Flag++)
        //        {
        //            SaveHi = CRC16Hi;
        //            SaveLo = CRC16Lo;
        //            CRC16Hi = (byte)(CRC16Hi >> 1);//高位右移一位
        //            CRC16Lo = (byte)(CRC16Lo >> 1);//低位右移一位
        //            if ((SaveHi & 0x01) == 0x01)//如果高位字节最后一位为
        //            {
        //                CRC16Lo = (byte)(CRC16Lo | 0x80);//则低位字节右移后前面补 否则自动补0
        //            }
        //            if ((SaveLo & 0x01) == 0x01)//如果LSB为1，则与多项式码进行异或
        //            {
        //                CRC16Hi = (byte)(CRC16Hi ^ CH);
        //                CRC16Lo = (byte)(CRC16Lo ^ CL);
        //            }
        //        }
        //    }
        //    returnVal[0] = CRC16Hi;//CRC高位
        //    returnVal[1] = CRC16Lo;//CRC低位
        //    return returnVal;
        //}

        #endregion

        #region StringToHEX
        public byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        #endregion

        #region 配置参数回复
        public void ConfigRe(List<byte> buffer,SerialPort Sp1,RadioButton Auto,RadioButton MT,TextBox CJDID)
        {

            
            byte[] bufferback = new byte[17];
            byte[] crc = new byte[9];
            byte[] crcreturn = new byte[2];
            CJDID.Text = buffer[6].ToString("x2") + buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2");
            bufferback[0] = 0x5F; bufferback[1] = 0x5F;
            bufferback[2] = 0x00; bufferback[3] = 0x0B;
            bufferback[4] = buffer[6]; bufferback[5] = buffer[7]; bufferback[6] = buffer[8]; bufferback[7] = buffer[9];
            bufferback[8] = buffer[4]; bufferback[9] = buffer[5]; bufferback[10] = buffer[10];
            bufferback[11] = 0x22; bufferback[12] = 0x00;
            for (int i = 4, j = 0; i < 13; i++, j++)
            {
                crc[j] = bufferback[i];
            }
            crcreturn = CRC16(crc,crc.Length);
            bufferback[13] = crcreturn[1];
            bufferback[14] = crcreturn[0];
            bufferback[15] = 0x55; bufferback[16] = 0xAA;
            byte[] serfbyte = new byte[bufferback.Length];
            for (int ii = 0; ii < bufferback.Length; ii++)
            {
                serfbyte[ii] = bufferback[ii];
            }
            if (Auto.Checked)
            {
                Sp1.Write(serfbyte, 0, serfbyte.Length);
            }
            
        }
        #endregion

        #region 允许接入帧
        public void Request(List<byte> buffer , ComboBox comboBox1,ComboBox comboBox2, ComboBox comboBox3,SerialPort Sp1,Label dianlanhao1, Label dianlanhao2, Label dianlanhao3, Label dianlanhao4,
            Label ICshuliang1, Label ICshuliang2, Label ICshuliang3, Label ICshuliang4, Label youchuchonghao1, Label youchuchonghao2, Label youchuchonghao3, Label youchuchonghao4,
            Label DLHSFxiangtong1, Label DLHSFxiangtong2, Label DLHSFxiangtong3, Label DLHSFxiangtong4,TextBox CJDID)
        {

            CJDID.Text = buffer[6].ToString("x2") + buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2");
            string[] CableNumbers = new string[4];
            string[] ICNumbers = new string[4];
            string[] ICStates = new string[4];
            string[] CableStates = new string[4];
            byte[] PassConfig = new byte[12];
            buffer.CopyTo(14, PassConfig, 0, PassConfig.Length);
            for (int i = 0, j = 0; i < PassConfig.Length; i += 3, j++)
            {
                char[] State = new char[8];
                StringBuilder pass = new StringBuilder();
                byte[] States = new byte[3];
                States[0] = PassConfig[i];
                pass.Append(States[0]);
                for (int k = 0; k < 1; k++)
                {
                    string zh = Convert.ToString(States[k], 2);
                    string tp = zh.PadLeft(8, '0');
                    pass.Append(tp);
                }
                pass.CopyTo(0, State, 0, State.Length);
                if (State[6] == 1)
                {
                    ICStates[j] = "不相同";
                }
                else
                {
                    ICStates[j] = "相同";
                }
                if (State[7] == 1)  
                {
                    CableStates[j] = "有";
                }
                else
                {
                    CableStates[j] = "无";
                }


                CableNumbers[j] = Convert.ToInt32(PassConfig[i + 1]).ToString();
                ICNumbers[j] = Convert.ToInt32(PassConfig[i + 2]).ToString();

            }
            dianlanhao1.Text = CableNumbers[0].ToString(); dianlanhao2.Text = CableNumbers[1].ToString(); dianlanhao3.Text = CableNumbers[2].ToString(); dianlanhao4.Text = CableNumbers[3].ToString();
            ICshuliang1.Text = ICNumbers[0].ToString(); ICshuliang2.Text = ICNumbers[1].ToString(); ICshuliang3.Text = ICNumbers[2].ToString(); ICshuliang4.Text = ICNumbers[3].ToString();
            youchuchonghao1.Text = ICStates[0].ToString(); youchuchonghao2.Text = ICStates[1].ToString(); youchuchonghao3.Text = ICStates[2].ToString(); youchuchonghao4.Text = ICStates[3].ToString();
            DLHSFxiangtong1.Text = CableStates[0].ToString(); DLHSFxiangtong2.Text = CableStates[1].ToString(); DLHSFxiangtong3.Text = CableStates[2].ToString(); DLHSFxiangtong4.Text = CableStates[3].ToString();

            byte[] bufferback = new byte[20];
            byte[] crc = new byte[12];
            byte[] crcreturn = new byte[2];
            byte[] Phase = new byte[2];
            string phase = Convert.ToString((Convert.ToInt32(comboBox1.Text) * 3600 + Convert.ToInt32(comboBox2.Text) * 60 + Convert.ToInt32(comboBox3.Text)), 16).PadLeft(4, '0');
            Phase = strToToHexByte(phase); 
            bufferback[0] = 0x5F; bufferback[1] = 0x5F;
            bufferback[2] = 0x00; bufferback[3] = 0x0E;
            bufferback[4] = buffer[6]; bufferback[5] = buffer[7]; bufferback[6] = buffer[8]; bufferback[7] = buffer[9];
            bufferback[8] = buffer[4]; bufferback[9] = buffer[5]; bufferback[10] = buffer[10];
            bufferback[11] = 0x02; bufferback[12] = 0x01; bufferback[13] = 0x01;
            bufferback[14] = Phase[0]; bufferback[15] = Phase[1];
            for (int i = 4, j = 0; i < 16; i++, j++)
            {
                crc[j] = bufferback[i];
            }
            crcreturn = CRC16(crc,crc.Length);
            bufferback[16] = crcreturn[1];
            bufferback[17] = crcreturn[0];
            bufferback[18] = 0x55; bufferback[19] = 0xAA;
            byte[] serfbyte = new byte[bufferback.Length];
            for (int ii = 0; ii < 20; ii++)
            {
                serfbyte[ii] = bufferback[ii];
            }
            Sp1.Write(serfbyte, 0, serfbyte.Length);
        }
        #endregion

        #region 不回复的接入帧
        public void NoRPrequest(List<byte> buffer, SerialPort Sp1, Label dianlanhao1, Label dianlanhao2, Label dianlanhao3, Label dianlanhao4,
            Label ICshuliang1, Label ICshuliang2, Label ICshuliang3, Label ICshuliang4, Label youchuchonghao1, Label youchuchonghao2, Label youchuchonghao3, Label youchuchonghao4,
            Label DLHSFxiangtong1, Label DLHSFxiangtong2, Label DLHSFxiangtong3, Label DLHSFxiangtong4)
        {
            string[] CableNumbers = new string[4];
            string[] ICNumbers = new string[4];
            string[] ICStates = new string[4];
            string[] CableStates = new string[4];
            byte[] PassConfig = new byte[12];
            buffer.CopyTo(14, PassConfig, 0, PassConfig.Length);
            for (int i = 0, j = 0; i < PassConfig.Length; i += 3, j++)
            {
                char[] State = new char[8];
                StringBuilder pass = new StringBuilder();
                byte[] States = new byte[3];
                States[0] = PassConfig[i];
                pass.Append(States[0]);
                for (int k = 0; k < 1; k++)
                {
                    string zh = Convert.ToString(States[k], 2);
                    string tp = zh.PadLeft(8, '0');
                    pass.Append(tp);
                }
                pass.CopyTo(0, State, 0, State.Length);
                if (State[6] == 1)
                {
                    ICStates[j] = "不相同";
                }
                else
                {
                    ICStates[j] = "相同";
                }
                if (State[7] == 1)
                {
                    CableStates[j] = "有";
                }
                else
                {
                    CableStates[j] = "无";
                }


                CableNumbers[j] = Convert.ToInt32(PassConfig[i + 1]).ToString();
                ICNumbers[j] = Convert.ToInt32(PassConfig[i + 2]).ToString();

            }
            dianlanhao1.Text = CableNumbers[0].ToString(); dianlanhao2.Text = CableNumbers[1].ToString(); dianlanhao3.Text = CableNumbers[2].ToString(); dianlanhao4.Text = CableNumbers[3].ToString();
            ICshuliang1.Text = ICNumbers[0].ToString(); ICshuliang2.Text = ICNumbers[1].ToString(); ICshuliang3.Text = ICNumbers[2].ToString(); ICshuliang4.Text = ICNumbers[3].ToString();
            youchuchonghao1.Text = ICStates[0].ToString(); youchuchonghao2.Text = ICStates[1].ToString(); youchuchonghao3.Text = ICStates[2].ToString(); youchuchonghao4.Text = ICStates[3].ToString();
            DLHSFxiangtong1.Text = CableStates[0].ToString(); DLHSFxiangtong2.Text = CableStates[1].ToString(); DLHSFxiangtong3.Text = CableStates[2].ToString(); DLHSFxiangtong4.Text = CableStates[3].ToString();
        }
        #endregion

        #region 配置命令回复帧
        public void ConfigRP(List<byte> buffer,RadioButton auto)
        {
            if (buffer[13] == 0x01)
            {
                MessageBox.Show("配置修改成功！","提示");
                auto.Checked = true;
            }
            else if (buffer[13] == 0x00)
            {
                MessageBox.Show("配置修改失败！", "提示");
                
            }
        }
        #endregion

        #region 一通道
        public void OP1(List<byte> buffer , Label COM,Label session1,byte[] temperature_data_1, DataTable table,string SourceID,DataGridView dataGridView1)
        {
            string[] Lanhao = new string[32];
            string[] weizhi = new string[32];
            string[] wendu1 = new string[32];
            string[] shidu1 = new string[32];
            int len1 = 0;
            string PassagewayNum1 = Convert.ToString(buffer[14], 10);//第一通道，通道号 

            COM.Text = PassagewayNum1;

            len1 = buffer[15];  //第一通道参数组数

            session1.Text = len1.ToString();

            int serfNum = len1 * 6;//参数组数总长度

            temperature_data_1 = new byte[serfNum];

            buffer.CopyTo(16, temperature_data_1, 0, serfNum); //将第一通道参数组数复制至参数数组

            //-----------------------------------//温度参数//--------------------------------------------------//

            for (int i = 0, j = 0; i < temperature_data_1.Length; i += 6, j++)   //电缆号
            {
                if (temperature_data_1[i] == 0xff)
                {
                   Lanhao[j] = "--";
                }
                else
                {
                    string Cablenum = Convert.ToString(temperature_data_1[i], 10);
                    Lanhao[j] = Cablenum.ToString(); //→coble
                }
                


            }

            for (int a = 1, b = 0; a < temperature_data_1.Length; a += 6, b++) //位置号
            {
                if (temperature_data_1[a] == 0xff)
                {
                    weizhi[b] = "--";
                }
                else
                {
                    string localNum = Convert.ToString(temperature_data_1[a], 10);
                    weizhi[b] = localNum;
                }

               
            }
            #region 温度
            for (int c = 2, d = 0; c < temperature_data_1.Length; c += 6, d++) //温度
            {
                if (temperature_data_1[c] == 0x3f && temperature_data_1[c + 1] == 0xff)
                {
                    wendu1[d] = "--";
                }
                else if (temperature_data_1[c] == 0xff && temperature_data_1[c + 1] == 0xff)
                {
                    wendu1[d] = "--";
                }
                else
                {
                    
                    StringBuilder temperature_data = new StringBuilder();
                    byte[] zz = new byte[2] { temperature_data_1[c], temperature_data_1[c + 1] };

                    for (int k = 0; k < 2; k++)
                    {
                        string zh = Convert.ToString(zz[k], 2);
                        string tp = zh.PadLeft(8, '0');
                        temperature_data.Append(tp);
                    }
                    char[] c1c = new char[16];
                    temperature_data.CopyTo(0, c1c, 0, 16);
                    string canshutyep = null;
                    string wenduint = null;
                    string wendudouble = null;
                    string zhengfu = null;
                    for (int m = 0; m < 4; m++)
                    {
                        canshutyep += c1c[m].ToString();

                    }

                    for (int p = 5; p < 12; p++)
                    {
                        wenduint += c1c[p].ToString();

                    }
                    for (int q = 12; q < 16; q++)
                    {
                        wendudouble += c1c[q].ToString();

                    }
                    if (c1c[4].ToString() == "1")
                    {
                        zhengfu = "-";
                    }
                    else
                    {
                        zhengfu = "+";
                    }
                    wendu1[d] = zhengfu + Convert.ToInt32(wenduint, 2).ToString() + "." + Convert.ToInt32(wendudouble, 2);
                   
                }


            }
            #endregion
            
            #region 湿度
            for (int c = 4, d = 0; c < temperature_data_1.Length; c += 6, d++) //湿度
            {
                if (temperature_data_1[c] == 0x0E && temperature_data_1[c + 1] == 0xff)
                {
                    shidu1[d] = "--";
                }
                else if (temperature_data_1[c] == 0x0f )
                {
                    shidu1[d] = "--";
                }else
                {

                    StringBuilder RH_data = new StringBuilder();
                    byte[] zz = new byte[2] { temperature_data_1[c], temperature_data_1[c + 1] };

                    for (int k = 0; k < 2; k++)
                    {
                        string zh = Convert.ToString(zz[k], 2);
                        string tp = zh.PadLeft(8, '0');
                        RH_data.Append(tp);
                    }
                    char[] c1c = new char[16];
                    RH_data.CopyTo(0, c1c, 0, 16);
                    string canshutyep = null;
                    string shiduint = null;
                    string shidudouble = null;

                    for (int m = 0; m < 4; m++)
                    {
                        canshutyep += c1c[m].ToString();

                    }

                    for (int p = 5; p < 12; p++)
                    {
                        shiduint += c1c[p].ToString();

                    }
                    for (int q = 12; q < 16; q++)
                    {
                        shidudouble += c1c[q].ToString();

                    }
                    shidu1[d] = Convert.ToInt32(shiduint, 2).ToString() + "." + Convert.ToInt32(shidudouble, 2) + "%";
                }

            }
            #endregion
            for (int ii = 0; ii < len1; ii++)
            {
                
                DataRow r1 = table.NewRow();

                r1["时间"] = DateTime.Now.ToString();
                r1["采集端地址:端口"] = SourceID + ":" + PassagewayNum1;
                r1["缆号"] = Lanhao[ii];
                r1["地址"] = weizhi[ii];
                r1["温度（℃）"] = wendu1[ii];
                r1["湿度（%RH）"] = shidu1[ii];
                table.Rows.Add(r1);
            }
            
        }
        #endregion

        #region 状态参数回复帧
        public void State(List<byte> buffer, SerialPort Sp1, ComboBox Hours, ComboBox Minutes, ComboBox Second,RadioButton Auto,TextBox GataWayDown,TextBox CJDID)
        {
            CJDID.Text = buffer[6].ToString("x2") + buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2");
            byte[] bufferback = new byte[20];
            byte[] crc = new byte[12];
            byte[] crcreturn = new byte[2];
            byte[] Phase = new byte[2];
            string phase = Convert.ToString((Convert.ToInt32(Hours.Text) * 3600 + Convert.ToInt32(Minutes.Text) * 60 + Convert.ToInt32(Second.Text)), 16).PadLeft(4, '0');
            Phase = strToToHexByte(phase);
            bufferback[0] = 0x5F; bufferback[1] = 0x5F;
            bufferback[2] = 0x00; bufferback[3] = 0x0E;
            bufferback[4] = buffer[6]; bufferback[5] = buffer[7]; bufferback[6] = buffer[8]; bufferback[7] = buffer[9];
            bufferback[8] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(0, 2), 16)); bufferback[9] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(2, 2), 16)); bufferback[10] = buffer[10];
            bufferback[11] = 0x04; bufferback[12] = 0x01; bufferback[13] = 0x01;
            bufferback[14] = Phase[0]; bufferback[15] = Phase[1];
            for (int i = 4, j = 0; i < 16; i++, j++)
            {
                crc[j] = bufferback[i];
            }
            crcreturn = CRC16(crc,crc.Length);
            bufferback[16] = crcreturn[1];
            bufferback[17] = crcreturn[0];
            bufferback[18] = 0x55; bufferback[19] = 0xAA;
            byte[] serfbyte = new byte[bufferback.Length];
            for (int ii = 0; ii < 20; ii++)
            {
                serfbyte[ii] = bufferback[ii];
            }
            if (Auto.Checked)
            {
                Sp1.Write(serfbyte, 0, serfbyte.Length);
            }
            
        }
        #endregion

        #region 控件调整
        public void Adjust(DataGridView dataGridView1)
        {
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            dataGridView1.AutoResizeRows();
        }
        #endregion






    }
}

