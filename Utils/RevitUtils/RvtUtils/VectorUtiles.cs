using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitUtils.RvtUtils
{
    public class VectorUtiles
    {
        /// <summary>
        /// 输入构件的方向向量 判断是否垂直
        /// </summary>
        /// <param name="xyz1"></param>
        /// <param name="xyz2"></param>
        /// <returns></returns>
        public static bool CheckVertical(XYZ xyz1, XYZ xyz2)
        {
            return Math.Abs((xyz1.X * xyz2.X + xyz1.Y * xyz2.Y + xyz1.Z * xyz2.Z)) <= 0.0001 ? true : false; //数据精度问题~
        }

        /// <summary>
        /// 判断两向量是否平行
        /// </summary>
        /// <param name="xyz1"></param>
        /// <param name="xyz2"></param>
        /// <returns></returns>
        public static bool CheckParallel(XYZ xyz1, XYZ xyz2)
        {
            bool tag = Math.Abs(xyz1.X * xyz2.Y - xyz1.Y * xyz2.X) < 0.00001 &&
                    Math.Abs(xyz1.Y * xyz2.Z - xyz1.Z * xyz2.Y) < 0.00001 &&
                    Math.Abs(xyz1.X * xyz2.Z - xyz1.Z * xyz2.X) < 0.00001;
            return tag;
        }
    }
}
