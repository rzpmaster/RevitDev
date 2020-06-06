using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvoidObstruction
{
    class Resolver
    {
        Document document;

        Detector m_detector;
        PipingSystemType m_pipingSystemType;
        public Resolver(ExternalCommandData commandData)
        {
            document = commandData.Application.ActiveUIDocument.Document;

            m_detector = new Detector(document);

            FilteredElementCollector collector = new FilteredElementCollector(document);
            var pipingSystemTypes = collector.OfClass(typeof(PipingSystemType)).ToElements();
            foreach (PipingSystemType pipingSystemType in pipingSystemTypes)
            {
                //给水 排水
                if (pipingSystemType.SystemClassification == MEPSystemClassification.SupplyHydronic ||
                   pipingSystemType.SystemClassification == MEPSystemClassification.ReturnHydronic)
                {
                    m_pipingSystemType = pipingSystemType;
                    break;
                }
            }
        }

        public void Resolve()
        {
            List<Element> pipes = new List<Element>();
            FilteredElementCollector collector = new FilteredElementCollector(document);
            pipes.AddRange(collector.OfClass(typeof(Pipe)).ToElements());
            foreach (Element pipe in pipes)
            {
                Resolve(pipe as Pipe);
            }
        }

        private void Resolve(Pipe pipe)
        {
            var parameter = pipe.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM);
            var levelId = parameter.AsElementId();
            var systemTypeId = m_pipingSystemType.Id;

            Line pipeLine = (pipe.Location as LocationCurve).Curve as Line;

            //计算和水管中心线碰撞的障碍物 refcontext 集合
            List<ReferenceWithContext> obstructionRefArr = m_detector.Obstructions(pipeLine);
            //Just allow Pipe, Beam, and Duct.
            Filter(pipe, obstructionRefArr);
            if (obstructionRefArr.Count == 0)
            {
                // There are no intersection found.
                return;
            }

            XYZ dir = pipeLine.GetEndPoint(1) - pipeLine.GetEndPoint(0);

            //根据碰撞物 构造断面
            List<Section> sections = Section.BuildSections(obstructionRefArr, dir.Normalize());

            //合并距离较小的断面
            for (int i = sections.Count - 2; i >= 0; i--)
            {
                XYZ detal = sections[i].End - sections[i + 1].Start;
                if (detal.GetLength() < pipe.Diameter * 3)
                {
                    sections[i].Refs.AddRange(sections[i + 1].Refs);
                    sections.RemoveAt(i + 1);
                }
            }

            //完成每个断面翻管,形成一组 U 型管线
            foreach (Section sec in sections)
            {
                Resolve(pipe, sec);
            }

            //连接相邻的断面
            for (int i = 1; i < sections.Count; i++)
            {
                XYZ start = sections[i - 1].End;
                XYZ end = sections[i].Start;

                Pipe tmpPipe = Pipe.Create(document, systemTypeId, pipe.PipeType.Id, levelId, start, end);

                CopyParameters(pipe, tmpPipe);

                // Create elbow fitting to connect previous section with tmpPipe.
                Connector conn1 = FindConnector(sections[i - 1].Pipes[2], start);
                Connector conn2 = FindConnector(tmpPipe, start);
                document.Create.NewElbowFitting(conn1, conn2);

                // Create elbow fitting to connect current section with tmpPipe.
                Connector conn3 = FindConnector(sections[i].Pipes[0], end);
                Connector conn4 = FindConnector(tmpPipe, end);
                document.Create.NewElbowFitting(conn3, conn4);
            }

            //将断面和原来的管线连接的管道连接起来

            //原来管线的起终点连接件连接的 conn
            Connector startConn = FindConnectedTo(pipe, pipeLine.GetEndPoint(0));
            Connector endConn = FindConnectedTo(pipe, pipeLine.GetEndPoint(1));

            Pipe startPipe = null;
            if (null != startConn)
            {
                // Create a pipe between pipe's start connector and pipe's start section.
                startPipe = Pipe.Create(document, pipe.PipeType.Id, levelId, startConn, sections[0].Start);
            }
            else
            {
                // Create a pipe between pipe's start point and pipe's start section.
                startPipe = Pipe.Create(document, systemTypeId, pipe.PipeType.Id, levelId, sections[0].Start, pipeLine.GetEndPoint(0));
            }
            CopyParameters(pipe, startPipe);
            //连接开始点和断面
            Connector connStart1 = FindConnector(startPipe, sections[0].Start);
            Connector connStart2 = FindConnector(sections[0].Pipes[0], sections[0].Start);
            document.Create.NewElbowFitting(connStart1, connStart2);

            Pipe endPipe = null;
            int count = sections.Count;
            if (null != endConn)
            {
                // Create a pipe between pipe's end connector and pipe's end section.
                endPipe = Pipe.Create(document, pipe.PipeType.Id, levelId, endConn, sections[count - 1].End);
            }
            else
            {
                // Create a pipe between pipe's end point and pipe's end section.
                endPipe = Pipe.Create(document, systemTypeId, pipe.PipeType.Id, levelId, sections[count - 1].End, pipeLine.GetEndPoint(1));
            }
            CopyParameters(pipe, endPipe);
            //连接断面和终点
            Connector connEnd1 = FindConnector(endPipe, sections[count - 1].End);
            Connector connEnd2 = FindConnector(sections[count - 1].Pipes[2], sections[count - 1].End);
            document.Create.NewElbowFitting(connEnd1, connEnd2);

            // Delete the pipe after resolved.
            document.Delete(pipe.Id);
        }

        /// <summary>
        /// 解决每一个断面的 U 型管道的连接
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="section"></param>
        private void Resolve(Pipe pipe, Section section)
        {
            //找到与管线平行的线（难点）
            Line offset = FindRoute(pipe, section);

            Line sectionLine = Line.CreateBound(section.Start, section.End);
            Line side1 = Line.CreateBound(sectionLine.GetEndPoint(0), offset.GetEndPoint(0));
            Line side2 = Line.CreateBound(offset.GetEndPoint(1), sectionLine.GetEndPoint(1));

            PipeType pipeType = pipe.PipeType;
            XYZ start = side1.GetEndPoint(0);
            XYZ startOffset = offset.GetEndPoint(0);
            XYZ endOffset = offset.GetEndPoint(1);
            XYZ end = side2.GetEndPoint(1);

            var parameter = pipe.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM);
            var levelId = parameter.AsElementId();

            //创建 U 型管道
            var systemTypeId = m_pipingSystemType.Id;
            Pipe pipe1 = Pipe.Create(document, systemTypeId, pipeType.Id, levelId, start, startOffset);
            Pipe pipe2 = Pipe.Create(document, systemTypeId, pipeType.Id, levelId, startOffset, endOffset);
            Pipe pipe3 = Pipe.Create(document, systemTypeId, pipeType.Id, levelId, endOffset, end);

            //设置半径
            CopyParameters(pipe, pipe1);
            CopyParameters(pipe, pipe2);
            CopyParameters(pipe, pipe3);

            section.Pipes.Add(pipe1);
            section.Pipes.Add(pipe2);
            section.Pipes.Add(pipe3);

            //连接弯头
            Connector conn1 = FindConnector(pipe1, startOffset);
            Connector conn2 = FindConnector(pipe2, startOffset);
            document.Create.NewElbowFitting(conn1, conn2);

            Connector conn3 = FindConnector(pipe2, endOffset);
            Connector conn4 = FindConnector(pipe3, endOffset);
            document.Create.NewElbowFitting(conn3, conn4);
        }

        private Connector FindConnector(Pipe pipe, XYZ conXYZ)
        {
            ConnectorSet conns = pipe.ConnectorManager.Connectors;
            foreach (Connector conn in conns)
            {
                if (conn.Origin.IsAlmostEqualTo(conXYZ))
                {
                    return conn;
                }
            }
            return null;
        }

        private Connector FindConnectedTo(Pipe pipe, XYZ conXYZ)
        {
            Connector connItself = FindConnector(pipe, conXYZ);
            ConnectorSet connSet = connItself.AllRefs;
            foreach (Connector conn in connSet)
            {
                if (conn.Owner.Id.IntegerValue != pipe.Id.IntegerValue &&
                    conn.ConnectorType == ConnectorType.End)
                {
                    return conn;
                }
            }
            return null;
        }

        private void CopyParameters(Pipe source, Pipe target)
        {
            double diameter = source.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble();
            target.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(diameter);
        }


        /// <summary>
        /// 找个给定断面 与 管线平行的线（难点）
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        private Line FindRoute(Pipe pipe, Section section)
        {
            double minLength = pipe.Diameter * 2;
            double jumpStep = pipe.Diameter;

            //获取偏移方向
            List<XYZ> dirs = new List<XYZ>();
            XYZ crossDir = null;
            foreach (ReferenceWithContext gref in section.Refs)
            {
                Element elem = document.GetElement(gref.GetReference());
                Line locationLine = (elem.Location as LocationCurve).Curve as Line;
                XYZ refDir = locationLine.GetEndPoint(1) - locationLine.GetEndPoint(0);
                refDir = refDir.Normalize();
                if (refDir.IsAlmostEqualTo(section.PipeCenterLineDirection) || refDir.IsAlmostEqualTo(-section.PipeCenterLineDirection))
                {
                    continue;
                }
                crossDir = refDir.CrossProduct(section.PipeCenterLineDirection);
                dirs.Add(crossDir.Normalize());
                break;
            }

            //如果障碍物和管线平行,用叉乘法获取不到偏移方向
            if (dirs.Count == 0)
            {
                // 计算四个方向上的法向量
                List<Autodesk.Revit.DB.XYZ> perDirs = PerpendicularDirs(section.PipeCenterLineDirection, 4);
                dirs.Add(perDirs[0].Normalize());
                dirs.Add(perDirs[1].Normalize());
                //dirs.Add(perDirs[2]);
                //dirs.Add(perDirs[3]);
            }

            Line foundLine = null;
            while (null == foundLine)
            {
                section.Inflate(0, jumpStep);
                section.Inflate(1, jumpStep);

                // Find solution in the given directions.
                for (int i = 0; null == foundLine && i < dirs.Count; i++)
                {
                    //以断面起点和终点为新的起点，偏移的方向为方向，继续寻找障碍物
                    List<ReferenceWithContext> obs1 = m_detector.Obstructions(section.Start, dirs[i]);
                    List<ReferenceWithContext> obs2 = m_detector.Obstructions(section.End, dirs[i]);

                    Filter(pipe, obs1);
                    Filter(pipe, obs2);

                    // Find out the minimal intersections in two opposite direction.
                    ReferenceWithContext[] mins1 = GetClosestSectionsToOrigin(obs1);
                    ReferenceWithContext[] mins2 = GetClosestSectionsToOrigin(obs2);

                    // Find solution in the given direction and its opposite direction.
                    for (int j = 0; null == foundLine && j < 2; j++)
                    {
                        if (mins1[j] != null && Math.Abs(mins1[j].Proximity) < minLength ||
                            mins2[j] != null && Math.Abs(mins2[j].Proximity) < minLength)
                        {
                            continue;
                        }

                        // Calculate the maximal height that the parallel line can be reached.
                        double maxHight = 1000 * pipe.Diameter;
                        if (mins1[j] != null && mins2[j] != null)
                        {
                            maxHight = Math.Min(Math.Abs(mins1[j].Proximity), Math.Abs(mins2[j].Proximity));
                        }
                        else if (mins1[j] != null)
                        {
                            maxHight = Math.Abs(mins1[j].Proximity);
                        }
                        else if (mins2[j] != null)
                        {
                            maxHight = Math.Abs(mins2[j].Proximity);
                        }

                        XYZ dir = (j == 1) ? dirs[i] : -dirs[i];

                        // Calculate the parallel line which can avoid obstructions.
                        foundLine = FindParallelLine(pipe, section, dir, maxHight);
                    }
                }
            }
            return foundLine;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="section"></param>
        /// <param name="dir">需要找的平行直线先对与管线的方向</param>
        /// <param name="maxLength">在这个方向上允许移动的最大距离</param>
        /// <returns></returns>
        private Line FindParallelLine(Pipe pipe, Section section, XYZ dir, double maxLength)
        {
            double step = 0.5 * pipe.Diameter;
            double hight = 2 * pipe.Diameter;
            while (hight <= maxLength)
            {
                hight += step;
                XYZ detal = dir * hight;
                Line line = Line.CreateBound(section.Start + detal, section.End + detal);
                List<ReferenceWithContext> refs = m_detector.Obstructions(line);
                Filter(pipe, refs);

                if (refs.Count == 0)
                {
                    return line;
                }
            }
            return null;
        }

        private ReferenceWithContext[] GetClosestSectionsToOrigin(List<ReferenceWithContext> refs)
        {
            ReferenceWithContext[] mins = new ReferenceWithContext[2];
            if (refs.Count == 0)
            {
                return mins;
            }

            //如果第一个大于零，那后面的所有的都大于零；第一个就是最小值
            if (refs[0].Proximity > 0)
            {
                mins[1] = refs[0];
                return mins;
            }

            for (int i = 0; i < refs.Count - 1; i++)
            {
                if (refs[i].Proximity < 0 && refs[i + 1].Proximity > 0)
                {
                    mins[0] = refs[i];
                    mins[1] = refs[i + 1];
                    return mins;
                }
            }

            mins[0] = refs[refs.Count - 1];

            return mins;
        }

        /// <summary>
        /// 计算给定向量的法向量
        /// </summary>
        /// <param name="dir">给定向量</param>
        /// <param name="count"></param>
        /// <returns></returns>
        private List<XYZ> PerpendicularDirs(XYZ dir, int count)
        {
            List<XYZ> dirs = new List<XYZ>();
            Plane plane = Plane.CreateByNormalAndOrigin(dir, XYZ.Zero);
            Arc arc = Arc.Create(plane, 1.0, 0, 6.28); //圆

            double delta = 1.0 / (double)count;
            for (int i = 1; i <= count; i++)
            {
                XYZ pt = arc.Evaluate(delta * i, true);
                dirs.Add(pt);
            }

            return dirs;
        }

        private void Filter(Pipe pipe, List<ReferenceWithContext> refs)
        {
            for (int i = refs.Count - 1; i >= 0; i--)
            {
                Reference cur = refs[i].GetReference();
                Element curElem = document.GetElement(cur);
                if (curElem.Id == pipe.Id ||
                (!(curElem is Pipe) && !(curElem is Duct) &&
                curElem.Category.Id.IntegerValue != (int)BuiltInCategory.OST_StructuralFraming))
                {
                    refs.RemoveAt(i);
                }
            }
        }
    }
}
