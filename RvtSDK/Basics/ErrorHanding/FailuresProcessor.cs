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
    /// 自定义的错误处理程序
    /// </summary>
    class FailuresProcessor : IFailuresProcessor
    {
        /// <summary>
        /// This method is being called in case of exception or document destruction to dismiss any possible pending failure UI that may have left on the screen 
        /// </summary>
        /// <param name="document"></param>
        public void Dismiss(Document document)
        {
            // 在发生异常或文档销毁时执行，以消除留在屏幕上的任何可能挂起的UI
        }

        /// <summary>
        /// Method that Revit will invoke to process failures at the end of transaction. 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public FailureProcessingResult ProcessFailures(FailuresAccessor failuresAccessor)
        {
            // 在事务结束时调用

            IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();
            if (fmas.Count == 0)
            {
                return FailureProcessingResult.Continue;
            }

            String transactionName = failuresAccessor.GetTransactionName();

            // 用事件名称区分是否是要处理的错误信息
            if (transactionName.Equals("Error_FailuresProcessor"))
            {
                foreach (FailureMessageAccessor fma in fmas)
                {
                    FailureDefinitionId id = fma.GetFailureDefinitionId();
                    if (id == Command.m_idError)
                    {
                        // 解决这个错误（用默认的错误处理器）
                        failuresAccessor.ResolveFailure(fma);
                        TaskDialog.Show("title", $"我捕捉到了消息内容为\"{fma.GetDescriptionText()}\"的错误,我在后台已经把他解决掉了0.0~~");
                    }
                }

                // 不再抛出
                return FailureProcessingResult.ProceedWithCommit;
            }
            else
            {
                // 没有做处理，继续抛出
                return FailureProcessingResult.Continue;
            }
        }
    }
}
