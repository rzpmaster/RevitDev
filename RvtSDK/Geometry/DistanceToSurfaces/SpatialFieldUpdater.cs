using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceToSurfaces
{
    class SpatialFieldUpdater : IUpdater
    {
        AddInId addinID;
        UpdaterId updaterID;
        ElementId sphereID;
        ElementId viewID;

        public SpatialFieldUpdater(AddInId id, ElementId sphere, ElementId view)
        {
            addinID = id;
            sphereID = sphere;
            viewID = view;
            updaterID = new UpdaterId(addinID, new Guid("FBF2F6B2-4C06-42d4-97C1-D1B4EB593EFF"));
        }

        #region IUpdater Members
        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;

            View view = doc.GetElement(viewID) as View;
            FamilyInstance sphere = doc.GetElement(sphereID) as FamilyInstance;
            LocationPoint sphereLP = sphere.Location as LocationPoint;
            XYZ sphereXYZ = sphereLP.Point;

            SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(view);
            if (sfm == null) sfm = SpatialFieldManager.CreateSpatialFieldManager(view, 3); // Three measurement values for each point
            sfm.Clear();

            FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id);
            ElementCategoryFilter wallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            ElementCategoryFilter massFilter = new ElementCategoryFilter(BuiltInCategory.OST_Mass);
            LogicalOrFilter filter = new LogicalOrFilter(wallFilter, massFilter);
            ICollection<Element> elements = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            foreach (Face face in GetFaces(elements))
            {
                int idx = sfm.AddSpatialFieldPrimitive(face.Reference);
                List<double> doubleList = new List<double>();
                IList<UV> uvPts = new List<UV>();
                IList<ValueAtPoint> valList = new List<ValueAtPoint>();
                BoundingBoxUV bb = face.GetBoundingBox();
                for (double u = bb.Min.U; u < bb.Max.U; u = u + (bb.Max.U - bb.Min.U) / 15)
                {
                    for (double v = bb.Min.V; v < bb.Max.V; v = v + (bb.Max.V - bb.Min.V) / 15)
                    {
                        UV uvPnt = new UV(u, v);
                        uvPts.Add(uvPnt);
                        XYZ faceXYZ = face.Evaluate(uvPnt);
                        // Specify three values for each point
                        doubleList.Add(faceXYZ.DistanceTo(sphereXYZ));
                        doubleList.Add(-faceXYZ.DistanceTo(sphereXYZ));
                        doubleList.Add(faceXYZ.DistanceTo(sphereXYZ) * 10);
                        valList.Add(new ValueAtPoint(doubleList));
                        doubleList.Clear();
                    }
                }
                FieldDomainPointsByUV pnts = new FieldDomainPointsByUV(uvPts);
                FieldValues vals = new FieldValues(valList);

                AnalysisResultSchema resultSchema1 = new AnalysisResultSchema("Schema 1", "Schema 1 Description");
                IList<int> registeredResults = new List<int>();
                registeredResults = sfm.GetRegisteredResults();
                int idx1 = 0;
                if (registeredResults.Count == 0)
                {
                    idx1 = sfm.RegisterResult(resultSchema1);
                }
                else
                {
                    idx1 = registeredResults.First();
                }
                sfm.UpdateSpatialFieldPrimitive(idx, pnts, vals, idx1);
            }
        }

        public string GetAdditionalInformation()
        {
            return "Calculate distance from sphere to walls and display results";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls;
        }

        public UpdaterId GetUpdaterId()
        {
            return updaterID;
        }

        public string GetUpdaterName()
        {
            return "Distance to Surfaces";
        }
        #endregion


        private FaceArray GetFaces(ICollection<Element> elements)
        {
            FaceArray faceArray = new FaceArray();
            Options options = new Options();
            options.ComputeReferences = true;
            foreach (Element element in elements)
            {
                GeometryElement geomElem = element.get_Geometry(options);
                if (geomElem != null)
                {
                    foreach (GeometryObject geomObj in geomElem)
                    {
                        Solid solid = geomObj as Solid;
                        if (solid != null)
                        {
                            foreach (Face f in solid.Faces)
                            {
                                faceArray.Append(f);
                            }
                        }
                        GeometryInstance inst = geomObj as GeometryInstance;
                        if (inst != null) // in-place family walls
                        {
                            foreach (Object o in inst.SymbolGeometry)
                            {
                                Solid s = o as Solid;
                                if (s != null)
                                {
                                    foreach (Face f in s.Faces)
                                    {
                                        faceArray.Append(f);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return faceArray;
        }
    }
}
