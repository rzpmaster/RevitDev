using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitUtils.RvtUtils
{
    public static class ParameterUtils
    {
        public static string GetParamterStringValue(this Element element, BuiltInParameter builtInParameter)
        {
            Parameter parameter = element.get_Parameter(builtInParameter);
            if (parameter != null)
            {
                var storageType = parameter.StorageType;
                if (storageType== StorageType.String)
                {
                    return parameter.AsString();
                }
            }
            else
            {
                //所查询的参数非类型参数或参数不存在
                
            }
            return string.Empty;
        }

        public static string GetParamterAsStringValue(this Element element, BuiltInParameter builtInParameter)
        {
            Parameter parameter = element.get_Parameter(builtInParameter);
            if (parameter != null)
            {
                var storageType = parameter.StorageType;
                if (storageType == StorageType.Double)
                {
                    return parameter.AsValueString();
                }
            }
            else
            {
                //所查询的参数非类型参数或参数不存在

            }
            return string.Empty;
        }

        public static double GetParamterDoubleValue(this Element element, BuiltInParameter builtInParameter)
        {
            Parameter parameter = element.get_Parameter(builtInParameter);
            if (parameter != null)
            {
                var storageType = parameter.StorageType;
                if (storageType == StorageType.Double)
                {
                    return parameter.AsDouble();
                }
            }
            else
            {
                //所查询的参数非类型参数或参数不存在

            }
            return -1.0;
        }

        public static int GetParamterIntegerValue(this Element element, BuiltInParameter builtInParameter)
        {
            Parameter parameter = element.get_Parameter(builtInParameter);
            if (parameter != null)
            {
                var storageType = parameter.StorageType;
                if (storageType == StorageType.Integer)
                {
                    return parameter.AsInteger();
                }
            }
            else
            {
                //所查询的参数非类型参数或参数不存在

            }
            return -1;
        }

        public static ElementId GetParamterElementIdValue(this Element element, BuiltInParameter builtInParameter)
        {
            Parameter parameter = element.get_Parameter(builtInParameter);
            if (parameter != null)
            {
                var storageType = parameter.StorageType;
                if (storageType == StorageType.ElementId)
                {
                    return parameter.AsElementId();
                }
            }
            else
            {
                //所查询的参数非类型参数或参数不存在

            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Parameter FindParaByName(this Element element, string name)
        {
            Parameter findPara = null;

            foreach (Parameter para in element.Parameters)
            {
                if (para.Definition.Name.Contains(name))
                {
                    findPara = para;
                    break;
                }
            }

            return findPara;
        }
    }
}
