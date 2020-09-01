using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections;

namespace GridCreation.CreateForms
{
    public class CreateWithSelectedCurvesData : CreateGridsData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="application">Revit application</param>
        /// <param name="selectedCurves">Array contains geometry curves of selected lines or arcs </param>
        /// <param name="labels">List contains all existing labels in Revit document</param>
        public CreateWithSelectedCurvesData(UIApplication application, CurveArray selectedCurves, ArrayList labels)
           : base(application, labels)
        {
            m_selectedCurves = selectedCurves;
        }

        /// <summary>
        /// Whether to delete selected lines/arc after creation
        /// </summary>
        public bool DeleteSelectedElements
        {
            get
            {
                return m_deleteSelectedElements;
            }
            set
            {
                m_deleteSelectedElements = value;
            }
        }

        /// <summary>
        /// Bubble location of grids
        /// </summary>
        public BubbleLocation BubbleLocation
        {
            get
            {
                return m_bubbleLocation;
            }
            set
            {
                m_bubbleLocation = value;
            }
        }

        /// <summary>
        /// Label of first grid
        /// </summary>
        public String FirstLabel
        {
            get
            {
                return m_firstLabel;
            }
            set
            {
                m_firstLabel = value;
            }
        }

        /// <summary>
        /// Create grids
        /// </summary>
        public void CreateGrids()
        {
            int errorCount = 0;

            CurveArray curves = new CurveArray();

            int i = 0;
            foreach (Curve curve in m_selectedCurves)
            {
                try
                {
                    Line line = curve as Line;
                    if (line != null) // Selected curve is a line
                    {
                        Line lineToCreate;
                        lineToCreate = TransformLine(line, m_bubbleLocation);
                        if (i == 0)
                        {
                            Grid grid;
                            // Create the first grid
                            grid = CreateLinearGrid(lineToCreate);

                            try
                            {
                                // Set label of first grid
                                grid.Name = m_firstLabel;
                            }
                            catch (System.ArgumentException)
                            {
                                ShowMessage(resManager.GetString("FailedToSetLabel") + m_firstLabel + "!",
                                            resManager.GetString("FailureCaptionSetLabel"));
                            }
                        }
                        else
                        {
                            AddCurveForBatchCreation(ref curves, lineToCreate);
                        }
                    }
                    else // Selected curve is an arc
                    {
                        Arc arc = curve as Arc;
                        if (arc != null)
                        {
                            if (arc.IsBound) // Part of a circle
                            {
                                Arc arcToCreate;
                                arcToCreate = TransformArc(arc, m_bubbleLocation);

                                if (i == 0)
                                {
                                    Grid grid;
                                    // Create arc grid
                                    grid = NewGrid(arcToCreate);

                                    try
                                    {
                                        // Set label of first grid
                                        grid.Name = m_firstLabel;
                                    }
                                    catch (System.ArgumentException)
                                    {
                                        ShowMessage(resManager.GetString("FailedToSetLabel") + m_firstLabel + "!",
                                                    resManager.GetString("FailureCaptionSetLabel"));
                                    }
                                }
                                else
                                {
                                    AddCurveForBatchCreation(ref curves, arcToCreate);
                                }
                            }
                            else // Arc is a circle
                            {
                                // In Revit UI user can select a circle to create a grid, but actually two grids 
                                // (One from 0 to 180 degree and the other from 180 degree to 360) will be created. 
                                // In RevitAPI using NewGrid method with a circle as its argument will raise an exception. 
                                // Therefore in this sample we will create two arcs from the upper and lower parts of the 
                                // circle, and then create two grids on the base of the two arcs to accord with UI.
                                Arc upperArc = null;
                                Arc lowerArc = null;

                                TransformCircle(arc, ref upperArc, ref lowerArc, m_bubbleLocation);
                                // Create grids
                                if (i == 0)
                                {
                                    Grid gridUpper;
                                    // Create arc grid
                                    gridUpper = NewGrid(upperArc);
                                    try
                                    {
                                        // Set label of first grid
                                        gridUpper.Name = m_firstLabel;
                                    }
                                    catch (System.ArgumentException)
                                    {
                                        ShowMessage(resManager.GetString("FailedToSetLabel") + m_firstLabel + "!",
                                                    resManager.GetString("FailureCaptionSetLabel"));
                                    }
                                    AddCurveForBatchCreation(ref curves, lowerArc);
                                }
                                else
                                {
                                    AddCurveForBatchCreation(ref curves, upperArc);
                                    AddCurveForBatchCreation(ref curves, lowerArc);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ++errorCount;
                    continue;
                }

                ++i;
            }

            // Create grids with curves
            CreateGrids(curves);

            if (m_deleteSelectedElements)
            {
                try
                {
                    foreach (Element e in Command.GetSelectedModelLinesAndArcs(m_revitDoc))
                    {
                        m_revitDoc.Delete(e.Id);
                    }
                }
                catch (Exception)
                {
                    ShowMessage(resManager.GetString("FailedToDeletedLinesOrArcs"),
                                resManager.GetString("FailureCaptionDeletedLinesOrArcs"));
                }
            }

            if (errorCount != 0)
            {
                ShowMessage(resManager.GetString("FailedToCreateGrids"),
                            resManager.GetString("FailureCaptionCreateGrids"));
            }
        }

        // Selected curves in current document
        private CurveArray m_selectedCurves;
        // Whether to delete selected lines/arc after creation
        private bool m_deleteSelectedElements;
        // Label of first grid
        private String m_firstLabel;
        // Bubble location of grids
        private BubbleLocation m_bubbleLocation;
    }
}
