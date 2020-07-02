using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeometryRealted
{
    public class GeometryInfo
    {
        public Element Element { get; set; }
        public ElementType ElementType { get; set; }

        public List<GeometryInstance> GeometryInstances { get; set; }
        public List<Solid> Solids { get; set; }
        public List<object> Others { get; set; }


        public bool IsSimilarType
        {
            get
            {
                return ElementType?.IsSimilarType(ElementType.Id) ?? false;
            }
        }

        public GeometryInfo(Element element)
        {
            if (element == null)
            {
                return;
            }

            Element = element;
            ElementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
            GeometryInstances = new List<GeometryInstance>();
            Solids = new List<Solid>();
            Others = new List<object>();

            GeometryElement geomElem = element.get_Geometry(new Options());
            if (geomElem == null)
            {
                return;
            }

            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj == null)
                {
                    continue;
                }

                if (geomObj is Solid)
                {
                    Solids.Add(geomObj as Solid);
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstances.Add(geomObj as GeometryInstance);
                }
                else
                {
                    Others.Add(geomObj);
                }
            }

        }

        public override string ToString()
        {
            if (Element == null)
            {
                return string.Empty;
            }

            return string.Format("{0}的类型是{1},{2}简单类型，他的Gemoetry包括：GemoInstance有{3}个；Solid有{4}个；Others有{5}个",
                                    Element.Name,
                                    Element.Category.Name,
                                    IsSimilarType ? "是" : "不是",
                                    GeometryInstances.Count,
                                    Solids.Count,
                                    Others.Count);
        }
    }
}
