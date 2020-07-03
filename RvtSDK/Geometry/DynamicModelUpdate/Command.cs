using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicModelUpdate
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        Document m_document;
        UIDocument m_documentUI;
        AddInId m_thisAppId;

        SectionUpdater m_sectionUpdater = null;

        static List<ElementId> idsToWatch = new List<ElementId>();
        static ElementId m_oldSectionId = ElementId.InvalidElementId;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_document = commandData.Application.ActiveUIDocument.Document;
            m_documentUI = commandData.Application.ActiveUIDocument;
            m_thisAppId = commandData.Application.ActiveAddInId;

            if (m_sectionUpdater == null)
            {
                using (Transaction tran = new Transaction(m_document, "Register Section Updater"))
                {
                    tran.Start();

                    m_sectionUpdater = new SectionUpdater(m_thisAppId);
                    m_sectionUpdater.Register(m_document);

                    tran.Commit();
                }
            }

            TaskDialog.Show("Message", "Please select a section view, then select a window.");

            ElementId modelId = null;
            Element sectionElement = null;
            ElementId sectionId = null;

            try
            {
                Reference referSection = m_documentUI.Selection.PickObject(ObjectType.Element, "Please select a section view.");
                if (referSection != null)
                {
                    Element sectionElem = m_document.GetElement(referSection);
                    if (sectionElem != null)
                    {
                        sectionElement = sectionElem;
                    }
                }
                Reference referModel = m_documentUI.Selection.PickObject(ObjectType.Element, "Please select a window to associated with the section view.");
                if (referModel != null)
                {
                    Element model = m_document.GetElement(referModel);
                    if (model != null)
                    {
                        if (model is FamilyInstance)
                            modelId = model.Id;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                TaskDialog.Show("Message", "The selection has been canceled.");
                return Result.Cancelled;
            }

            if (modelId == null)
            {
                TaskDialog.Show("Error", "The model is supposed to be a window.\n The operation will be canceled.");
                return Result.Cancelled;
            }

            // Find the real ViewSection for the selected section element.
            string name = sectionElement.Name;
            FilteredElementCollector collector = new FilteredElementCollector(m_document);
            collector.WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_Views));
            var viewElements = from element in collector
                               where element.Name == name
                               select element;

            List<Autodesk.Revit.DB.Element> sectionViews = viewElements.ToList<Autodesk.Revit.DB.Element>();
            if (sectionViews.Count == 0)
            {
                TaskDialog.Show("Message", "Cannot find the view name " + name + "\n The operation will be canceled.");
                return Result.Failed;
            }
            sectionId = sectionViews[0].Id;

            // Associated the section view to the window, and add a trigger for it.
            if (!idsToWatch.Contains(modelId) || m_oldSectionId != sectionId)
            {
                idsToWatch.Clear();
                idsToWatch.Add(modelId);
                m_oldSectionId = sectionId;
                UpdaterRegistry.RemoveAllTriggers(m_sectionUpdater.GetUpdaterId());
                m_sectionUpdater.AddTriggerForUpdater(m_document, idsToWatch, sectionId, sectionElement);
                TaskDialog.Show("Message", "The ViewSection id: " + sectionId + " has been associated to the window id: " + modelId + "\n You can try to move or modify the window to see how the updater works.");
            }
            else
            {
                TaskDialog.Show("Message", "The model has been already associated to the ViewSection.");
            }

            m_document.DocumentClosing += UnregisterSectionUpdaterOnClose;

            return Result.Succeeded;
        }

        private void UnregisterSectionUpdaterOnClose(object sender, DocumentClosingEventArgs e)
        {
            idsToWatch.Clear();
            m_oldSectionId = ElementId.InvalidElementId;

            if (m_sectionUpdater != null)
            {
                UpdaterRegistry.UnregisterUpdater(m_sectionUpdater.GetUpdaterId());
                m_sectionUpdater = null;
            }
        }
    }
}
