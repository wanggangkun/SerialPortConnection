using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Windows.Forms.DataVisualization.Charting;

namespace SerialPortConnection
{
    public partial class Form3 : Form
    {
        string strcon = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\asus\Documents\GreenHouseData.mdf;Integrated Security = True; Connect Timeout = 30";
        public Form3()
        {
            InitializeComponent();
        }


        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            string yearData = dateTimePicker1.Value.Year.ToString();
            string monthData = dateTimePicker1.Value.Month.ToString();
            string dayData = dateTimePicker1.Value.Day.ToString();
            string date = yearData + "/" + monthData + "/" + dayData;
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(strcon))
                {
                    sqlcon.Open();
                    string sql = "SELECT * FROM data where time like '%" + date + "%'";
                    SqlCommand command = new SqlCommand(sql, sqlcon);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        data.Text = "";
                        while (reader.Read())
                        {
                            data.Text += reader[0].ToString() + "\r\n" + "温度："
                                + reader[1].ToString() + "\r\n" + "湿度：" + reader[2].ToString() + "\r\n"
                                + "CO_1：" + reader[3].ToString() + "\r\n" + "CO_2：" + reader[4].ToString() + "\r\n"
                                + "光照_1：" + reader[5].ToString() + "\r\n" + "光照_2：" + reader[6].ToString() + "\r\n"
                                + "土壤_1：" + reader[7].ToString() + "\r\n" + "土壤_2：" + reader[8].ToString() + "\r\n";
                        }
                    }
                    else
                    {
                        data.Text = "这天没有数据" + "\r\n";
                    }
                    reader.Close();
                }
            }
            catch (Exception)
            {
                data.Text += "这天没有数据";
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            //DataTable dt = default(DataTable);
            //设置图表Y轴对应项
            //chart1.Series[0].YValueMembers = "time";

            //设置图表X轴对应项
            //chart1.Series[0].XValueMember = "data";
            //chart1.Series[0].ChartType = SeriesChartType.Spline;
            //chart1.Series[0].Color = System.Drawing.Color.Blue;

            //chart1.Series[0].IsValueShownAsLabel = true;
            // chart1.Series[0].IsVisibleInLegend = false;
            //绑定数据

            chart1.Visible = false;

            String[] arr = new String[] { "温度", "湿度", "CO_1", "CO_2", "光照_1", "光照_2", "土壤_1", "土壤_2" };
            for (int i = 0; i < arr.Length; i++)
            {
                comboBox1.Items.Add(arr[i]);
            }
            //下面两种方法都可以为ComboBox赋初试选中值
            //comboBox1.SelectedIndex = 0;
            //comboBox1.SelectedItem = "排球";
            //label1.Text = comboBox1.GetItemText(comboBox1.Items[comboBox1.SelectedIndex]);
            //chart1.DataBind();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label1.Text = comboBox1.GetItemText(comboBox1.Items[comboBox1.SelectedIndex]);
            chart1.Visible = true;
            chart1.ChartAreas[0].BackColor = System.Drawing.Color.DimGray;

            DataTable dt = new DataTable();
            dt.Columns.Add("time");
            dt.Columns.Add("data");
            DataRow dr;
            string yearData = dateTimePicker1.Value.Year.ToString();
            string monthData = dateTimePicker1.Value.Month.ToString();
            string dayData = dateTimePicker1.Value.Day.ToString();
            string date = yearData + "/" + monthData + "/" + dayData;
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(strcon))
                {
                    sqlcon.Open();
                    string sql = "SELECT * FROM data where time like '%" + date + "%'";
                    SqlCommand command = new SqlCommand(sql, sqlcon);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dr = dt.NewRow();
                            dr["time"] = reader[0].ToString().Split(' ')[1];
                            switch (comboBox1.SelectedIndex)
                            {
                                case 0:
                                    dr["data"] = reader[1];
                                    break;
                                case 1:
                                    dr["data"] = reader[2];
                                    break;
                                case 2:
                                    dr["data"] = reader[3];
                                    break;
                                case 3:
                                    dr["data"] = reader[4];
                                    break;
                                case 4:
                                    dr["data"] = reader[5];
                                    break;
                                case 5:
                                    dr["data"] = reader[6];
                                    break;
                                case 6:
                                    dr["data"] = reader[7];
                                    break;
                                case 7:
                                    dr["data"] = reader[8];
                                    break;
                            }
                            dt.Rows.Add(dr);
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception)
            {
                label1.Text = "这天没有数据";
            }
            chart1.DataSource = dt;
            //设置图表Y轴对应项
            chart1.Series[0].YValueMembers = "data";

            //设置图表X轴对应项
            chart1.Series[0].XValueMember = "time";
            chart1.Series[0].ChartType = SeriesChartType.Spline;
            chart1.Series[0].Color = System.Drawing.Color.Blue;

            chart1.Series[0].IsValueShownAsLabel = true;
            chart1.Series[0].IsVisibleInLegend = false;
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            chart1.DataBind();
        }
    }
}
