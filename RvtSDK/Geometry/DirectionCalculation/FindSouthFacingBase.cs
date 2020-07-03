using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectionCalculation
{
    abstract class FindSouthFacingBase
    {
        public FindSouthFacingBase(ExternalCommandData commandData)
        {
            Application = commandData.Application.Application;
            Document = commandData.Application.ActiveUIDocument.Document;
        }

        #region Helper properties
        protected Autodesk.Revit.ApplicationServices.Application Application { get; set; }

        protected Document Document { get; set; }
        #endregion

        public abstract void Execute(bool useProjectLocationNorth);

        protected bool IsSouthFacing(XYZ direction)
        {
            double angleToSouth = direction.AngleTo(-XYZ.BasisY);

            return Math.Abs(angleToSouth) < Math.PI / 4;
        }

        protected XYZ TransformByProjectLocation(XYZ direction)
        {
            ProjectPosition position = Document.ActiveProjectLocation.GetProjectPosition(XYZ.Zero);
            Transform transform = Transform.CreateRotation(XYZ.BasisZ, position.Angle);
            // Rotate the input direction by the transform
            XYZ rotatedDirection = transform.OfVector(direction);
            return rotatedDirection;
        }

        #region Debugging Aids
        /// <summary>
        /// Debugging aid.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="curve"></param>
        public void Write(String label, Curve curve)
        {
            if (m_writer == null)
                m_writer = new System.IO.StreamWriter(@"c:\Directions.txt");
            XYZ start = curve.GetEndPoint(0);
            XYZ end = curve.GetEndPoint(1);

            m_writer.WriteLine(String.Format(label + " {0} {1}", XYZToString(start), XYZToString(end)));
        }

        private String XYZToString(XYZ point)
        {
            return "( " + point.X + ", " + point.Y + ", " + point.Z + ")";
        }

        /// <summary>
        /// Debugging aid.
        /// </summary>
        public void CloseFile()
        {
            if (m_writer != null)
                m_writer.Close();
        }
        #endregion

        private System.IO.TextWriter m_writer;
    }
}
