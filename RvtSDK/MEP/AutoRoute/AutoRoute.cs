using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using RevitUtils.RvtElem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRoute
{
    class AutoRoute
    {
        Document m_document;

        /// <summary>
        /// The mechanical system that will be created
        /// 机械系统，包含主机械设备，和其他元素，通常在rvt中用一种颜色表示
        /// </summary>
        MechanicalSystem m_mechanicalSystem;
        /// <summary>
        /// The system type id of the duct
        /// 过滤 复制 修改，例如：送风系统
        /// </summary>
        ElementId m_systemTypeId;
        /// <summary>
        /// The type of the ducts in the system
        /// 过滤 复制 修改，例如：半径弯头/T 形三通
        /// </summary>
        ElementId m_ductTypeId;

        DuctType m_ductType;
        Level m_level;

        /// <summary>
        /// 一个管件的最小长度
        /// </summary>
        const double min1FittingLength = 5;

        /// <summary>
        /// 风管的最小长度
        /// </summary>
        const double minDuctLength = 0.5;

        /// <summary>
        /// 垂直偏移量
        /// </summary>
        const double verticalTrunkOffset = 15;

        /// <summary>
        /// 水平可选偏移长度
        /// </summary>
        const double horizontalOptionalTrunkOffset = 5;

        /// <summary>
        /// 两个管件的最小长度
        /// </summary>
        const double min2FittingsLength = min1FittingLength * 2;

        /// <summary>
        /// 一个风管加两个管件的长度
        /// </summary>
        const double min1Duct2FittingsLength = min1FittingLength * 2 + minDuctLength;

        public AutoRoute(Document document)
        {
            this.m_document = document;
            this.m_ductTypeId = new ElementId(142431); //半径弯头/T 形三通
            this.m_systemTypeId = ElementId.InvalidElementId;
        }

        public Result Run()
        {
            Trace.Listeners.Clear();
            Trace.AutoFlush = true;
            string outputFileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "AutoRoute.log");
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }
            TextWriterTraceListener listener = new TextWriterTraceListener(outputFileName);
            Trace.Listeners.Add(listener);

            //
            ElementClassFilter systemTypeFilter = new ElementClassFilter(typeof(MEPSystemType));
            FilteredElementCollector C = new FilteredElementCollector(m_document);
            C.WherePasses(systemTypeFilter);
            foreach (MEPSystemType type in C)
            {
                if (type.SystemClassification == MEPSystemClassification.SupplyAir)
                {
                    m_systemTypeId = type.Id; //送风系统
                    break;
                }
            }

            Transaction transaction = new Transaction(m_document, "Sample_AutoRoute");
            try
            {
                transaction.Start();

                //标高
                m_level = Level.Create(m_document, 0.0);

                //Element
                List<Autodesk.Revit.DB.ElementId> ids = new List<ElementId>();
                ids.Add(new ElementId(730127)); //baseEquipment
                ids.Add(new ElementId(730237));
                ids.Add(new ElementId(730244));

                //Init Elements Info
                FamilyInstance[] instances = new FamilyInstance[3];
                BoundingBoxXYZ[] boxes = new BoundingBoxXYZ[3];
                Connector[] conns = new Connector[3];

                ConnectorSetIterator csi = null;
                for (int i = 0; i < ids.Count; i++)
                {
                    Element element = m_document.GetElement(ids[i]);
                    if (null == element)
                    {
                        return Result.Failed;
                    }

                    instances[i] = element as FamilyInstance;
                    csi = ConnectorInfo.GetConnectors(element).ForwardIterator();
                    csi.MoveNext();
                    conns[i] = csi.Current as Connector;
                    boxes[i] = instances[i].get_BoundingBox(m_document.ActiveView);
                }

                //Find the "Out" and "SupplyAir" connector on the base equipment
                //the first element is base equipment
                var baseEquipment = instances[0];
                csi = ConnectorInfo.GetConnectors(baseEquipment).ForwardIterator();
                while (csi.MoveNext())
                {
                    Connector conn = csi.Current as Connector;
                    if (conn.Domain == Domain.DomainHvac ||
                        conn.Domain == Domain.DomainPiping)
                    {
                        //conn.Direction 只有DomainHvac和DomainPiping有
                        if (conn.Direction == FlowDirectionType.Out)
                        {
                            DuctSystemType ductSystemType = DuctSystemType.UndefinedSystemType;
                            try
                            {
                                //DuctSystemType PipeSystemType ElectricalSystemType
                                //每个连接件只有上述三个中的一个，调用其他时回报异常
                                ductSystemType = conn.DuctSystemType;
                            }
                            catch { continue; }
                            if (ductSystemType == DuctSystemType.SupplyAir)
                            {
                                conns[0] = conn;
                                break;
                            }
                        }
                    }

                }

                //mechanicalSystem
                m_mechanicalSystem = CreateMechanicalSystem(
                        //[378728][SupplyAir][Out][RectProfile][OST_MechanicalEquipment]
                        new ConnectorInfo(baseEquipment, conns[0].Origin.X, conns[0].Origin.Y, conns[0].Origin.Z),
                    new ConnectorInfo[]{
                        //[378707][SupplyAir][In][RectProfile]
                        new ConnectorInfo(instances[1], conns[1].Origin.X, conns[1].Origin.Y, conns[1].Origin.Z),
                        //[378716][SupplyAir][In][RectProfile]
                        new ConnectorInfo(instances[2], conns[2].Origin.X, conns[2].Origin.Y, conns[2].Origin.Z)
                    },
                    DuctSystemType.SupplyAir
                );

                //all elements max bbox
                var maxbbox = BoundingBoxUtils.GetElementsBounding(boxes.ToList());
                double minX = maxbbox.Min.X, minY = maxbbox.Min.Y, minZ = maxbbox.Min.Z;
                double maxX = maxbbox.Max.X, maxY = maxbbox.Max.Y, maxZ = maxbbox.Max.Z;

                //Calculate the optional values(可选值) for the trunk ducts
                double midX = (minX + maxX) / 2;
                double midY = (minY + maxY) / 2;
                double[] baseXValues = new double[3] { midX, (minX + midX) / 2, (maxX + midX) / 2 };
                double[] baseYValues = new double[3] { midY, (minY + midY) / 2, (maxY + midY) / 2 };

                //ductType
                m_ductType = m_document.GetElement(m_ductTypeId) as DuctType;

                List<XYZ> points = new List<XYZ>();

                //conn[0]相关点
                #region conn[0]
                XYZ connectorDirection = conns[0].CoordinateSystem.BasisZ;
                if (0 == connectorDirection.DistanceTo(new XYZ(-1, 0, 0)))
                {
                    points.Add(new XYZ(conns[0].Origin.X - min1FittingLength, conns[0].Origin.Y, conns[0].Origin.Z));
                    points.Add(new XYZ(conns[0].Origin.X - min2FittingsLength, conns[0].Origin.Y, conns[0].Origin.Z + min1FittingLength));
                    points.Add(new XYZ(conns[0].Origin.X - min2FittingsLength, conns[0].Origin.Y, maxZ + verticalTrunkOffset - min1FittingLength));
                }
                else if (0 == connectorDirection.DistanceTo(new XYZ(1, 0, 0)))
                {
                    points.Add(new XYZ(conns[0].Origin.X + min1FittingLength, conns[0].Origin.Y, conns[0].Origin.Z));
                    points.Add(new XYZ(conns[0].Origin.X + min2FittingsLength, conns[0].Origin.Y, conns[0].Origin.Z + min1FittingLength));
                    points.Add(new XYZ(conns[0].Origin.X + min2FittingsLength, conns[0].Origin.Y, maxZ + verticalTrunkOffset - min1FittingLength));
                }
                else if (0 == connectorDirection.DistanceTo(new XYZ(0, -1, 0)))
                {
                    points.Add(new XYZ(conns[0].Origin.X, conns[0].Origin.Y - min1FittingLength, conns[0].Origin.Z));
                    points.Add(new XYZ(conns[0].Origin.X, conns[0].Origin.Y - min2FittingsLength, conns[0].Origin.Z + min1FittingLength));
                    points.Add(new XYZ(conns[0].Origin.X, conns[0].Origin.Y - min2FittingsLength, maxZ + verticalTrunkOffset - min1FittingLength));
                }
                else if (0 == connectorDirection.DistanceTo(new XYZ(0, 1, 0)))
                {
                    points.Add(new XYZ(conns[0].Origin.X, conns[0].Origin.Y + min1FittingLength, conns[0].Origin.Z));
                    points.Add(new XYZ(conns[0].Origin.X, conns[0].Origin.Y + min2FittingsLength, conns[0].Origin.Z + min1FittingLength));
                    points.Add(new XYZ(conns[0].Origin.X, conns[0].Origin.Y + min2FittingsLength, maxZ + verticalTrunkOffset - min1FittingLength));
                } 
                #endregion

                //开始创建风管
                List<Duct> ducts = new List<Duct>();
                List<Connector> connectors = new List<Connector>();
                List<Connector> baseConnectors = new List<Connector>();

                ducts.Add(Duct.Create(m_document, m_ductTypeId, m_level.Id, conns[0], points[0]));
                ducts.Add(Duct.Create(m_document, m_systemTypeId, m_ductTypeId, m_level.Id, points[1], points[2]));

                connectors.Add(ConnectorInfo.GetConnector(ducts[0], points[0]));
                connectors.Add(ConnectorInfo.GetConnector(ducts[1], points[1]));
                connectors.Add(ConnectorInfo.GetConnector(ducts[1], points[2]));

                //连接管道，二选一，效果是一样的
                //connectors[0].ConnectTo(connectors[1]);
                m_document.Create.NewElbowFitting(connectors[0], connectors[1]);

                baseConnectors.Add(connectors[2]);

                //Create the vertical ducts for terminals
                points.Clear();
                ducts.Clear();

                //conn[1] conn[2] 相关点
                points.Add(new XYZ(conns[1].Origin.X, conns[1].Origin.Y, maxZ + verticalTrunkOffset - min1FittingLength));
                points.Add(new XYZ(conns[2].Origin.X, conns[2].Origin.Y, maxZ + verticalTrunkOffset - min1FittingLength));

                ducts.Add(Duct.Create(m_document, m_ductTypeId, m_level.Id, conns[1], points[0]));
                ducts.Add(Duct.Create(m_document, m_ductTypeId, m_level.Id, conns[2], points[1]));
                baseConnectors.Add(ConnectorInfo.GetConnector(ducts[0], points[0]));
                baseConnectors.Add(ConnectorInfo.GetConnector(ducts[1], points[1]));


                ////最顶部的 baseConnectors 相关
                //SortConnectorsByX(baseConnectors);
                //for (int i = 0; i < baseYValues.Length; ++i)
                //{
                //    if (ConnectSystemOnXAxis(baseConnectors, baseYValues[i]))
                //    {
                //        LogUtility.WriteMechanicalSystem(m_mechanicalSystem);
                //        return Autodesk.Revit.UI.Result.Succeeded;
                //    }
                //}

                SortConnectorsByY(baseConnectors);
                for (int i = 0; i < baseXValues.Length; ++i)
                {
                    if (ConnectSystemOnYAxis(baseConnectors, baseXValues[i]))
                    {
                        LogUtility.WriteMechanicalSystem(m_mechanicalSystem);

                        transaction.Commit();
                        return Autodesk.Revit.UI.Result.Succeeded;
                    }
                }


                ////如果任然无法连接，把干管放到maxbbox外
                //SortConnectorsByX(baseConnectors);
                //if (ConnectSystemOnXAxis(baseConnectors, maxY + horizontalOptionalTrunkOffset))
                //{
                //    LogUtility.WriteMechanicalSystem(m_mechanicalSystem);
                //    return Autodesk.Revit.UI.Result.Succeeded;
                //}

                //SortConnectorsByY(baseConnectors);
                //if (ConnectSystemOnYAxis(baseConnectors, maxX + horizontalOptionalTrunkOffset))
                //{
                //    LogUtility.WriteMechanicalSystem(m_mechanicalSystem);
                //    return Autodesk.Revit.UI.Result.Succeeded;
                //}


                ////如果任然无法连接，随便连一个，让revit报错吧
                //connectors.Clear();
                //SortConnectorsByX(baseConnectors);
                //connectors.AddRange(CreateDuct(new XYZ(baseConnectors[0].Origin.X + min1FittingLength, baseYValues[0], maxZ + verticalTrunkOffset), new XYZ(baseConnectors[1].Origin.X - min1FittingLength, baseYValues[0], maxZ + verticalTrunkOffset)));
                //connectors.AddRange(CreateDuct(new XYZ(baseConnectors[1].Origin.X + min1FittingLength, baseYValues[0], maxZ + verticalTrunkOffset), new XYZ(baseConnectors[2].Origin.X - min1FittingLength, baseYValues[0], maxZ + verticalTrunkOffset)));
                //ConnectWithElbowFittingOnXAxis(baseConnectors[0], connectors[0]);
                //ConnectWithElbowFittingOnXAxis(baseConnectors[2], connectors[3]);
                //ConnectWithTeeFittingOnXAxis(baseConnectors[1], connectors[1], connectors[2], false);
            }
            catch (Exception ex)
            {
                transaction.RollBack();
                Trace.WriteLine(ex.ToString());
                return Autodesk.Revit.UI.Result.Failed;
            }
            finally
            {
                Trace.Flush();
                listener.Close();
                Trace.Close();
                Trace.Listeners.Remove(listener);
            }

            transaction.RollBack();
            return Result.Succeeded;
        }

        private bool ConnectSystemOnYAxis(List<Connector> baseConnectors, double baseX)
        {
            //Check the count of the base connectors
            if (null == baseConnectors || 3 != baseConnectors.Count)
            {
                return false;
            }
            for (int i = 0; i < baseConnectors.Count; ++i)
            {
                //Check the distance of the connector from the trunk
                if (baseConnectors[i].Origin.X != baseX && Math.Abs(baseConnectors[i].Origin.X - baseX) < min1Duct2FittingsLength)
                {
                    return false;
                }
                //Check the distance of the connectors on Y axis
                for (int j = i + 1; j < baseConnectors.Count; ++j)
                {
                    if (baseConnectors[j].Origin.Y != baseConnectors[i].Origin.Y && baseConnectors[j].Origin.Y - baseConnectors[i].Origin.Y < min2FittingsLength)
                    {
                        return false;
                    }
                }
            }
            try
            {
                double baseZ = baseConnectors[0].Origin.Z + min1FittingLength;
                //Create the ducts and elbow fittings to connect the vertical ducts and the trunk ducts
                List<Connector> connectors = new List<Connector>();

                if (baseConnectors[0].Origin.Y == baseConnectors[1].Origin.Y)
                {
                    //All 3 connectors are with the same Y value
                    if (baseConnectors[1].Origin.Y == baseConnectors[2].Origin.Y)
                    {
                        return false;
                    }
                    else
                    {
                        //The 1st and 2nd base connectors are on the same side of the trunk
                        if (Math.Sign(baseConnectors[0].Origin.X - baseX) * Math.Sign(baseConnectors[1].Origin.X - baseX) == 1)
                        {
                            return false;
                        }

                        //Create the trunk
                        connectors = CreateDuct(new XYZ(baseX, baseConnectors[0].Origin.Y + min1FittingLength, baseZ), new XYZ(baseX, baseConnectors[2].Origin.Y - min1FittingLength, baseZ));

                        //Create a tee fitting connecting the 1st and 2nd base connectors to the trunk
                        ConnectWithTeeFittingOnYAxis(baseConnectors[0], baseConnectors[1], connectors[0], true);

                        //Create an elbow fitting connection the 3rd base connector to the trunk
                        ConnectWithElbowFittingOnYAxis(baseConnectors[2], connectors[1]);
                    }
                }
                else
                {
                    //Create the segment of duct on the trunk to be connected to the 1st base connector
                    connectors = CreateDuct(new XYZ(baseX, baseConnectors[0].Origin.Y + min1FittingLength, baseZ), new XYZ(baseX, baseConnectors[1].Origin.Y - min1FittingLength, baseZ));

                    //Create an elbow fitting connection the 1st base connector with the trunk
                    ConnectWithElbowFittingOnYAxis(baseConnectors[0], connectors[0]);

                    if (baseConnectors[1].Origin.Y == baseConnectors[2].Origin.Y)
                    {
                        //The 2nd and 3rd connectors are on the same side of the trunk
                        if (Math.Sign(baseConnectors[1].Origin.X - baseX) * Math.Sign(baseConnectors[2].Origin.X - baseX) == 1)
                        {
                            return false;
                        }
                        //Create a tee fitting connecting the 2nd and 3rd base connectors to the trunk
                        ConnectWithTeeFittingOnYAxis(baseConnectors[1], baseConnectors[2], connectors[1], true);
                    }
                    else
                    {
                        connectors.AddRange(CreateDuct(new XYZ(baseX, baseConnectors[1].Origin.Y + min1FittingLength, baseZ), new XYZ(baseX, baseConnectors[2].Origin.Y - min1FittingLength, baseZ)));
                        //Create a tee fitting connecting the 2nd base connector to the trunk
                        ConnectWithTeeFittingOnYAxis(baseConnectors[1], connectors[1], connectors[2], false);
                        //Create an elbow fitting connection the 3rd base connector to the trunk
                        ConnectWithElbowFittingOnYAxis(baseConnectors[2], connectors[3]);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 在y方向上创建弯头
        /// </summary>
        /// <param name="baseConn"></param>
        /// <param name="conn"></param>
        private void ConnectWithElbowFittingOnYAxis(Connector baseConn, Connector conn)
        {
            double baseX = conn.Origin.X;
            double baseZ = conn.Origin.Z;
            List<Connector> connectors = new List<Connector>();

            //If the distance of the two connectors on the X axis is greater than 2, create a duct on the X axis and then connect it to the 2 connectors with elbow fittings
            if (Math.Abs(baseConn.Origin.X - baseX) > min1Duct2FittingsLength)
            {
                connectors.AddRange(CreateDuct(new XYZ(baseConn.Origin.X - Math.Sign(baseConn.Origin.X - baseX), baseConn.Origin.Y, baseZ), new XYZ(baseX + Math.Sign(baseConn.Origin.X - baseX), baseConn.Origin.Y, baseZ)));
                //connectors[0].ConnectTo(baseConn);
                m_document.Create.NewElbowFitting(connectors[0], baseConn);
                //connectors[1].ConnectTo(conn);
                m_document.Create.NewElbowFitting(connectors[1], conn);
            }
            //If the distance of the two connectors on the X axis is less than 2, connect them with an elbow fitting
            else
            {
                baseConn.ConnectTo(conn);
                m_document.Create.NewElbowFitting(baseConn, conn);
            }
        }

        /// <summary>
        /// 在y方向上创建三通
        /// </summary>
        /// <param name="conn1"></param>
        /// <param name="conn2"></param>
        /// <param name="conn3"></param>
        /// <param name="flag"></param>
        private void ConnectWithTeeFittingOnYAxis(Connector conn1, Connector conn2, Connector conn3, bool flag)
        {
            double baseX = conn3.Origin.X;
            double baseZ = conn3.Origin.Z;

            List<XYZ> points = new List<XYZ>();
            List<Duct> ducts = new List<Duct>();
            List<Connector> connectors = new List<Connector>();

            //Connect two base connectors to a connector on the trunk
            if (true == flag)
            {

                Connector baseConn1 = conn1;
                Connector baseConn2 = conn2;
                Connector conn = conn3;

                connectors.AddRange(CreateDuct(new XYZ(baseConn1.Origin.X - Math.Sign(baseConn1.Origin.X - baseX), baseConn1.Origin.Y, baseZ), new XYZ(baseX + Math.Sign(baseConn1.Origin.X - baseX), baseConn1.Origin.Y, baseZ)));
                connectors.AddRange(CreateDuct(new XYZ(baseConn2.Origin.X - Math.Sign(baseConn2.Origin.X - baseX), baseConn2.Origin.Y, baseZ), new XYZ(baseX + Math.Sign(baseConn2.Origin.X - baseX), baseConn2.Origin.Y, baseZ)));

                connectors[0].ConnectTo(baseConn1);
                connectors[2].ConnectTo(baseConn2);
                m_document.Create.NewElbowFitting(connectors[0], baseConn1);
                m_document.Create.NewElbowFitting(connectors[2], baseConn2);

                connectors[1].ConnectTo(connectors[3]);
                connectors[1].ConnectTo(conn);
                connectors[3].ConnectTo(conn);
                m_document.Create.NewTeeFitting(connectors[1], connectors[3], conn);
            }
            //Connect a base connector to two connectors on the trunk
            else
            {
                Connector baseConn = conn1;

                if (Math.Abs(baseConn.Origin.X - baseX) > min1Duct2FittingsLength)
                {
                    connectors.AddRange(CreateDuct(new XYZ(baseConn.Origin.X - Math.Sign(baseConn.Origin.X - baseX), baseConn.Origin.Y, baseZ), new XYZ(baseX + Math.Sign(baseConn.Origin.X - baseX), baseConn.Origin.Y, baseZ)));

                    //这里连接依次就可以了，两次都连接有可能导致立管的位置不是竖直的
                    //baseConn.ConnectTo(connectors[0]);
                    m_document.Create.NewElbowFitting(connectors[0], baseConn);

                    //三通只能通过Create创建，上面注释的无效
                    //connectors[1].ConnectTo(conn2);
                    //connectors[1].ConnectTo(conn3);
                    //conn2.ConnectTo(conn3);
                    //注意参数，第一个和第二个必须在一条直线上
                    //三通有方向，但是目前这个方法还不直到方向如何定义
                    m_document.Create.NewTeeFitting(conn2, conn3, connectors[1]);
                }
                else
                {
                    baseConn.ConnectTo(conn2);
                    baseConn.ConnectTo(conn3);
                    conn2.ConnectTo(conn3);
                    m_document.Create.NewTeeFitting(conn2, conn3, baseConn);
                }
            }
        }

        private bool ConnectSystemOnXAxis(List<Connector> baseConnectors, double baseY)
        {
            //Check the count of the base connectors
            if (null == baseConnectors || 3 != baseConnectors.Count)
            {
                return false;
            }
            for (int i = 0; i < baseConnectors.Count; ++i)
            {
                //Check the distance of the connector from the trunk
                if (baseConnectors[i].Origin.Y != baseY && Math.Abs(baseConnectors[i].Origin.Y - baseY) < min1Duct2FittingsLength)
                {
                    return false;
                }
                //Check the distance of the connectors on X axis
                for (int j = i + 1; j < baseConnectors.Count; ++j)
                {
                    if (baseConnectors[j].Origin.X != baseConnectors[i].Origin.X && baseConnectors[j].Origin.X - baseConnectors[i].Origin.X < min2FittingsLength)
                    {
                        return false;
                    }
                }
            }
            try
            {
                double baseZ = baseConnectors[0].Origin.Z + min1FittingLength;
                //Create the ducts and elbow fittings to connect the vertical ducts and the trunk ducts
                List<Connector> connectors = new List<Connector>();

                if (baseConnectors[0].Origin.X == baseConnectors[1].Origin.X)
                {
                    //All 3 connectors are with the same X value
                    if (baseConnectors[1].Origin.X == baseConnectors[2].Origin.X)
                    {
                        return false;
                    }
                    else
                    {
                        //The 1st and 2nd base connectors are on the same side of the trunk
                        if (Math.Sign(baseConnectors[0].Origin.Y - baseY) * Math.Sign(baseConnectors[1].Origin.Y - baseY) == 1)
                        {
                            return false;
                        }

                        //Create the trunk
                        connectors = CreateDuct(new XYZ(baseConnectors[0].Origin.X + min1FittingLength, baseY, baseZ), new XYZ(baseConnectors[2].Origin.X - min1FittingLength, baseY, baseZ));

                        //Create a tee fitting connecting the 1st and 2nd base connectors to the trunk
                        ConnectWithTeeFittingOnXAxis(baseConnectors[0], baseConnectors[1], connectors[0], true);

                        //Create an elbow fitting connection the 3rd base connector to the trunk
                        ConnectWithElbowFittingOnXAxis(baseConnectors[2], connectors[1]);
                    }
                }
                else
                {
                    //Create the segment of duct on the trunk to be connected to the 1st base connector
                    connectors = CreateDuct(new XYZ(baseConnectors[0].Origin.X + min1FittingLength, baseY, baseZ), new XYZ(baseConnectors[1].Origin.X - min1FittingLength, baseY, baseZ));

                    //Create an elbow fitting connection the 1st base connector with the trunk
                    ConnectWithElbowFittingOnXAxis(baseConnectors[0], connectors[0]);

                    if (baseConnectors[1].Origin.X == baseConnectors[2].Origin.X)
                    {
                        //The 2nd and 3rd connectors are on the same side of the trunk
                        if (Math.Sign(baseConnectors[1].Origin.Y - baseY) * Math.Sign(baseConnectors[2].Origin.Y - baseY) == 1)
                        {
                            return false;
                        }
                        //Create a tee fitting connecting the 2nd and 3rd base connectors to the trunk
                        ConnectWithTeeFittingOnXAxis(baseConnectors[1], baseConnectors[2], connectors[1], true);
                    }
                    else
                    {
                        connectors.AddRange(CreateDuct(new XYZ(baseConnectors[1].Origin.X + min1FittingLength, baseY, baseZ), new XYZ(baseConnectors[2].Origin.X - min1FittingLength, baseY, baseZ)));
                        //Create a tee fitting connecting the 2nd base connector to the trunk
                        ConnectWithTeeFittingOnXAxis(baseConnectors[1], connectors[1], connectors[2], false);
                        //Create an elbow fitting connection the 3rd base connector to the trunk
                        ConnectWithElbowFittingOnXAxis(baseConnectors[2], connectors[3]);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 在x方向上创建弯头
        /// </summary>
        /// <param name="connector1"></param>
        /// <param name="connector2"></param>
        private void ConnectWithElbowFittingOnXAxis(Connector baseConn, Connector conn)
        {
            double baseY = conn.Origin.Y;
            double baseZ = conn.Origin.Z;
            List<Connector> connectors = new List<Connector>();

            //If the distance of the two connectors on the Y axis is greater than 2, create a duct on the Y axis and then connect it to the 2 connectors with elbow fittings
            if (Math.Abs(baseConn.Origin.Y - baseY) > min1Duct2FittingsLength)
            {
                connectors.AddRange(CreateDuct(new XYZ(baseConn.Origin.X, baseConn.Origin.Y - Math.Sign(baseConn.Origin.Y - baseY), baseZ), new XYZ(baseConn.Origin.X, baseY + Math.Sign(baseConn.Origin.Y - baseY), baseZ)));
                connectors[0].ConnectTo(baseConn);
                m_document.Create.NewElbowFitting(connectors[0], baseConn);
                connectors[1].ConnectTo(conn);
                m_document.Create.NewElbowFitting(connectors[1], conn);
            }
            //If the distance of the two connectors on the Y axis is less than 2, connect them with an elbow fitting
            else
            {
                baseConn.ConnectTo(conn);
                m_document.Create.NewElbowFitting(baseConn, conn);
            }
        }

        /// <summary>
        /// 在x方向上创建三通
        /// </summary>
        /// <param name="connector1"></param>
        /// <param name="connector2"></param>
        /// <param name="connector3"></param>
        /// <param name="v"></param>
        private void ConnectWithTeeFittingOnXAxis(Connector conn1, Connector conn2, Connector conn3, bool flag)
        {
            double baseY = conn3.Origin.Y;
            double baseZ = conn3.Origin.Z;

            List<XYZ> points = new List<XYZ>();
            List<Duct> ducts = new List<Duct>();
            List<Connector> connectors = new List<Connector>();

            //Connect two base connectors to a connector on the trunk
            if (true == flag)
            {

                Connector baseConn1 = conn1;
                Connector baseConn2 = conn2;
                Connector conn = conn3;

                connectors.AddRange(CreateDuct(new XYZ(baseConn1.Origin.X, baseConn1.Origin.Y - Math.Sign(baseConn1.Origin.Y - baseY), baseZ), new XYZ(baseConn1.Origin.X, baseY + Math.Sign(baseConn1.Origin.Y - baseY), baseZ)));
                connectors.AddRange(CreateDuct(new XYZ(baseConn2.Origin.X, baseConn2.Origin.Y - Math.Sign(baseConn2.Origin.Y - baseY), baseZ), new XYZ(baseConn2.Origin.X, baseY + Math.Sign(baseConn2.Origin.Y - baseY), baseZ)));

                connectors[0].ConnectTo(baseConn1);
                connectors[2].ConnectTo(baseConn2);
                m_document.Create.NewElbowFitting(connectors[0], baseConn1);
                m_document.Create.NewElbowFitting(connectors[2], baseConn2);

                connectors[1].ConnectTo(connectors[3]);
                connectors[1].ConnectTo(conn);
                connectors[3].ConnectTo(conn);
                m_document.Create.NewTeeFitting(connectors[1], connectors[3], conn);
            }
            //Connect a base connector to two connectors on the trunk
            else
            {
                Connector baseConn = conn1;

                if (Math.Abs(baseConn.Origin.Y - baseY) > min1Duct2FittingsLength)
                {
                    connectors.AddRange(CreateDuct(new XYZ(baseConn.Origin.X, baseConn.Origin.Y - Math.Sign(baseConn.Origin.Y - baseY), baseZ), new XYZ(baseConn.Origin.X, baseY + Math.Sign(baseConn.Origin.Y - baseY), baseZ)));
                    baseConn.ConnectTo(connectors[0]);
                    m_document.Create.NewElbowFitting(connectors[0], baseConn);

                    connectors[1].ConnectTo(conn2);
                    connectors[1].ConnectTo(conn3);
                    conn2.ConnectTo(conn3);
                    m_document.Create.NewTeeFitting(conn2, conn3, connectors[1]);
                }
                else
                {
                    baseConn.ConnectTo(conn2);
                    baseConn.ConnectTo(conn3);
                    conn2.ConnectTo(conn3);
                    m_document.Create.NewTeeFitting(conn2, conn3, baseConn);
                }
            }
        }


        private List<Connector> CreateDuct(XYZ point1, XYZ point2)
        {
            List<Connector> connectors = new List<Connector>();

            Duct duct = Duct.Create(m_document, m_systemTypeId, m_ductTypeId, m_level.Id, point1, point2);

            connectors.Add(ConnectorInfo.GetConnector(duct, point1));
            connectors.Add(ConnectorInfo.GetConnector(duct, point2));

            return connectors;
        }

        private void SortConnectorsByY(List<Connector> connectors)
        {
            for (int i = 0; i < connectors.Count; ++i)
            {
                double min = connectors[i].Origin.Y;
                int minIndex = i;
                for (int j = i; j < connectors.Count; ++j)
                {
                    if (connectors[j].Origin.Y < min)
                    {
                        min = connectors[j].Origin.Y;
                        minIndex = j;
                    }
                }
                Connector t = connectors[i];
                connectors[i] = connectors[minIndex];
                connectors[minIndex] = t;
            }
        }

        private void SortConnectorsByX(List<Connector> connectors)
        {
            for (int i = 0; i < connectors.Count; ++i)
            {
                double min = connectors[i].Origin.X;
                int minIndex = i;
                for (int j = i; j < connectors.Count; ++j)
                {
                    if (connectors[j].Origin.X < min)
                    {
                        min = connectors[j].Origin.X;
                        minIndex = j;
                    }
                }
                Connector t = connectors[i];
                connectors[i] = connectors[minIndex];
                connectors[minIndex] = t;
            }
        }

        private MechanicalSystem CreateMechanicalSystem(ConnectorInfo baseConnector, ConnectorInfo[] connectors, DuctSystemType systemType)
        {
            ConnectorSet cset = null;
            if (connectors != null)
            {
                cset = new ConnectorSet();
                foreach (ConnectorInfo ci in connectors)
                {
                    cset.Insert(ci.Connector);
                }
            }

            MechanicalSystem mechanicalSystem = m_document.Create.
                NewMechanicalSystem(baseConnector == null ? null : baseConnector.Connector, cset, systemType);
            return mechanicalSystem;
        }
    }
}
