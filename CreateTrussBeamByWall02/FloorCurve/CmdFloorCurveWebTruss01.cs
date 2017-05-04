using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace FloorCurve
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CmdFloorCurveWebTruss01 : IExternalCommand
    {
        //定义全局变量参数
        public const double distance = 900/304.5;
        public const double SmallNumber = 0.0001;
        public const double divideDistance = 800/304.5;

        public const double BeamStartExtension = -3;
        public const double BeamEndExtension = -3;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            Document document = uiApp.ActiveUIDocument.Document;
            Selection sel = uiApp.ActiveUIDocument.Selection;

            app = uiApp.Application;

            Transaction trans = new Transaction(document, "拾取楼板在边线处放置桁架");
            trans.Start();

            
            
            Reference refelem = sel.PickObject(ObjectType.Element, "选取一块楼板 ");
            Floor floor = document.GetElement(refelem) as Floor;
            //取得楼板上表面
            Face faceUp = General.FindFloorUpFace(floor);
            //取得楼板下表面
            Face faceUnder = General.FindFloorUnderFace(floor);

            List<List<Curve> > retangleCurevs = new List<List<Curve>>();//楼板底面轮廓线，外轮廓线放第一个集合，洞口放在后面的集合中
            List<CurveLoop> retangleCurveLoops = new List<CurveLoop>();
            retangleCurveLoops = General.GetCurveloop(app, floor);
            foreach (var curveLoop in retangleCurveLoops)
            {
                string curvesInfo = null;
                List<Curve> curves = new List<Curve>();
                foreach (Curve curve in curveLoop)
                {
                    curves.Add(curve);
                    //curvesInfo += string.Format("{0}, {1}, {2}, {3}, {4}, {5}", curve.GetEndPoint(0).X,
                    //    curve.GetEndPoint(0).Y, curve.GetEndPoint(0).Z, curve.GetEndPoint(1).X, curve.GetEndPoint(1).Y,
                    //    curve.GetEndPoint(1).Z);

                    //curvesInfo += string.Format("{0}, {1}, {2}, {3}, ", curve.GetEndPoint(0).X,
                    //    curve.GetEndPoint(0).Y, curve.GetEndPoint(1).X, curve.GetEndPoint(1).Y);
                }
                //TaskDialog.Show("curves中线的个数", curves.Count.ToString());
                //TaskDialog.Show("点坐标", curvesInfo);

                //对其逆时针排序后再放入集合集合中
                List<Curve> sortCurves = General.SortCurves(curves);
                //降序排列
                sortCurves = sortCurves.OrderByDescending(x => x.Length).ToList();
                retangleCurevs.Add(sortCurves);
                foreach (var curve in sortCurves)
                {
                    curvesInfo += string.Format("{0}, {1}, {2}, {3}, ",
                        curve.GetEndPoint(0).X, curve.GetEndPoint(0).Y, curve.GetEndPoint(1).X, curve.GetEndPoint(1).Y);
                }
                //TaskDialog.Show("sortCurves中线的个数", sortCurves.Count.ToString());
                //TaskDialog.Show("点坐标", curvesInfo);
            }
            //TaskDialog.Show("retangleCurevs:", retangleCurevs.Count.ToString());

            Curve thsMaxLengthCurve = General.GetMaxLengthInCurves(retangleCurevs[0]);//获取楼板外轮廓最长边进行分割
            List<XYZ> theMaxLengthLineDiviudePoints = General.GetPointsByDivideWithDistance(thsMaxLengthCurve, distance);//分割点坐标
            string strInfoTset2 = null;
            foreach (var x in theMaxLengthLineDiviudePoints)
            {
                strInfoTset2 += String.Format("{0}, {1}, {2},", x.X, x.Y, x.Z);
            }
            //TaskDialog.Show("分割点坐标", strInfoTset2);

            //返回分割点平移方向
            Line theMaxLengthLine = thsMaxLengthCurve as Line;
            XYZ directionOfDividePoints = theMaxLengthLine.Direction.CrossProduct(new XYZ(0, 0, 1));

            string strInfoTest1 = null;
            strInfoTest1 = string.Format("{0}, {1}, {2}, ", directionOfDividePoints.X, directionOfDividePoints.Y,
                directionOfDividePoints.Z);
            //TaskDialog.Show("平移方向", strInfoTest1); 

            //找到楼板下表面轮廓线之中与最长轮廓线平行的线
            Curve parallelCurveWiththsMaxLengthCurve = General.GetCurveParallelCurves(theMaxLengthLine as Curve,
                retangleCurevs[0]);


            ////计算两条平行线之间的距离
            /// 
            XYZ pointInparallelCurveWiththsMaxLengthCurve = new XYZ(
                parallelCurveWiththsMaxLengthCurve.GetEndPoint(0).X, parallelCurveWiththsMaxLengthCurve.GetEndPoint(0).Y,
                parallelCurveWiththsMaxLengthCurve.GetEndPoint(0).Z);
            //求得分割点集合偏移距离
            double offsetDistance = General.TheDistanceBetweenPointAndLine(thsMaxLengthCurve,
                pointInparallelCurveWiththsMaxLengthCurve);

            //求得偏移点集合之后的点集合
            List<XYZ> theMaxLengthLineDiviudePointsOffset = General.OffsetCurves(theMaxLengthLineDiviudePoints,
                directionOfDividePoints, offsetDistance);
            string strInfoTset6 = null;
            foreach (var x in theMaxLengthLineDiviudePointsOffset)
            {
                strInfoTset6 += String.Format("{0}, {1}, {2},", x.X, x.Y, x.Z);
            }
            //TaskDialog.Show("偏移点坐标", strInfoTset6);

            ////绘制底部桁架下弦定位线
            /// 
            List<Curve> chordLocationCurveUnder = new List<Curve>();
            for (int i = 0; i < theMaxLengthLineDiviudePoints.Count; i++)
            {
                Line line = Line.CreateBound(theMaxLengthLineDiviudePoints[i], theMaxLengthLineDiviudePointsOffset[i]);
                chordLocationCurveUnder.Add(line);
            }
            //TaskDialog.Show("线的个数", chordLocationCurveUnder.Count.ToString());


            ////判断定位线与洞口边线的相交情况，如果相交取出交点，并将定位线从定位线集合中删除
            /// 
            /// 
            List<XYZ> insectionPointsOfOpenLineAndchordLocationUnder = new List<XYZ>();//下弦定位线与洞口边线的交点
            for (int i = 0; i < chordLocationCurveUnder.Count; i++)
            {
                var curve = chordLocationCurveUnder[i];
                List<XYZ> insectPoints = new List<XYZ>(); //一根定位线与洞口边线的交点集合
                for (int j = 1; j < retangleCurevs.Count; j++)
                {
                    foreach (var cv in retangleCurevs[j])
                    {
                        XYZ insectPoint = General.GetIntersectionSet(curve, cv);
                        if (insectPoint != null)
                        {
                            insectPoints.Add(insectPoint);
                            //chordLocationCurveUnder.RemoveAt(i);
                            //break;
                        }
                    }
                    if (insectPoints.Count == 2)
                    {
                        insectionPointsOfOpenLineAndchordLocationUnder.AddRange(insectPoints);
                    }
                }
                //foreach (var cv in retangleCurevs[1])
                //{
                //    XYZ insectPoint = General.GetIntersectionSet(curve, cv);
                //    if (insectPoint != null)
                //    {
                //        insectPoints.Add(insectPoint);
                //        //chordLocationCurveUnder.RemoveAt(i);
                //        //break;
                //    }
                //}
                //if (insectPoints.Count == 2)
                //{
                //    insectionPointsOfOpenLineAndchordLocationUnder.AddRange(insectPoints);
                //}
            }
            //TaskDialog.Show("交点个数", insectionPointsOfOpenLineAndchordLocationUnder.Count.ToString());
            //输出交点坐标查看 
            string strInfoTest4 = null;
            foreach (XYZ xyz in insectionPointsOfOpenLineAndchordLocationUnder)
            {
                strInfoTest4 += String.Format("{0}, {1}, {2},", xyz.X, xyz.Y, xyz.Z);
            }
            //TaskDialog.Show("交点坐标", strInfoTest4);


            //移除相交的定位线
            for (int i = (chordLocationCurveUnder.Count - 1); i > 0; i--)
            {
                var curve = chordLocationCurveUnder[i];
                List<XYZ> insectPoints = new List<XYZ>(); //一根定位线与洞口边线的交点集合
                for (int j = 1; j < retangleCurevs.Count; j++)
                {
                    foreach (var cv in retangleCurevs[j])  //洞口轮廓线
                    {
                        XYZ insectPoint = General.GetIntersectionSet(curve, cv);
                        if (insectPoint != null)
                        {
                            //insectPoints.Add(insectPoint);
                            chordLocationCurveUnder.RemoveAt(i);
                            //chordLocationCurveUnder.Remove(chordLocationCurveUnder[i]);
                            break;
                        }
                    }
                }
                


                //if (insectPoints.Count >= 1)
                //{
                //    chordLocationCurveUnder.RemoveAt(i);
                //}
            }
            //TaskDialog.Show("定位线数目", chordLocationCurveUnder.Count.ToString());


            #region 另一种方法，未完成
            //另一种方法，未完成
            ////List<Curve> newChordLocaCurvesUnder = new List<Curve>();
            //List<Curve> chordLocationCurveInOpen = new List<Curve>();
            //foreach (var curve in chordLocationCurveUnder)
            //{
            //    //newChordLocaCurvesUnder.Add(curve);
            //    List<XYZ> insectPoints = new List<XYZ>(); //一根定位线与洞口边线的交点集合
            //    foreach (var cv in retangleCurevs[1])  //洞口轮廓线
            //    {
            //        XYZ insectPoint = General.GetIntersectionSet(curve, cv);
            //        if (insectPoint != null)
            //        {
            //            chordLocationCurveInOpen.Add(curve);
            //        }
            //    }
            //}
            //TaskDialog.Show("洞口定位线数目", chordLocationCurveInOpen.Count.ToString());
            ////去重后，再从chordLocationCurveUnder中removerange（chordLocationCurveInOpen）。注意此时chordLocationCurveInOpen中的线集合必须在chordLocationCurveUnder中连续放置
            #endregion

            #region 通过直接移除原定位线集合之中的与洞口轮廓线相交的定位线（有错误，待改正，直接移除集合之中的某一个元素，该元素后面的所有元素会向前进一步，所以元素顺序会发生变化，可以试试反向删除，或者能不能直接根据元素内容相同而进行删除

            //for (int i = 0; i < chordLocationCurveUnder.Count; i++)
            //{
            //    var curve = chordLocationCurveUnder[i];
            //    List<XYZ> insectPoints = new List<XYZ>(); //一根定位线与洞口边线的交点集合
            //    foreach (var cv in retangleCurevs[1])  //洞口轮廓线
            //    {
            //        XYZ insectPoint = General.GetIntersectionSet(curve, cv);
            //        if (insectPoint != null)
            //        {
            //            chordLocationCurveUnder.RemoveAt(i);
            //            break;
            //        }
            //    }
            //}
            //TaskDialog.Show("定位线数目", chordLocationCurveUnder.Count.ToString());

            #endregion

            #region 也可以通过判断交点是否在定位线上来删除定位线

            //也可以通过判断交点是否在定位线上来删除定位线

            #endregion


            #region 洞口定位线

            // 添加新的洞口处定位线到定位线集合之中
            List<Curve> locationInOpening = new List<Curve>(); //洞口处定位线集合,靠近最长边处
            foreach (var dividePoint in theMaxLengthLineDiviudePoints)
            {
                List<Curve> curves = new List<Curve>(); //洞口定位线，未筛选
                foreach (var point in insectionPointsOfOpenLineAndchordLocationUnder)
                {
                    //if ((Math.Abs(point.X - dividePoint.X) < SmallNumber && Math.Abs(point.Y - dividePoint.Y) < SmallNumber) || 
                    //    (Math.Abs(point.X - dividePoint.X) < SmallNumber && Math.Abs(point.Z - dividePoint.Z) < SmallNumber) || 
                    //    (Math.Abs(point.Y - dividePoint.Y) < SmallNumber && Math.Abs(point.Z - dividePoint.Z) < SmallNumber))
                    double x = Math.Abs(dividePoint.X - point.X);
                    double y = Math.Abs(dividePoint.Y - point.Y);
                    double z = Math.Abs(dividePoint.Z - point.Z);
                    //string strInfoTest5 = null;
                    //strInfoTest5 = String.Format("{0}, {1}, {2},", x, y, z);
                    //TaskDialog.Show("差值:", strInfoTest5);
                    if (y < SmallNumber && z < SmallNumber)
                    {
                        Line line = Line.CreateBound(dividePoint, point);
                        if (line.Length > 200/304.5)
                        {
                            curves.Add(line);
                        }

                        //MessageBox.Show("hello");
                    }
                }
                //将长度短的加入定位线，即筛选多余的定位线
                //TaskDialog.Show("个数：", curves.Count.ToString());
                if (curves.Count > 1)
                {
                    //curves.OrderByDescending(x => x.Length).ToList();
                    //curves.OrderBy(x => x.Length).ToList();
                    Curve minLengthCurve = General.GetMinLengthInCurves(curves);
                    locationInOpening.Add(minLengthCurve);
                }
                else if (curves.Count == 1)
                {
                    continue;
                }
            }
            List<Curve> locationInOpeningSort = General.GetCurvesParallelDirection(locationInOpening,
                directionOfDividePoints); //对洞口定位线进行按方向排序，即将所有的线指定为某个方向
            //TaskDialog.Show("洞口处定位线的个数", locationInOpeningSort.Count.ToString());

            List<Curve> locationInOpeningOffset = new List<Curve>(); //洞口处定位线集合（偏移点侧)
            foreach (var dividePoint in theMaxLengthLineDiviudePointsOffset)
            {
                List<Curve> curves = new List<Curve>(); //洞口定位线，未筛选
                foreach (var point in insectionPointsOfOpenLineAndchordLocationUnder)
                {
                    //if ((Math.Abs(point.X - dividePoint.X) < SmallNumber && Math.Abs(point.Y - dividePoint.Y) < SmallNumber) || 
                    //    (Math.Abs(point.X - dividePoint.X) < SmallNumber && Math.Abs(point.Z - dividePoint.Z) < SmallNumber) || 
                    //    (Math.Abs(point.Y - dividePoint.Y) < SmallNumber && Math.Abs(point.Z - dividePoint.Z) < SmallNumber))
                    double x = Math.Abs(dividePoint.X - point.X);
                    double y = Math.Abs(dividePoint.Y - point.Y);
                    double z = Math.Abs(dividePoint.Z - point.Z);
                    //string strInfoTest5 = null;
                    //strInfoTest5 = String.Format("{0}, {1}, {2},", x, y, z);
                    //TaskDialog.Show("差值:", strInfoTest5);
                    if (y < SmallNumber && z < SmallNumber)
                    {
                        Line line = Line.CreateBound(dividePoint, point);
                        if (line.Length > 200/304.5)
                        {
                            curves.Add(line);
                        }
                        //MessageBox.Show("hello");
                    }
                }
                //将长度短的加入定位线，即筛选多余的定位线
                //TaskDialog.Show("个数：", curves.Count.ToString());
                if (curves.Count > 1)
                {
                    curves.OrderByDescending(x => x.Length).ToList();
                    //curves.OrderBy(x => x.Length).ToList();
                    //Curve minLengthCurve = General.GetMinLengthInCurves(curves);
                    //locationInOpening.Add(minLengthCurve);
                    locationInOpeningOffset.Add(curves[curves.Count - 1]);
                }
                else if (curves.Count == 1)
                {
                    continue;
                }
            }
            List<Curve> locationInOpeningOffsetSort = General.GetCurvesParallelDirection(locationInOpeningOffset,
                directionOfDividePoints);
            //TaskDialog.Show("洞口处定位线（偏移点侧)的个数", locationInOpeningOffsetSort.Count.ToString());

            #endregion


            #region 先试着画下表面桁架下弦

            ////接下来是将洞口定位线加入到总的定位线集合之中，
            ////并降序、逆时针排序一次(或者将它们按照同一个方向指定，方便生成桁架杆件时定位横截面方向）

            //可以先试着画下表面桁架下弦
            ////创建桁架竖杆放置在刚刚生成的线上

            //在文档中找到名字为"楼板-上下弦"的结构框架类型
            string trussTypeName = "楼板-上下弦";
            FamilySymbol trussType = null;
            ElementFilter trussCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
            ElementFilter familySymbolFilter = new ElementClassFilter(typeof(FamilySymbol));
            LogicalAndFilter andFilter = new LogicalAndFilter(trussCategoryFilter, familySymbolFilter);
            FilteredElementCollector trussSymbols = new FilteredElementCollector(document);
            trussSymbols = trussSymbols.WherePasses(andFilter);
            bool symbolFound = false;
            foreach (FamilySymbol element in trussSymbols)
            {
                if (element.Name == trussTypeName)
                {
                    symbolFound = true;
                    trussType = element;
                    break;
                }
            }
            //如果没有找到，就加载一个族文件
            //if (!symbolFound)
            //{

            //}
            //遍历过滤取得当前文档中的某个视图
            //下面这种过滤方法也可以使用逻辑过滤器的方法，有一个使用BuiltInCatagory过滤实例名字的方法
            Level level = null;
            FilteredElementCollector lvlFilter = new FilteredElementCollector(document);
            lvlFilter.OfClass(typeof(Level)).OfCategory(BuiltInCategory.OST_Levels);
            var elementss = from ele in lvlFilter
                            let instance = ele as Level
                            where instance.Name == "标高 2"
                            select ele;
            List<Level> levels = elementss.Cast<Level>().ToList(); //把LINQ方法找到的Level单元放到Levels集合中
            if (levels.Count >= 1)
            {
                level = levels.First();
            }
            else
            {
                throw new Exception("没有找到F2标高");
            }
            if (levels.Count >= 2)
            {
                Level ll = levels[1]; //获取Levels集合中第二个元素的方法
            }

            //使用族类型创建下弦杆
            List<Curve> columnTrussUnderCurves = new List<Curve>();
            columnTrussUnderCurves.AddRange(chordLocationCurveUnder);
            columnTrussUnderCurves.AddRange(locationInOpeningSort);
            columnTrussUnderCurves.AddRange(locationInOpeningOffsetSort);
            if (symbolFound)
            {
                foreach (var curve in columnTrussUnderCurves)
                {
                    FamilyInstance colTruss = document.Create.NewFamilyInstance(curve, trussType, level,
                        StructuralType.Beam);
                    StructuralFramingUtils.DisallowJoinAtEnd(colTruss, 0);
                    StructuralFramingUtils.DisallowJoinAtEnd(colTruss, 1);
                    //将创建的族实例以中性轴为旋转轴，旋转一定的角度
                    colTruss.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(Math.PI);
                }
            }
            //复制到上弦定位线
            List<Curve> columnTrussUpCurves = General.DuplicateCurvesByOffset(columnTrussUnderCurves, new XYZ(0, 0, 1),
                400/304.5);
            if (symbolFound)
            {
                foreach (var curve in columnTrussUpCurves)
                {
                    FamilyInstance colTruss = document.Create.NewFamilyInstance(curve, trussType, level,
                        StructuralType.Beam);
                    StructuralFramingUtils.DisallowJoinAtEnd(colTruss, 0);
                    StructuralFramingUtils.DisallowJoinAtEnd(colTruss, 1);
                    //将创建的族实例以中性轴为旋转轴，旋转一定的角度
                    //colTruss.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(Math.PI * 3 / 2);
                }
            }


            //添加竖杆
            List<List<Curve>> columnBarLocationCurves = new List<List<Curve>>();

            List<List<Curve>> chordLocationCurvesGroup = new List<List<Curve>>();//上下弦杆对，集合

            for (int i = 0; i < columnTrussUnderCurves.Count; i++)
            {
                List<Curve> curves = new List<Curve>();
                curves.Add(columnTrussUnderCurves[i]);
                curves.Add(columnTrussUpCurves[i]);
                chordLocationCurvesGroup.Add(curves);
            }

            foreach (var curves in chordLocationCurvesGroup)
            {
                List<Curve> columnInOneLocationCurve = new List<Curve>();
                Line line1 = Line.CreateBound(curves[0].GetEndPoint(0), curves[1].GetEndPoint(0));
                columnInOneLocationCurve.Add(line1);
                Line line2 = Line.CreateBound(curves[0].GetEndPoint(1), curves[1].GetEndPoint(1));
                columnInOneLocationCurve.Add(line2);
                columnBarLocationCurves.Add(columnInOneLocationCurve);
            }

            if (symbolFound)
            {
                foreach (var curve in columnBarLocationCurves)
                {
                    for (int i = 0; i < curve.Count; i++)
                    {
                        if (i == 0)
                        {
                            FamilyInstance colTruss = document.Create.NewFamilyInstance(curve[i], trussType, level,
                                StructuralType.Beam);
                            //将创建的族实例以中性轴为旋转轴，旋转一定的角度
                            colTruss.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(Math.PI);
                        }
                        if (i == 1)
                        {
                            FamilyInstance colTruss = document.Create.NewFamilyInstance(curve[i], trussType, level,
                                StructuralType.Beam);
                            StructuralFramingUtils.DisallowJoinAtEnd(colTruss, 0);
                            StructuralFramingUtils.DisallowJoinAtEnd(colTruss, 1);
                            //将创建的族实例以中性轴为旋转轴，旋转一定的角度
                            //colTruss.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(Math.PI);
                        }  
                    }
                }
            }

            #endregion


            ////生成斜腹杆
            foreach (var curves in chordLocationCurvesGroup)
            {
                List<List<Curve>> webTrussCurves = General.GetWebTruss(curves, divideDistance);//斜腹杆
                foreach (var webTrussCurve in webTrussCurves)
                {
                    foreach (var curve in webTrussCurve)
                    {
                        FamilyInstance colTruss = document.Create.NewFamilyInstance(curve, trussType, level,
                        StructuralType.Beam);

                        StructuralFramingUtils.DisallowJoinAtEnd(colTruss, 0);
                        StructuralFramingUtils.DisallowJoinAtEnd(colTruss, 1);
                        //colTruss.get_Parameter(BuiltInParameter.START_EXTENSION).Set((-3) / 304.5);//起点缩进
                        //colTruss.get_Parameter(BuiltInParameter.END_EXTENSION).Set((-3) / 304.5);//终点缩进


                        //将创建的族实例以中性轴为旋转轴，旋转一定的角度
                        //colTruss.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(Math.PI * 3 / 2);
                    }
                }
            }




            List<Curve> curvesList = new List<Curve>();

            XYZ point1 = new XYZ(0, 0, 0);
            XYZ point2 = new XYZ(0, 1, 0);
            XYZ point3 = new XYZ(0, 2, 0);

            Line line11 = Line.CreateBound(point1, point2);
            Line line22 = Line.CreateBound(point1, point2);
            Line line33 = Line.CreateBound(point1, point3);
            curvesList.Add(line11);
            curvesList.Add(line22);
            curvesList.Add(line33);

            List<Curve> curvesDistinct = General.DistinctCurve(curvesList, new XYZ(0, 1, 0));

            TaskDialog.Show("去重后个数：", curvesDistinct.Count.ToString());
            




            trans.Commit();
            return Result.Succeeded;

        }

    }
}