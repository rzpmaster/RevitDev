using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorHanding
{
    /// <summary>
    /// 自定义的错误前处理程序
    /// </summary>
    class FailurePreproccessor : IFailuresPreprocessor
    {
        /// <summary>
        /// This method is called when there have been failures found at the end of a transaction and Revit is about to start processing them. 
        /// </summary>
        /// <param name="failuresAccessor"></param>
        /// <returns></returns>
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            // 在事务结束时,Revit即将处理这些错误之前调用

            IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();
            if (fmas.Count == 0)
            {
                return FailureProcessingResult.Continue;
            }

            String transactionName = failuresAccessor.GetTransactionName();

            // 用事件名称区分是否是要处理的错误信息
            if (transactionName.Equals("Warning_FailurePreproccessor"))
            {
                foreach (FailureMessageAccessor fma in fmas)
                {
                    FailureDefinitionId id = fma.GetFailureDefinitionId();
                    if (id == Command.m_idWarning)
                    {
                        // 删除警告
                        failuresAccessor.DeleteWarning(fma);
                    }
                }

                return FailureProcessingResult.ProceedWithCommit;
            }
            else if (transactionName.Equals("Warning_FailurePreproccessor_OverlappedWall"))
            {
                foreach (FailureMessageAccessor fma in fmas)
                {
                    FailureDefinitionId id = fma.GetFailureDefinitionId();
                    if (id == BuiltInFailures.OverlapFailures.WallsOverlap)
                    {
                        // 删除错误
                        failuresAccessor.DeleteWarning(fma);
                    }
                }

                return FailureProcessingResult.ProceedWithCommit;
            }
            else
            {
                // 不做处理
                return FailureProcessingResult.Continue;
            }
        }
    }
}
