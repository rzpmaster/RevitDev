using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.RevitAddIns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CancelSave
{
    [Transaction(TransactionMode.Manual)]
    class CancelSave : IExternalApplication
    {
        const string thisAddinFileName = "CancelSave.addin";

        // The dictionary contains document hashcode and its original "Project Status" pair.
        Dictionary<int, string> documentOriginalStatusDic = new Dictionary<int, string>();
        int hashcodeofCurrentClosingDoc;

        #region IExternalApplication Members
        public Result OnShutdown(UIControlledApplication application)
        {
            //注销事件
            application.ControlledApplication.DocumentOpened -= new EventHandler<DocumentOpenedEventArgs>(ReservePojectOriginalStatus);
            application.ControlledApplication.DocumentCreated -= new EventHandler<DocumentCreatedEventArgs>(ReservePojectOriginalStatus);

            application.ControlledApplication.DocumentSaving -= new EventHandler<DocumentSavingEventArgs>(CheckProjectStatusUpdate);
            application.ControlledApplication.DocumentSavingAs -= new EventHandler<DocumentSavingAsEventArgs>(CheckProjectStatusUpdate);

            // finalize the log file.
            LogManager.LogFinalize();

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //注册事件
            application.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(ReservePojectOriginalStatus);
            application.ControlledApplication.DocumentCreated += new EventHandler<DocumentCreatedEventArgs>(ReservePojectOriginalStatus);

            application.ControlledApplication.DocumentSaving += new EventHandler<DocumentSavingEventArgs>(CheckProjectStatusUpdate);
            application.ControlledApplication.DocumentSavingAs += new EventHandler<DocumentSavingAsEventArgs>(CheckProjectStatusUpdate);

            application.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(MemClosingDocumentHashCode);
            application.ControlledApplication.DocumentClosed += new EventHandler<DocumentClosedEventArgs>(RemoveStatusofClosedDocument);

            return Result.Succeeded;
        }
        #endregion

        #region EventHandler

        /// <summary>
        /// Event handler method for DocumentOpened and DocumentCreated events.
        /// 此方法将在打开或创建文档后保留“项目状态”值
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">Event arguments that contains the event data.</param>
        private void ReservePojectOriginalStatus(Object sender, RevitAPIPostDocEventArgs args)
        {
            Document doc = args.Document;

            if (doc.IsFamilyDocument)
            {
                return;
            }

            // write log file. 
            LogManager.WriteLog(args, doc);

            // get the hashCode of this document.
            int docHashCode = doc.GetHashCode();

            // retrieve the current value of "Project Status". 
            string currentProjectStatus = RetrieveProjectCurrentStatus(doc);
            // reserve "Project Status" current value in one dictionary, and use this project's hashCode as key.
            documentOriginalStatusDic.Add(docHashCode, currentProjectStatus);

            // write log file. 
            LogManager.WriteLog("   Current Project Status: " + currentProjectStatus);
        }

        /// <summary>
        /// Event handler method for DocumentSaving and DocumentSavingAs events.
        /// 此方法将检查“项目状态”是否已更新，并保留当前值作为原始值。
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">Event arguments that contains the event data.</param>
        private void CheckProjectStatusUpdate(Object sender, RevitAPIPreDocEventArgs args)
        {
            Document doc = args.Document;

            if (doc.IsFamilyDocument)
            {
                return;
            }

            // write log file.
            LogManager.WriteLog(args, doc);

            // retrieve the current value of "Project Status". 
            string currentProjectStatus = RetrieveProjectCurrentStatus(args.Document);

            // get the old value of "Project Status" for one dictionary.
            string originalProjectStatus = documentOriginalStatusDic[doc.GetHashCode()];

            // write log file.
            LogManager.WriteLog("   Current Project Status: " + currentProjectStatus + "; Original Project Status: " + originalProjectStatus);

            // project status has not been updated.
            if ((string.IsNullOrEmpty(currentProjectStatus) && string.IsNullOrEmpty(originalProjectStatus)) ||
                (0 == string.Compare(currentProjectStatus, originalProjectStatus, true)))
            {
                DealNotUpdate(args);//处理项目状态未更新的情况
                return;
            }

            // update "Project Status" value reserved in the dictionary.
            documentOriginalStatusDic.Remove(doc.GetHashCode());
            documentOriginalStatusDic.Add(doc.GetHashCode(), currentProjectStatus);
        }

        private void MemClosingDocumentHashCode(Object sender, DocumentClosingEventArgs args)
        {
            hashcodeofCurrentClosingDoc = args.Document.GetHashCode();
        }

        private void RemoveStatusofClosedDocument(Object sender, DocumentClosedEventArgs args)
        {
            if (args.Status.Equals(RevitAPIEventStatus.Succeeded) && (documentOriginalStatusDic.ContainsKey(hashcodeofCurrentClosingDoc)))
            {
                documentOriginalStatusDic.Remove(hashcodeofCurrentClosingDoc);
            }
        }
        #endregion


        ///
        /// private Method
        ///
        

        /// <summary>
        /// 处理项目状态未更新的情况
        /// If the event is Cancellable, cancel it and inform user else just inform user the status.
        /// 如果事件是可取消的，请取消它并通知用户，否则只需通知用户状态。
        /// </summary>
        /// <param name="args">Event arguments that contains the event data.</param>
        private static void DealNotUpdate(RevitAPIPreDocEventArgs args)
        {
            string mainMessage;
            string additionalText;
            TaskDialog taskDialog = new TaskDialog("CancelSave Sample");

            if (args.Cancellable)
            {
                args.Cancel(); // cancel this event if it is cancellable. 

                mainMessage = "CancelSave sample detected that the Project Status parameter on Project Info has not been updated. The file will not be saved.\nCancelSave示例检测到项目信息上的项目状态参数尚未更新。将不保存该文件。"; // prompt to user.              
            }
            else
            {
                // will not cancel this event since it isn't cancellable. 
                mainMessage = "The file is about to save. But CancelSave sample detected that the Project Status parameter on Project Info has not been updated.\n文件即将保存。但是CancelSave示例检测到项目信息上的项目状态参数尚未更新。"; // prompt to user.              
            }

            // taskDialog will not show when do regression test.
            if (!LogManager.RegressionTestNow)
            {
                additionalText = "You can disable this permanently by uninstaling the CancelSave sample from Revit. Remove or rename CancelSave.addin from the addins directory.\n您可以通过从Revit中取消保存示例来永久禁用此选项:从addins目录中删除或重命名CancelSave.addin";

                // use one taskDialog to inform user current situation.     
                taskDialog.MainInstruction = mainMessage;
                taskDialog.MainContent = additionalText;
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Open the addins directory");
                taskDialog.CommonButtons = TaskDialogCommonButtons.Close;
                taskDialog.DefaultButton = TaskDialogResult.Close;
                TaskDialogResult tResult = taskDialog.Show();
                if (TaskDialogResult.CommandLink1 == tResult)
                {
                    System.Diagnostics.Process.Start("explorer.exe", DetectAddinFileLocation(args.Document.Application));
                }
            }

            // write log file.
            LogManager.WriteLog("   Project Status is not updated, taskDialog informs user: " + mainMessage);
        }

        /// <summary>
        /// Retrieve current value of Project Status.
        /// </summary>
        /// <param name="doc">Document of which the Project Status will be retrieved.</param>
        /// <returns>Current value of Project Status.</returns>
        private static string RetrieveProjectCurrentStatus(Document doc)
        {
            // Project information is unavailable for Family document.
            if (doc.IsFamilyDocument)
            {
                return null;
            }

            // get project status stored in project information object and return it.
            return doc.ProjectInformation.Status;
        }

        private static string DetectAddinFileLocation(Autodesk.Revit.ApplicationServices.Application applictaion)
        {
            string addinFileFolderLocation = null;
            IList<RevitProduct> installedRevitList = RevitProductUtility.GetAllInstalledRevitProducts();

            foreach (RevitProduct revit in installedRevitList)
            {
                if (revit.Version.ToString().Contains(applictaion.VersionNumber))
                {
                    string allUsersAddInFolder = revit.AllUsersAddInFolder;
                    string currentUserAddInFolder = revit.CurrentUserAddInFolder;

                    if (File.Exists(Path.Combine(allUsersAddInFolder, thisAddinFileName)))
                    {
                        addinFileFolderLocation = allUsersAddInFolder;
                    }
                    else if (File.Exists(Path.Combine(currentUserAddInFolder, thisAddinFileName)))
                    {
                        addinFileFolderLocation = currentUserAddInFolder;
                    }

                    break;
                }
            }

            return addinFileFolderLocation;
        }
    }
}
