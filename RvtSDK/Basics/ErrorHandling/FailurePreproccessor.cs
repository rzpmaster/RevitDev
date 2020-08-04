using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorHandling
{
    class FailurePreproccessor : IFailuresPreprocessor
    {
        /// <summary>
        /// 当事务结束时发现故障，并且Revit即将开始处理这些故障时，将调用此方法
        /// </summary>
        /// <param name="failuresAccessor"></param>
        /// <returns></returns>
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();
            if (fmas.Count == 0)
            {
                return FailureProcessingResult.Continue;
            }

            String transactionName = failuresAccessor.GetTransactionName();
            if (transactionName.Equals("Warning_FailurePreproccessor"))
            {
                foreach (FailureMessageAccessor fma in fmas)
                {
                    FailureDefinitionId id = fma.GetFailureDefinitionId();
                    if (id == Command.m_idWarning)
                    {
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
                        failuresAccessor.DeleteWarning(fma);
                    }
                }

                return FailureProcessingResult.ProceedWithCommit;
            }
            else
            {
                return FailureProcessingResult.Continue;
            }
        }
    }
}
