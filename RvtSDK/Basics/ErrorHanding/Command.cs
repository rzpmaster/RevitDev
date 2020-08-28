using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorHanding
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class Command : IExternalCommand, IExternalApplication
    {
        public static FailureDefinitionId m_idError;
        public static FailureDefinitionId m_idWarning;

        #region IExternalCommand

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_revitApp = commandData.Application.Application;
            m_doc = commandData.Application.ActiveUIDocument.Document;

            Level level1 = GetLevel();
            if (level1 == null)
            {
                throw new Exception("[ERROR] Failed to get level 1");
            }

            //
            // Post a warning and resolve it in FailurePreproccessor
            try
            {
                //PostWarningAndResolveInFailurePreproccessor();
            }
            catch (System.Exception)
            {
                message = "Failed to commit transaction Warning_FailurePreproccessor";
                return Result.Failed;
            }

            //
            // Dismiss the overlapped wall warning in FailurePreproccessor
            try
            {
                //DismissWarningInFailurePreproccesser(level1);
            }
            catch (System.Exception)
            {
                message = "Failed to commit transaction Warning_FailurePreproccessor_OverlappedWall";
                return Result.Failed;
            }

            //
            // Post an error and resolve it in FailuresProcessingEvent
            try
            {
                //PostErrorAndResolveInFaliuresProcessingEvent(level1);
            }
            catch (System.Exception)
            {
                message = "Failed to commit transaction Error_FailuresProcessingEvent";
                return Result.Failed;
            }

            //
            // Post an error and resolve it in FailuresProcessor
            try
            {
                PostErrorAndResolveInFailuresProcessor(level1);
            }
            catch (System.Exception)
            {
                message = "Failed to commit transaction Error_FailuresProcessor";
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void PostErrorAndResolveInFailuresProcessor(Level level1)
        {
            FailuresProcessor processor = new FailuresProcessor();
            Application.RegisterFailuresProcessor(processor);
            Transaction transaction = new Transaction(m_doc, "Error_FailuresProcessor");
            transaction.Start();

            Line line = Line.CreateBound(new XYZ(0, 20, 0), new XYZ(20, 20, 0));
            Wall wall = Wall.Create(m_doc, line, level1.Id, false);
            m_doc.Regenerate();

            FailureMessage fm = new FailureMessage(m_idError);
            FailureResolution fr = DeleteElements.Create(m_doc, wall.Id);
            fm.AddResolution(FailureResolutionType.DeleteElements, fr);
            m_doc.PostFailure(fm);
            transaction.Commit();
        }

        private void PostErrorAndResolveInFaliuresProcessingEvent(Level level1)
        {
            //m_revitApp.FailuresProcessing += new EventHandler<Autodesk.Revit.DB.Events.FailuresProcessingEventArgs>(FailuresProcessing);
            Transaction transaction = new Transaction(m_doc, "Error_FailuresProcessingEvent");
            transaction.Start();

            Line line = Line.CreateBound(new XYZ(0, 10, 0), new XYZ(20, 10, 0));
            Wall wall = Wall.Create(m_doc, line, level1.Id, false);
            m_doc.Regenerate();

            FailureMessage fm = new FailureMessage(m_idError);
            FailureResolution fr = DeleteElements.Create(m_doc, wall.Id);
            fm.AddResolution(FailureResolutionType.DeleteElements, fr);
            m_doc.PostFailure(fm);
            transaction.Commit();
        }

        private void FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            FailuresAccessor failuresAccessor = e.GetFailuresAccessor();
            //failuresAccessor
            String transactionName = failuresAccessor.GetTransactionName();

            IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();
            if (fmas.Count == 0)
            {
                e.SetProcessingResult(FailureProcessingResult.Continue);
                return;
            }

            if (transactionName.Equals("Error_FailuresProcessingEvent"))
            {
                foreach (FailureMessageAccessor fma in fmas)
                {
                    FailureDefinitionId id = fma.GetFailureDefinitionId();
                    if (id == Command.m_idError)
                    {
                        failuresAccessor.ResolveFailure(fma);
                    }
                }

                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
                return;
            }

            e.SetProcessingResult(FailureProcessingResult.Continue);
        }

        private void DismissWarningInFailurePreproccesser(Level level1)
        {
            Transaction transaction = new Transaction(m_doc, "Warning_FailurePreproccessor_OverlappedWall");
            FailureHandlingOptions options = transaction.GetFailureHandlingOptions();
            FailurePreproccessor preproccessor = new FailurePreproccessor();
            options.SetFailuresPreprocessor(preproccessor);
            transaction.SetFailureHandlingOptions(options);

            transaction.Start();
            Line line = Line.CreateBound(new XYZ(-10, 0, 0), new XYZ(-20, 0, 0));
            Wall wall1 = Wall.Create(m_doc, line, level1.Id, false);
            Wall wall2 = Wall.Create(m_doc, line, level1.Id, false);
            m_doc.Regenerate();

            transaction.Commit();
        }

        private void PostWarningAndResolveInFailurePreproccessor()
        {
            Transaction transaction = new Transaction(m_doc, "Warning_FailurePreproccessor");
            FailureHandlingOptions options = transaction.GetFailureHandlingOptions();
            FailurePreproccessor preproccessor = new FailurePreproccessor();
            options.SetFailuresPreprocessor(preproccessor);
            transaction.SetFailureHandlingOptions(options);

            transaction.Start();
            FailureMessage fm = new FailureMessage(m_idWarning);
            m_doc.PostFailure(fm);
            transaction.Commit();
        }

        private Level GetLevel()
        {
            Level level1 = null;

            FilteredElementCollector collector = new FilteredElementCollector(m_doc);
            ElementClassFilter filter = new ElementClassFilter(typeof(Level));
            IList<Element> levels = collector.WherePasses(filter).ToElements();

            foreach (Level level in levels)
            {
                if (level.Name.Equals("标高 1"))
                {
                    level1 = level;
                    break;
                }
            }

            return level1;
        }

        #endregion

        #region IExternalApplication

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Create failure definition Ids
                // 这玩意儿(FailureDefinition)必须在 Shartup()中创建

                Guid guid1 = new Guid("0C3F66B5-3E26-4d24-A228-7A8358C76D39");
                Guid guid2 = new Guid("93382A45-89A9-4cfe-8B94-E0B0D9542D34");
                m_idWarning = new FailureDefinitionId(guid1);
                m_idError = new FailureDefinitionId(guid2);

                // 定义一个 错误定义(警告)
                m_fdWarning = FailureDefinition.CreateFailureDefinition(m_idWarning, FailureSeverity.Warning, "I am the warning.");

                // 添加两种可能的解决方案的类型(在 FailureMessage 中定义真正的处理方案,在这里的 FailureDefinition 中只指明将来要添加的解决方案的类型)
                m_fdWarning.AddResolutionType(FailureResolutionType.MoveElements, "MoveElements", typeof(DeleteElements));
                m_fdWarning.AddResolutionType(FailureResolutionType.DeleteElements, "DeleteElements", typeof(DeleteElements));
                // 设置默认解决办法为第二个
                m_fdWarning.SetDefaultResolutionType(FailureResolutionType.DeleteElements);

                // 定义一个 错误定义(错误)
                m_fdError = FailureDefinition.CreateFailureDefinition(m_idError, FailureSeverity.Error, "I am the error");

                // 添加两种可能的解决方案的类型(在 FailureMessage 中定义真正的处理方案,在这里的 FailureDefinition 中只指明将来要添加的解决方案的类型)
                m_fdError.AddResolutionType(FailureResolutionType.DetachElements, "DetachElements", typeof(DeleteElements));
                m_fdError.AddResolutionType(FailureResolutionType.DeleteElements, "DeleteElements", typeof(DeleteElements));
                // 设置默认解决办法为第二个
                m_fdError.SetDefaultResolutionType(FailureResolutionType.DeleteElements);
            }
            catch (System.Exception)
            {
                return Result.Failed;
            }

            TaskDialog.Show("title", "has definited two FailureDefinitions.");

            return Result.Succeeded;
        }

        #endregion

        private FailureDefinition m_fdWarning;
        private FailureDefinition m_fdError;

        private Application m_revitApp;
        private Document m_doc;
    }
}
