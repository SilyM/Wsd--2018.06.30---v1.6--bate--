using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;


namespace _LQCW
{

    public partial class Form1 : Form
    {
        
        SerialPort Sp1 = new SerialPort(); //初始化串口实例
        MyClass.Myclass MyClass = new _LQCW.MyClass.Myclass();
        public Form1()
        {
            InitializeComponent();
        }
   

        private void Form1_Load(object sender, EventArgs e)
        {

          

            MyClass.Myclass MyClass = new _LQCW.MyClass.Myclass();
            INIFILE.Profile.LoadProfile(); //加载本地Config文件

           

            MyClass.DecomSerf(cbBaudRate, cbDataBits, cbStop, cbParity);

            MyClass.COMCHECK(cbSerial);

            Sp1.BaudRate = 115200;
            Control.CheckForIllegalCrossThreadCalls = false;
            Sp1.DataReceived += new SerialDataReceivedEventHandler(Sp1_DataReceived);
            Sp1.DtrEnable = true;
            Sp1.RtsEnable = true;
            Sp1.ReadTimeout =100000 ; //读取时间设定为2s
            Sp1.Close();
            this.dataGridView1.AllowUserToAddRows = false;

            Hours.SelectedText = "00";
            Minutes.SelectedText = "00";
            Second.SelectedText = "30";

            AutoMode.Checked = true;

            DownMode.SelectedIndex = 0;

            groupBox5.Enabled = false;
        }
        //---------------------------------------------------------------------------------------------------------------------//
        #region 串口数据接收 1
        byte[] gwid = new byte[2];
        byte[] caijiid = new byte[4];
        
        void Sp1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(300);

            int len1 = 0;
            int len2 = 0;
            int len3 = 0;
            int len4 = 0;

           
            byte[] temperature_data_1 = null;
            byte[] temperature_data_2 = null;
            byte[] temperature_data_3 = null;
            byte[] temperature_data_4 = null;

          

            DataTable table = new DataTable();

            DataColumn c0 = new DataColumn("时间", typeof(string));
            DataColumn c1 = new DataColumn("采集端地址:端口", typeof(string));
            DataColumn c2 = new DataColumn("缆号", typeof(string));
            DataColumn c3 = new DataColumn("地址", typeof(string));
            DataColumn c4 = new DataColumn("温度（℃）", typeof(string));
            DataColumn c5 = new DataColumn("湿度（%RH）", typeof(string));


            table.Columns.Add(c0);
            table.Columns.Add(c1);
            table.Columns.Add(c2);
            table.Columns.Add(c3);
            table.Columns.Add(c4);
            table.Columns.Add(c5);

            
            


            if (Sp1.IsOpen)
            {
                DateTime dt = DateTime.Now;

                //byte[] byteRead = new byte[Sp1.BytesToRead]; //获取Sp1.数据个数
               List<byte> buffer = new List<byte>(4096 * 2);
               
                try
                {
                    
                    Byte[] receivedData = new Byte[Sp1.BytesToRead];
                    Sp1.Read(receivedData, 0, receivedData.Length);
                    buffer.AddRange(receivedData);  //将数据缓存至List<>
                    int n = buffer.Count;
                    //ushort cd = 0;
                    Sp1.DiscardInBuffer();
                    //cd = (ushort)(cd ^ buffer[2]); cd = (ushort)(cd << 8); cd = (ushort)(cd ^ buffer[3]);

                    string strrcv = null;

                    for (int i = 0; i < receivedData.Length; i++)
                    {
                        strrcv += receivedData[i].ToString("x2");
                    }
                    txtReceive.Text += strrcv + "\r\n";

                    if (txtReceive.TextLength > 500)
                    {
                        txtReceive.Clear();
                    }

                    MyClass.Save_file(strrcv, dt.ToString(), "Sp1");

                    

                    #region 通讯协议解析

                    if (buffer[0] == 0x5F && buffer[1] == 0x5F && buffer[n - 1] == 0xAA && buffer[n - 2] == 0x55)
                    {
                        
                        //if (n < cd)
                        //{
                        //    return;
                        //}

                        #region 配置参数回复
                        if (buffer[11] == 0x21) 
                        {
                            if (AutoMode.Checked)
                            {
                                caijiid[0] = buffer[6]; caijiid[1] = buffer[7]; caijiid[2] = buffer[8]; caijiid[3] = buffer[9];
                                MyClass.ConfigRe(buffer, Sp1,AutoMode,MTMode,COLLID);
                            }
                            
                            return;
                        }
                        #endregion

                        #region 接入请求，允许接入帧
                        if (buffer[11] == 0x01)
                        {
                            if (AutoMode.Checked)
                            {
                                MyClass.Request(buffer, Hours, Minutes, Second, Sp1, CableNumbers1, CableNumbers2, CableNumbers3, CableNumbers4, ICNumbers1, ICNumbers2, ICNumbers3, ICNumbers4, CableStates1, CableStates2, CableStates3, CableStates4, ICStates1, ICStates2, ICStates3, ICStates4,COLLID);
                            }
                            else if (MTMode.Checked)
                            {
                                MyClass.NoRPrequest(buffer, Sp1, CableNumbers1, CableNumbers2, CableNumbers3, CableNumbers4, ICNumbers1, ICNumbers2, ICNumbers3, ICNumbers4, CableStates1, CableStates2, CableStates3, CableStates4, ICStates1, ICStates2, ICStates3, ICStates4);
                            }
                            
                            return;
                        }
                        #endregion

                        #region 状态数据帧

                       

                        if (buffer[11] == 0x03)
                            {
                                #region 目标ID 网关号
                                ushort a = 0;
                                a = (ushort)(a ^ buffer[4]); a = (ushort)(a << 8); a = (ushort)(a ^ buffer[5]);
                                string TargetID = a.ToString("X2");
                                gatawayID.Text = TargetID;
                                #endregion;  

                                #region 源ID 采集端号
                                Int32 b = 0;
                                b = (Int32)(b ^ buffer[6]); b = (Int32)(b << 8); b = (Int32)(b ^ buffer[7]); b = (Int32)(b << 8); b = (Int32)(b ^ buffer[8]); b = (Int32)(b << 8); b = (Int32)(b ^ buffer[9]);
                                string SourceID = b.ToString("X2");

                                #endregion

                                #region 序列号  
                                string SerialNum = Convert.ToString(buffer[10], 10);

                                #endregion

                                #region 通道个数  COM
                                string Passageway = Convert.ToString(buffer[12], 10);

                                #endregion 

                                #region 电池电量
                                string battery = Convert.ToString(buffer[13], 10);
                                BatteryPower.Text = battery.ToString() + "%";
                                #endregion

                                #region 通道数位0
                                if (Convert.ToInt32(Passageway)==0)
                                {
                                    DataRow r1 = table.NewRow();
                                    r1["时间"]= DateTime.Now.ToString(); 
                                    r1["采集端地址:端口"] = SourceID.ToString();
                                    r1["缆号"] = "--";
                                    r1["地址"] = "--";
                                    r1["温度（℃）"] = "--";
                                    r1["湿度（%RH）"] = "--";
                                    table.Rows.Add(r1);
                                    
                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        this.dataGridView1.DataSource = table;
                                    }));
                                    dataGridView1.AutoResizeRows();
                                    gatawayID.Text = TargetID.ToString();
                                    COM.Text = "无";COM2.Text = "";COM3.Text = "";COM4.Text = "";
                                    session1.Text = "无";session2.Text = "";session3.Text = "";session4.Text = "";
                                    MyClass.State(buffer, Sp1, Hours, Minutes, Second, AutoMode, GataWayDown, COLLID);
                                return;

                                }
                                #endregion

                                #region 一通道
                                if (Convert.ToInt16(Passageway) == 1)
                                {
                                MyClass.OP1(buffer, COM, session1, temperature_data_1, table, SourceID, dataGridView1);

                                session2.Text = "";session3.Text = "";session4.Text = "";
                                COM2.Text = "";COM3.Text = "";COM4.Text = "";
                                
                                MyClass.Adjust(dataGridView1);
                                //状态参数回复帧
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    this.dataGridView1.DataSource = table;
                                }));
                                MyClass.State(buffer, Sp1, Hours, Minutes, Second,AutoMode,GataWayDown,COLLID);
                                for(int i = 0;i<dataGridView1.RowCount; i++)
                                {
                                    MyClass.Save_file(dataGridView1.Rows[i].Cells[1].Value.ToString() + "  " + dataGridView1.Rows[i].Cells[2].Value.ToString() + "  " + dataGridView1.Rows[i].Cells[3].Value.ToString() + "  " + dataGridView1.Rows[i].Cells[4].Value.ToString() + "  " + dataGridView1.Rows[i].Cells[5].Value.ToString(), dt.ToString(), "shuju");
                                }
                                
                                 
                                return;

                            }
                                #endregion

                                #region 双通道


                                if (Convert.ToInt16(Passageway) == 2)
                                {
                                string[] Lanhao = new string[32];
                                string[] weizhi = new string[32];
                                string[] wendu1 = new string[32];
                                string[] shidu1 = new string[32];
                                string PassagewayNum1 = Convert.ToString(buffer[14], 10);//第一通道，通道号
                                COM.Text = PassagewayNum1;
                                len1 = buffer[15];  //第一通道参数组数
                                session1.Text = len1.ToString();
                                int serfNum = len1 * 6;//参数组数总个数
                                temperature_data_1 = new byte[serfNum];
                                buffer.CopyTo(16, temperature_data_1, 0, serfNum); //将第一通道参数组数复制至参数数组
                                                                                   //-----------------------------------//温度参数//--------------------------------------------------//
                                for (int i = 0, j = 0; i < temperature_data_1.Length; i += 6, j++)
                                {
                                    if (temperature_data_1[i] == 0xFF)
                                    {
                                        Lanhao[j] = "--";
                                    }
                                    else
                                    {
                                        string Cablenum = Convert.ToString(temperature_data_1[i], 10);
                                        Lanhao[j] = Cablenum;
                                    }
                                    
                                }
                                for (int i = 1, j = 0; i < temperature_data_1.Length; i += 6, j++)
                                {
                                    if (temperature_data_1[i] == 0xff)
                                    {
                                        weizhi[j] = "--";
                                    }
                                    else
                                    {
                                        string localNum = Convert.ToString(temperature_data_1[i], 10);
                                        weizhi[j] = localNum;
                                    }
                                    
                                }

                                for (int i = 2, j = 0; i < temperature_data_1.Length; i += 6, j++)
                                {
                                    if (temperature_data_1[i] == 0x3f && temperature_data_1[i + 1] == 0xff)
                                    {
                                        wendu1[j] = "--";
                                    }
                                    else if (temperature_data_1[i] == 0xff & temperature_data_1[i + 1] == 0xFF)
                                    {
                                        wendu1[j] = "--";
                                    }
                                    else
                                    {
                                     
                                        StringBuilder temperature_data = new StringBuilder();
                                        byte[] zz = new byte[2] { temperature_data_1[i], temperature_data_1[i + 1] };

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
                                        wendu1[j] = zhengfu + Convert.ToInt32(wenduint, 2).ToString() + "." + Convert.ToInt32(wendudouble, 2);

                                    }

                                }
                                #region 湿度1
                                for (int c = 4, d = 0; c < temperature_data_1.Length; c += 6, d++) //湿度
                                {
                                    if (temperature_data_1[c] == 0x0E && temperature_data_1[c + 1] == 0xff)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else if (temperature_data_1[c] == 0x0f)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else
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



                                for (int i = 0; i < len1; i++)
                                {

                                    DataRow r1 = table.NewRow();
                                    r1["时间"] = DateTime.Now.ToString();
                                    r1["采集端地址:端口"] = SourceID + ":" + PassagewayNum1;
                                    r1["缆号"] = Lanhao[i];
                                    r1["地址"] = weizhi[i];
                                    r1["温度（℃）"] = wendu1[i];
                                    r1["湿度（%RH）"] =shidu1[i];
                                    table.Rows.Add(r1);
                                }
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    this.dataGridView1.DataSource = table;
                                }));

                                string PassagewayNum2 = Convert.ToString(buffer[14 + serfNum + 2]); //第二通道，通道号
                                COM2.Text = PassagewayNum2.ToString();
                                len2 = buffer[15 + serfNum + 2]; //第二通道参数组数
                                session2.Text = len2.ToString();
                                int serfNum1 = len2 * 6; //第二通道参数总个数
                                temperature_data_2 = new byte[serfNum1];

                                buffer.CopyTo(16 + serfNum + 2, temperature_data_2, 0, serfNum1);

                                //-----------------------------------//温度参数//--------------------------------------------------//
                                for (int i = 0, j = 0; i < temperature_data_2.Length; i += 6, j++)
                                {
                                    if (temperature_data_2[i] == 0xff)
                                    {
                                        Lanhao[j] = "--";
                                    }
                                    else
                                    {
                                        string Cablenum = Convert.ToString(temperature_data_2[i], 10);
                                        Lanhao[j] = Cablenum;
                                    }
                                 
                                }
                                for (int i = 1, j = 0; i < temperature_data_2.Length; i += 6, j++)
                                {
                                    if (temperature_data_2[i] == 0xff)
                                    {
                                        Lanhao[j] = "--";
                                    }
                                    else
                                    {
                                        string localNum = Convert.ToString(temperature_data_2[i], 10);
                                        weizhi[j] = localNum;
                                    }
                                  
                                }
                                for (int i = 2, j = 0; i < temperature_data_2.Length; i += 6, j++)
                                {
                                    if (temperature_data_2[i] == 0x3f && temperature_data_2[i + 1] == 0xff)
                                    {
                                        wendu1[j] = "--";
                                    }
                                    else if (temperature_data_2[i] == 0xff && temperature_data_2[i + 1] == 0xff)
                                    {
                                        wendu1[j] = "--";
                                    }
                                    else
                                    {
                                     
                                        StringBuilder temperature_data = new StringBuilder();
                                        byte[] zz = new byte[2] { temperature_data_2[i], temperature_data_2[i + 1] };

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
                                        wendu1[j] = zhengfu + Convert.ToInt32(wenduint, 2).ToString() + "." + Convert.ToInt32(wendudouble, 2);

                                    }

                                }
                                #region 湿度2
                                for (int c = 4, d = 0; c < temperature_data_2.Length; c += 6, d++) //湿度
                                {
                                    if (temperature_data_2[c] == 0x0E && temperature_data_2[c + 1] == 0xff)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else if (temperature_data_2[c] == 0x0f )
                                    {
                                        shidu1[c] = "--";
                                    }else
                                    {

                                        StringBuilder RH_data = new StringBuilder();
                                        byte[] zz = new byte[2] { temperature_data_2[c], temperature_data_2[c + 1] };

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
                                for (int i = 0; i < len2; i++)
                                {

                                    DataRow r1 = table.NewRow();
                                    r1["时间"] = DateTime.Now.ToString();
                                    r1["采集端地址:端口"] = SourceID + ":" + PassagewayNum2;
                                    r1["缆号"] = Lanhao[i];
                                    r1["地址"] = weizhi[i];
                                    r1["温度（℃）"] = wendu1[i];
                                    r1["湿度（%RH）"] = shidu1[i];
                                    table.Rows.Add(r1);
                                }
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    this.dataGridView1.DataSource = table;
                                }));
                                MyClass.Adjust(dataGridView1);
                                //状态参数回复帧
                                session3.Text = "";session4.Text = "";
                                COM3.Text = "";COM4.Text = "";
                                MyClass.State(buffer, Sp1, Hours, Minutes, Second,AutoMode,GataWayDown, COLLID);
                                return;
                            }
                                #endregion

                                #region 三通道
                                if (Convert.ToInt16(Passageway) == 3)
                                {
                                    string[] Lanhao = new string[32];
                                    string[] weizhi = new string[32];
                                    string[] wendu1 = new string[32];
                                    string[] shidu1 = new string[32];
                                    string PassagewayNum1 = Convert.ToString(buffer[14], 10);//第一通道，通道号
                                    COM.Text = PassagewayNum1;
                                    len1 = buffer[15];  //第一通道参数组数
                                    session1.Text = len1.ToString();
                                    int serfNum = len1 * 6;//参数组数总个数
                                    temperature_data_1 = new byte[serfNum];
                                    buffer.CopyTo(16, temperature_data_1, 0, serfNum); //将第一通道参数组数复制至参数数组
                                                                                       //-----------------------------------//温度参数1//--------------------------------------------------//
                                    for (int i = 0, j = 0; i < temperature_data_1.Length; i += 6, j++)
                                    {
                                         if (temperature_data_1[i] == 0xff)
                                         { 
                                            Lanhao[j] = "--";
                                         }
                                         else
                                         {
                                            string Cablenum = Convert.ToString(temperature_data_1[i], 10);
                                            Lanhao[j] = Cablenum;
                                         }
                                       
                                    }
                                    for (int i = 1, j = 0; i < temperature_data_1.Length; i += 6, j++)
                                    {
                                    if (temperature_data_1[i] == 0xff)
                                    {
                                        weizhi[j] = "--";
                                    }
                                    else
                                    {
                                        string localNum = Convert.ToString(temperature_data_1[i], 10);
                                        weizhi[j] = localNum;
                                    }
                                        
                                    }
                                    for (int i = 2, j = 0; i < temperature_data_1.Length; i += 6, j++)
                                    {
                                        if (temperature_data_1[i] == 0x3f && temperature_data_1[i + 1] == 0xff)
                                        {
                                            wendu1[j] = "--";
                                        }
                                        else if (temperature_data_1[i] == 0xff && temperature_data_1[i + 1] == 0xff)
                                    {
                                        wendu1[j] = "--";
                                    }else
                                        {
                                            
                                            StringBuilder temperature_data = new StringBuilder();
                                            byte[] zz = new byte[2] { temperature_data_1[i], temperature_data_1[i + 1] };

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
                                            wendu1[j] = zhengfu + Convert.ToInt32(wenduint, 2).ToString() + "." + Convert.ToInt32(wendudouble, 2);
                                        }

                                    }
                                #region 湿度1
                                for (int c = 4, d = 0; c < temperature_data_1.Length; c += 6, d++) //湿度
                                {
                                    if (temperature_data_1[c] == 0x0E && temperature_data_1[c + 1] == 0xff)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else if (temperature_data_1[c] == 0x0f)
                                    {
                                        shidu1[c] = "--";
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
                                for (int i = 0; i < len1; i++)
                                    {

                                        DataRow r1 = table.NewRow();
                                        r1["时间"] = DateTime.Now.ToString();
                                        r1["采集端地址:端口"] = SourceID + ":" + PassagewayNum1;
                                        r1["缆号"] = Lanhao[i];
                                        r1["地址"] = weizhi[i];
                                        r1["温度（℃）"] = wendu1[i];
                                        r1["湿度（%RH）"] = shidu1[i];
                                        table.Rows.Add(r1);
                                    }
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    this.dataGridView1.DataSource = table;
                                }));
                                    dataGridView1.AutoResizeRows();



                                    string PassagewayNum2 = Convert.ToString(buffer[14 + serfNum + 2]); //第二通道，通道号
                                    COM2.Text = PassagewayNum2.ToString();
                                    len2 = buffer[15 + serfNum + 2]; //第二通道参数组数
                                    session2.Text = len2.ToString();
                                    int serfNum1 = len2 * 6; //第二通道参数总个数
                                    temperature_data_2 = new byte[serfNum1];

                                    buffer.CopyTo(16 + serfNum + 2, temperature_data_2, 0, serfNum1);

                                    //-----------------------------------//温度参数//--------------------------------------------------//
                                    for (int i = 0, j = 0; i < temperature_data_2.Length; i += 6, j++)
                                    {
                                    if (temperature_data_2[i] == 0xff)
                                    {
                                        Lanhao[j] = "--";
                                    }
                                    else
                                    {
                                        string Cablenum = Convert.ToString(temperature_data_2[i], 10);
                                        Lanhao[j] = Cablenum;
                                    }
                                        
                                    }
                                    for (int i = 1, j = 0; i < temperature_data_2.Length; i += 6, j++)
                                    {
                                    if (temperature_data_2[i] == 0xff)
                                    {
                                        wendu1[j] = "--";
                                    }
                                    else
                                    {
                                        string localNum = Convert.ToString(temperature_data_2[i], 10);
                                        weizhi[j] = localNum;
                                    }
                                       
                                    }
                                    for (int i = 2, j = 0; i < temperature_data_2.Length; i += 6, j++)
                                    {
                                        if (temperature_data_2[i] == 0x3f && temperature_data_2[i + 1] == 0xff)
                                        {
                                            wendu1[j] = "--";
                                        }
                                        else if (temperature_data_2[i] == 0xff)
                                    {
                                        wendu1[j] = "--";
                                    }
                                    else
                                        {
                                          
                                            StringBuilder temperature_data = new StringBuilder();
                                            byte[] zz = new byte[2] { temperature_data_2[i], temperature_data_2[i + 1] };

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
                                            wendu1[j] = zhengfu + Convert.ToInt32(wenduint, 2).ToString() + "." + Convert.ToInt32(wendudouble, 2);

                                        }

                                    }
                                #region 湿度2
                                for (int c = 4, d = 0; c < temperature_data_2.Length; c += 6, d++) //湿度
                                {
                                    if (temperature_data_2[c] == 0x0E && temperature_data_2[c + 1] == 0xff)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else if (temperature_data_2[c] == 0x0f)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else
                                    {

                                        StringBuilder RH_data = new StringBuilder();
                                        byte[] zz = new byte[2] { temperature_data_2[c], temperature_data_2[c + 1] };

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
                                for (int i = 0; i < len2; i++)
                                    {

                                        DataRow r1 = table.NewRow();
                                        r1["时间"] = DateTime.Now.ToString();
                                        r1["采集端地址:端口"] = SourceID + ":" + PassagewayNum2;
                                        r1["缆号"] = Lanhao[i];
                                        r1["地址"] = weizhi[i];
                                        r1["温度（℃）"] = wendu1[i];
                                        r1["湿度（%RH）"] = shidu1[i];
                                        table.Rows.Add(r1);
                                    }
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    this.dataGridView1.DataSource = table;
                                }));
                                    dataGridView1.AutoResizeRows();



                                    string PassagewayNmu3 = Convert.ToString(buffer[14 + serfNum + serfNum1 + 4]);
                                    COM3.Text = PassagewayNmu3;
                                    len3 = buffer[15 + serfNum + serfNum1 + 4];
                                    session3.Text = len3.ToString();
                                    int serfNum2 = len3 * 6;
                                    temperature_data_3 = new byte[serfNum2];
                                    buffer.CopyTo(16 + serfNum + serfNum1 + 4, temperature_data_3, 0, serfNum2);
                                    //-----------------------------------//温度参数3//--------------------------------------------------//
                                    for (int i = 0, j = 0; i < temperature_data_3.Length; i += 6, j++)
                                    {
                                    if (temperature_data_3[i] == 0xff)
                                    {
                                        Lanhao[j] = "--";
                                    }
                                        string Cablenum = Convert.ToString(temperature_data_3[i], 10);
                                        Lanhao[j] = Cablenum;
                                    }
                                    for (int i = 1, j = 0; i < temperature_data_3.Length; i += 6, j++)
                                    {
                                    if (temperature_data_3[i] == 0xff)
                                    {
                                        weizhi[j] = "--";
                                    }
                                    else
                                    {
                                        string localNum = Convert.ToString(temperature_data_3[i], 10);
                                        weizhi[j] = localNum;
                                    }
                                       
                                    }
                                    for (int i = 2, j = 0; i < temperature_data_3.Length; i += 6, j++)
                                    {

                                        if (temperature_data_3[i] == 0x3f && temperature_data_3[i + 1] == 0xff)
                                        {
                                            wendu1[j] = "--";
                                        }
                                        else if (temperature_data_3[i] == 0xff&&temperature_data_3[i+1]==0xff)
                                        {
                                            wendu1[j] = "--";
                                        }
                                        else
                                        {
                                            
                                            StringBuilder temperature_data = new StringBuilder();
                                            byte[] zz = new byte[2] { temperature_data_3[i], temperature_data_3[i + 1] };

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
                                            wendu1[j] = zhengfu + Convert.ToInt32(wenduint, 2).ToString() + "." + Convert.ToInt32(wendudouble, 2);
                                           
                                        }
                                    }

                                #region 湿度3
                                for (int c = 4, d = 0; c < temperature_data_3.Length; c += 6, d++) //湿度
                                {
                                    if (temperature_data_3[c] == 0x0E && temperature_data_3[c + 1] == 0xff)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else if (temperature_data_3[c] == 0x0f)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else
                                    {

                                        StringBuilder RH_data = new StringBuilder();
                                        byte[] zz = new byte[2] { temperature_data_3[c], temperature_data_3[c + 1] };

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

                                for (int i = 0; i < len3; i++)
                                    {

                                        DataRow r1 = table.NewRow();
                                        r1["时间"] = DateTime.Now.ToString();
                                        r1["采集端地址:端口"] = SourceID + ":" + PassagewayNmu3;
                                        r1["缆号"] = Lanhao[i];
                                        r1["地址"] = weizhi[i];
                                        r1["温度（℃）"] = wendu1[i];
                                        r1["湿度（%RH）"] = shidu1[i];
                                        table.Rows.Add(r1);
                                    }

                                session4.Text = "";
                                COM4.Text = "";
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    this.dataGridView1.DataSource = table;
                                }));
                                    dataGridView1.AutoResizeRows();
                                    for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                                    {
                                        dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                                        this.dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                                    }

                                //状态参数回复帧

                                MyClass.State(buffer, Sp1, Hours, Minutes, Second, AutoMode,GataWayDown, COLLID);
                                return;
                            }
                                #endregion

                                #region 四通道
                                if (Convert.ToInt16(Passageway) == 4)
                                {
                                    string[] Lanhao = new string[32];
                                    string[] weizhi = new string[32];
                                    string[] wendu1 = new string[32];
                                    string[] shidu1 = new string[32];
                                    string PassagewayNum1 = Convert.ToString(buffer[14], 10);//第一通道，通道号
                                    COM.Text = PassagewayNum1;
                                    len1 = buffer[15];  //第一通道参数组数
                                    session1.Text = len1.ToString();
                                    int serfNum = len1 * 6;//参数组数总个数
                                    temperature_data_1 = new byte[serfNum];
                                    buffer.CopyTo(16, temperature_data_1, 0, serfNum); //将第一通道参数组数复制至参数数组

                                    for (int i = 0, j = 0; i < temperature_data_1.Length; i += 6, j++)
                                    {
                                    if (temperature_data_1[i] == 0xff)
                                    {
                                        Lanhao[j] = "--";
                                    }
                                    else
                                    {
                                        string Cablenum = Convert.ToString(temperature_data_1[i], 10);
                                        Lanhao[j] = Cablenum;
                                    }
                                        
                                    }
                                    for (int i = 1, j = 0; i < temperature_data_1.Length; i += 6, j++)
                                    {
                                    if (temperature_data_1[i] == 0xff)
                                    {
                                        weizhi[i] = "--";
                                    }
                                    else
                                    {
                                        string localNum = Convert.ToString(temperature_data_1[i], 10);
                                        weizhi[j] = localNum;
                                    }
                                        
                                    }
                                    for (int i = 2, j = 0; i < temperature_data_1.Length; i += 6, j++)
                                    {

                                        if (temperature_data_1[i] == 0x3f && temperature_data_1[i + 1] == 0xff)
                                        {
                                            wendu1[j] = "--";
                                        }
                                        else if (temperature_data_1[i] == 0xff && temperature_data_1[i + 1] == 0xff)
                                    {
                                        wendu1[j] = "--";
                                    }else
                                        {
                                           
                                            StringBuilder temperature_data = new StringBuilder();
                                            byte[] zz = new byte[2] { temperature_data_1[i], temperature_data_1[i + 1] };

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
                                            wendu1[j] = zhengfu + Convert.ToInt32(wenduint, 2).ToString() + "." + Convert.ToInt32(wendudouble, 2);
                                            
                                        }


                                    }
                                #region 湿度1
                                for (int c = 4, d = 0; c < temperature_data_1.Length; c += 6, d++) //湿度
                                {
                                    if (temperature_data_1[c] == 0x0E && temperature_data_1[c + 1] == 0xff)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else if (temperature_data_1[c] == 0x0f)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else
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
                                for (int i = 0; i < len1; i++)
                                    {

                                        DataRow r1 = table.NewRow();
                                        r1["时间"] = DateTime.Now.ToString();
                                        r1["采集端地址:端口"] = SourceID + ":" + PassagewayNum1;
                                        r1["缆号"] = Lanhao[i];
                                        r1["地址"] = weizhi[i];
                                        r1["温度（℃）"] = wendu1[i];
                                        r1["湿度（%RH）"] = shidu1[i];
                                        table.Rows.Add(r1);
                                    }
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    this.dataGridView1.DataSource = table;
                                }));

                                    dataGridView1.AutoResizeRows();

                                    string PassagewayNum2 = Convert.ToString(buffer[14 + serfNum + 2]); //第二通道，通道号
                                    COM2.Text = PassagewayNum2;
                                    len2 = buffer[15 + serfNum + 2]; //第二通道参数组数
                                    session2.Text = len2.ToString(); 
                                    int serfNum1 = len2 * 6; //第二通道参数总个数
                                    temperature_data_2 = new byte[serfNum1];
                                    buffer.CopyTo(16 + serfNum + 2, temperature_data_2, 0, serfNum1);
                                    //-----------------------------------//温度参数2//--------------------------------------------------//
                                    for (int i = 0, j = 0; i < temperature_data_2.Length; i += 6, j++)
                                    {
                                    if (temperature_data_2[i] == 0xff)
                                    {
                                        Lanhao[j] = "--";

                                    }
                                    else
                                    {
                                        string Cablenum = Convert.ToString(temperature_data_2[i], 10);
                                        Lanhao[j] = Cablenum;
                                    }
                                        
                                    }
                                    for (int i = 1, j = 0; i < temperature_data_2.Length; i += 6, j++)
                                    {
                                    if (temperature_data_2[i] == 0xff)
                                    {
                                        weizhi[j] = "--";
                                    }
                                    else
                                    {
                                        string localNum = Convert.ToString(temperature_data_2[i], 10);
                                        weizhi[j] = localNum;
                                    }
                                        
                                    }
                                    for (int i = 2, j = 0; i < temperature_data_2.Length; i += 6, j++)
                                    {

                                        if (temperature_data_2[i] == 0x3f && temperature_data_2[i + 1] == 0xff)
                                        {
                                            wendu1[j] = "--";
                                        }
                                        else if (temperature_data_2[i] == 0xff && temperature_data_2[i + 1] == 0xff)
                                    {
                                        wendu1[j] = "--";
                                    }else
                                        {
                                            
                                            StringBuilder temperature_data = new StringBuilder();
                                            byte[] zz = new byte[2] { temperature_data_2[i], temperature_data_2[i + 1] };

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
                                            wendu1[j] = zhengfu + Convert.ToInt32(wenduint, 2).ToString() + "." + Convert.ToInt32(wendudouble, 2);
                                            
                                        }

                                    }
                                #region 湿度2
                                for (int c = 4, d = 0; c < temperature_data_2.Length; c += 6, d++) //湿度
                                {
                                    if (temperature_data_2[c] == 0x0E && temperature_data_2[c + 1] == 0xff)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else if (temperature_data_2[c] == 0x0f)
                                    {
                                        shidu1[c] = "--";
                                    }else
                                    {

                                        StringBuilder RH_data = new StringBuilder();
                                        byte[] zz = new byte[2] { temperature_data_2[c], temperature_data_2[c + 1] };

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
                                for (int i = 0; i < len2; i++)
                                    {

                                        DataRow r1 = table.NewRow();
                                        r1["时间"] = DateTime.Now.ToString();
                                        r1["采集端地址:端口"] = SourceID + ":" + PassagewayNum2;
                                        r1["缆号"] = Lanhao[i];
                                        r1["地址"] = weizhi[i];
                                        r1["温度（℃）"] = wendu1[i];
                                        r1["湿度（%RH）"] = shidu1[i];
                                        table.Rows.Add(r1);
                                    }
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    this.dataGridView1.DataSource = table;
                                }));

                                    dataGridView1.AutoResizeRows();


                                    string PassagewayNum3 = Convert.ToString(buffer[14 + serfNum + serfNum1 + 4]);
                                    COM3.Text = PassagewayNum3;
                                    len3 = buffer[15 + serfNum + serfNum1 + 4];
                                    session3.Text = len3.ToString();
                                    int serfNum2 = len3 * 6;
                                    temperature_data_3 = new byte[serfNum2];
                                    buffer.CopyTo(16 + serfNum + serfNum1 + 4, temperature_data_3, 0, serfNum2);
                                    //-----------------------------------//温度参数3//--------------------------------------------------//
                                    for (int i = 0, j = 0; i < temperature_data_3.Length; i += 6, j++)
                                    {
                                    if (temperature_data_3[i] == 0xff)
                                    {
                                        Lanhao[j] = "--";
                                    }
                                    else
                                    {
                                        string Cablenum = Convert.ToString(temperature_data_3[i], 10);
                                        Lanhao[j] = Cablenum;
                                    }
                                        
                                    }
                                    for (int i = 1, j = 0; i < temperature_data_3.Length; i += 6, j++)
                                    {
                                    if (temperature_data_3[i] == 0xff)
                                    {
                                        weizhi[j] = "--";
                                    }
                                    else
                                    {
                                        string localNum = Convert.ToString(temperature_data_3[i], 10);
                                        weizhi[j] = localNum;
                                    }
                                        
                                    }
                                    for (int i = 2, j = 0; i < temperature_data_3.Length; i += 6, j++)
                                    {

                                        if (temperature_data_3[i] == 0x3f && temperature_data_3[i + 1] == 0xff)
                                        {
                                            wendu1[j] = "--";
                                        }
                                        else if (temperature_data_3[i] == 0xff && temperature_data_3[i + 1] == 0xff)
                                    {
                                        wendu1[j] = "--";
                                    }else
                                        {
                                           
                                            StringBuilder temperature_data = new StringBuilder();
                                            byte[] zz = new byte[2] { temperature_data_3[i], temperature_data_3[i + 1] };

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
                                            wendu1[j] = zhengfu + Convert.ToInt32(wenduint, 2).ToString() + "." + Convert.ToInt32(wendudouble, 2);
                                            
                                        }

                                    }
                                #region 湿度3
                                for (int c = 4, d = 0; c < temperature_data_3.Length; c += 6, d++) //湿度
                                {
                                    if (temperature_data_3[c] == 0x0E && temperature_data_3[c + 1] == 0xff)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else if (temperature_data_3[c] == 0x0f)
                                    {
                                        shidu1[c] = "--";
                                    }else
                                    {

                                        StringBuilder RH_data = new StringBuilder();
                                        byte[] zz = new byte[2] { temperature_data_3[c], temperature_data_3[c + 1] };

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
                                for (int i = 0; i < len3; i++)
                                    {

                                        DataRow r1 = table.NewRow();
                                        r1["时间"] = DateTime.Now.ToString();
                                        r1["采集端地址:端口"] = SourceID + ":" + PassagewayNum3;
                                        r1["缆号"] = Lanhao[i];
                                        r1["地址"] = weizhi[i];
                                        r1["温度（℃）"] = wendu1[i];
                                        r1["湿度（%RH）"] = shidu1[i];
                                        table.Rows.Add(r1);
                                    }
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    this.dataGridView1.DataSource = table;
                                }));

                                    dataGridView1.AutoResizeRows();


                                    string PassagewayNum4 = Convert.ToString(buffer[14 + serfNum + serfNum1 + serfNum2 + 6]);
                                    COM4.Text = PassagewayNum4;
                                    len4 = buffer[15 + serfNum + serfNum1 + serfNum2 + 6];
                                    session4.Text = len4.ToString();
                                    int serfNum3 = len4 * 6;
                                    temperature_data_4 = new byte[serfNum3];
                                    buffer.CopyTo(16 + serfNum + serfNum1 + serfNum2 + 6, temperature_data_4, 0, serfNum3);
                                    //-----------------------------------//温度参数3//--------------------------------------------------//
                                    for (int i = 0, j = 0; i < temperature_data_4.Length; i += 6, j++)
                                    {
                                    if (temperature_data_4[i] == 0xff)
                                    {
                                        Lanhao[j]="--";
                                    }
                                    else
                                    {
                                        string Cablenum = Convert.ToString(temperature_data_4[i], 10);
                                        Lanhao[j] = Cablenum;
                                    }
                                        
                                    }
                                    for (int i = 1, j = 0; i < temperature_data_4.Length; i += 6, j++)
                                {
                                    if (temperature_data_4[i] == 0xff)
                                    {
                                        weizhi[j] = "--";
                                    }
                                    else
                                    {
                                        string localNum = Convert.ToString(temperature_data_4[i], 10);
                                        weizhi[j] = localNum;
                                    }
                                       
                                    }
                                    for (int i = 2, j = 0; i < temperature_data_4.Length; i += 6, j++)
                                    {

                                        if (temperature_data_4[i] == 0x3f && temperature_data_4[i + 1] == 0xff)
                                        {
                                            wendu1[j] = "--";
                                        }
                                        else if (temperature_data_4[i] == 0xff && temperature_data_4[i + 1] == 0xff)
                                    {
                                        wendu1[j] = "--";

                                    }else
                                        {
                                           
                                            StringBuilder temperature_data = new StringBuilder();
                                            byte[] zz = new byte[2] { temperature_data_4[i], temperature_data_4[i + 1] };

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
                                            wendu1[j] = zhengfu + Convert.ToInt32(wenduint, 2).ToString() + "." + Convert.ToInt32(wendudouble, 2);
                                            
                                        }

                                    }
                                #region 湿度4
                                for (int c = 4, d = 0; c < temperature_data_4.Length; c += 6, d++) //湿度
                                {
                                    if (temperature_data_4[c] == 0x0E && temperature_data_4[c + 1] == 0xff)
                                    {
                                        shidu1[c] = "--";
                                    }
                                    else if (temperature_data_4[c] == 0x0f)
                                    {
                                        shidu1[c] = "--";
                                    }else
                                    {

                                        StringBuilder RH_data = new StringBuilder();
                                        byte[] zz = new byte[2] { temperature_data_4[c], temperature_data_4[c + 1] };

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
                                for (int i = 0; i < len4; i++)
                                    {

                                        DataRow r1 = table.NewRow();
                                        r1["时间"] = DateTime.Now.ToString();
                                        r1["采集端地址:端口"] = SourceID + ":" + PassagewayNum4;
                                        r1["缆号"] = Lanhao[i];
                                        r1["地址"] = weizhi[i];
                                        r1["温度（℃）"] = wendu1[i];
                                        r1["湿度（%RH）"] = shidu1[i];
                                        table.Rows.Add(r1);
                                    }
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    this.dataGridView1.DataSource = table;
                                }));
                                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                                    {
                                        dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                                        this.dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                                    }
                                    dataGridView1.AutoResizeRows();
                                //状态参数回复帧

                                MyClass.State(buffer, Sp1, Hours, Minutes, Second, AutoMode,GataWayDown, COLLID);
                                return;
                            }
                            #endregion

                                
                            


                        }
                        #endregion

                        #region 配置命令回复帧
                        if (buffer[11] == 0x41)
                        {
                            MyClass.ConfigRP(buffer,AutoMode);
                            return;
                        }
                        #endregion
                    }
                }
                    #endregion
                
                catch 
                {
                   
                }


            }
            else
            {
                MessageBox.Show("请打开某个串口", "错误提示");
            }
            
        }
        #endregion

        private void btnSwitch_Click(object sender, EventArgs e)
        {
            if (!Sp1.IsOpen)
            {
                try
                {
                    string serialName = cbSerial.SelectedItem.ToString();
                    Sp1.PortName = serialName;
                    //串口设置
                    string strBaudRate = cbBaudRate.Text;
                    string strDateBits = cbDataBits.Text;
                    string strStopBits = cbStop.Text;
                    Int32 iBaudRate = Convert.ToInt32(strBaudRate);
                    Int32 idateBits = Convert.ToInt32(strDateBits);

                    Sp1.BaudRate = iBaudRate;
                    Sp1.DataBits = idateBits;
                    Sp1.StopBits = StopBits.One;
                    Sp1.Parity = Parity.None;

                    if (Sp1.IsOpen == true)
                    {
                        Sp1.Close();
                    }
                    tsSpNum.Text = "串口号：" + Sp1.PortName + "|";
                    tsBaudRate.Text = "波特率：" + Sp1.BaudRate + "|";
                    tsDataBits.Text = "数据位：" + Sp1.DataBits + "|";
                    tsStopBits.Text = "停止位：" + Sp1.StopBits + "|";
                    tsParity.Text = "校验位:" + Sp1.Parity + "|";

                    cbSerial.Enabled = false;
                    cbBaudRate.Enabled = false;
                    cbStop.Enabled = false;
                    cbDataBits.Enabled = false;
                    cbParity.Enabled = false;

                    Sp1.Open();

                    btnSwitch.Text = "关闭串口";
                }
                catch(System.Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message, "Error");
                    tmSend.Enabled = false;
                }
            }
            else
            {
                tsSpNum.Text = "串口号：未指定|";
                tsBaudRate.Text = "波特率：未指定|";
                tsDataBits.Text = "数据位：未指定|";
                tsStopBits.Text = "停止位：未指定|";
                tsParity.Text = "校验位:未指定|";

                cbSerial.Enabled = false;
                cbBaudRate.Enabled = false;
                cbStop.Enabled = false;
                cbDataBits.Enabled = false;
                cbParity.Enabled = false;

                cbSerial.Enabled = true;
                cbBaudRate.Enabled = true;
                cbStop.Enabled = true;
                cbDataBits.Enabled = true;
                cbParity.Enabled = true;

                Sp1.Close();
                btnSwitch.Text = "打开串口";
                tmSend.Enabled = false;

            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtReceive.Clear();
            
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            this.dataGridView1.ScrollBars = ScrollBars.Both;
        }

        private void CableStates2_Click(object sender, EventArgs e)
        {

        }

        private void ReadConfig_Click(object sender, EventArgs e)
        {
            if (GataWayDown.Text.Trim() == string.Empty)
            {
                MessageBox.Show("提示！", "请输入网关ID！");
            }
            else if(COLLID.Text.Trim() != String.Empty)
            {
                byte[] Comm = new byte[19];
                Comm[0] = 0x5F; Comm[1] = 0x5F; Comm[2] = 0x00; Comm[3] = 0x0D; Comm[4] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(0, 2), 16)); Comm[5] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(2, 2), 16));
                Comm[6] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(4, 2), 16)); Comm[7] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(6, 2), 16));
                Comm[8] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(0, 2), 16)); Comm[9] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(2, 2), 16));
                Comm[10] = 0x01; Comm[11] = 0x23; Comm[12] = 0x01; Comm[13] = 0xC0; Comm[14] = 0x01;
                byte[] crc = new byte[11];
                byte[] crcreturn = new byte[2];
                for (int i = 4, j = 0; i < 15; i++, j++)
                {
                    crc[j] = Comm[i];
                }
                crcreturn = MyClass.CRC16(crc, crc.Length);
                Comm[15] = crcreturn[1];
                Comm[16] = crcreturn[0];
                Comm[17] = 0x55; Comm[18] = 0xAA;
                Sp1.Write(Comm, 0, Comm.Length);
            }
            else
            {
               
                byte[] Comm = new byte[19];
                Comm[0] = 0x5F; Comm[1] = 0x5F; Comm[2] = 0x00; Comm[3] = 0x0D; Comm[4] = 0xEF; Comm[5] = 0xFF; Comm[6] = 0xFF; Comm[7] = 0xFF;
                Comm[8] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(0, 2), 16)); Comm[9] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(2, 2), 16));
                Comm[10] = 0x01; Comm[11] = 0x23; Comm[12] = 0x01; Comm[13] = 0xC0; Comm[14] = 0x01;
                byte[] crc = new byte[11];
                byte[] crcreturn = new byte[2];
                for (int i = 4, j = 0; i < 15; i++, j++)
                {
                    crc[j] = Comm[i];
                }
                crcreturn = MyClass.CRC16(crc,crc.Length);
                Comm[15] = crcreturn[1];
                Comm[16] = crcreturn[0];
                Comm[17] = 0x55; Comm[18] = 0xAA;
                Sp1.Write(Comm, 0, Comm.Length);
            }
           

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void ReadSX_Click(object sender, EventArgs e)
        {
            try
            {
                if (GataWayDown.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("请输入网关ID", "提示");
                }
                else 
                {
                    if (COLLID.Text.Trim() == string.Empty)
                    {

                        byte[] Comm = new byte[19];
                        Comm[0] = 0x5F; Comm[1] = 0x5F; Comm[2] = 0x00; Comm[3] = 0x0D; Comm[4] = caijiid[0]; Comm[5] = caijiid[1]; Comm[6] = caijiid[2]; Comm[7] = caijiid[3];
                        Comm[8] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(0, 2), 16)); Comm[9] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(2, 2), 16));
                        Comm[10] = 0x01; Comm[11] = 0x23; Comm[12] = 0x01; Comm[13] = 0xC0; Comm[14] = 0x02;
                        byte[] crc = new byte[11];
                        byte[] crcreturn = new byte[2];
                        for (int i = 4, j = 0; i < 15; i++, j++)
                        {
                            crc[j] = Comm[i];
                        }
                        crcreturn = MyClass.CRC16(crc,crc.Length);
                        Comm[15] = crcreturn[1];
                        Comm[16] = crcreturn[0];
                        Comm[17] = 0x55; Comm[18] = 0xAA;
                        Sp1.Write(Comm, 0, Comm.Length);
                    }
                    else
                    {
                        byte[] Comm = new byte[19];
                        Comm[0] = 0x5F; Comm[1] = 0x5F; Comm[2] = 0x00; Comm[3] = 0x0D; Comm[4] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(0, 2), 16)); Comm[5] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(2, 2), 16));
                        Comm[6] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(4, 2), 16)); Comm[7] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(6, 2), 16));
                        Comm[8] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(0, 2), 16)); Comm[9] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(2, 2), 16));
                        Comm[10] = 0x01; Comm[11] = 0x23; Comm[12] = 0x01; Comm[13] = 0xC0; Comm[14] = 0x02;
                        byte[] crc = new byte[11];
                        byte[] crcreturn = new byte[2];
                        for (int i = 4, j = 0; i < 15; i++, j++)
                        {
                            crc[j] = Comm[i];
                        }
                        crcreturn = MyClass.CRC16(crc,crc.Length);
                        Comm[15] = crcreturn[1];
                        Comm[16] = crcreturn[0];
                        Comm[17] = 0x55; Comm[18] = 0xAA;
                        Sp1.Write(Comm, 0, Comm.Length);
                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
          
           
        }

        private void RdState_Click(object sender, EventArgs e)
        {
            if (cbTimeSend.Checked)
            {
                tmSend.Enabled = true;
            }
            else
            {
                tmSend.Enabled = false;
            }
            if (GataWayDown.Text.Trim() == string.Empty)
            {
                MessageBox.Show("提示！", "请输入网关ID！");
            }
            else if (GataWayDown.Text.Trim() != string.Empty && COLLID.Text.Trim()== string.Empty)
            {
                byte[] Comm = new byte[19];
                Comm[0] = 0x5F; Comm[1] = 0x5F; Comm[2] = 0x00; Comm[3] = 0x0D; Comm[4] = caijiid[0]; Comm[5] = caijiid[1]; Comm[6] = caijiid[2]; Comm[7] = caijiid[3];
                Comm[8] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(0, 2), 16)); Comm[9] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(2, 2), 16));
                Comm[10] = 0x01; Comm[11] = 0x23; Comm[12] = 0x01; Comm[13] = 0xC0; Comm[14] = 0x03;
                byte[] crc = new byte[11];
                byte[] crcreturn = new byte[2];
                for (int i = 4, j = 0; i < 15; i++, j++)
                {
                    crc[j] = Comm[i];
                }
                crcreturn = MyClass.CRC16(crc,crc.Length);
                Comm[15] = crcreturn[1];
                Comm[16] = crcreturn[0];
                Comm[17] = 0x55; Comm[18] = 0xAA;
                Sp1.Write(Comm, 0, Comm.Length);
            }
            else
            {
                byte[] Comm = new byte[19];
                Comm[0] = 0x5F; Comm[1] = 0x5F; Comm[2] = 0x00; Comm[3] = 0x0D; Comm[4] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(0, 2), 16)); Comm[5] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(2, 2), 16));
                Comm[6] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(4, 2), 16)); Comm[7] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(6, 2), 16));
                Comm[8] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(0, 2), 16)); Comm[9] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(2, 2), 16));
                Comm[10] = 0x01; Comm[11] = 0x23; Comm[12] = 0x01; Comm[13] = 0xC0; Comm[14] = 0x03;
                byte[] crc = new byte[11];
                byte[] crcreturn = new byte[2];
                for (int i = 4, j = 0; i < 15; i++, j++)
                {
                    crc[j] = Comm[i];
                }
                crcreturn = MyClass.CRC16(crc,crc.Length);
                Comm[15] = crcreturn[1];
                Comm[16] = crcreturn[0];
                Comm[17] = 0x55; Comm[18] = 0xAA;
                Sp1.Write(Comm, 0, Comm.Length);
            }
        }

        private void WrConfig_Click(object sender, EventArgs e)
        {
            
            byte[] Comm = new byte[19];
            Comm[0] = 0x5F; Comm[1] = 0x5F; Comm[2] = 0x00; Comm[3] = 0x0D; Comm[4] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(0, 2), 16)); Comm[5] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(2, 2), 16)); Comm[6] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(4, 2), 16)); Comm[7] = Convert.ToByte(Convert.ToInt32(COLLID.Text.Substring(6, 2), 16));
            Comm[8] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(0, 2), 16)); Comm[9] = Convert.ToByte(Convert.ToInt32(GataWayDown.Text.Substring(2, 2), 16));
            Comm[10] = 0x01; Comm[11] = 0x23; Comm[12] = 0x01; Comm[13] = 0xC1;

            if (DownMode.Text == "被动发送请求及状态数据")
            {
                Comm[14] = 0x01;
            }
            else if (DownMode.Text == "主动发送请求及状态数据")
            {
                Comm[14] = 0x00;
            }

            byte[] crc = new byte[11];
            byte[] crcreturn = new byte[2];
            for (int i = 4, j = 0; i < 15; i++, j++)
            {
                crc[j] = Comm[i];
            }
            crcreturn = MyClass.CRC16(crc,crc.Length);
            Comm[15] = crcreturn[1];
            Comm[16] = crcreturn[0];
            Comm[17] = 0x55; Comm[18] = 0xAA;
            Sp1.Write(Comm, 0, Comm.Length);

        }

        private void DownMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void MTMode_CheckedChanged(object sender, EventArgs e)
        {
            groupBox5.Enabled = true;
        }

        private void AutoMode_CheckedChanged(object sender, EventArgs e)
        {
            groupBox5.Enabled = false;
        }
        private void tmSend_Tick(object sender, EventArgs e)
        {
            //转换时间间隔
            string strSecond = txtSecond.Text;
            try
            {
                int isecond = int.Parse(strSecond) * 1000;//Interval以微秒为单位
                tmSend.Interval = isecond;
                if (tmSend.Enabled == true)
                {
                    RdState.PerformClick();
                }
            }
            catch 
            {
                tmSend.Enabled = false;
                MessageBox.Show("错误的定时输入！", "Error");
            }

        }

      
    }
}
