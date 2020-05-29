using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBIM.Terminal.Common.Utils
{
    public static class ParameterUtils
    {
		public static Parameter FindParaByName(ParameterSet paras, string name)
		{
			Parameter findPara = null;

			foreach (Parameter para in paras)
			{
				if (para.Definition.Name == name)
				{
					findPara = para;
				}
			}

			return findPara;
		}

		public static Parameter FindParaByNameNotExactly(ParameterSet paras, string name)
		{
			Parameter findPara = null;

			foreach (Parameter para in paras)
			{
				if (para.Definition.Name.Contains(name))
				{
					findPara = para;
				}
			}

			return findPara;
		}
	}
}
