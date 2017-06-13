using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using INIFILE;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Windows.Forms.DataVisualization.Charting;

namespace SerialPortConnection
{
    public partial class Form1 : Form
    {
        SerialPort sp1 = new SerialPort();
        //sp1.ReceivedBytesThreshold = 1;//只要有1个字符送达端口时便触发DataReceived事件 

        string strcon = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\asus\Documents\GreenHouseData.mdf;Integrated Security = True; Connect Timeout = 30";

        string data = "";
        string dataPiece = "";

        float[] Picdata = new float[6] { 0, 0, 0, 0, 0, 0 };    //画图所用数据
        public Form1()
        {
            InitializeComponent();
            //this.IsMdiContainer = true;
        }

        

        //加载
        private void Form1_Load(object sender, EventArgs e)
        {
            INIFILE.Profile.LoadProfile();//加载所有

            //画图设置
            chart1.Visible = false;
            //设置图表Y轴对应项
            chart1.Series[0].YValueMembers = "data";

            //设置图表X轴对应项
            chart1.Series[0].XValueMember = "time";
            chart1.Series[0].ChartType = SeriesChartType.Spline;
            chart1.Series[0].Color = System.Drawing.Color.Blue;
            chart1.ChartAreas[0].BackColor = System.Drawing.Color.DimGray;

            chart1.Series[0].IsValueShownAsLabel = true;
            chart1.Series[0].IsVisibleInLegend = false;
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

            //下拉菜单设置
            String[] arr = new String[] { "温度", "湿度", "CO_1", "CO_2", "光照_1", "光照_2", "土壤_1", "土壤_2" };
            for (int i = 0; i < arr.Length; i++)
            {
                comboBox1.Items.Add(arr[i]);
            }
            // 预置波特率
            switch (Profile.G_BAUDRATE)
            {
                case "300":
                    cbBaudRate.SelectedIndex = 0;
                    break;
                case "600":
                    cbBaudRate.SelectedIndex = 1;
                    break;
                case "1200":
                    cbBaudRate.SelectedIndex = 2;
                    break; 
                case "2400":
                    cbBaudRate.SelectedIndex = 3;
                    break;
                case "4800":
                    cbBaudRate.SelectedIndex = 4;
                    break;
                case "9600":
                    cbBaudRate.SelectedIndex = 5;
                    break;
                case "19200":
                    cbBaudRate.SelectedIndex = 6;
                    break; 
                case "38400":
                    cbBaudRate.SelectedIndex = 7;
                    break;
                case "115200":
                    cbBaudRate.SelectedIndex = 8;
                    break;
                default:
                    {
                        MessageBox.Show("波特率预置参数错误。");
                        return;
                    }                  
            }

            //预置波特率
            switch (Profile.G_DATABITS)
            {
                case "5":
                    cbDataBits.SelectedIndex = 0;
                    break;
                case "6":
                    cbDataBits.SelectedIndex = 1;
                    break; 
                case "7":
                    cbDataBits.SelectedIndex = 2;
                    break; 
                case  "8":
                    cbDataBits.SelectedIndex = 3;
                    break;
                default:
                    {
                        MessageBox.Show("数据位预置参数错误。");
                        return;
                    }

            }
            //预置停止位
            switch (Profile.G_STOP)
            {
                case "1":
                    cbStop.SelectedIndex = 0;
                        break;
                case "1.5":
                    cbStop.SelectedIndex = 1;
                    break;
                case "2":
                    cbStop.SelectedIndex = 2;
                    break;
                default:
                    {
                        MessageBox.Show("停止位预置参数错误。");
                        return;
                    }
            }

            //预置校验位
            switch(Profile.G_PARITY)
            {
                case "NONE":
                    cbParity.SelectedIndex = 0;
                    break;
                case "ODD":
                    cbParity.SelectedIndex = 1;
                    break;
                case "EVEN":
                    cbParity.SelectedIndex = 2;
                    break;
                default:
                    {
                        MessageBox.Show("校验位预置参数错误。");
                        return;
                    }
            }

            //检查是否含有串口
            string[] str = SerialPort.GetPortNames();
            if (str == null)
            {
                MessageBox.Show("本机没有串口！", "Error");
                return;
            }

            //添加串口项目
            //foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
            //{//获取有多少个COM口
                //System.Diagnostics.Debug.WriteLine(s);
            //    cbSerial.Items.Add(s);
            //}
            cbSerial.Items.Add("COM3");
            cbSerial.Items.Add("COM4");
            cbSerial.Items.Add("COM5");
            cbSerial.Items.Add("COM6");
            cbSerial.Items.Add("COM7");
            //串口设置默认选择项
            cbSerial.SelectedIndex = 0;         //note：获得COM3口，但别忘修改
            //cbBaudRate.SelectedIndex = 5;
           // cbDataBits.SelectedIndex = 3;
           // cbStop.SelectedIndex = 0;
          //  cbParity.SelectedIndex = 0;
            sp1.BaudRate = 9600;

            Control.CheckForIllegalCrossThreadCalls = false;    //这个类中我们不检查跨线程的调用是否合法(因为.net 2.0以后加强了安全机制,，不允许在winform中直接跨线程访问控件的属性)
            sp1.DataReceived += new SerialDataReceivedEventHandler(sp1_DataReceived);
            //sp1.ReceivedBytesThreshold = 1;

            rdSendStr.Checked = true;  //单选按钮默认是选中的
            rbRcvStr.Checked = true;

            dataBaseConnect.Checked = false;

            //准备就绪              
            sp1.DtrEnable = true;
            sp1.RtsEnable = true;
            //设置数据读取超时为1秒
            sp1.ReadTimeout = 1000;

            sp1.Close();
        }

        //数据接收
        void sp1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Byte[] receivedData = new Byte[sp1.BytesToRead];        //创建接收字节数组
                //如果出现接受错误可换用以下代码
                //Byte[] receivedData = new Byte[sp1.ReadBufferSize];        //大小为接受缓存区大小
                sp1.Read(receivedData, 0, receivedData.Length);         //读取数据
                //string text = sp1.Read();   //Encoding.ASCII.GetString(receivedData);
                sp1.DiscardInBuffer();                                  //清空SerialPort控件的Buffer
                //这是用以显示字符串
                if (rbRcvStr.Checked == true)
                {
                    string strRcv = null;
                    for (int i = 0; i < receivedData.Length; i++)
                    {
                        strRcv += ((char)Convert.ToInt32(receivedData[i]));
                    }
                    dataPiece = strRcv;
                    //txtRe2.Text = dataPiece;
                    data += dataPiece;  //将接受的数据字符拼接在一起
                    //接受到完整字符串后处理
                    if (data.Contains("ok")  && data.Contains("start"))
                    {
                        //textBox1.Text = data;
                        string[] dataArray = data.Split(' ');   //将接受的字符串
                        txtRe2.Text = dataArray[1];
                        txtRe3.Text = dataArray[2];
                        txtRe4.Text = dataArray[3];
                        txtRe5.Text = dataArray[4];
                        txtRe6.Text = dataArray[5];
                        txtRe7.Text = dataArray[6];
                        txtRe8.Text = dataArray[7];
                        txtRe9.Text = dataArray[8];
                        //输出当前时间
                        DateTime dt = DateTime.Now;
                        txtRe1.Text = dt.ToString();
                        data = "";

                        //更新画图数据
                        Picdata[5] = Picdata[4];
                        Picdata[4] = Picdata[3];
                        Picdata[3] = Picdata[2];
                        Picdata[2] = Picdata[1];
                        Picdata[1] = Picdata[0];

                        switch (comboBox1.SelectedIndex)
                        {
                            case 0:
                                Picdata[0] = float.Parse(txtRe2.Text);
                                break;
                            case 1:
                                Picdata[0] = float.Parse(txtRe3.Text);
                                break;
                            case 2:
                                Picdata[0] = float.Parse(txtRe4.Text);
                                break;
                            case 3:
                                Picdata[0] = float.Parse(txtRe5.Text);
                                break;
                            case 4:
                                Picdata[0] = float.Parse(txtRe6.Text);
                                break;
                            case 5:
                                Picdata[0] = float.Parse(txtRe7.Text);
                                break;
                            case 6:
                                Picdata[0] = float.Parse(txtRe7.Text);
                                break;
                            case 7:
                                Picdata[0] = float.Parse(txtRe9.Text);
                                break;
                        }
                        DataTable Picdt = new DataTable();
                        Picdt = CreatDataTable();

                        //设置图表的数据源
                        chart1.DataSource = Picdt;
                        //绑定数据
                        chart1.DataBind();

                        //数据库保存数据
                        if (saveData.Checked)
                        {
                            SqlConnection sqlcon = new SqlConnection(strcon);
                            try
                            {
                                string timeData = txtRe1.Text.Trim();
                                float temperatureData = float.Parse(txtRe2.Text.Trim());
                                float humidityData = float.Parse(txtRe3.Text.Trim());
                                float CO_1Data = float.Parse(txtRe4.Text.Trim());
                                float CO_2Data = float.Parse(txtRe5.Text.Trim());
                                float illumination_1Data = float.Parse(txtRe6.Text.Trim());
                                float illumination_2Data = float.Parse(txtRe7.Text.Trim());
                                float soil_1Data = float.Parse(txtRe8.Text.Trim());
                                float soil_2Data = float.Parse(txtRe9.Text.Trim());
                                SqlCommand command = new SqlCommand();
                                command.CommandText = "INSERT INTO data(time,temperature,humidity,CO_1,CO_2,illumination_1,illumination_2,soil_1,soil_2) VALUES('"
                                    + timeData + "'," + temperatureData + "," + humidityData + "," + CO_1Data + ","
                                    + CO_2Data + "," + illumination_1Data + "," + illumination_2Data
                                    + "," + soil_1Data + "," + soil_2Data + ")";
                                command.CommandType = CommandType.Text;
                                command.Connection = sqlcon;
                                sqlcon.Open();
                                if (command.ExecuteNonQuery() == 1)
                                {
                                    dataBaseState.Text = "数据保存成功";
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            finally
                            {
                                sqlcon.Close();
                            }
                        }
                    }
                }                        
                else
                {
                    string strRcv = null;
                    //int decNum = 0;//存储十进制
                    for (int i = 0; i < receivedData.Length; i++) //窗体显示
                    {

                        strRcv += receivedData[i].ToString("X2");  //16进制显示
                    }
                    dataPiece = strRcv;
                    data += dataPiece;
                    if (data.Contains("ok") && data.Contains("start"))
                    {
                        string[] dataArray = data.Split(' ');
                        txtRe2.Text = dataArray[1];
                        txtRe3.Text = dataArray[2];
                        txtRe4.Text = dataArray[3];
                        txtRe5.Text = dataArray[4];
                        txtRe6.Text = dataArray[5];
                        txtRe7.Text = dataArray[6];
                        txtRe8.Text = dataArray[7];
                        txtRe9.Text = dataArray[8];
                        //输出当前时间
                        DateTime dt = DateTime.Now;
                        txtRe1.Text = dt.ToString();
                        data = "";

                        Picdata[5] = Picdata[4];
                        Picdata[4] = Picdata[3];
                        Picdata[3] = Picdata[2];
                        Picdata[2] = Picdata[1];
                        Picdata[1] = Picdata[0];

                        switch (comboBox1.SelectedIndex)
                        {
                            case 0:
                                Picdata[0] = float.Parse(txtRe2.Text);
                                break;
                            case 1:
                                Picdata[0] = float.Parse(txtRe3.Text);
                                break;
                            case 2:
                                Picdata[0] = float.Parse(txtRe4.Text);
                                break;
                            case 3:
                                Picdata[0] = float.Parse(txtRe5.Text);
                                break;
                            case 4:
                                Picdata[0] = float.Parse(txtRe6.Text);
                                break;
                            case 5:
                                Picdata[0] = float.Parse(txtRe7.Text);
                                break;
                            case 6:
                                Picdata[0] = float.Parse(txtRe7.Text);
                                break;
                            case 7:
                                Picdata[0] = float.Parse(txtRe9.Text);
                                break;
                        }
                        DataTable Picdt = new DataTable();
                        Picdt = CreatDataTable();

                        //设置图表的数据源
                        chart1.DataSource = Picdt;
                        //绑定数据
                        chart1.DataBind();
                        if (saveData.Checked)
                        {
                            SqlConnection sqlcon = new SqlConnection(strcon);
                            try
                            {
                                string timeData = txtRe1.Text.Trim();
                                string temperatureData = txtRe2.Text.Trim();
                                string humidityData = txtRe3.Text.Trim();
                                string CO_1Data = txtRe4.Text.Trim();
                                string CO_2Data = txtRe5.Text.Trim();
                                string illumination_1Data = txtRe6.Text.Trim();
                                string illumination_2Data = txtRe7.Text.Trim();
                                string soil_1Data = txtRe8.Text.Trim();
                                string soil_2Data = txtRe9.Text.Trim();
                                SqlCommand command = new SqlCommand();
                                command.CommandText = "INSERT INTO data(time,temperature,humidity,CO_1,CO_2,illumination_1,illumination_2,soil_1,soil_2) VALUES('"
                                    + timeData + "','" + temperatureData + "','" + humidityData + "','" + CO_1Data + "','"
                                    + CO_2Data + "','" + illumination_1Data + "','" + illumination_2Data
                                    + "','" + soil_1Data + "','" + soil_2Data + "')";
                                command.CommandType = CommandType.Text;
                                command.Connection = sqlcon;
                                sqlcon.Open();
                                if (command.ExecuteNonQuery() == 1)
                                {
                                    dataBaseState.Text = "数据保存成功";
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            finally
                            {
                                sqlcon.Close();
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "出错提示");
                txtSend.Text = "";
            }       
        }

        //发送按钮
        private void btnSend_Click(object sender, EventArgs e)
        {
            //txtRe1.Text = "";       //清空文本
            //txtRe2.Text = "";
            data = "";
            if (cbTimeSend.Checked)
            {
                tmSend.Enabled = true;
            }
            else
            {
                tmSend.Enabled = false;
            }

            if (!sp1.IsOpen) //如果没打开
            {
                tmSend.Enabled = false;
                cbTimeSend.CheckState = CheckState.Unchecked;
                MessageBox.Show("请先打开串口！", "Error");
                return;
            }

            String strSend = txtSend.Text;
            if (radio1.Checked == true)	//“HEX发送” 按钮 
            {
                //处理数字转换
                string sendBuf = strSend;
                string sendnoNull = sendBuf.Trim();
                string sendNOComma = sendnoNull.Replace(',', ' ');    //去掉英文逗号
                string sendNOComma1 = sendNOComma.Replace('，', ' '); //去掉中文逗号
                string strSendNoComma2 = sendNOComma1.Replace("0x", "");   //去掉0x
                strSendNoComma2.Replace("0X", "");   //去掉0X
                string[] strArray = strSendNoComma2.Split(' ');

                int byteBufferLength = strArray.Length;
                for (int i = 0; i < strArray.Length; i++ )
                {
                    if (strArray[i]=="")
                    {
                        byteBufferLength--;
                    }
                }               
               // int temp = 0;
                byte[] byteBuffer = new byte[byteBufferLength];
                int ii = 0;
                for (int i = 0; i < strArray.Length; i++)        //对获取的字符做相加运算
                {
                  
                    Byte[] bytesOfStr = Encoding.Default.GetBytes(strArray[i]);
                    
                    int decNum = 0;
                    if (strArray[i] == "")
                    {
                        //ii--;     //加上此句是错误的，下面的continue以延缓了一个ii，不与i同步
                        continue;
                    }
                    else
                    {
                         decNum = Convert.ToInt32(strArray[i], 16); //atrArray[i] == 12时，temp == 18 
                    }
                           
                   try    //防止输错，使其只能输入一个字节的字符
                   {
                       byteBuffer[ii] = Convert.ToByte(decNum);        
                   }
                   catch (System.Exception ex)
                   {
                       MessageBox.Show("字节越界，请逐个字节输入！", "Error");
                       tmSend.Enabled = false;
                       return;
                   }

                   ii++;    
                }
                sp1.Write(byteBuffer, 0, byteBuffer.Length);
            }
            else		//以字符串形式发送时 
            {
                string str = txtSend.Text;
                sp1.Write(str+"\r\n");    //写入数据
            }
        }

        //开关按钮
        private void btnSwitch_Click(object sender, EventArgs e)
        {
            //serialPort1.IsOpen
            if (!sp1.IsOpen)
            {
                try
                {
                    //设置串口号
                    string serialName = cbSerial.SelectedItem.ToString();
                    sp1.PortName = serialName;

                    //设置各“串口设置”
                    string strBaudRate = cbBaudRate.Text;
                    string strDateBits = cbDataBits.Text;
                    string strStopBits = cbStop.Text;
                    Int32 iBaudRate = Convert.ToInt32(strBaudRate);
                    Int32 iDateBits = Convert.ToInt32(strDateBits);

                    sp1.BaudRate = iBaudRate;       //波特率
                    sp1.DataBits = iDateBits;       //数据位
                    switch (cbStop.Text)            //停止位
                    {
                        case "1":
                            sp1.StopBits = StopBits.One;
                            break;
                        case "1.5":
                            sp1.StopBits = StopBits.OnePointFive;
                            break;
                        case "2":
                            sp1.StopBits = StopBits.Two;
                            break;
                        default:
                            MessageBox.Show("Error：参数不正确!", "Error");
                            break;
                    }
                    switch (cbParity.Text)             //校验位
                    {
                        case "无":
                            sp1.Parity = Parity.None;
                            break;
                        case "奇校验":
                            sp1.Parity = Parity.Odd;
                            break;
                        case "偶校验":
                            sp1.Parity = Parity.Even;
                            break;
                        default:
                            MessageBox.Show("Error：参数不正确!", "Error");
                            break;
                    }

                    if (sp1.IsOpen == true)//如果打开状态，则先关闭一下
                    {
                        sp1.Close();
                    }
                    //状态栏设置
                    tsSpNum.Text = "串口号：" + sp1.PortName + "|";
                    tsBaudRate.Text = "波特率：" + sp1.BaudRate + "|";
                    tsDataBits.Text = "数据位：" + sp1.DataBits + "|";
                    tsStopBits.Text = "停止位：" + sp1.StopBits + "|";
                    tsParity.Text = "校验位：" + sp1.Parity + "|";

                    //设置必要控件不可用
                    cbSerial.Enabled = false;
                    cbBaudRate.Enabled = false;
                    cbDataBits.Enabled = false;
                    cbStop.Enabled = false;
                    cbParity.Enabled = false;

                    sp1.Open();     //打开串口
                    btnSwitch.Text = "关闭串口";
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message, "Error");
                    cbSerial.Enabled = true;
                    cbBaudRate.Enabled = true;
                    cbDataBits.Enabled = true;
                    cbStop.Enabled = true;
                    cbParity.Enabled = true;
                    tmSend.Enabled = false;
                    return;
                }
            }
            else
            {
                //状态栏设置
                tsSpNum.Text = "串口号：未指定|";
                tsBaudRate.Text = "波特率：未指定|";
                tsDataBits.Text = "数据位：未指定|";
                tsStopBits.Text = "停止位：未指定|";
                tsParity.Text = "校验位：未指定|";
                //恢复控件功能
                //设置必要控件不可用
                cbSerial.Enabled = true;
                cbBaudRate.Enabled = true;
                cbDataBits.Enabled = true;
                cbStop.Enabled = true;
                cbParity.Enabled = true;

                sp1.Close();                    //关闭串口
                btnSwitch.Text = "打开串口";
                tmSend.Enabled = false;         //关闭计时器
            }
        }

        //清空按钮
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtRe1.Text = "";       //清空文本
            txtRe2.Text = "";
            txtRe3.Text = "";
            txtRe4.Text = "";
            txtRe5.Text = "";
            txtRe6.Text = "";
            txtRe7.Text = "";
            txtRe8.Text = "";
            txtRe9.Text = "";
        }

        //关闭时事件
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            INIFILE.Profile.SaveProfile();
            sp1.Close();
        }

        private void txtSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (radio1.Checked== true)
            {
                //正则匹配
                string patten = "[0-9a-fA-F]|\b|0x|0X| "; //“\b”：退格键
                Regex r = new Regex(patten);
                Match m = r.Match(e.KeyChar.ToString());

                if (m.Success )//&&(txtSend.Text.LastIndexOf(" ") != txtSend.Text.Length-1))
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }//end of radio1
            else
            {
                e.Handled = false;
            }
        }

        private void txtSend_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            
            //设置各“串口设置”
            string strBaudRate = cbBaudRate.Text;
            string strDateBits = cbDataBits.Text;
            string strStopBits = cbStop.Text;
            Int32 iBaudRate = Convert.ToInt32(strBaudRate);
            Int32 iDateBits = Convert.ToInt32(strDateBits);

            Profile.G_BAUDRATE = iBaudRate+"";       //波特率
            Profile.G_DATABITS = iDateBits+"";       //数据位
            switch (cbStop.Text)            //停止位
            {
                case "1":
                    Profile.G_STOP = "1";
                    break;
                case "1.5":
                    Profile.G_STOP = "1.5";
                    break;
                case "2":
                    Profile.G_STOP ="2";
                    break;
                default:
                    MessageBox.Show("Error：参数不正确!", "Error");
                    break;
            }
            switch (cbParity.Text)             //校验位
            {
                case "无":
                    Profile.G_PARITY = "NONE";
                    break;
                case "奇校验":
                    Profile.G_PARITY = "ODD";
                    break;
                case "偶校验":
                    Profile.G_PARITY = "EVEN";
                    break;
                default:
                    MessageBox.Show("Error：参数不正确!", "Error");
                    break;
            }

            //保存设置
            // public static string G_BAUDRATE = "1200";//给ini文件赋新值，并且影响界面下拉框的显示
            //public static string G_DATABITS = "8";
            //public static string G_STOP = "1";
            //public static string G_PARITY = "NONE";
            Profile.SaveProfile();
        }

        //定时器
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
                    btnSend.PerformClick();
                }
            }
            catch (System.Exception)
            {
                tmSend.Enabled = false;
                MessageBox.Show("错误的定时输入！", "Error");
            }
            
        }

        private void txtSecond_KeyPress(object sender, KeyPressEventArgs e)
        {
            string patten = "[0-9]|\b"; //“\b”：退格键
            Regex r = new Regex(patten);
            Match m = r.Match(e.KeyChar.ToString());

            if (m.Success)
            {
                e.Handled = false;   //没操作“过”，系统会处理事件    
            }
            else
            {
                e.Handled = true;
            }
        }

        private void dataBaseConnect_CheckedChanged(object sender, EventArgs e)
        {            
            if (dataBaseConnect.Checked)
            {
                SqlConnection sqlcon;
                using (sqlcon = new SqlConnection(strcon))
                {
                    sqlcon.Open();
                    MessageBox.Show("数据库连接状态：" + sqlcon.State.ToString());
                }
            }
            else
            {
                MessageBox.Show("数据库连接已断开");
            }
        }

        
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            title.Text = comboBox1.GetItemText(comboBox1.Items[comboBox1.SelectedIndex]);
            chart1.Visible = true;
            
            DataTable dt = new DataTable();
            for(int i=0;i<6;i++)
            {
                Picdata[i] = 0;
            }
            dt = InitDataTable();

            chart1.DataSource = dt;
           
            chart1.DataBind();
        }

        private DataTable InitDataTable()
        {
            //Create a DataTable as the data source of the Chart control
            DataTable Picdt = new DataTable();

            //Add three columns to the DataTable
            Picdt.Columns.Add("time");
            Picdt.Columns.Add("data");

            DataRow dr;

            //Add rows to the table which contains some random data for demonstration
            dr = Picdt.NewRow();
            dr["time"] = "1";
            dr["data"] = 0;
            Picdt.Rows.Add(dr);

            dr = Picdt.NewRow();
            dr["time"] = "2";
            dr["data"] = 0;
            Picdt.Rows.Add(dr);

            dr = Picdt.NewRow();
            dr["time"] = "3";
            dr["data"] = 0;
            Picdt.Rows.Add(dr);

            dr = Picdt.NewRow();
            dr["time"] = "4";
            dr["data"] = 0;
            Picdt.Rows.Add(dr);

            dr = Picdt.NewRow();
            dr["time"] = "5";
            dr["data"] = 0;
            Picdt.Rows.Add(dr);

            dr = Picdt.NewRow();
            dr["time"] = "6";
            dr["data"] = 0;
            Picdt.Rows.Add(dr);

            return Picdt;
        }
        private DataTable CreatDataTable()
        {
            //Create a DataTable as the data source of the Chart control
            DataTable Picdt = new DataTable();

            //Add three columns to the DataTable
            Picdt.Columns.Add("time");
            Picdt.Columns.Add("data");

            DataRow dr;

            //Add rows to the table which contains some random data for demonstration
            dr = Picdt.NewRow();
            dr["time"] = "1";
            dr["data"] = Picdata[5];
            Picdt.Rows.Add(dr);

            dr = Picdt.NewRow();
            dr["time"] = "2";
            dr["data"] = Picdata[4];
            Picdt.Rows.Add(dr);

            dr = Picdt.NewRow();
            dr["time"] = "3";
            dr["data"] = Picdata[3];
            Picdt.Rows.Add(dr);

            dr = Picdt.NewRow();
            dr["time"] = "4";
            dr["data"] = Picdata[2];
            Picdt.Rows.Add(dr);

            dr = Picdt.NewRow();
            dr["time"] = "5";
            dr["data"] = Picdata[1];
            Picdt.Rows.Add(dr);

            dr = Picdt.NewRow();
            dr["time"] = "6";
            dr["data"] = Picdata[0];
            Picdt.Rows.Add(dr);

            return Picdt;
        }
    }
}
