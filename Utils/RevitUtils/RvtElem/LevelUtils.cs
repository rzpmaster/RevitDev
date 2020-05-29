using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace CBIM.Terminal.Common.Utils
{
    public class LevelUtils
    {
        public static List<Level> GetAllLevels(Document doc)
        {
            FilteredElementCollector levelFilter = new FilteredElementCollector(doc);
            ICollection<Element> levelCollect = levelFilter.OfClass(typeof(Level)).ToElements();

            List<Level> levelList = new List<Level>();
            foreach (Element item in levelCollect)
            {
                Level level = item as Level;
                levelList.Add(level);
            }

            levelList.Sort(delegate (Level a, Level b) { return a.Elevation.CompareTo(b.Elevation); });

            return levelList;
        }
    }
}
