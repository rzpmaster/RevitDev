using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorHandling
{
    class FailuresProcessor : IFailuresProcessor
    {
        /// <summary>
        /// 在发生异常或文档销毁时调用此方法，以消除可能留在屏幕上的任何可能挂起的失败UI
        /// </summary>
        /// <param name="document"></param>
        public void Dismiss(Document document)
        {
        }

        /// <summary>
        /// Revit将在事务结束时处理失败时调用该方法。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public FailureProcessingResult ProcessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();
            if (fmas.Count == 0)
            {
                return FailureProcessingResult.Continue;
            }

            String transactionName = failuresAccessor.GetTransactionName();
            if (transactionName.Equals("Error_FailuresProcessor"))
            {
                foreach (FailureMessageAccessor fma in fmas)
                {
                    FailureDefinitionId id = fma.GetFailureDefinitionId();
                    if (id == Command.m_idError)
                    {
                        failuresAccessor.ResolveFailure(fma);
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
