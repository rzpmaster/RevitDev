using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GridCreation.CreateForms;
using System;
using System.Collections;
using System.Windows.Forms;

namespace GridCreation
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document document = commandData.Application.ActiveUIDocument.Document;

                // Get all selected lines and arcs 
                CurveArray selectedCurves = GetSelectedCurves(commandData.Application.ActiveUIDocument.Document);

                // Show UI
                GridCreationOptionData gridCreationOption = new GridCreationOptionData(!selectedCurves.IsEmpty);
                using (GridCreationOptionForm gridCreationOptForm = new GridCreationOptionForm(gridCreationOption))
                {
                    DialogResult result = gridCreationOptForm.ShowDialog();
                    if (result == DialogResult.Cancel)
                    {
                        return Autodesk.Revit.UI.Result.Cancelled;
                    }

                    ArrayList labels = GetAllLabelsOfGrids(document);
                    ForgeTypeId unit = GetLengthUnitType(document);
                    switch (gridCreationOption.CreateGridsMode)
                    {
                        case CreateMode.Select: // Create grids with selected lines/arcs
                            CreateWithSelectedCurvesData data = new CreateWithSelectedCurvesData(commandData.Application, selectedCurves, labels);
                            using (CreateWithSelectedCurvesForm createWithSelected = new CreateWithSelectedCurvesForm(data))
                            {
                                result = createWithSelected.ShowDialog();
                                if (result == DialogResult.OK)
                                {
                                    // Create grids
                                    Transaction transaction = new Transaction(document, "CreateGridsWithSelectedCurves");
                                    transaction.Start();
                                    data.CreateGrids();
                                    transaction.Commit();
                                }
                            }
                            break;

                        //case CreateMode.Orthogonal: // Create orthogonal grids
                        //    CreateOrthogonalGridsData orthogonalData = new CreateOrthogonalGridsData(commandData.Application, unit, labels);
                        //    using (CreateOrthogonalGridsForm orthogonalGridForm = new CreateOrthogonalGridsForm(orthogonalData))
                        //    {
                        //        result = orthogonalGridForm.ShowDialog();
                        //        if (result == DialogResult.OK)
                        //        {
                        //            // Create grids
                        //            Transaction transaction = new Transaction(document, "CreateOrthogonalGrids");
                        //            transaction.Start();
                        //            orthogonalData.CreateGrids();
                        //            transaction.Commit();
                        //        }
                        //    }
                        //    break;

                        //case CreateMode.RadialAndArc: // Create radial and arc grids
                        //    CreateRadialAndArcGridsData radArcData = new CreateRadialAndArcGridsData(commandData.Application, unit, labels);
                        //    using (CreateRadialAndArcGridsForm radArcForm = new CreateRadialAndArcGridsForm(radArcData))
                        //    {
                        //        result = radArcForm.ShowDialog();
                        //        if (result == DialogResult.OK)
                        //        {
                        //            // Create grids
                        //            Transaction transaction = new Transaction(document, "CreateRadialAndArcGrids");
                        //            transaction.Start();
                        //            radArcData.CreateGrids();
                        //            transaction.Commit();
                        //        }
                        //    }
                        //    break;
                    }

                    if (result == DialogResult.OK)
                    {
                        return Autodesk.Revit.UI.Result.Succeeded;
                    }
                    else
                    {
                        return Autodesk.Revit.UI.Result.Cancelled;
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Autodesk.Revit.UI.Result.Failed;
            }
        }

        private ForgeTypeId GetLengthUnitType(Document document)
        {
            ForgeTypeId specTypeId = SpecTypeId.Length;
            Units projectUnit = document.GetUnits();
            try
            {
                FormatOptions formatOption = projectUnit.GetFormatOptions(specTypeId);
                return formatOption.GetUnitTypeId();
            }
            catch (System.Exception /*e*/)
            {
                return UnitTypeId.Feet;
            }
        }

        private ArrayList GetAllLabelsOfGrids(Document document)
        {
            ArrayList labels = new ArrayList();
            FilteredElementIterator itor = new FilteredElementCollector(document).OfClass(typeof(Grid)).GetElementIterator();
            itor.Reset();
            for (; itor.MoveNext();)
            {
                Grid grid = itor.Current as Grid;
                if (null != grid)
                {
                    labels.Add(grid.Name);
                }
            }

            return labels;
        }

        public static ElementSet GetSelectedModelLinesAndArcs(Document document)
        {
            UIDocument newUIdocument = new UIDocument(document);
            ElementSet elements = new ElementSet();
            foreach (ElementId elementId in newUIdocument.Selection.GetElementIds())
            {
                elements.Insert(newUIdocument.Document.GetElement(elementId));
            }
            ElementSet tmpSet = new ElementSet();
            foreach (Autodesk.Revit.DB.Element element in elements)
            {
                if ((element is ModelLine) || (element is ModelArc) || (element is DetailLine) || (element is DetailArc))
                {
                    tmpSet.Insert(element);
                }
            }

            return tmpSet;
        }

        private CurveArray GetSelectedCurves(Document document)
        {
            CurveArray selectedCurves = new CurveArray();
            UIDocument newUIdocument = new UIDocument(document);
            ElementSet elements = new ElementSet();
            foreach (ElementId elementId in newUIdocument.Selection.GetElementIds())
            {
                elements.Insert(newUIdocument.Document.GetElement(elementId));
            }
            foreach (Autodesk.Revit.DB.Element element in elements)
            {
                if ((element is ModelLine) || (element is ModelArc))
                {
                    ModelCurve modelCurve = element as ModelCurve;
                    Curve curve = modelCurve.GeometryCurve;
                    if (curve != null)
                    {
                        selectedCurves.Append(curve);
                    }
                }
                else if ((element is DetailLine) || (element is DetailArc))
                {
                    DetailCurve detailCurve = element as DetailCurve;
                    Curve curve = detailCurve.GeometryCurve;
                    if (curve != null)
                    {
                        selectedCurves.Append(curve);
                    }
                }
            }

            return selectedCurves;
        }
    }
}
