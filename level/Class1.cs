using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 水准
{
    class Station
    {
        public double Height_difference;//高差
        public string Hsd;//后视点
        public string Qsd;//前视点
        public double StationNum;//测站数
        public double V;//改正数
        public double Corrected_elevation_difference;//实际高差


        //构造函数
        public Station(string hsd,string qsd,double station,double height)
        {
            this.Hsd = hsd;
            this.Qsd = qsd;
            this.Height_difference = height;
            this.StationNum = station;

        }
        //将获取的测站转化为点号
        public Point Specialization_to_pointnumber()
        {
            Point res = new Point(Hsd);
            return res;
        }
        //计算改正后高差
        public void calc_correct()
        {
            Corrected_elevation_difference = Height_difference + V;
        }

    }
    class Point
    {
        public string Name;
        public double Altitude = 0;
        public Point(string name)
        {
            this.Name = name;
        }
        public string to_print()
        {
            return "name:" + Name + "\tHright:" + Altitude + "\n";
        }
    }
}
