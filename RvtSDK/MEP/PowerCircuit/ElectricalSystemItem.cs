using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerCircuit
{
    public class ElectricalSystemItem
    {
        String m_name;
        ElementId m_id;

        public ElectricalSystemItem(ElectricalSystem es)
        {
            m_name = es.Name;
            m_id = es.Id;
        }

        public ElementId Id
        {
            get
            {
                return m_id;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
        }
    }
}
