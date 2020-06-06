using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitUtils.RvtUtils
{
    public class MathHelper
    {
        #region 单位转换
        const double convertFootToMm = 12 * 25.4;

        /// <summary>
        /// 毫米到英尺的转换系数
        /// </summary>
        public static double Mm2Foot
        {
            get
            {
                return 1 / convertFootToMm;
            }
        }

        /// <summary>
        /// 英尺到毫米的转换系数
        /// </summary>
        public static double Foot2Mm
        {
            get
            {
                return convertFootToMm;
            }
        }
        #endregion

        #region 比较大小
        const double tolerance = 1e-06;

        public static bool IsZero(double a, double tolerance = tolerance)
        {
            return tolerance > Math.Abs(a);
        }

        public static bool IsEqual(double a, double b)
        {
            return IsZero(b - a);
        }
        #endregion

        /// <summary>
        /// 计算所数的所有因数
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static List<int> GetFactorizationNums(int num)
        {
            List<int> list = new List<int>();

            if (num < 0)
            {
                num = -num;
                return GetFactorizationNums(num);
            }
            else if (num == 0)
            {
                return null;
            }
            else
            {
                for (int i = 1; i <= num; i++)
                {
                    int a = num / i;
                    if (i * a == num)
                    {
                        list.Add(i);
                    }
                }
            }


            return list;
        }
    }
}
