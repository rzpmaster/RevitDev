using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorHandling
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand, IExternalApplication
    {
        /// <summary>
        /// The failure definition id for warning
        /// </summary>
        public static FailureDefinitionId m_idWarning;
        /// <summary>
        /// The failure definition id for error
        /// </summary>
        public static FailureDefinitionId m_idError;
        /// <summary>
        /// The failure definition for warning
        /// </summary>
        private FailureDefinition m_fdWarning;
        /// <summary>
        /// The failure definition for error
        /// </summary>
        private FailureDefinition m_fdError;
        /// <summary>
        /// The Revit application
        /// </summary>
        private Application m_revitApp;
        /// <summary>
        /// The active document
        /// </summary>
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            throw new NotImplementedException();
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            throw new NotImplementedException();
        }

        public Result OnStartup(UIControlledApplication application)
        {
            throw new NotImplementedException();
        }
    }
}
