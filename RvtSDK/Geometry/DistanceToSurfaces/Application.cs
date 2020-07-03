using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceToSurfaces
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class Application : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(docOpen);
            return Result.Succeeded;
        }

        private void docOpen(object sender, DocumentOpenedEventArgs e)
        {
            Autodesk.Revit.ApplicationServices.Application app = sender as Autodesk.Revit.ApplicationServices.Application;
            UIApplication uiApp = new UIApplication(app);
            Document doc = uiApp.ActiveUIDocument.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.WherePasses(new ElementClassFilter(typeof(FamilyInstance)));
            var sphereElements = from element in collector where element.Name == "sphere" select element;
            if (sphereElements.Count() == 0)
            {
                TaskDialog.Show("Error", "Sphere family must be loaded");
                return;
            }
            FamilyInstance sphere = sphereElements.Cast<FamilyInstance>().First<FamilyInstance>();
            FilteredElementCollector viewCollector = new FilteredElementCollector(doc);
            ICollection<Element> views = viewCollector.OfClass(typeof(View3D)).ToElements();
            var viewElements = from element in viewCollector where element.Name == "AVF" select element;
            if (viewElements.Count() == 0)
            {
                TaskDialog.Show("Error", "A 3D view named 'AVF' must exist to run this application.");
                return;
            }
            View view = viewElements.Cast<View>().First<View>();

            SpatialFieldUpdater updater = new SpatialFieldUpdater(uiApp.ActiveAddInId, sphere.Id, view.Id);
            if (!UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId())) UpdaterRegistry.RegisterUpdater(updater);
            ElementCategoryFilter wallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            ElementClassFilter familyFilter = new ElementClassFilter(typeof(FamilyInstance));
            ElementCategoryFilter massFilter = new ElementCategoryFilter(BuiltInCategory.OST_Mass);
            IList<ElementFilter> filterList = new List<ElementFilter>();
            filterList.Add(wallFilter);
            filterList.Add(familyFilter);
            filterList.Add(massFilter);
            LogicalOrFilter filter = new LogicalOrFilter(filterList);

            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeGeometry());
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementDeletion());
        }
    }
}
