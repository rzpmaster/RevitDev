using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBIM.Terminal.Common.Utils
{
    /// <summary>
    /// MepCurve的继承子类：
    /// Duct(风管) FlexDuct(软风管) Pipe(水管) FlexPipe(软水管) Wire(导线) 
    /// CableTrayConduitBase(桥架 导管)
    /// InsulationLiningBase(管道外包内衬)
    /// MEPAnalyticalConnection
    /// </summary>
    public class MepCurveUtils
    {
        //MEPCurve
    }

    public class ConnectorUtils
    {
        public static ConnectorSet GetConnectors(Element element)
        {
            if (element == null) return null;

            if (element is MEPCurve)
            {
                MEPCurve mepCurve = element as MEPCurve;
                return mepCurve.ConnectorManager.Connectors;
            }

            if (element is MEPSystem)
            {
                MEPSystem system = element as MEPSystem;
                return system.ConnectorManager.Connectors;
            }

            if (element is FamilyInstance)
            {
                FamilyInstance fi = element as FamilyInstance;
                if (fi.MEPModel != null)
                {
                    return fi.MEPModel.ConnectorManager.Connectors;
                }
            }

            return null;
        }

        public static XYZ GetConnectorDirection(Connector conn)
        {
            Transform t = conn.CoordinateSystem;
            return new XYZ(t.BasisZ.X, t.BasisZ.Y, t.BasisZ.Z);
        }
    }
}
