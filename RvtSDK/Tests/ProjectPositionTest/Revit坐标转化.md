# Revit坐标转化

### Revit里面的几种坐标系：

模型坐标系： 也理解为全局坐标系

视图坐标系：与模型坐标系之间的转换

族坐标系： 在制作族有一个坐标系，族插入到模型中，其中的几何体有自己在模型中的位置，需要进行模型坐标系的转换。（GeometryInstance.Transform 属性获取转换矩阵）（也可以直接从GeometryInstance对象的GeometryInstance.GetInstanceGeomery()  方法直接返回在模型坐标系下的坐标）

链接模型坐标系： 链接模型的位置在host模型中的位置，需要坐标转换（ViewSection.CropBox.Transform获取转换矩阵）