using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;

namespace PostCommandWorkflow
{
    class PostCommandRevisionMonitor
    {
        Document document;
        AddInCommandBinding binding = null;
        ExternalEvent externalEvent = null;

        /// <summary>
        /// 存储上一次的修订版本号
        /// </summary>
        int storedRevisionCount = 0;

        public PostCommandRevisionMonitor(Document doc)
        {
            this.document = doc;
        }

        internal void Activate()
        {
            // Save the number of revisions as an initial count.
            storedRevisionCount = GetRevisionCount(document);

            // Setup event for saving.
            document.DocumentSaving += OnSavingPromptForRevisions;
        }

        internal void Deactivate()
        {
            // Remove the event for saving.
            document.DocumentSaving -= OnSavingPromptForRevisions;
        }

        private void OnSavingPromptForRevisions(object sender, DocumentSavingEventArgs args)
        {
            Document doc = (Document)sender;
            UIApplication uiApp = new UIDocument(doc).Application;

            if (doc.IsModified)
            {
                // Compare number of revisions with saved count
                int revisionCount = GetRevisionCount(doc);
                if (revisionCount <= storedRevisionCount)
                {
                    // Show dialog with explanation and options
                    TaskDialog td = new TaskDialog("Revisions not created.");
                    td.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                    td.MainInstruction = "此文件已经修改，但还未添加新修订。";
                    td.ExpandedContent = "因为文档已经修改，所以通常需要发布一个新的修订版本号。";
                    td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "添加修订");
                    td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "取消保存");
                    td.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "继续保存(不推荐)");
                    td.TitleAutoPrefix = false;
                    td.AllowCancellation = false;
                    TaskDialogResult result = td.Show();

                    switch (result)
                    {
                        case TaskDialogResult.CommandLink1:  // Add revision now
                            {
                                // cancel first save
                                args.Cancel();

                                // 注册DialogBoxShowing事件 用于隐藏"Document not saved" dialog
                                uiApp.DialogBoxShowing += HideDocumentNotSaved;

                                // post command for editing revisions
                                PromptToEditRevisionsAndResave(uiApp);
                                break;
                            }
                        case TaskDialogResult.CommandLink2:  // Cancel save
                            {
                                // cancel saving only
                                args.Cancel();
                                break;
                            }
                        case TaskDialogResult.CommandLink3:  // Proceed with save
                            {
                                // do nothing
                                break;
                            }
                    }
                }
                else
                {
                    storedRevisionCount = revisionCount;
                }
            }
        }

        private void HideDocumentNotSaved(object sender, DialogBoxShowingEventArgs args)
        {
            TaskDialogShowingEventArgs tdArgs = args as TaskDialogShowingEventArgs;

            // 由于阻止了保存文件，会弹出“为保存文件”对话框，这里将它频闭掉
            if (tdArgs != null && tdArgs.Message.Contains("未保存"))
                args.OverrideResult(0x0008);
        }

        /// <summary>
        /// 提示编辑修订版本号,并重新保存
        /// </summary>
        /// <param name="application"></param>
        private void PromptToEditRevisionsAndResave(UIApplication application)
        {
            // Setup external event to be notified when activity is done
            externalEvent = ExternalEvent.Create(new PostCommandRevisionMonitorEventHandler(this));

            // Setup event to be notified when revisions command starts (this is a good place to raise this external event)
            RevitCommandId id = RevitCommandId.LookupPostableCommandId(PostableCommand.SheetIssuesOrRevisions);
            if (binding == null)
            {
                binding = application.CreateAddInCommandBinding(id);
            }

            binding.BeforeExecuted += ReactToRevisionsAndSchedulesCommand;
            //在执行RevitCommandId之前，执行externalEvent，也就是 CleanupAfterRevisionEdit 方法，这个方法注销了注册的事件，并 repost了 Save 命令
            //这里为什么要使用外部事件？先执行屏蔽“未保存文件”对话框，然后外部事件排队，执行修订命令的对话框，执行结束后revit空闲，这里才执行 CleanupAfterRevisionEdit 方法，注销事件（以防止影响到其他命令），在调用保存方法。

            // Post the revision editing command
            application.PostCommand(id);
        }

        private void ReactToRevisionsAndSchedulesCommand(object sender, BeforeExecutedEventArgs e)
        {
            if (externalEvent != null)
                externalEvent.Raise();//实际执行了 CleanupAfterRevisionEdit 方法。
        }

        /// <summary>
        /// 修改修订号后,退订事件,重新调用 保存命令
        /// </summary>
        /// <param name="uiApp"></param>
        private void CleanupAfterRevisionEdit(UIApplication uiApp)
        {
            // Remove dialog box showing
            uiApp.DialogBoxShowing -= HideDocumentNotSaved;

            if (binding != null)
                binding.BeforeExecuted -= ReactToRevisionsAndSchedulesCommand;
            externalEvent = null;

            // Repost the save command
            uiApp.PostCommand(RevitCommandId.LookupPostableCommandId(PostableCommand.Save));
        }

        /// <summary>
        /// 外部事件,用于退订uiapp的事件
        /// </summary>
        class PostCommandRevisionMonitorEventHandler : IExternalEventHandler
        {
            PostCommandRevisionMonitor monitor;

            public PostCommandRevisionMonitorEventHandler(PostCommandRevisionMonitor monitor)
            {
                this.monitor = monitor;
            }

            public void Execute(UIApplication app)
            {
                monitor.CleanupAfterRevisionEdit(app);
            }

            public string GetName()
            {
                return nameof(PostCommandRevisionMonitorEventHandler);
            }
        }

        /// <summary>
        /// 获取当前文档的修订版本号
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static int GetRevisionCount(Document doc)
        {
            // Find revision objects
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Revisions);
            return collector.ToElementIds().Count;
        }
    }
}