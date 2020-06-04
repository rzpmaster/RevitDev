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
        Application application;

        Detector m_detector;
        PipingSystemType m_pipingSystemType;
        public Resolver(ExternalCommandData commandData)
        {
            document = commandData.Application.ActiveUIDocument.Document;
            application = commandData.Application.Application;

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
