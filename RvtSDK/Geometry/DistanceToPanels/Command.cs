using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceToPanels
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        UIApplication m_uiApp;
        UIDocument m_uiDoc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_uiApp = commandData.Application;
            m_uiDoc = m_uiApp.ActiveUIDocument;

            // get the target element to be used for the Distance computation
            ElementSet collection = new ElementSet();
            foreach (ElementId elementId in m_uiDoc.Selection.GetElementIds())
            {
                collection.Insert(m_uiDoc.Document.GetElement(elementId));
            }

            Parameter param = null;
            ElementSet es = new ElementSet();
            foreach (ElementId elementId in m_uiDoc.Selection.GetElementIds())
            {
                es.Insert(m_uiDoc.Document.GetElement(elementId));
            }
            XYZ targetPoint = getTargetPoint(es);

            // get all the divided surfaces in the Revit document
            List<DividedSurface> dsList = GetElements<DividedSurface>();
            foreach (DividedSurface ds in dsList)
            {
                GridNode gn = new GridNode();
                int u = 0;
                while (u < ds.NumberOfUGridlines)
                {
                    gn.UIndex = u;
                    int v = 0;
                    while (v < ds.NumberOfVGridlines)
                    {
                        gn.VIndex = v;
                        if (ds.IsSeedNode(gn))
                        {
                            FamilyInstance familyinstance = ds.GetTileFamilyInstance(gn, 0);
                            if (familyinstance != null)
                            {
                                param = familyinstance.LookupParameter("Distance");
                                if (param == null) throw new Exception("Panel family must have a Distance instance parameter");
                                else
                                {
                                    LocationPoint loc = familyinstance.Location as LocationPoint;
                                    XYZ panelPoint = loc.Point;

                                    double d = Math.Sqrt(Math.Pow((targetPoint.X - panelPoint.X), 2) + Math.Pow((targetPoint.Y - panelPoint.Y), 2) + Math.Pow((targetPoint.Z - panelPoint.Z), 2));
                                    param.Set(d);
                                }
                            }
                        }
                        v++;
                    }
                    u++;
                }
            }

            return Result.Succeeded;
        }

        private List<T> GetElements<T>() where T : Element
        {
            List<T> returns = new List<T>();
            FilteredElementCollector collector = new FilteredElementCollector(m_uiDoc.Document);
            ICollection<Element> founds = collector.OfClass(typeof(T)).ToElements();
            foreach (Element elem in founds)
            {
                returns.Add(elem as T);
            }
            return returns;
        }

        private XYZ getTargetPoint(ElementSet collection)
        {
            FamilyInstance targetElement = null;
            if (collection.Size != 1)
            {
                throw new Exception("必须选择一个构件，从中可以测量到面板的距离");
            }
            else
            {
                foreach (Element e in collection)
                {
                    targetElement = e as FamilyInstance;
                }
            }

            if (null == targetElement)
            {
                throw new Exception("必须选择一个构件，从中可以测量到面板的距离");
            }
            LocationPoint targetLocation = targetElement.Location as LocationPoint;
            return targetLocation.Point;
        }
    }
}
