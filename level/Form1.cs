﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
namespace 水准
{
    public partial class Form1 : Form

    {
        List<Station> data_list_station = new List<Station>();//保存所有测站信息
        double start_station_height;
        double end_station_height;
        readonly List<Point> data_point = new List<Point>();
        bool canAdjustment = false;
        double all_station = 0;
        double Closure_difference = 0;
        double limit_height = 0;
        public Form1()
        {
            InitializeComponent();
            dataGridView1.BackgroundColor = Color.FromArgb(255, 255, 255);//设置表格容器背景色
            dataGridView1.ReadOnly = true;

        }

        private void 打开文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "*.txt|*.txt|(*.*)|*.*";

            string path;
            string[] data;
            string temp;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                #region openfile
                try
                {
                    path = openFileDialog1.FileName;
                    StreamReader file = new StreamReader(path);
                    temp = file.ReadLine();
                    data = temp.Split(',');
                    start_station_height = Convert.ToDouble(data[1]);
                    temp = file.ReadLine();
                    data = temp.Split(',');
                    end_station_height = Convert.ToDouble(data[1]);
                    //先获取已知点高程，再去读取未知点
                    temp = file.ReadLine();
                    while (temp != null && temp != "")
                    {
                        data = temp.Split(',');
                        Station s1 = new Station(data[0], data[1], Convert.ToDouble(data[2]), Convert.ToDouble(data[3]));
                        data_list_station.Add(s1);
                        temp = file.ReadLine();
                    }
                    file.Close();

                    //写入表格
                    foreach (Station s in data_list_station)//将每一个测站转化为对应的点
                    {
                        Point p1 = s.Specialization_to_pointnumber();
                        data_point.Add(p1);
                    }
                    Point p = new Point(data_list_station[data_list_station.Count - 1].Qsd);
                    data_point.Add(p);
                    data_point[0].Altitude = start_station_height;
                    data_point[data_point.Count - 1].Altitude = end_station_height;//写入对应起始点的高程

                    foreach (Point temp_p in data_point)
                    {
                        int index = dataGridView1.Rows.Add();
                        dataGridView1.Rows[index].Cells[0].Value = temp_p.Name;
                        if (temp_p.Altitude != 0)
                        {
                            dataGridView1.Rows[index].Cells[5].Value = temp_p.Altitude;
                        }
                        dataGridView1.Rows.Add();
                    }
                    int col = 1;
                    foreach (Station s2 in data_list_station)
                    {
                        dataGridView1.Rows[col].Cells[1].Value = s2.StationNum;
                        dataGridView1.Rows[col].Cells[2].Value = s2.Height_difference;
                        col += 2;
                    }

                }
                catch (Exception a)
                {
                    MessageBox.Show(a.Message);
                }
                #endregion
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string Help = "此软件为水准平差计算\n数据格式要求为：\n前两行为已知点点号,实际高程" +
                "\n后面为待测点依次输入\n后视点,前视点(点号按照循序编写并且为数字从1开始)\n,中间测站数,高差\n点击计算即可";
            MessageBox.Show(Help, "帮助", MessageBoxButtons.OKCancel);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void 清屏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            richTextBox1.Clear();
        }

        private void 保存文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region savefile
            if (richTextBox1.Text == "")
            {
                MessageBox.Show("无报告！");
                return;
            }
            int num = 1;
            saveFileDialog1.Filter = "(*.txt)|*.txt";
            saveFileDialog1.FileName = "平差报告" + num++;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = saveFileDialog1.FileName;
                StreamWriter sw = new StreamWriter(path);
                sw.Write(richTextBox1.Text);
                sw.Close();
                MessageBox.Show("Saved successfully!");
            }
            else
            {
                MessageBox.Show("Save Failed!");

            }
            #endregion
        }

        private void 计算闭合差ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region calc
            try
            {
                all_station = 0;
                limit_height = 0;
                Closure_difference = 0;
                if (data_list_station.Count == 0)
                {
                    MessageBox.Show("无数据，无法计算闭合差！");
                    return;
                }
                double actual_value = 0;
                //计算限差
                for (int i = 0; i < data_list_station.Count; i++)
                {
                    all_station += data_list_station[i].StationNum;
                    limit_height = 12 * Math.Sqrt(all_station);
                    actual_value += data_list_station[i].Height_difference;
                }
                double Theoretical_value = end_station_height - start_station_height;
                Closure_difference = actual_value - Theoretical_value;
                string close = "闭合差为：" + Closure_difference + "\n限差为：" + limit_height;
                if (Math.Abs(Closure_difference) <= limit_height)
                {
                    canAdjustment = true;
                }
                else
                {
                    close += "数据超限！";
                }
                MessageBox.Show(close);
            }
            catch
            {
                MessageBox.Show("计算闭合差失败！");
            }
            #endregion
        }

        private void 近似平差ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (canAdjustment == false)
                {
                    MessageBox.Show("未计算闭合差 或者 数据超限，不能进行平差！");
                    return;
                }
                for (int i = 0; i < data_list_station.Count; i++)
                {
                    //计算改正数
                    data_list_station[i].V = (data_list_station[i].StationNum / all_station) * -Closure_difference;
                    data_list_station[i].calc_correct();
                }
                int col = 1;
                //写入高差改正数和改正数
                for (int i = 0; i < data_list_station.Count; i++)
                {
                    dataGridView1.Rows[col].Cells[3].Value = data_list_station[i].V;
                    dataGridView1.Rows[col].Cells[4].Value = data_list_station[i].Corrected_elevation_difference;
                    col += 2;
                }
                //计算改正后高程并写入
                int range = 0;
                int range_write = 2;
                for (int i = 1; i < data_point.Count - 1; i++)
                {
                    data_point[i].Altitude = data_list_station[range].Corrected_elevation_difference
                        + start_station_height;
                    dataGridView1.Rows[range_write].Cells[5].Value = data_point[i].Altitude;
                    range++;
                    range_write += 2;
                    start_station_height = data_point[i].Altitude;
                }
            }
            catch (Exception a)
            {
                MessageBox.Show(a.Message);
            }
        }

        private void 生成报告ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int n = data_point.Count;
                richTextBox1.Text = "---------------------------报告--------------------------------\r\n";
                richTextBox1.Text += "已知点信息：\r\n";
                richTextBox1.Text += "点名： " + data_point[0].Name + "       高程: "
                    + data_point[0].Altitude + "\r\n";
                richTextBox1.Text += "点名： " + data_point[data_point.Count - 1].Name
                    + "       高程: " + data_point[data_point.Count - 1].Altitude + "\r\n";
                richTextBox1.Text += "----------------------------------------------------------\r\n";
                richTextBox1.Text += "近似平差结果：\r\n";
                richTextBox1.Text += "限差：" + limit_height;
                richTextBox1.Text += "闭合差：" + Closure_difference + "\n各点数据：\n";
                foreach (Point temp_p in data_point)
                {
                    richTextBox1.Text += temp_p.to_print();
                }
                tabControl1.SelectedIndex = 1;
            }
            catch
            {
                MessageBox.Show("无平差结果无法生成报告!");
            }

        }

        private void 严密平差ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (canAdjustment == false)
                {
                    MessageBox.Show("未计算闭合差 或者 数据超限，不能进行平差！");
                    return;
                }
                int station = data_list_station.Count;//测段数
                Matrix B = new Matrix(station, data_point.Count - 2);
                Matrix L = new Matrix(station, 1);
                int range = 0;
                double start = start_station_height;
                Dictionary<string, double> map1 = new Dictionary<string, double>();//创建哈希映射用于近似高程计算
                foreach (Point p in data_point)
                {
                    map1.Add(p.Name, p.Altitude);
                }
                for (int i = 0; i < station - 1; i++)
                {
                    map1[data_list_station[i].Qsd] = start + data_list_station[i].Height_difference;
                    start = map1[data_list_station[i].Qsd];
                }
                var tem = new double[data_list_station.Count];//用于初始化权阵

                for (int i = 0; i < data_list_station.Count; i++)//B矩阵赋值
                {
                    if (IsYzd(data_list_station[i].Hsd))
                    {
                        B[i, Convert.ToInt32(data_list_station[i].Qsd) - 1] = 1;
                    }
                    else if (IsYzd(data_list_station[i].Qsd))
                    {
                        B[i, Convert.ToInt32(data_list_station[i].Hsd) - 1] = -1;
                    }
                    else
                    {
                        B[i, Convert.ToInt32(data_list_station[i].Hsd) - 1] = -1;
                        B[i, Convert.ToInt32(data_list_station[i].Qsd) - 1] = 1;
                    }
                    //L矩阵赋值
                    L[i, 0] = map1[data_list_station[i].Qsd] - map1[data_list_station[i].Hsd] - data_list_station[i].Height_difference;
                    tem[i] = 1 / data_list_station[i].StationNum;//p=c/n;c=1
                }
                Matrix P = new Matrix(tem);
                var n = B.Transpose() * P * B;
                var w = B.Transpose() * P * L;
                var x = n.Inverse() * w;
                var v = B * x - L;
                for (int i = 0; i < data_list_station.Count; i++)
                {
                    data_list_station[i].V = -v[i, 0];
                    data_list_station[i].calc_correct();
                }
                int col = 1;
                //写入高差改正数和改正数
                for (int i = 0; i < data_list_station.Count; i++)
                {
                    dataGridView1.Rows[col].Cells[3].Value = data_list_station[i].V;
                    dataGridView1.Rows[col].Cells[4].Value = data_list_station[i].Corrected_elevation_difference;
                    col += 2;
                }
                //计算改正后高程并写入
                range = 0;
                int range_write = 2;
                for (int i = 1; i < data_point.Count - 1; i++)
                {
                    data_point[i].Altitude = data_list_station[range].Corrected_elevation_difference
                        + start_station_height;
                    dataGridView1.Rows[range_write].Cells[5].Value = data_point[i].Altitude;
                    range++;
                    range_write += 2;
                    start_station_height = data_point[i].Altitude;
                }
            }
            catch (Exception a)
            {

                MessageBox.Show(a.Message);
            }
        }
        //判断是否为已知点
        private bool IsYzd(string s)
        {
            if (s == data_point[0].Name || s == data_point[data_point.Count - 1].Name)
            {
                return true;
            }
            return false;
        }
    }
}
