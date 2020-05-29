using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBIM.Terminal.Common.Utils
{
    public class LinkedElementUtils
    {
        public static Document GetLinkedDocumnet(Document currDoc, Reference linkedRef)
        {
            string stableReflink = linkedRef.ConvertToStableRepresentation(currDoc).Split(':')[0];
            Reference refLink = Reference.ParseFromStableRepresentation(currDoc, stableReflink);
            RevitLinkInstance rli_return = currDoc.GetElement(refLink) as RevitLinkInstance;
            Document linkdoc = rli_return.GetLinkDocument();

            return linkdoc;
        }
    }
}
