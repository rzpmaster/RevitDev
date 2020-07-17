using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeometryTest
{
    class GeometryTestUtils
    {
        public static GeometryInstance GetInstanceGeometry(FamilyInstance familyInstance)
        {
            var geom = familyInstance.get_Geometry(new Options());
            foreach (GeometryObject geomObj in geom)
            {
                if (geomObj is GeometryInstance)
                {
                    return geomObj as GeometryInstance;
                }
            }
            return null;
        }

        public static Solid GetGeometryElementSolid(GeometryElement geoElem)
        {
            Solid solid = null;

            foreach (GeometryObject obj in geoElem)
            {
                if (obj is Solid)
                {
                    solid = obj as Solid;
                }
                else if (obj is GeometryInstance)
                {
                    GeometryElement instGeo = (obj as GeometryInstance).GetInstanceGeometry();
                    solid = GetGeometryElementSolid(instGeo);
                    if (null != solid && 0 < solid.Faces.Size)
                    {
                        break;
                    }
                    else
                    {
                        GeometryElement syGeo = (obj as GeometryInstance).GetSymbolGeometry();
                        solid = GetGeometryElementSolid(syGeo);
                    }
                }

                if (null != solid && 0 < solid.Faces.Size)
                {
                    break;
                }
            }

            return solid;
        }
    }
}
