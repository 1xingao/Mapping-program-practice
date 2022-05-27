using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
namespace Wire_adjustment
{
    public partial class Form1 : Form
    {
        private double[] Mss;//观测边长
        private double[] Maa;//测量角度
        private double[] Azimuth;//边长方位角
        private List<DatacCass> point_data = new List<DatacCass>();
        private int pointlenght;
        private double[] mX0 = new double[4]; // 已知点坐标数组，未知点坐标数组(首尾为已知点)
        private double[] mY0 = new double[4];
        private int isEcho = 1;
        private int isLeft = 1;
        public bool Flag = true;
        private string[] pointname;
        private double fbeta; // 方位角闭合差
        private double fb0;//方向限差
        private double fdiret;//坐标闭合差
        private double fd0;//坐标限差
        string[] data;
        /// <summary>
        /// 初始化
        /// </summary>
        private void init()
        {
            point_data.Clear();
            pointlenght = 0;
            isEcho = 1;
            isLeft = 1;
            Flag = true;
            fbeta = 0;
            fb0 = 0;
            fdiret = 0;
            fd0 = 0;
            yes = false;
            richTextBox1.Text = "";
            dataGridView1.Rows.Clear();
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 计算近似坐标方位角
        /// </summary>
        private void CalcAzimuth()
        {
            double aa0;//起始边方位角
            aa0 = Func.DirectAB(mX0[0], mY0[0], mX0[1], mY0[1]);
            Azimuth[0] = aa0;
            if (isEcho == 1)
            {
                for (int i = 1; i < Azimuth.Length; i++)
                {
                    if (isLeft == 1)
                    {
                        Azimuth[i] = Azimuth[i - 1] + Maa[i - 1] + Math.PI;
                        while (Azimuth[i] >= 2 * Math.PI)
                        {
                            Azimuth[i] -= 2 * Math.PI;
                        }
                    }
                    else
                    {
                        Azimuth[i] = Azimuth[i - 1] - Maa[i - 1] - Math.PI;
                        while (Azimuth[i] < 0)
                        {
                            Azimuth[i] += 2 * Math.PI;
                        }
                    }
                }
            }
            else
            {
                if (isLeft == 1)
                    Maa[0] = 2 * Math.PI - Maa[0];
                Maa[Maa.Length - 1] = Maa[Maa.Length - 1] + (2 * Math.PI - Maa[0]); // 最后一个夹角（转为附和导线)
                if (Maa[Maa.Length - 1] >= 2 * Math.PI)
                    Maa[Maa.Length - 1] = Maa[Maa.Length - 1] - 2 * Math.PI;
                for (int i = 1; i <= Azimuth.Length - 1; i++)
                {
                    Azimuth[i] = Azimuth[i - 1] - Math.PI - Maa[i - 1];
                    if (Azimuth[i] < 0)
                        Azimuth[i] = Azimuth[i] + 2 * Math.PI;
                    if (Azimuth[i] < 0)
                        Azimuth[i] = Azimuth[i] + 2 * Math.PI; // 最多不超-4PI
                }
            }
        }
        /// <summary>
        /// 改正坐标方位角
        /// </summary>
        private void AdjDirect()
        {
            double aae;//终边方位角

            double fbeta0 = 24 * Math.Sqrt(Azimuth.Length - 1) / 10000;// 方位角闭合差限差(三级导线),转为dd.mmss形式
            aae = Func.DirectAB(mX0[2], mY0[2], mX0[3], mY0[3]);
            // 计算角度闭合差
            fbeta = Azimuth[Azimuth.Length - 1] - aae;
            fb0 = fbeta0;
            if (fb0 > fbeta0)
            {
                MessageBox.Show("数据超限！");
                Flag = false;
                return;
            }
            //计算改正方位角
            double Vbeat0 = -fbeta / (Azimuth.Length - 1);
            for (int i = 0; i < Azimuth.Length; i++)
            {
                Azimuth[i] += Vbeat0;
                if (Azimuth[i] >= 2 * Math.PI)
                {
                    Azimuth[i] -= 2 * Math.PI;
                }
                if (Azimuth[i] <= 0)
                {
                    Azimuth[i] += 2 * Math.PI;
                }
            }
        }
        /// <summary>
        /// 计算近似坐标
        /// </summary>
        private void CalcCoordinate()
        {
            point_data.Add(new DatacCass(mX0[1], mY0[1]));
            double lastx = mX0[1];
            double lasty = mY0[1];
            for (int i = 1; i < pointlenght + 2; i++)
            {
                double tex = lastx + Mss[i - 1] * Math.Cos(Azimuth[i]);
                double tey = lasty + Mss[i - 1] * Math.Sin(Azimuth[i]);
                var temp_class = new DatacCass(tex, tey);
                point_data.Add(temp_class);
                lastx = tex;
                lasty = tey;

            }
        }
        /// <summary>
        /// 坐标平差
        /// </summary>
        private void AdjCoor()
        {
            double allS = 0;
            double fs0 = 5000.0;
            fd0 = fs0;
            foreach (var s in Mss)
            {
                allS += s;
            }
            double fx = mX0[2] - point_data[point_data.Count - 1].X;
            double fy = mY0[2] - point_data[point_data.Count - 1].Y;
            double fs = Math.Sqrt(fx * fx + fy * fy);
            fs = Math.Floor(1 / (fs / allS));
            fdiret = fs;
            if (fs < fs0)
            {
                MessageBox.Show("坐标闭合差超限！");
                Flag = false;
                return;
            }
            double[] Vx = new double[point_data.Count - 1];
            double[] Vy = new double[point_data.Count - 1];
            double vx_ = fx / allS;
            double vy_ = fy / allS;
            for (int i = 0; i < Vx.Length; i++)
            {
                Vx[i] = vx_ * Mss[i];
                Vy[i] = vy_ * Mss[i];
            }
            //计算坐标平差值
            double totalVx, totalVy;
            totalVx = totalVy = 0.0;
            for (int i = 1; i < point_data.Count; i++)
            {
                totalVx += Vx[i - 1];
                totalVy += Vy[i - 1];
                point_data[i].X += totalVx;
                point_data[i].Y += totalVy;
            }
        }

        /// <summary>
        /// 写入已知点信息
        /// </summary>
        void ImportKnown_point()
        {
            if (isEcho == 1)//附和
            {
                textBox2.Text = Convert.ToString(mX0[0]);
                textBox5.Text = Convert.ToString(mX0[1]);
                textBox7.Text = Convert.ToString(mX0[2]);
                textBox9.Text = Convert.ToString(mX0[3]);
                textBox3.Text = Convert.ToString(mY0[0]);
                textBox4.Text = Convert.ToString(mY0[1]);
                textBox6.Text = Convert.ToString(mY0[2]);
                textBox8.Text = Convert.ToString(mY0[3]);
            }
            else
            {
                textBox2.Text = Convert.ToString(mX0[0]);
                textBox5.Text = Convert.ToString(mX0[1]);
                textBox3.Text = Convert.ToString(mY0[0]);
                textBox4.Text = Convert.ToString(mY0[1]);
            }
        }

        private void 导入数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                init();
                openFileDialog1.Filter = "*.txt|*.txt|(*.*)|*.*";
                string filepath;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    filepath = openFileDialog1.FileName;
                    StreamReader file = new StreamReader(filepath);
                    string temp;

                    isEcho = Convert.ToInt32(file.ReadLine().Trim());//读取是否为附和导线
                    if (isEcho == 0)
                    {
                        checkBox2.Checked = false;
                        groupBox6.Enabled = false;
                        groupBox7.Enabled = false;
                    }
                    else
                    {
                        checkBox2.Checked = true;
                        groupBox7.Enabled = true;
                        groupBox6.Enabled = true;
                    }

                    pointlenght = Convert.ToInt32(file.ReadLine().Trim());//读取未知点点数
                    textBox1.Text = Convert.ToString(pointlenght);//写入未知点数
                    pointname = file.ReadLine().Trim().Split(',');
                    data = new string[pointlenght + 4];
                    if (isEcho == 0)
                    {
                        for (int i = 0; i < pointname.Length; i++)
                        {
                            data[i] = pointname[i];
                        }
                        data[data.Length - 2] = pointname[1];
                        data[data.Length - 1] = pointname[0];
                    }
                    else
                    {
                        data[0] = pointname[0];
                        data[1] = pointname[1];
                        data[data.Length - 2] = pointname[2];
                        data[data.Length - 1] = pointname[3];
                        for (int i = 0; i < pointlenght; i++)
                        {
                            data[i + 2] = pointname[i + 4];
                        }
                    }
                    int range = 0;
                    for (int i = 0; i < data.Length; i++)//写入点号数据到表格
                    {
                        dataGridView1.Rows.Add(2);
                        dataGridView1.Rows[range].Cells[0].Value = data[i];
                        range += 2;
                    }
                    Azimuth = new double[pointlenght + 3];//边长方位角比点号多一个加上两个已知边
                    temp = file.ReadLine().Trim();
                    var tem = temp.Split(',');
                    if (isEcho == 0)//闭合
                    {
                        mX0[0] = Convert.ToDouble(tem[0]);
                        mX0[1] = Convert.ToDouble(tem[2]);
                        mX0[2] = Convert.ToDouble(tem[2]);
                        mX0[3] = Convert.ToDouble(tem[0]);
                        mY0[0] = Convert.ToDouble(tem[1]);
                        mY0[1] = Convert.ToDouble(tem[3]);
                        mY0[2] = Convert.ToDouble(tem[3]);
                        mY0[3] = Convert.ToDouble(tem[1]);
                    }
                    else
                    {

                        mX0[0] = Convert.ToDouble(tem[0]);
                        mX0[1] = Convert.ToDouble(tem[2]);
                        mX0[2] = Convert.ToDouble(tem[4]);
                        mX0[3] = Convert.ToDouble(tem[6]);
                        mY0[0] = Convert.ToDouble(tem[1]);
                        mY0[1] = Convert.ToDouble(tem[3]);
                        mY0[2] = Convert.ToDouble(tem[5]);
                        mY0[3] = Convert.ToDouble(tem[7]);
                    }
                    ImportKnown_point();
                    isLeft = Convert.ToInt32(file.ReadLine().Trim());
                    if (isLeft == 0)
                    {
                        checkBox1.Checked = false;
                    }
                    else
                    {
                        checkBox1.Checked = true;
                    }
                    temp = file.ReadLine().Trim();
                    tem = temp.Split(',');
                    Maa = new double[pointlenght + 2];
                    for (int i = 0; i < pointlenght + 2; i++)
                    {
                        Maa[i] = Convert.ToDouble(tem[i]);
                    }
                    range = 2;
                    for (int i = 0; i < Maa.Length; i++)//写入测角数据
                    {
                        dataGridView1.Rows[range].Cells[1].Value = Convert.ToString(Maa[i]);
                        range += 2;
                        Maa[i] = Func.DMS2Hu(Maa[i]);
                    }

                    Mss = new double[pointlenght + 1];
                    temp = file.ReadLine().Trim();
                    tem = temp.Split(',');
                    for (int i = 0; i < pointlenght + 1; i++)
                    {
                        Mss[i] = Convert.ToDouble(tem[i]);
                    }
                    range = 3;
                    for (int i = 0; i < Mss.Length; i++)
                    {
                        dataGridView1.Rows[range].Cells[2].Value = Convert.ToString(Mss[i]);
                        range += 2;
                    }
                }
            }
            catch (Exception a)
            {
                MessageBox.Show(a.Message);
            }
        }
        public bool yes = false;
        private void 计算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (yes)
                {
                    MessageBox.Show("已经计算完成！", "error", MessageBoxButtons.OKCancel);
                    return;
                }
                CalcAzimuth();
                if (!Flag) { return; }
                AdjDirect();
                CalcCoordinate();
                if (!Flag) { return; }
                AdjCoor();
                CalculationResults();
                yes = true;
            }
            catch (Exception a)
            {
                MessageBox.Show(a.Message);
            }
        }

        private void 导出报告ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = "文本文件(*.txt)|*.txt";
                saveFileDialog1.FileName = "平差报告";
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
            }
            catch (Exception a)
            {
                MessageBox.Show(a.Message);
            }
        }
        private void CalculationResults()
        {
            string result = "";
            result += "--------------------平差结果-------------------\n";
            result += "平差后各点坐标:\n";
            int range = 1;
            foreach (var cla in point_data)
            {
                result += "点" + data[range++] + ":\tx:" + cla.X.ToString("0.0000") + "--y:" + cla.Y.ToString("0.0000") + "\n";
            }
            result += "角度闭合差：" + fbeta.ToString("0.0000000") + "\t角度限差：" + fb0.ToString("0.0000");
            result += "\n坐标闭合差：" + fdiret + "\t坐标闭合差：" + fd0;
            result += "\n平差后各边方位角:\n";
            range = 0;
            for (int i = 0; i < Azimuth.Length; i++)
            {
                result += data[range++] + data[range] + "：" + Azimuth[i].ToString("0.0000") + "\n";
            }
            richTextBox1.Text = result;
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 数据格式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string help = "[数据格式]\n第一行：导线类型，1-附和导线，0-闭合导线\n第二行：未知点数量\n第三行：点名(先已知点后未知点)\n";
            help += "第四行：已知点坐标(x1,y1,x2,y2)\n第五行：测角类型，1-左角，0-右角\n";
            help += "第六行：观测角数值(dd.mmss,按照未知点号顺序)\n第七行：观测边长(未知点号顺序)";
            richTextBox1.Text = help;
        }
    }
}
