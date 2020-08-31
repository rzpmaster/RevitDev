using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuplicateViews
{
    /// <summary>
    /// 复制所有可复制的绘图视图和明细表
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class DuplicateAcrossDocumentsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Autodesk.Revit.ApplicationServices.Application application = commandData.Application.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;

            // Find target document - it must be the only other open document in session
            Document toDocument = null;
            IEnumerable<Document> documents = application.Documents.Cast<Document>();
            if (documents.Count<Document>() != 2)
            {
                TaskDialog.Show("No target document",
                                "This tool can only be used if there are two documents (a source document and target document).");
                return Result.Cancelled;
            }
            foreach (Document loadedDoc in documents)
            {
                if (loadedDoc.Title != doc.Title)
                {
                    toDocument = loadedDoc;
                    break;
                }
            }

            // Collect schedules and drafting views
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            List<Type> viewTypes = new List<Type>();
            viewTypes.Add(typeof(ViewSchedule));    // 明细表视图
            viewTypes.Add(typeof(ViewDrafting));    // 绘图试图
            ElementMulticlassFilter filter = new ElementMulticlassFilter(viewTypes);
            collector.WherePasses(filter);

            // skip view-specfic schedules (e.g. Revision Schedules);
            // These should not be copied as they are associated to another view that cannot be copied
            collector.WhereElementIsViewIndependent(); 
            

            // Copy all schedules together so that any dependency elements are copied only once
            IEnumerable<ViewSchedule> schedules = collector.OfType<ViewSchedule>();
            DuplicateViewUtils.DuplicateSchedules(doc, schedules, toDocument);
            int numSchedules = schedules.Count<ViewSchedule>();

            // Copy drafting views together
            IEnumerable<ViewDrafting> draftingViews = collector.OfType<ViewDrafting>();
            int numDraftingElements =
                DuplicateViewUtils.DuplicateDraftingViews(doc, draftingViews, toDocument);
            int numDrafting = draftingViews.Count<ViewDrafting>();

            // Show results
            TaskDialog.Show("Statistics",
                   String.Format("Copied: \n" +
                                "\t{0} schedules.\n" +
                                "\t{1} drafting views.\n" +
                                "\t{2} new drafting elements created.",
                   numSchedules, numDrafting, numDraftingElements));

            return Result.Succeeded;
        }
    }
}
