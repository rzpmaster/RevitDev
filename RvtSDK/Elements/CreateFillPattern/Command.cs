using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateFillPattern
{
    /// <summary>
    /// 材质中的填充区域相关 暂时还没搞懂 用到再来看吧
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                using (PatternForm patternForm = new PatternForm(commandData))
                {
                    patternForm.ShowDialog();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
