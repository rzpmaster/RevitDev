using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRoute
{
    class ConnectorInfo
    {
        Element m_owner;

        XYZ m_origin;
        Connector m_connector;

        public Connector Connector
        {
            get { return m_connector; }
            set { m_connector = value; }
        }

        public ConnectorInfo(Element owner, XYZ origin)
        {
            m_owner = owner;
            m_origin = origin;
            m_connector = ConnectorInfo.GetConnector(owner, origin);
        }

        public ConnectorInfo(Element owner, double x, double y, double z) : this(owner, new XYZ(x, y, z))
        {
        }

        public static Connector GetConnector(Element owner, XYZ connectorOrigin)
        {
            ConnectorSet connectors = GetConnectors(owner);
            foreach (Connector conn in connectors)
            {
                if (conn.ConnectorType == ConnectorType.Logical)
                    continue;
                if (conn.Origin.IsAlmostEqualTo(connectorOrigin))
                    return conn;
            }
            return null;
        }

        public static ConnectorSet GetConnectors(Autodesk.Revit.DB.Element element)
        {
            if (element == null) return null;
            FamilyInstance fi = element as FamilyInstance;
            if (fi != null && fi.MEPModel != null)
            {
                return fi.MEPModel.ConnectorManager.Connectors;
            }
            MEPSystem system = element as MEPSystem;
            if (system != null)
            {
                return system.ConnectorManager.Connectors;
            }

            MEPCurve duct = element as MEPCurve;
            if (duct != null)
            {
                return duct.ConnectorManager.Connectors;
            }
            return null;
        }
    }

    class LogUtility
    {
        public const string InvalidString = "[!]";

        public static void WriteElement(Element element)
        {
            WriteElement(element, true);
        }

        public static void WriteElement(Element element, bool writeId)
        {
            if (element == null)
            {
                Trace.WriteLine("null"); return;
            }
            int elementId = element.Id.IntegerValue;
            int familyId = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsElementId().IntegerValue;
            string familyName = LogUtility.InvalidString;
            Element objectType = GetElement<Element>(element.Document, familyId);
            string elementName = LogUtility.InvalidString;
            try { elementName = element.Name; }
            catch { }
            if (objectType != null)
            {
                Parameter familyNameParameter = objectType.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                if (familyNameParameter != null)
                    familyName = familyNameParameter.AsString();
            }
            BuiltInCategory category = (BuiltInCategory)(element.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM).AsElementId().IntegerValue);

            string msg = "Type: " + element.GetType().FullName + "\r\n";
            msg += "Name: " + familyName + ":" + elementName + "\r\n";
            if (writeId) msg += "Id: " + elementId + "\r\n";
            msg += "Category: " + category + "\r\n";
            msg += "FamilyId: " + familyId + "\r\n";

            Trace.WriteLine(msg);
        }

        public static void WriteMechanicalSystem(MechanicalSystem system)
        {
            string flow = InvalidString;
            try { flow = system.GetFlow().ToString(); }
            catch (Exception) { }

            Trace.WriteLine("Flow: " + flow);
            Trace.WriteLine("IsWellConnected: " + system.IsWellConnected);
            Trace.WriteLine("SystemType: " + system.SystemType);
            Trace.WriteLine("+DuctNetwork");
            Trace.Indent();
            foreach (Element element in system.DuctNetwork)
            {
                LogUtility.WriteElement(element, false);
                Trace.WriteLine("");
            }
            Trace.Unindent();
            WriteMEPSystem(system);
        }

        public static void WriteMEPSystem(MEPSystem system)
        {
            WriteElement(system.BaseEquipment);
            Trace.Unindent();
            Trace.WriteLine("+BaseEquipmentConnector");
            Trace.Indent();
            WriteConnector(system.BaseEquipmentConnector);
            Trace.Unindent();
            Trace.WriteLine("+Elements");
            Trace.Indent();
            foreach (Element element in system.Elements)
            {
                WriteElement(element);
                Trace.WriteLine("");
            }
            Trace.Unindent();
            Trace.WriteLine("+ConnectorManager");
            Trace.Indent();
            WriteConnectorManager(system.ConnectorManager);
            Trace.Unindent();
        }

        public static void WriteConnectorManager(ConnectorManager connectorManager)
        {
            Trace.WriteLine("+Connectors");
            Trace.Indent();
            WriteConnectorSet2(connectorManager.Connectors);
            Trace.Unindent();
            Trace.WriteLine("+UnusedConnectors");
            Trace.Indent();
            WriteConnectorSet2(connectorManager.UnusedConnectors);
            Trace.Unindent();
        }

        /// <summary>
        /// 只写入当前connectorSet包含的conn
        /// </summary>
        /// <param name="connectorSet"></param>
        private static void WriteConnectorSet(ConnectorSet connectorSet)
        {
            SortedDictionary<string, List<Connector>> connectors = new SortedDictionary<string, List<Connector>>();
            foreach (Connector conn in connectorSet)
            {
                string connId = GetConnectorId(conn);
                if (conn.ConnectorType == ConnectorType.Logical)
                {
                    foreach (Connector logLinkConn in conn.AllRefs)
                    {
                        connId += GetConnectorId(logLinkConn);
                    }
                }
                if (!connectors.ContainsKey(connId))
                {
                    connectors.Add(connId, new List<Connector>());
                }

                connectors[connId].Add(conn);
            }

            foreach (string key in connectors.Keys)
            {
                foreach (Connector conn in connectors[key])
                {
                    WriteConnector(conn);
                    Trace.WriteLine("");
                }
            }
        }

        /// <summary>
        /// 遍历读取connectorSet中所有conn，包括conn.AllRef
        /// </summary>
        /// <param name="connectorSet"></param>
        private static void WriteConnectorSet2(ConnectorSet connectorSet)
        {
            SortedDictionary<string, List<Connector>> connectors = new SortedDictionary<string, List<Connector>>();
            foreach (Connector conn in connectorSet)
            {
                string connId = GetConnectorId(conn);
                if (conn.ConnectorType == ConnectorType.Logical)
                {
                    foreach (Connector logLinkConn in conn.AllRefs)
                    {
                        connId += GetConnectorId(logLinkConn);
                    }
                }
                if (!connectors.ContainsKey(connId))
                {
                    connectors.Add(connId, new List<Connector>());
                }

                connectors[connId].Add(conn);
            }
            foreach (string key in connectors.Keys)
            {
                foreach (Connector conn in connectors[key])
                {
                    WriteConnector(conn);
                    Trace.WriteLine("+AllRefs");
                    Trace.Indent();
                    WriteConnectorSet(conn.AllRefs);
                    Trace.Unindent();
                    Trace.WriteLine("");
                }
            }
        }

        public static void WriteConnector(Connector connector)
        {
            if (connector == null)
            {
                Trace.WriteLine("null"); return;
            }

            string connInfo = GetConnectorId(connector);

            Trace.WriteLine(connInfo);
        }


        /// <summary>
        /// 获得 conn 的信息
        /// </summary>
        /// <param name="connector"></param>
        /// <returns></returns>
        public static string GetConnectorId(Connector connector)
        {
            if (connector == null)
            {
                return "null";
            }

            //ownerId
            int ownerId = connector.Owner.Id.IntegerValue;
            //systemId
            string systemId = InvalidString;
            try { systemId = connector.MEPSystem.Id.IntegerValue.ToString(); }
            catch { }

            //connShape
            object connShape = InvalidString;
            try { connShape = connector.Shape; }
            catch { }
            //connSize
            object connSize = InvalidString;
            try { connSize = GetShapeInfo(connector); }
            catch { }
            //connOrigin
            object connLocation = GetLocation(connector);
            //eumnConnectorType
            object connAType = connector.ConnectorType;

            //
            object connType = InvalidString;
            object connDirection = InvalidString;
            object connIsConnected = InvalidString;
            switch (connector.Domain)
            {
                case Domain.DomainElectrical:   //电气连接件
                    connType = connector.ElectricalSystemType;
                    break;
                case Domain.DomainHvac:         //暖通连接件
                    connType = connector.DuctSystemType;
                    connDirection = connector.Direction;
                    connIsConnected = connector.IsConnected;
                    break;
                case Domain.DomainPiping:       //给排水连接件
                    connType = connector.PipeSystemType;
                    connDirection = connector.Direction;
                    connIsConnected = connector.IsConnected;
                    break;
                case Domain.DomainUndefined:
                default:
                    connType = Domain.DomainUndefined;
                    break;
            }
            return string.Format("ownerId[{0}]\tconnType[{1}]\tconnDirection[{2}]\tconnShape[{3}]\tconnSize[{4}]\tconnLocation[{5}]\tconnAType[{6}]\tconnIsConnected[{7}]\tsystemId[{8}]\t",
                ownerId, connType, connDirection, connShape, connSize, connLocation,
                connAType, connIsConnected, systemId);
        }

        private static string GetShapeInfo(Connector conn)
        {
            switch (conn.Shape)
            {
                case ConnectorProfileType.Invalid:
                    break;
                case ConnectorProfileType.Oval:         //椭圆
                    break;
                case ConnectorProfileType.Rectangular:  //矩形
                    return string.Format("{0}\" x {1}\"", conn.Width, conn.Height);
                case ConnectorProfileType.Round:        //圆形
                    return string.Format("{0}\"", conn.Radius);
                default:
                    break;
            }
            return InvalidString;
        }

        private static object GetLocation(Connector conn)
        {
            if (conn.ConnectorType == ConnectorType.Logical)
            {
                //注意，逻辑连接件没有 Origin
                return InvalidString;
            }
            Autodesk.Revit.DB.XYZ origin = conn.Origin;
            return string.Format("{0},{1},{2}", origin.X, origin.Y, origin.Z);
        }

        public static T GetElement<T>(Document document, int eid) where T : Autodesk.Revit.DB.Element
        {
            Autodesk.Revit.DB.ElementId elementId = new ElementId(eid);
            return document.GetElement(elementId) as T;
        }

    }
}
