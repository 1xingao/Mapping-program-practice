using System;

namespace Matrix
{
    public class Matrix
    {
        /// <summary>
        /// 用于执行运算的元素
        /// </summary>
        public double[,] Element { get; set; }
        /// <summary>
        /// 行数
        /// </summary>
        public int Row => Element.GetLength(0);
        /// <summary>
        /// 列数
        /// </summary>
        public int Col => Element.GetLength(1);
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public double this[int i, int j]
        {
            get
            {
                if (i < Row && j < Col)
                    return Element[i, j];
                else
                {
                    System.Exception ex = new Exception("索引超出界限!");
                    throw ex;
                }
            }
            set
            {
                Element[i, j] = value;
            }
        }
        #region 初始化
        /// <summary>
        /// 
        /// 交错矩阵初始化
        /// </summary>
        /// <param name="matrix"></param>
        public Matrix(double[][] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Element = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    Element[i, j] = matrix[i][j];
        }
        /// <summary>
        /// 二维数组初始化
        /// </summary>
        /// <param name="matrix"></param>
        public Matrix(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Element = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    Element[i, j] = matrix[i,j];
        }
        /// <summary>
        /// 直接初始化默认为零
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="num"></param>
        public Matrix(int row,int col,double num = .0)
        {
            Element = new double[row, col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Element[i, j] = num;
                }
            }
        }
        /// <summary>
        /// 创建diag类型的矩阵
        /// </summary>
        /// <param name="matrix"></param>
        public Matrix(double[] matrix)
        {
            int rows = matrix.Length;
            int index = 0;
            Element = new double[rows, rows];
            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < rows; j++)
                {
                    if (i == j)
                    {
                        Element[i,j] = matrix[index++];
                    } 
                }
            }     
        }
        #endregion
        #region 矩阵运算

        /// <summary>
        /// 矩阵加法运算
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            Matrix array = new Matrix(m1.Row, m1.Col);
            if (m1.Row == m2.Row && m1.Col == m2.Col)
            {
                for (int i = 0; i < m1.Row; i++)
                {
                    for (int j = 0; j < m1.Col; j++)
                    {
                        array[i, j] = m1[i, j] + m2[i, j];
                    }
                }
            }
            return array;
        }
        /// <summary>
        /// 矩阵减法运算
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            Matrix array = new Matrix(m1.Row, m1.Col);
            if (m1.Row == m2.Row && m1.Col == m2.Col)
            {
                for (int i = 0; i < m1.Row; i++)
                {
                    for (int j = 0; j < m1.Col; j++)
                    {
                        array[i, j] = m1[i, j] - m2[i, j];
                    }
                }
            }
            return array;
        }
        /// <summary>
        /// 矩阵转置
        /// </summary>
        /// <returns></returns>
        public Matrix Transpose()
        {
            Matrix result = new Matrix(Element.GetLength(1), Element.GetLength(0));
            for (int i = 0; i < Element.GetLength(0); i++)
            {
                for (int j = 0; j < Element.GetLength(1); j++)
                {
                    result[j, i] = Element[i, j];
                }
            }
             return result;
        }
        /// <summary>
        /// 代数余子式求行列式
        /// </summary>
        /// <param name="martix"></param>
        /// <returns></returns>
        public double Determinant(Matrix martix)
        {
            double sum = 0;
            int sign = 1;
            if (martix.Row == 1)
            {
                return martix[0, 0];
            }
            for (int i = 0; i < martix.Row; i++)
            {
                Matrix tempmatrix = new Matrix(martix.Row - 1, martix.Col - 1);
                for (int j = 0; j < martix.Row - 1; j++)
                   for (int k = 0; k < martix.Col - 1; k++)
                       tempmatrix[j, k] = martix[j + 1, k >= i ? k + 1 : k];
                sum += sign * martix[0, i] * Determinant(tempmatrix);
                sign *= (-1);
            }
            return sum;
        }
        /// <summary>
        /// 伴随矩阵
        /// </summary>
        /// <param name="martix"></param>
        /// <returns></returns>
        private Matrix Complement(Matrix martix)
        {
            Matrix result = new(martix.Row, martix.Col);
            if (martix.Row == martix.Col)//方阵
            {
                for (int i = 0; i < martix.Row; i++)
                {
                    for (int j = 0; j < martix.Col; j++)
                    {
                        //生成aij的余子式矩阵
                        double[,] complement = new double[martix.Row - 1, martix.Col - 1];//n-1阶
                        Matrix martix1 = new Matrix(complement);//aij的余子式矩阵
                        int row = 0;
                        for (int k = 0; k < martix.Row; k++)
                        {
                            int column = 0;
                            if (k == i)//去除第i行
                               continue;
                            for (int l = 0; l < martix.Row; l++)
                            {
                                if (l == j)//去除第j列
                                    continue;
                                martix1[row, column++] = martix[k, l];
                            }
                            row++;
                        }
                        result[i, j] = Math.Pow(-1, i + j) * Determinant(martix1);
                    }
                }
            }
            return result.Transpose();
        }
        /// <summary>
        /// 矩阵求逆
        /// </summary>
        /// <returns></returns>
        public Matrix Inverse()
        {
            Matrix result = new Matrix(Element.GetLength(1), Element.GetLength(0));
            if (Element.GetLength(0) == Element.GetLength(1))//方阵
            {
                Matrix martix = new Matrix(Element);
                if (Determinant(martix) != 0)
                {
                    if (martix.Row > 1)
                        result = Complement(martix) * (1 / Determinant(martix));
                    else
                        result.Element[0, 0] = 1 / martix[0, 0];
                }
            }
            return result;
        }
        /// <summary>
        /// 乘法
        /// </summary>
        /// <param name="num"></param>
        /// <param name="martix1"></param>
        /// <returns></returns>
        public static Matrix operator *(double num, Matrix martix1)
        {
            Matrix result = new Matrix(martix1.Row, martix1.Col);
           
           for (int i = 0; i < martix1.Row; i++)
           {
                for (int j = 0; j < martix1.Col; j++)
                {
                    result[i, j] = martix1[i, j] * num;
                }
            }
            return result;
        }
        public static Matrix operator *(Matrix martix1,double num)
        {
            Matrix result = new Matrix(martix1.Row, martix1.Col);

            for (int i = 0; i < martix1.Row; i++)
            {
                for (int j = 0; j < martix1.Col; j++)
                {
                    result[i, j] = martix1[i, j] * num;
                }
            }
            return result;
        }
        public static Matrix operator *(Matrix martix1, Matrix martix2)
        {
            
            Matrix result = new Matrix(martix1.Row,martix2.Col);
            if (martix1.Col == martix2.Row)
                for (int row1 = 0; row1 < martix1.Row; row1++)
                {
                    int row2 = 0;
                    for (int column2 = 0; column2 < martix2.Col; column2++)
                    {
                        double Sum = 0;
                        for (int column1 = 0; column1 < martix1.Col; column1++)
                        {
                            Sum += martix1[row1, column1] * martix2[column1, row2];
                        }
                        result[row1, column2] = Sum;
                        row2++;
                    }
                }
            return result;
        }
        #endregion
    }
}
