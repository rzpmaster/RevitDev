using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectPositionTest
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;


            //1 项目基点 
            ProjectLocation projectLocation = doc.ActiveProjectLocation;
            ProjectPosition position = projectLocation.GetProjectPosition(XYZ.Zero);

            // 获取 项目基点在测量点下的转换矩阵
            XYZ translationVector = new XYZ(position.EastWest, position.NorthSouth, position.Elevation);
            Transform translationTransform = Transform.CreateTranslation(translationVector);
            Transform rotationTransform = Transform.CreateRotation(XYZ.BasisZ, position.Angle);
            // 转换矩阵
            Transform finalTransform = translationTransform.Multiply(rotationTransform);
            //在数值上等于 项目基点 OST_ProjectBasePoint SharedPosition 属性的值 
            XYZ basePoint = finalTransform.OfPoint(XYZ.Zero);
            XYZ zore = finalTransform.Inverse.OfPoint(basePoint);

            //2 项目位置信息
            SiteLocation siteLocation = doc.SiteLocation;


            //3 BasePoint
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var basePoints = collector.OfClass(typeof(BasePoint)).ToElements();

            // 会得到两个 basePoint

            // 第一个是 项目基点 OST_ProjectBasePoint ，position属性永远为（0，0，0）isShared属性永远为false 表示不会被移动 移动它整个图形都会动；所有元素的Location属性是和它挂钩的，也就是说，移动元素，Location属性并不会改变
            // 它有 EastWest NorthSouth Elevation 以及 Angle 参数，和上面的 ProjectPosition 中的属性是意义对应的，他们都是相对于测量点的数值
            // 我们常说的 模型坐标系 的原点就是它，证据是他和模型一起动，它相对于模型是绝对静止的

            // 第二个是 测量点 OST_SharedBasePoint ，sharedPosition属性永远为（0，0，0）isShared属性永远为true
            // 它也有 EastWest NorthSouth Elevation 以及 Angle 参数，但是都是0，说明这些属性都是相对于他自己的数字。
            // 所以所有有关Location的计算都和他没关系，（目前我只知道）他只影响标高的elevation的值


            //4 总结

            //如果链接文件中项目基点没有和Host文件中的项目基点对齐，链接文件的 revitlinkedinstance 中会有一个 transform(totaltransform) ，通过这个矩阵变换，可以把链接文件的项目基点和Host文件的项目基点对齐（revitlinkedinstance类中也有相应的方法）。然后直接获取linkdoc中的location，就可以代表host文件中的真正位置。

            //在链接文件中，如果要获取元素的位置信息，一定要看这个 revitlinkedinstance 的 totaltransform 是否为0，如果不是，都需要应用这个转换矩阵。
            //如果转化了的位置信息，在host文件中又做了一定的转换，得到新的位置信息，想要知道它应在在连接文件中在什么位置时，只需要反向应用上述得到的矩阵，即应用 totaltransform.

            return Result.Succeeded;
        }


    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class LinkPositionCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            var linkedInstance = doc.GetElement(r) as RevitLinkInstance;

            if (linkedInstance == null)
            {
                TaskDialog.Show("Tip", "选择的不是链接文件");
                return Result.Cancelled;
            }

            // 链接文件嵌套两层会丢失底层Document
            //Document linkeddoc = GetDeeppestLinkedDocument(linkedInstance);
            //linkedInstance=doc.GetElement(new ElementId(337204))

            var column = linkedInstance.GetLinkDocument().GetElement(new ElementId(336825));

            if (column == null)
            {
                TaskDialog.Show("Tip", "没有找到336825这个元素");
                return Result.Failed;
            }

            // 至少目前看来 GetTotalTransform() 和 GetTransform() 返回的值总是一样的，我不知道在什么情况下不一样，正如api 描述 'For most of other instances, it simply returns the inherent transform.' 但是为了安全起见，还是使用前者比较安全 
            Transform transform = linkedInstance.GetTotalTransform();
            LocationPoint lc = column.Location as LocationPoint;

            string msg = "链接文件中的位置:{0}\r\nHost文件中的位置:{1}";
            XYZ pointInHost = transform.OfPoint(lc.Point);
            TaskDialog.Show("Tip", string.Format(msg,
                PointStringMm(lc.Point),
                PointStringMm(pointInHost)));

            Transform t = Transform.CreateTranslation(new XYZ(-5000 / 304.8, 0, 0));
            XYZ npInHost = t.OfPoint(pointInHost);
            TaskDialog.Show("Tip", string.Format("向左移动5000mm后\r\n" + msg,
                PointStringMm(transform.Inverse.OfPoint(npInHost)),
                PointStringMm(npInHost)));


            return Result.Succeeded;
        }

        private static Document GetDeeppestLinkedDocument(RevitLinkInstance linkedInstance)
        {
            var linkeddoc = linkedInstance.GetLinkDocument();
            var collector = new FilteredElementCollector(linkeddoc);
            var innerLinkedInstances = collector.OfClass(typeof(RevitLinkInstance)).ToElements();
            if (innerLinkedInstances.Any())
            {
                var innerLinkedInstance = innerLinkedInstances.First() as RevitLinkInstance;
                return innerLinkedInstance.GetLinkDocument();
            }
            return linkeddoc;
        }

        #region Unit Convert
        const double _inch_to_mm = 25.4;
        const double _foot_to_mm = 12 * _inch_to_mm;

        static string PointStringMm(XYZ p)
        {
            return string.Format("({0},{1},{2})",
              RealString(p.X * _foot_to_mm),
              RealString(p.Y * _foot_to_mm),
              RealString(p.Z * _foot_to_mm));
        }

        static string RealString(double a)
        {
            return a.ToString("0.##");
        } 
        #endregion

    }
}
