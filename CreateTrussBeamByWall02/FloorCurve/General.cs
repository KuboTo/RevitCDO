using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Architecture;
using System.IO;
using System.Windows.Input;
using Application = Autodesk.Revit.ApplicationServices.Application;


namespace FloorCurve
{
    public static class General
    {
        #region 传入一个楼板，返回楼板的上表面

        /// <summary>
        /// 返回楼板上表面
        /// </summary>
        /// <param name="floor"></param>
        /// <returns></returns>
        public static Face FindFloorUpFace(Floor floor)
        {
            Face normalFace = null;
            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.DetailLevel = ViewDetailLevel.Medium;
            GeometryElement e = floor.get_Geometry(opt);
            foreach (GeometryObject obj in e)
            {
                Solid solid = obj as Solid;
                if (solid != null && solid.Faces.Size > 0)
                {
                    foreach (Face face in solid.Faces)
                    {
                        PlanarFace pf = face as PlanarFace;
                        if (null != pf)
                        {
                            if (pf.Normal.AngleTo(new XYZ(0, 0, 1)) < 0.001)
                            {
                                string edgeInfo = null;
                                normalFace = face;
                            }
                        }
                    }
                }
            }
            return normalFace;
        }

        #endregion

        #region 传入一个楼板，返回楼板的下表面

        /// <summary>
        /// 返回楼板下表面
        /// </summary>
        /// <param name="floor"></param>
        /// <returns></returns>
        public static Face FindFloorUnderFace(Floor floor)
        {
            Face normalFace = null;
            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.DetailLevel = ViewDetailLevel.Medium;
            GeometryElement e = floor.get_Geometry(opt);
            foreach (GeometryObject obj in e)
            {
                Solid solid = obj as Solid;
                if (solid != null && solid.Faces.Size > 0)
                {
                    foreach (Face face in solid.Faces)
                    {
                        PlanarFace pf = face as PlanarFace;
                        if (null != pf)
                        {
                            if (pf.Normal.AngleTo(new XYZ(0, 0, -1)) < 0.001)
                            {
                                string edgeInfo = null;
                                normalFace = face;
                            }
                        }
                    }
                }
            }
            return normalFace;
        }

        #endregion

        #region 得到一个元素集合的LocationCurves

        /// <summary>
        /// 得到一个元素集合的LocationCurves
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static List<Curve> GetLocationCurves(List<Element> elements)
        {
            List<Curve> locationCurves = new List<Curve>();
            //double z=0;
            //foreach (var item in elements)
            //{
            //    Location loc = item.Location;
            //    if (loc is LocationCurve)
            //    {
            //        z = (loc as LocationCurve).Curve.GetEndPoint(0).Z;
            //        break;
            //    }
            //}
            foreach (var item in elements)
            {
                Location loc = item.Location;
                if (loc is LocationCurve)
                {
                    LocationCurve locCurve = loc as LocationCurve;
                    Curve crv = locCurve.Curve;
                    locationCurves.Add(crv);
                }
                //else if (loc is LocationPoint)
                //{
                //    XYZ point = (loc as LocationPoint).Point;
                //    point = ProjectPoint(point, z);
                //    XYZ point1 = point + zDirection * 301 / 304.8;
                //    locationCurves.Add(Line.CreateBound(point, point1));
                //}
            }

            return locationCurves;
        }

        public static bool IsCurveLoopHasFourCurve(CurveLoop curveLoop)
        {
            int i = 0;
            foreach (Curve item in curveLoop)
            {
                i++;
            }
            if (i == 4)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region 得到一个基于curve的元素的LocationCurve

        /// <summary>
        /// 得到一个基于curve的元素的LocationCurve
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static Curve GetLocationCurve(Element e)
        {
            LocationCurve locCurve = e.Location as LocationCurve;
            return locCurve.Curve;
        }

        #endregion

        #region 楼板底面的所有轮廓线,未经偏移等处理

        /// <summary>
        /// 楼板底面的所有轮廓线,未经偏移等处理
        /// </summary>
        /// <param name="floor"></param>
        /// <returns></returns>
        public static List<CurveLoop> GetCurveloop(Application app, Floor floor)
        {
            Options opt = app.Create.NewGeometryOptions();
            opt.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement GeoElem = floor.get_Geometry(opt);
            List<CurveLoop> blist = new List<CurveLoop>();
            // ArrayList blist = new ArrayList();
            foreach (GeometryObject geomObj in GeoElem)
            {
                Solid solid = geomObj as Solid;
                foreach (Face face in solid.Faces)
                {
                    Face faceloop = face;
                    XYZ zDiriction = new XYZ(0, 0, -1);
                    UV uv = new UV(0, 0);
                    XYZ normal = faceloop.ComputeNormal(uv);
                    if (normal.IsAlmostEqualTo(zDiriction))
                    {
                        blist = faceloop.GetEdgesAsCurveLoops().ToList();
                    }
                }
            }
            return blist;
        }

        #endregion

        #region 楼板顶面的所有轮廓线,未经偏移等处理

        /// <summary>
        /// 楼板顶面的所有轮廓线,未经偏移等处理
        /// </summary>
        /// <param name="floor"></param>
        /// <returns></returns>
        public static List<CurveLoop> GetUpCurveloop(Application app, Floor floor)
        {
            Options opt = app.Create.NewGeometryOptions();
            opt.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement GeoElem = floor.get_Geometry(opt);
            List<CurveLoop> blist = new List<CurveLoop>();
            // ArrayList blist = new ArrayList();
            foreach (GeometryObject geomObj in GeoElem)
            {
                Solid solid = geomObj as Solid;
                foreach (Face face in solid.Faces)
                {
                    Face faceloop = face;
                    XYZ zDiriction = new XYZ(0, 0, 1);
                    UV uv = new UV(0, 0);
                    XYZ normal = faceloop.ComputeNormal(uv);
                    if (normal.IsAlmostEqualTo(zDiriction))
                    {
                        blist = faceloop.GetEdgesAsCurveLoops().ToList();
                    }
                }
            }
            return blist;
        }

        #endregion

        #region 获得面的轮廓线

        /// <summary>
        /// 获得面的轮廓线
        /// </summary>
        /// <param name="face">传入的面</param>
        /// <returns>返回面的轮廓线集合</returns>
        public static List<Curve> GetCurvesInFace(Face face)
        {
            List<Curve> edgeLines = new List<Curve>();
            if (null != face)
            {
                IList<CurveLoop> curves = face.GetEdgesAsCurveLoops();
                foreach (CurveLoop curveloop in curves)
                {
                    foreach (Curve curve in curveloop)
                    {
                        Line line = curve as Line;
                        edgeLines.Add(line);
                    }
                }
            }
            return edgeLines;
        }

        #endregion

        #region 传入线的结合，获得平行于X轴正方向的线

        /// <summary>
        /// 获得平行于X轴正方向的线
        /// </summary>
        /// <param name="curve">传入的线</param>
        /// <returns>平行于X轴正方向的线</returns>
        public static Curve GetLineParallelZX(Curve curve)
        {
            Line line = curve as Line;
            if (line != null && Math.Abs(line.Direction.AngleTo(new XYZ(1, 0, 0))) < 0.000000000001)
            {
                return line;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region 传入线的结合，获得平行于X轴负方向的线

        /// <summary>
        /// 获得平行于X轴负方向的线
        /// </summary>
        /// <param name="curve">传入的线</param>
        /// <returns>平行于X轴负方向的线</returns>
        public static Curve GetLineParallelFX(Curve curve)
        {
            Line line = curve as Line;
            if (line != null && Math.Abs(line.Direction.AngleTo(new XYZ(-1, 0, 0))) < 0.000000000001)
            {
                return line;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region 传入线的结合，获得平行于Y轴正方向的线

        /// <summary>
        /// 获得平行于Y轴正方向的线
        /// </summary>
        /// <param name="curve">传入的线</param>
        /// <returns>平行于X轴正方向的线</returns>
        public static Curve GetLineParallelZY(Curve curve)
        {
            Line line = curve as Line;
            if (line != null && Math.Abs(line.Direction.AngleTo(new XYZ(0, 1, 0))) < 0.000000000001)
            {
                return line;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region 传入线的结合，获得平行于Y轴负方向的线

        /// <summary>
        /// 获得平行于Y轴负方向的线
        /// </summary>
        /// <param name="curve">传入的线</param>
        /// <returns>平行于X轴正方向的线</returns>
        public static Curve GetLineParallelFY(Curve curve)
        {
            Line line = curve as Line;
            if (line != null && Math.Abs(line.Direction.AngleTo(new XYZ(0, -1, 0))) < 0.000000000001)
            {
                return line;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region 获取平行于各个方向线（待测试方法）

        ///// <summary>
        ///// 获得平行于X轴负方向的线
        ///// </summary>
        ///// <param name="curve">传入的线</param>
        ///// <returns>平行于X轴负方向的线</returns>
        //public static IList<Curve> GetLineParallelFX(Curve curve)
        //{
        //    IList<Curve> lineParallelFX = new List<Curve>();
        //    Line line = curve as Line;
        //    if (line != null && Math.Abs(line.Direction.AngleTo(new XYZ(-1, 0, 0))) < 0.000000000001)
        //    {
        //        lineParallelFX.Add(line);
        //    }
        //    return lineParallelFX;
        //}

        ///// <summary>
        ///// 获得平行于Y轴正方向的线
        ///// </summary>
        ///// <param name="curve">传入的线</param>
        ///// <returns>平行于X轴正方向的线</returns>
        //public static IList<Curve> GetLineParallelZY(Curve curve)
        //{
        //    IList<Curve> lineParallelZY = new List<Curve>();
        //    Line line = curve as Line;
        //    if (line != null && Math.Abs(line.Direction.AngleTo(new XYZ(0, 1, 0))) < 0.000000000001)
        //    {
        //        lineParallelZY.Add(line);
        //    }
        //    return lineParallelZY;
        //}

        ///// <summary>
        ///// 获得平行于Y轴负方向的线
        ///// </summary>
        ///// <param name="curve">传入的线</param>
        ///// <returns>平行于X轴正方向的线</returns>
        //public static IList<Curve> GetLineParallelFY(Curve curve)
        //{
        //    IList<Curve> lineParallelFY = new List<Curve>();
        //    Line line = curve as Line;
        //    if (line != null && Math.Abs(line.Direction.AngleTo(new XYZ(0, -1, 0))) < 0.000000000001)
        //    {
        //        lineParallelFY.Add(line);
        //    }
        //    return lineParallelFY;
        //}

        #endregion

        #region 给定曲线curve，将其按一定的间距distance进行划分，取得划分点的集合,包含起点和终点

        /// <summary>
        /// 给定曲线curve，将其按一定的间距distance进行划分，取得划分点的集合,包含起点和终点
        /// </summary>
        /// <param name="curve">需要划分的曲线</param>
        /// <param name="distance">划分间距</param>
        /// <returns>返回按照一定间距划分所得点集合</returns>
        public static List<XYZ> GetPointsByDivideWithDistance(Curve curve, Double distance)
        {
            List<XYZ> dividePoints = new List<XYZ>();
            XYZ startPoint = curve.GetEndPoint(0);
            XYZ endPoint = curve.GetEndPoint(1);

            dividePoints.Add(startPoint);

            double deta = distance/(curve.Length);
            for (int i = 1; i < ((curve.Length/distance) + 1); i++)
            {
                XYZ point = new XYZ();
                double j = i*deta;
                if (j < 1)
                {
                    point = curve.Evaluate(j, true);
                    dividePoints.Add(point);
                }
            }
            dividePoints.Add(endPoint);

            return dividePoints;
        }

        #endregion

        #region 得到一个curve的等分点，其中n为等分数量

        /// <summary>
        /// 得到一个curve的等分点，其中n为等分数量
        /// </summary>
        /// <param name="curve">待等分的曲线</param>
        /// <param name="n">等分数量</param>
        /// <returns>返回值为等分点集合</returns>
        public static List<XYZ> EquallyDividedPointOfACurve(Curve curve, int n)
        {
            List<XYZ> allPoints = new List<XYZ>();

            XYZ p0 = curve.GetEndPoint(0);
            XYZ p1 = curve.GetEndPoint(1);

            allPoints.Add(p0);
            double length = curve.Length;
            double divLength = length/n;
            XYZ direction = (curve as Line).Direction;
            for (int i = 1; i < n; i++)
            {
                XYZ p = p0 + direction*divLength*i;
                allPoints.Add(p);
            }
            allPoints.Add(p1);

            return allPoints;
        }

        #endregion

        #region 根据手动选取的参照，对元素进行分类：楼板上部墙体、楼板下部墙体、楼板、模型线。

        /// <summary>
        /// 根据手动选取的参照，对元素进行分类：楼板上部墙体、楼板下部墙体、楼板、模型线。
        /// 注意：对于模型线，只获取楼板标高及以下的模型线，因为屋顶的划分也要手工添加模型线，不能把屋顶的模型线也给加进来了
        /// 可以对筛选元素进行适当的增删
        /// </summary>
        /// <param name="allRef"></param>
        /// <param name="upWalls"></param>
        /// <param name="lowerWalls"></param>
        /// <param name="floors"></param>
        /// <param name="modelLines"></param>
        public static void ClassifyFloorsWallsModelLines(Document document, List<Reference> allRef,
            out List<Element> upWalls, out List<Element> lowerWalls, out List<Element> floors)
        {

            upWalls = new List<Element>();
            lowerWalls = new List<Element>();

            List<Element> walls = new List<Element>();

            List<ElementId> allElementId = new List<ElementId>();
            foreach (var item in allRef)
            {
                Element e = document.GetElement(item) as Element;
                allElementId.Add(e.Id);
            }

            //过滤出楼板
            FilteredElementCollector collector = new FilteredElementCollector(document, allElementId);
            floors = collector.OfClass(typeof (Floor)).ToList();

            //过滤出模型线
            //collector = new FilteredElementCollector(doc, allElementId);
            //List<Element> modelLines_pre = collector.OfCategory(BuiltInCategory.OST_Lines).ToList();

            //过滤出墙
            collector = new FilteredElementCollector(document, allElementId);
            walls = collector.OfClass(typeof (Wall)).ToList();

            //得到楼板标高
            Level floorLevel = document.GetElement(floors[0].LevelId) as Level;

            //将楼板标高上部和下部的墙体分开放入集合
            foreach (var item in walls)
            {
                Level wallLevel = document.GetElement(item.LevelId) as Level;
                if (wallLevel.Elevation < floorLevel.Elevation)
                {
                    lowerWalls.Add(item);
                }
                else
                {
                    upWalls.Add(item);
                }
            }

            List<string> familySymbolStrings = new List<string>();
            foreach (var item in lowerWalls)
            {
                familySymbolStrings.Add(item.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString());
            }
            familySymbolStrings = familySymbolStrings.Distinct().ToList();
            if (familySymbolStrings.Count > 1)
            {
                List<List<Element>> classifiedWalls = new List<List<Element>>();
                foreach (var item in familySymbolStrings)
                {
                    List<Element> oneKindWalls = new List<Element>();
                    foreach (var itemm in lowerWalls)
                    {
                        if (itemm.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString() == item)
                        {
                            oneKindWalls.Add(itemm);
                        }
                    }
                    classifiedWalls.Add(oneKindWalls);
                }
                classifiedWalls = classifiedWalls.OrderBy(x => x.Count).ToList();
                classifiedWalls.RemoveAt(0);
                lowerWalls = classifiedWalls[0];
            }

            //获得高度小于或等于楼板标高的模型线
            //foreach (var item in modelLines_pre)
            //{
            //    ModelLine modelLine = item as ModelLine;
            //    Curve moderLineCurve = modelLine.GeometryCurve;
            //    if (moderLineCurve.GetEndPoint(0).Z <= floorLevel.Elevation)
            //    {
            //        modelLines.Add(item);
            //    }
            //}
        }

        #endregion

        #region 楼板划分思想生成楼板桁架主函数

        /// <summary>
        /// 楼板划分思想生成楼板桁架主函数
        /// </summary>
        /// <param name="eleFloors"></param>楼板
        /// <param name="eleWalls"></param>上部的墙
        /// <param name="eleLowerWalls"></param>下部的墙
        public static void CreateFloorPreparation(Document document, Application app
            , List<Element> eleFloors, List<Element> eleWalls, List<Element> eleLowerWalls, UIDocument uiDoc)
        {
            Floor beginningFloor = eleFloors[0] as Floor;
            ElementId floorlevelId = beginningFloor.get_Parameter(BuiltInParameter.LEVEL_PARAM).AsElementId();
            //得到楼板顶面标高beginningFloorLevel
            Level beginningFloorLevel = document.GetElement(floorlevelId) as Level;
            FloorType ft = (eleFloors[0] as Floor).FloorType;

            List<CurveLoop> floorEdges = GetUpCurveloop(app, eleFloors[0] as Floor).ToList();
            floorEdges = floorEdges.OrderByDescending(x => x.GetExactLength()).ToList();

            List<Curve> lowerWallLocation = GetLocationCurves(eleLowerWalls);
            List<ModelCurve> modelCurves = General.DrawModelCurvesViaCurves(document, lowerWallLocation,
                beginningFloorLevel);
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof (ViewPlan));
            var elem = from element in collector
                where
                    General.IsDoubleAlmostEqualTo((element as ViewPlan).GenLevel.Elevation,
                        beginningFloorLevel.Elevation)
                select element;
            ViewPlan levelView = elem.First() as ViewPlan;
            uiDoc.ActiveView = levelView;

            collector = new FilteredElementCollector(document);
            ElementClassFilter filter1 = new ElementClassFilter(typeof (Wall));
            ElementClassFilter filter2 = new ElementClassFilter(typeof (Floor));
            List<ElementFilter> filters = new List<ElementFilter>();
            filters.Add(filter1);
            filters.Add(filter2);
            LogicalOrFilter orFilter = new LogicalOrFilter(filters);
            collector.WherePasses(orFilter);
            List<ElementId> hideElementId = new List<ElementId>();
            foreach (var item in collector.ToList())
            {
                hideElementId.Add(item.Id);
            }
            Transaction tran = new Transaction(document, "Hide Elements");
            tran.Start();
            if (levelView != null) levelView.HideElements(hideElementId);
            tran.Commit();




            //List<List<Curve>> rectangles = RectangleFromCurvesConsideringHole(cutLines, floorEdges, beginningFloorLevel.Elevation);
            //List<Element> allNewFloors = CreateFloorFromRectangles(rectangles, ft, beginningFloorLevel);

            //CreateFloor(allNewFloors, eleWalls, optimizedLines);
        }

        #endregion

        #region 给定原始曲线和标高，先将曲线投影到该标高平面上，然后绘制出在该标高上的模型线

        /// <summary>
        ///  给定原始曲线和标高，绘制出在该标高上的模型线
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="originCurves"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static List<ModelCurve> DrawModelCurvesViaCurves(Document doc, List<Curve> originCurves, Level level)
        {
            List<Curve> projectedCurves = ProjectCurves(originCurves, level.Elevation);
            List<ModelCurve> modelCurves = new List<ModelCurve>();
            Transaction tran = new Transaction(doc, "Create Model Curves");
            tran.Start();
            SketchPlane sketchPlane = SketchPlane.Create(doc, level.Id);

            foreach (var item in projectedCurves)
            {
                ModelCurve modelCurve = doc.Create.NewModelCurve(item, sketchPlane);
                modelCurves.Add(modelCurve);
            }
            tran.Commit();

            return modelCurves;
        }

        #endregion

        #region 将一个曲线集合投影到给定的一个Z值平面上

        /// <summary>
        /// 将一个曲线集合投影到给定的一个Z值平面上
        /// </summary>
        /// <param name="originCurves1"></param>
        /// <param name="originCurves2"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static List<Curve> ProjectCurves(List<Curve> originCurves1, double z)
        {
            List<Curve> newCurves = new List<Curve>();
            foreach (var item in originCurves1)
            {
                XYZ p0 = item.GetEndPoint(0);
                XYZ p1 = item.GetEndPoint(1);
                XYZ p00 = new XYZ(p0.X, p0.Y, z);
                XYZ p11 = new XYZ(p1.X, p1.Y, z);

                Curve newItem = Line.CreateBound(p00, p11);
                newCurves.Add(newItem);
            }
            return newCurves;
        }

        public static Curve ProjectCurve(Curve originCurve, double z)
        {
            XYZ p0 = originCurve.GetEndPoint(0);
            XYZ p1 = originCurve.GetEndPoint(1);
            XYZ p00 = new XYZ(p0.X, p0.Y, z);
            XYZ p11 = new XYZ(p1.X, p1.Y, z);
            return Line.CreateBound(p00, p11);
        }

        #endregion

        #region 有1e-5容差的判断两个double值是否相等

        /// <summary>
        /// 有1e-5容差的判断两个double值是否相等
        /// </summary>
        /// <param name="dou1"></param>
        /// <param name="dou2"></param>
        /// <returns></returns>
        public const double SMALL_NUMBER = 1e-5;

        public static bool IsDoubleAlmostEqualTo(double dou1, double dou2)
        {
            return Math.Abs(dou1 - dou2) < SMALL_NUMBER;
        }

        #endregion

        #region 对于一个曲线集合a，将所有线尽可能的贯通（注意，这种贯通只是将本来就首尾相连的线连成一条线，而不是将共线但并没有搭接的线相连）

        /// <summary>
        /// 对于一个曲线集合a，将所有线尽可能的贯通（注意，这种贯通只是将本来就首尾相连的线连成一条线，而不是将共线但并没有搭接的线相连）
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static List<Curve> ConnectAllPossibleCurvesInCurveList(List<Curve> a)
        {
            List<Curve> b = ConnectTwoCurvesInCurveList(a);
            while (b.Count != a.Count)
            {
                a = b;
                b = ConnectTwoCurvesInCurveList(a);
            }
            return b;
        }

        #endregion

        #region 对于一个curveList，如果其中有两根线存在两端连在一起的情况，那么就将这两根线连接起来，这个方法只进行一次这样的连接操作

        /// <summary>
        /// 对于一个curveList，如果其中有两根线存在两端连在一起的情况，那么就将这两根线连接起来，这个方法只进行一次这样的连接操作
        /// </summary>
        /// <param name="curveList"></param>
        /// <returns></returns>
        public static List<Curve> ConnectTwoCurvesInCurveList(List<Curve> curveList)
        {
            List<Curve> copy_curveList = new List<Curve>(curveList);
            for (int i = 0; i < curveList.Count; i++)
            {
                int ii = 0;
                for (int j = i + 1; j < curveList.Count; j++)
                {
                    if (curveList[i].Intersect(curveList[j]) == SetComparisonResult.Subset &&
                        IsTwoLineParallel(curveList[i], curveList[j]))
                    {
                        Curve curve = LongestCurveFromTwoParallelCurve(curveList[i], curveList[j]);
                        copy_curveList.Remove(curveList[i]);
                        copy_curveList.Remove(curveList[j]);
                        copy_curveList.Add(curve);
                        ii++;
                        break;
                    }
                }
                if (ii != 0)
                {
                    break;
                }
            }
            return copy_curveList;
        }

        #endregion

        #region 判断两条线是否平行，是平行的话则返回true，虽然很短，但是用的很多

        /// <summary>
        /// 判断两条线是否平行，是平行的话则返回true，虽然很短，但是用的很多
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <returns></returns>
        public static bool IsTwoLineParallel(Curve curve1, Curve curve2)
        {
            XYZ d1 = (curve1 as Line).Direction;
            XYZ d2 = (curve2 as Line).Direction;
            double angle = d1.AngleTo(d2);
            if (Math.Abs(angle - 0) < SMALL_NUMBER || Math.Abs(angle - Math.PI) < SMALL_NUMBER)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region 由两条共线的直线，连接最外侧两个点得到最长的那根直线

        /// <summary>
        /// 由两条共线的直线，连接最外侧两个点得到最长的那根直线
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static Curve LongestCurveFromTwoParallelCurve(Curve c1, Curve c2)
        {
            List<XYZ> points = new List<XYZ>();
            points.Add(c1.GetEndPoint(0));
            points.Add(c1.GetEndPoint(1));
            points.Add(c2.GetEndPoint(0));
            points.Add(c2.GetEndPoint(1));
            points = DistinctPoint(points);

            List<Curve> curves = new List<Curve>();
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    Curve curve = Line.CreateBound(points[i], points[j]);
                    curves.Add(curve);
                }
            }

            curves = curves.OrderByDescending(x => x.Length).ToList();

            return curves[0];
        }

        #endregion

        #region 有一定容差的方法来对点集合进行去重

        /// <summary>
        /// 有一定容差的方法来对点集合进行去重
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static List<XYZ> DistinctPoint(List<XYZ> points)
        {
            List<XYZ> newPoints = new List<XYZ>();
            newPoints.Add(points[0]);
            points.ForEach(x => { if (!newPoints.Any(y => y.IsAlmostEqualTo(x))) newPoints.Add(x); });

            return newPoints;
        }

        #endregion

        #region 有一定容差的方法来对线集合进行去重(有错误，未测试成功）

        /// <summary>
        /// 有一定容差的方法来对线集合进行去重
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<Curve> DistinctCurve(List<Curve> curves, XYZ direction)
        {
            //先对线的方向进行统一
            List<Curve> curvesList = GetCurvesParallelDirection(curves, direction);

            //去重之后的线集合
            List<Curve> curvesDistinct = new List<Curve>();

            for (int i = curves.Count-1; i > 0; i--)
            {
                for (int j = i - 1; j > 0; j--)
                {
                    if (curvesList[i].GetEndPoint(0).IsAlmostEqualTo(curvesList[j].GetEndPoint(0)) &&
                        curvesList[i].GetEndPoint(1).IsAlmostEqualTo(curvesList[j].GetEndPoint(1)))
                    {
                        curvesList.RemoveAt(i);
                        break;
                    }
                }
            }
            curvesDistinct = curvesList;
            return curvesDistinct;
        }
        
        #endregion


        #region 根据矩形创建楼板，如果一个矩形内部还有一个矩形，则生成一个含洞口的楼板,传入的矩形边界是个集合的集合

        /// <summary>
        /// 根据矩形创建楼板，如果一个矩形内部还有一个矩形，则生成一个含洞口的楼板
        /// </summary>
        /// <param name="rectangles"></param>
        /// <param name="floorType"></param>
        /// <param name="floorLevel"></param>
        /// <returns></returns>
        public static List<Element> CreateFloorFromRectangles(Document document, List<List<Curve>> rectangles,
            FloorType floorType, Level floorLevel)
        {
            List<Element> allFloors = new List<Element>();
            Transaction tran = new Transaction(document, "any");
            foreach (var item in rectangles) // 此处使用集合的集合，一次取出的是一个集合，包含正方形四条边
            {
                //如果是个矩形边界 则直接生成楼板
                if (item.Count == 4)
                {
                    CurveArray floorProfile = new CurveArray();
                    foreach (var itemm in item)
                    {
                        floorProfile.Append(itemm);
                    }
                    tran.Start();
                    Floor floor = document.Create.NewFloor(floorProfile, floorType, floorLevel, true);
                        //不知道floorProfile中线的顺序是否重要？
                    tran.Commit();
                    allFloors.Add(floor);
                }
                    //如果中间有洞口，则先生成楼板，再在楼板上开洞口，此时的item前4条线为外轮廓，后4条线为洞口
                else
                {
                    CurveArray floorProfile = new CurveArray();
                    CurveArray openingProfile = new CurveArray();
                    for (int i = 0; i < 4; i++)
                    {
                        floorProfile.Append(item[i]);
                    }
                    for (int i = 4; i < 8; i++)
                    {
                        openingProfile.Append(item[i]);
                    }
                    tran.Start();
                    Floor floor = document.Create.NewFloor(floorProfile, floorType, floorLevel, true);
                    tran.Commit();
                    tran.Start();
                    Opening opening = document.Create.NewOpening(floor, openingProfile, false);
                    tran.Commit();
                    allFloors.Add(floor);
                }
            }
            return allFloors;
        }

        #endregion

        #region 墙体基线以及模型线构成了一系列的矩形，考虑洞口（主要是楼梯洞口）处不生成矩形，其他处生成矩形

        /// <summary>
        /// 墙体基线以及模型线构成了一系列的矩形，考虑洞口（主要是楼梯洞口）处不生成矩形，其他处生成矩形
        /// </summary>
        /// <param name="allCurves"></param>
        /// <param name="holeOfElement"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static List<List<Curve>> RectangleFromCurvesConsideringHole(List<Curve> allCurves,
            List<CurveLoop> floorEdges, double z)
        {
            List<List<Curve>> rectangles_overAgain = RectanglesFromCurves(allCurves, floorEdges);

            rectangles_overAgain = DeteleSmallRectangleWhichIsInABigOne(rectangles_overAgain);
            List<CurveLoop> holeCurveLoops = new List<CurveLoop>(floorEdges);
            holeCurveLoops.RemoveAt(0);

            List<CurveLoop> holeOutBoardCurveLoops = new List<CurveLoop>();
            List<CurveLoop> holeInBoardCurveLoops = new List<CurveLoop>();
            ClassifyHole(allCurves, holeCurveLoops, out holeOutBoardCurveLoops, out holeInBoardCurveLoops, 200/304.8, z);

            List<List<XYZ>> projectedPointsOfOutBoardHole = ProjectPointsOfEveryHole(holeOutBoardCurveLoops, z);

            foreach (var item in projectedPointsOfOutBoardHole)
            {
                rectangles_overAgain = DeleteRectangleNearHoleFromRectangles(rectangles_overAgain, item);
            }

            foreach (var item in holeInBoardCurveLoops)
            {
                rectangles_overAgain = AddHoleRectangleToNearestRectangle(rectangles_overAgain, item, z);
            }


            return rectangles_overAgain;

        }

        #endregion

        #region 给定一些曲线，根据这些曲线来得到矩形

        /// <summary>
        /// 给定一些曲线，根据这些曲线来得到矩形
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<List<Curve>> RectanglesFromCurves(List<Curve> curves, List<CurveLoop> floorEdges)
        {
            List<XYZ> newPoints = NodeOfCurves(curves);
            List<Curve> floorEdgesCurves = new List<Curve>();
            foreach (CurveLoop item in floorEdges)
            {
                foreach (Curve itemm in item)
                {
                    floorEdgesCurves.Add(ProjectCurve(itemm, curves[0].GetEndPoint(0).Z));
                }
            }
            //int num = 0;
            List<List<Curve>> rectangles = new List<List<Curve>>();
            int iiii = 0;
            //从所有节点中任取四个点，尝试构成矩形
            for (int i = 0; i < newPoints.Count; i++)
            {
                for (int j = i + 1; j < newPoints.Count; j++)
                {
                    for (int l = j + 1; l < newPoints.Count; l++)
                    {
                        for (int m = l + 1; m < newPoints.Count; m++)
                        {
                            //num++;
                            List<XYZ> fourPoints = new List<XYZ>();
                            fourPoints.Add(newPoints[i]);
                            fourPoints.Add(newPoints[j]);
                            fourPoints.Add(newPoints[l]);
                            fourPoints.Add(newPoints[m]);
                            //if (fourPoints.Any(x=>x.IsAlmostEqualTo(new XYZ(-125.38553270394, 33.6982367092781, 10.498687664042))))
                            //{
                            //    if (fourPoints.Any(x=>x.IsAlmostEqualTo(new XYZ(-125.38553270394, 29.7612288352624, 10.498687664042))))
                            //    {
                            //        if (fourPoints.Any(x=>x.IsAlmostEqualTo(new XYZ(-119.480020892916, 33.6982367092781, 10.498687664042))))
                            //        {
                            //            if (fourPoints.Any(x=>x.IsAlmostEqualTo(new XYZ(-119.480020892916, 29.7612288352624, 10.498687664042))))
                            //            {
                            //                iiii++;
                            //            }
                            //        }
                            //    }
                            //}
                            //下面这个是用来判断该四个点能否组成一个矩形
                            if (IsFourPointsAbleToCreateARectangle(fourPoints))
                            {
                                List<Curve> rectangle = RectangleFromFourPoints(fourPoints);
                                if (IsRectangleSimpleOne(rectangle, curves, 10))
                                {
                                    rectangles.Add(rectangle);
                                }
                            }
                        }
                    }
                }
            }

            return rectangles;
        }

        public static bool IsRectangleInOriginFloor(List<Curve> rectangle, List<Curve> floorEdges)
        {
            XYZ centroidPoint = CentroidPointOfARectangle(rectangle);
            return IsPointInFace(centroidPoint, floorEdges);
        }

        #endregion

        #region 判断点是否在一个线集合所围成的平面图形内部；取得两条曲线的交点；判断点是否在面内（面是线集合所围成）

        /// <summary>
        /// 注释：此方法相交于DotIsInCurveList更准确，初步判断drawing的精度不够
        /// 
        /// 射线法
        /// 引射线法。就是从该点出发引一条射线，看这条射线和所有边的交点数目。
        /// 如果有奇数个交点，则说明在内部，如果有偶数个交点，则说明在外部。
        /// 这是所有方法中计算量最小的方法，在光线追踪算法中有大量的应用。
        /// </summary>
        /// <param name="point"></param>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static bool DotIsInCurveListTest2(XYZ point, List<Curve> curves)
        {
            int num = curves.Count;
            double[] arrayX = new double[num];
            double[] arrayY = new double[num];
            XYZ tempXYZ = new XYZ();
            for (int n = 0; n < curves.Count(); n++)
            {
                tempXYZ = curves[n].GetEndPoint(0);
                arrayX[n] = tempXYZ.X;
                arrayY[n] = tempXYZ.Y;
            }
            double testx = point.X;
            double testy = point.Y;
            int i, j, crossings = 0;
            for (i = 0, j = num - 1; i < num; j = i++)
            {
                if (((arrayY[i] > testy) != (arrayY[j] > testy)) &&
                    (testx < (arrayX[j] - arrayX[i])*(testy - arrayY[i])/(arrayY[j] - arrayY[i]) + arrayX[i]))
                    crossings++;
            }

            return (crossings%2 != 0);
        }

        public static XYZ GetTwoLineCrossPoint(Curve line1, Curve line2)
        {
            XYZ crossPoint = null;
            IntersectionResultArray resultAarry;
            SetComparisonResult result = line1.Intersect(line2, out resultAarry);
            if (result == SetComparisonResult.Overlap)
            {
                crossPoint = resultAarry.get_Item(0).XYZPoint;
            }

            return crossPoint;
        }

        public static bool IsPointInFace(XYZ point, List<Curve> curveList)
        {
            XYZ direction = new XYZ(998888888, 677776666, 0);
            XYZ point2 = point + direction;
            Curve curve = Line.CreateBound(point, point2);
            List<XYZ> points = new List<XYZ>();
            foreach (var item in curveList)
            {
                XYZ p = null;
                p = GetTwoLineCrossPoint(item, curve);
                if (p != null)
                {
                    points.Add(p);
                }
            }
            if (points.Count%2 == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region 给定一个曲线集合curves1，看curves2中有没有一个曲线的节点落在了curves1的范围内，如果没有的话，说明该矩形是个简单矩形（内部无墙线）

        /// <summary>
        /// 给定一个曲线集合curves1，看curves2中有没有一个曲线的节点落在了curves1的范围内，如果没有的话，说明该矩形是个简单矩形（内部无墙线）
        /// 其中n是用来控制判断精度的，n越大越精确，但是计算量也更大(但是我认为，计算机在进行这种循环的速度还是非常快的，这一点计算量影响可忽略)
        /// </summary>
        /// <param name="curves1"></param>
        /// <param name="curves2"></param>
        /// <returns></returns>
        public static bool IsRectangleSimpleOne(List<Curve> curves1, List<Curve> curves2, int n)
        {
            //List<Curve> offsetCurves1 = new List<Curve>();
            //foreach (var item in curves1)
            //{
            //    Curve newItem = item.CreateOffset(89 / 2 / 304.8, new XYZ(0, 0, 1));
            //    offsetCurves1.Add(newItem);
            //}
            int i = 0;
            foreach (var item in curves2)
            {
                //XYZ p0 = item.GetEndPoint(0);
                //XYZ p1 = item.GetEndPoint(1);
                //if (p0.IsAlmostEqualTo(new XYZ(-141.133564200003, 44.5250083628215, 10.498687664042))||p1.IsAlmostEqualTo(new XYZ(-141.133564200003, 44.5250083628215, 10.498687664042)))
                //{
                //    if (p0.IsAlmostEqualTo(new XYZ(-141.133564200003, 3.51450967515744, 10.498687664042)) || p1.IsAlmostEqualTo(new XYZ(-141.133564200003, 3.51450967515744, 10.498687664042)))
                //    {
                //        i++;
                //    }
                //}
                if (IsCurvePartlyInFace(curves1, item, n, 89))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region 将一个curves（环状）向内偏移一个比较小的量edgeAccuracyControl；获得curve的n等分点;看这n等分点中有没有一个在偏移后的curves范围内

        /// <summary>
        /// 将一个curves（环状）向内偏移一个比较小的量edgeAccuracyControl；获得curve的n等分点;看这n等分点中有没有一个在偏移后的curves范围内
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="curve"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static bool IsCurvePartlyInFace(List<Curve> curves, Curve curve, int n, double edgeAccuracyControl)
        {
            List<Curve> clockWiseCurves = SortCurves(curves);
            CurveLoop curveLoop = new CurveLoop();
            foreach (var item in clockWiseCurves)
            {
                curveLoop.Append(item);
            }
            curveLoop = CurveLoop.CreateViaOffset(curveLoop, edgeAccuracyControl/304.8, new XYZ(0, 0, 1));
            List<Curve> newCurves = new List<Curve>();
            foreach (Curve item in curveLoop)
            {
                newCurves.Add(item);
            }
            foreach (var item in EquallyDividedPointOfACurve(curve, n))
            {
                if (IsPointInFace(item, newCurves))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region 对Curves顺时针排序,这里的排序是指将矩形四条边的集合按照顺时针排序

        // <summary>
        /// 对Curves顺时针排序
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<Curve> SortCurves(List<Curve> curves)
        {
            Curve oCurve = null;

            XYZ p00 = curves[0].GetEndPoint(0);
            XYZ p01 = curves[0].GetEndPoint(1);
            XYZ p10 = curves[1].GetEndPoint(0);
            XYZ p11 = curves[1].GetEndPoint(1);
            XYZ p20 = curves[2].GetEndPoint(0);
            XYZ p21 = curves[2].GetEndPoint(1);
            XYZ p30 = curves[3].GetEndPoint(0);
            XYZ p31 = curves[3].GetEndPoint(1);

            List<Curve> newCurves = new List<Curve>(curves); //复制一份，以防原来的curves被改变 
            List<Curve> sortedCurves = new List<Curve>();

            #region TriedCode

            //Curve startCurve = newCurves.First();
            //newCurves.Remove(startCurve);//去掉第一条曲线
            //Curve startCurve = newCurves[0];
            //newCurves.Remove(startCurve);
            //sortedCurves.Add(startCurve);//加入第一条曲线
            //XYZ endPointOfStartLine = startCurve.GetEndPoint(1);
            //Curve secondCurve = HuntCurveByStartPoint(newCurves, endPointOfStartLine, out oCurve);
            //while (!secondCurve.GetEndPoint(1).IsAlmostEqualTo(startCurve.GetEndPoint(0)))
            ////只要当第二条曲线的端点不跟第一条曲线的起点重合，一直循环下去
            //{
            //    sortedCurves.Add(secondCurve);
            //    newCurves.Remove(oCurve);//去掉原来的曲线
            //    Curve c = HuntCurveByStartPoint(newCurves, secondCurve.GetEndPoint(1), out oCurve);
            //    secondCurve = c;
            //}
            //sortedCurves.Add(secondCurve); 

            #endregion

            Curve curve1 = newCurves[0];
            Curve curve2 = newCurves[0];
            newCurves.Remove(curve2);
            while (!curve2.GetEndPoint(1).IsAlmostEqualTo(curve1.GetEndPoint(0)))
            {
                sortedCurves.Add(curve2);
                curve2 = HuntCurveByStartPoint(newCurves, curve2.GetEndPoint(1), out oCurve);
                newCurves.Remove(oCurve);
            }
            sortedCurves.Add(curve2);
            CurveLoop cl = CurveLoop.Create(sortedCurves);
            XYZ upVector = new XYZ(0, 0, 1);
            if (cl.IsCounterclockwise(upVector)) //判断是否为逆时针
            {
                cl.Flip(); //如果是，反向
                return cl.ToList<Curve>();
            }
            else
                return sortedCurves;
        }

        #endregion

        #region 将线集合中的所有线按照某一个方向排列，即线的方向与指定方向平行

        /// <summary>
        /// 将线集合中的所有线按照某一个方向排列，即线的方向与指定方向平行
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<Curve> GetCurvesParallelDirection(List<Curve> curves, XYZ direction)
        {
            List<Curve> curvesParallelDirection = new List<Curve>();

            foreach (var curve in curves)
            {
                Line line = curve as Line;
                if (line.Direction.IsAlmostEqualTo(direction))
                {
                    curvesParallelDirection.Add(line);
                }
                else if (line.Direction.IsAlmostEqualTo(-direction))
                {
                    Line newLine = Line.CreateBound(line.GetEndPoint(1), line.GetEndPoint(0));
                    curvesParallelDirection.Add(newLine);
                }
            }
            return curvesParallelDirection;
        }

        #endregion

        #region 生成斜腹杆，给定一组上下弦杆对

        /// <summary>
        /// 生成斜腹杆，给定一组上下弦杆对
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<List<Curve>> GetWebTruss(List<Curve> curves, double distance)
        {
            List<List<Curve>> webTrussCurves = new List<List<Curve>>();//斜腹杆

            List<List<XYZ>> chordDividePointsGroup = new List<List<XYZ>>();//上下弦杆划分点对

            //取得弦杆的划分点
            List<XYZ> dividePointsUnder = GetPointsByDivideWithDistance(curves[0], distance);
            List<XYZ> dividePointsUp = GetPointsByDivideWithDistance(curves[1], distance);
            for (int i = 0; i < dividePointsUnder.Count; i++)
            {
                List<XYZ> dividePointsGroup = new List<XYZ>();
                dividePointsGroup.Add(dividePointsUnder[i]);
                dividePointsGroup.Add(dividePointsUp[i]);

                chordDividePointsGroup.Add(dividePointsGroup);
            }

            //定义绘制偏移距离
            double distanceOfPoint = 50/304.5;

            //绘制一个方向倾斜的腹板
            List<Curve> cv1 = new List<Curve>();
            for (int i = 0; i < dividePointsUnder.Count;)
            {
                int x = i;
                int y = i + 1;
                if (y < dividePointsUp.Count)
                {
                    XYZ startPoint = new XYZ(dividePointsUnder[x].X - distanceOfPoint, dividePointsUnder[x].Y, dividePointsUnder[x].Z);
                    XYZ endPoint = new XYZ(dividePointsUp[y].X + distanceOfPoint, dividePointsUp[y].Y, dividePointsUp[y].Z);
                    if (IsPointInCurve(curves[0], startPoint) && IsPointInCurve(curves[1], endPoint))
                    {
                        Line line = Line.CreateBound(startPoint, endPoint);
                        cv1.Add(line);
                        i = i + 2;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            //绘制另一个方向倾斜的腹杆
            List<Curve> cv2 = new List<Curve>();
            for (int i = 1; i < dividePointsUp.Count; )
            {
                int x = i;
                int y = i + 1;
                if (y < dividePointsUnder.Count)
                {
                    XYZ startPoint = new XYZ(dividePointsUp[x].X - distanceOfPoint, dividePointsUp[x].Y, dividePointsUp[x].Z);
                    XYZ endPoint = new XYZ(dividePointsUnder[y].X + distanceOfPoint, dividePointsUnder[y].Y, dividePointsUnder[y].Z);
                    if (IsPointInCurve(curves[1], startPoint) && IsPointInCurve(curves[0], endPoint))
                    {
                        Line line = Line.CreateBound(startPoint, endPoint);
                        cv2.Add(line);
                        i = i + 2;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            webTrussCurves.Add(cv1);
            webTrussCurves.Add(cv2);


            return webTrussCurves;
        }

        #endregion

        #region 寻找以一个点（已知为某条曲线的端点）为起点的曲线，如果找不到，则找以它为端点的曲线，再把此曲线反向。同时，输出原来的曲线

        /// <summary>
        /// 寻找以一个点（已知为某条曲线的端点）为起点的曲线，如果找不到，
        /// 则找以它为端点的曲线，再把此曲线反向。同时，输出原来的曲线
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private static Curve HuntCurveByStartPoint(List<Curve> curves, XYZ point, out Curve originCurve)
        {
            originCurve = null;
            Curve curve = HuntCurveByPoint(curves, point, "start");
            if (curve != null)
            {
                originCurve = curve;
                return curve;
            }
            else
            {
                curve = HuntCurveByPoint(curves, point, "end");
                if (curve != null)
                {
                    originCurve = curve;
                    Curve c = curve.CreateReversed();
                    return c;
                }
            }
            return null;
        }

        #endregion

        #region 在多条curve中，根据起点或端点找到curve

        /// <summary>
        /// 在多条curve中，根据起点或端点找到curve
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="point"></param>
        /// <param name="s">只能为“end”或“start”</param>
        /// <returns></returns>
        public static Curve HuntCurveByPoint(List<Curve> curves, XYZ point, string s)
        {
            Curve curve = null;
            if (s == "end")
            {
                foreach (Curve c in curves)
                {
                    if (c.GetEndPoint(1).IsAlmostEqualTo(point))
                    {
                        curve = c;
                        break;
                    }
                }
            }
            if (s == "start")
            {
                foreach (Curve c in curves)
                {
                    if (c.GetEndPoint(0).IsAlmostEqualTo(point))
                    {
                        curve = c;
                        break;
                    }
                }
            }
            return curve;
        }

        #endregion

        #region 根据四个点（能构成矩形的四个点）来得到一个矩形

        /// <summary>
        /// 根据四个点（能构成矩形的四个点）来得到一个矩形
        /// </summary>
        /// <param name="fourPoints"></param>
        /// <returns></returns>
        public static List<Curve> RectangleFromFourPoints(List<XYZ> fourPoints)
        {
            List<Curve> tryCurve = new List<Curve>();
            for (int i = 0; i < fourPoints.Count; i++)
            {
                for (int j = i + 1; j < fourPoints.Count; j++)
                {
                    tryCurve.Add(Line.CreateBound(fourPoints[i], fourPoints[j]));
                }
            }
            tryCurve = tryCurve.OrderByDescending(x => x.Length).ToList();
            tryCurve.RemoveAt(0);
            tryCurve.RemoveAt(0);

            List<Curve> clockWiseCurves = SortCurves(tryCurve);
            return clockWiseCurves;
        }

        #endregion

        #region 判断某四个点是否能得到一个矩形

        /// <summary>
        /// 判断某四个点是否能得到一个矩形
        /// </summary>
        /// <param name="fourPoints"></param>
        /// <returns></returns>
        public static bool IsFourPointsAbleToCreateARectangle(List<XYZ> fourPoints)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    for (int k = j + 1; k < 4; k++)
                    {
                        XYZ d1 = fourPoints[i] - fourPoints[j];
                        XYZ d2 = fourPoints[i] - fourPoints[k];
                        if (IsTwoDirectionParallel(d1, d2))
                        {
                            return false;
                        }
                    }
                }
            }
            List<Curve> tryCurve = new List<Curve>();
            for (int i = 0; i < fourPoints.Count; i++)
            {
                for (int j = i + 1; j < fourPoints.Count; j++)
                {

                    tryCurve.Add(Line.CreateBound(fourPoints[i], fourPoints[j]));


                }
            }
            double num = 0;
            for (int i = 0; i < tryCurve.Count; i++)
            {
                for (int j = i + 1; j < tryCurve.Count; j++)
                {
                    XYZ direction1 = (tryCurve[i] as Line).Direction;
                    XYZ direction2 = (tryCurve[j] as Line).Direction;
                    if (Math.Abs(direction1.AngleTo(direction2) - Math.PI/2) < SMALL_NUMBER)
                    {
                        num++;
                    }
                }
            }
            if (num >= 4)
            {

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region 判断两个方向是否平行，虽然很短，但是常用，所以单独写出来

        /// <summary>
        /// 判断两个方向是否平行，虽然很短，但是常用，所以单独写出来
        /// </summary>
        /// <param name="direction1"></param>
        /// <param name="direction2"></param>
        /// <returns></returns>
        public static bool IsTwoDirectionParallel(XYZ direction1, XYZ direction2)
        {
            return (Math.Abs(direction1.AngleTo(direction2) - 0) < SMALL_NUMBER ||
                    Math.Abs(direction1.AngleTo(direction2) - Math.PI) < SMALL_NUMBER);
        }

        #endregion

        /// <summary>
        /// 判断两个直线平行且方向一致
        /// </summary>
        /// <param name="dirVec1"></param>
        /// <param name="dirVec2"></param>
        /// <returns></returns>
        public static bool IsEqualDirection(XYZ dirVec1, XYZ dirVec2)

        {
            return Math.Abs(dirVec1.AngleTo(dirVec2) - 0) < SMALL_NUMBER;
        }

        /// <summary>
        /// 判断两个直线平行且方向相反
        /// </summary>
        /// <param name="dirVec1"></param>
        /// <param name="dirVec2"></param>
        /// <returns></returns>
        public static bool IsReverseDirection(XYZ dirVec1, XYZ dirVec2)
        {
            return Math.Abs(dirVec1.AngleTo(dirVec2) - Math.PI) < SMALL_NUMBER;
        }

        /// <summary>
        /// 判断两个直线夹角为90度
        /// </summary>
        /// <param name="dirVec1"></param>
        /// <param name="dirVec2"></param>
        /// <returns></returns>
        public static bool IsHalfOfPI(XYZ dirVec1, XYZ dirVec2)
        {
            return Math.Abs(dirVec1.AngleTo(dirVec2) - Math.PI/2) < SMALL_NUMBER;
        }

        /// <summary>
        /// 判断两个直线夹角为135度
        /// </summary>
        /// <param name="dirVec1"></param>
        /// <param name="dirVec2"></param>
        /// <returns></returns>
        public static bool IsOneAndHalfOfPI(XYZ dirVec1, XYZ dirVec2)
        {
            return Math.Abs(dirVec1.AngleTo(dirVec2) - Math.PI*3 / 2) < SMALL_NUMBER;
        }

        #region 判断两条线是否共线

        /// <summary>
        /// 判断两条线是否共线
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static bool IsTwoLineCollinear(Curve c1, Curve c2)
        {
            bool bol = false;
            if (IsTwoLineParallel(c1, c2))
            {
                XYZ p0 = c1.GetEndPoint(0);
                XYZ p1 = c1.GetEndPoint(1);
                XYZ p2 = c2.GetEndPoint(0);
                if (IsThreePointsCollinear(p0, p1, p2))
                {
                    bol = true;
                }
            }
            return bol;
        }

        #endregion

        #region 判断三点是否共线

        /// <summary>
        /// 判断三点是否共线
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static bool IsThreePointsCollinear(XYZ p1, XYZ p2, XYZ p3)
        {
            XYZ d1 = p2 - p1;
            XYZ d2 = p3 - p1;
            if (IsTwoDirectionParallel(d1, d2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region 根据墙基线以及模型线（全部都是投影并且以连成通长的）生成的矩形，有可能会出现图2、3所示的情况，这时就要删掉被大矩形包住的小矩形

        /// <summary>
        /// 根据墙基线以及模型线（全部都是投影并且以连成通长的）生成的矩形，有可能会出现图2、3所示的情况，这时就要删掉被大矩形包住的小矩形
        /// </summary>
        /// <param name="rectangles"></param>
        /// <returns></returns>
        public static List<List<Curve>> DeteleSmallRectangleWhichIsInABigOne(List<List<Curve>> rectangles)
        {
            List<List<Curve>> copy_rectangles = new List<List<Curve>>(rectangles);
            for (int i = 0; i < rectangles.Count; i++)
            {
                for (int j = i + 1; j < rectangles.Count; j++)
                {
                    if (TotalLengthOfCurveList(rectangles[i]) < TotalLengthOfCurveList(rectangles[j]))
                    {
                        XYZ centroidPoint = CentroidPointOfARectangle(rectangles[i]);
                        if (IsPointInFace(centroidPoint, rectangles[j]))
                        {
                            if (copy_rectangles.Contains(rectangles[i]))
                            {
                                copy_rectangles.Remove(rectangles[i]);
                            }
                        }
                    }
                    else if (TotalLengthOfCurveList(rectangles[i]) > TotalLengthOfCurveList(rectangles[j]))
                    {
                        XYZ centroidPoint = CentroidPointOfARectangle(rectangles[j]);
                        if (IsPointInFace(centroidPoint, rectangles[i]))
                        {
                            if (copy_rectangles.Contains(rectangles[j]))
                            {
                                copy_rectangles.Remove(rectangles[j]);
                            }
                        }
                    }
                }
            }
            return copy_rectangles;
        }

        #endregion

        #region 得到一个曲线集合长度总和

        /// <summary>
        /// 得到一个曲线集合长度总和
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static double TotalLengthOfCurveList(List<Curve> curves)
        {
            double totalLength = 0;
            foreach (var item in curves)
            {
                totalLength += item.Length;
            }
            return totalLength;
        }

        #endregion

        #region 将楼板洞口分类，包括板内洞口，和板外洞口（楼梯洞口）

        /// <summary>
        /// 将楼板洞口分类，包括板内洞口，和板外洞口（楼梯洞口）
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="holeCurveLoops"></param>
        /// <param name="holeOutBoardCurveLoops"></param>
        /// <param name="holeInBoardCurveLoops"></param>
        /// <param name="control"></param>
        public static void ClassifyHole(List<Curve> curves, List<CurveLoop> holeCurveLoops,
            out List<CurveLoop> holeOutBoardCurveLoops, out List<CurveLoop> holeInBoardCurveLoops, double control,
            double z)
        {
            holeOutBoardCurveLoops = new List<CurveLoop>();
            holeInBoardCurveLoops = new List<CurveLoop>();

            foreach (var item in holeCurveLoops)
            {
                //对于某一个洞口item而言，考虑item中每一条c到最近一条基线的距离，如果某一个c离最近基线距离大于control，则说明该洞口是个板内洞口，
                bool bol = true;
                foreach (Curve c in item)
                {
                    Curve projectC = ProjectCurve(c, z);
                    List<double> distances = new List<double>();
                    foreach (var itemm in curves)
                    {
                        double distance = GetTwoLineDistance(projectC, itemm);
                        distances.Add(distance);
                    }
                    distances = distances.OrderBy(x => x).ToList();
                    if (distances[0] > control)
                    {
                        bol = false;
                        break;
                    }
                }
                if (bol)
                {
                    if (IsCurveLoopHasFourCurve(item))
                    {
                        holeOutBoardCurveLoops.Add(item);
                    }

                }
                else
                {
                    holeInBoardCurveLoops.Add(item);
                }
            }
        }

        public static bool IsOutHole(CurveLoop holeCurveLoop, List<Curve> curves, double control)
        {

            foreach (Curve c in holeCurveLoop)
            {
                List<double> distances = new List<double>();
                foreach (var item in curves)
                {
                    double distance = GetTwoLineDistance(c, item);
                    distances.Add(distance);
                }
                distances = distances.OrderBy(x => x).ToList();
                if (distances[0] > control)
                {
                    return false;
                }

            }
            return true;
        }

        #endregion

        #region 得到两条直线最小距离，包括两条直线有很大的错开关系也能求出来

        /// <summary>
        /// 得到两条直线最小距离，包括两条直线有很大的错开关系也能求出来
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <returns></returns>
        public static double GetTwoLineDistance(Curve curve1, Curve curve2)
        {
            Curve extendCurve1 = ShortenOrExtendCurve(curve1, "extend", "extend", 1000);
            XYZ p0 = curve2.GetEndPoint(0);
            double distance = extendCurve1.Distance(p0);
            return distance;
        }

        #endregion

        #region 得到一条直线的中点

        /// <summary>
        /// 得到一条直线的中点
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static XYZ LineMiddlePoint(Curve c)
        {
            XYZ p0 = c.GetEndPoint(0);
            XYZ dire = (c as Line).Direction;
            double length = c.Length;
            XYZ p = p0 + dire*length/2;
            return p;
        }

        public static Curve ShortenOrExtendCurve(Curve curve, string start, string end, double changeLength)
        {
            XYZ p0 = curve.GetEndPoint(0);
            XYZ p1 = curve.GetEndPoint(1);
            XYZ p00;
            XYZ p11;
            XYZ direction = (curve as Line).Direction.Normalize();
            if (start == "shorten")
            {
                p00 = p0 + changeLength*direction;
            }
            else if (start == "extend")
            {
                p00 = p0 - changeLength*direction;
            }
            else
            {
                p00 = p0;
            }
            if (end == "shorten")
            {
                p11 = p1 - changeLength*direction;
            }
            else if (end == "extend")
            {
                p11 = p1 + changeLength*direction;
            }
            else
            {
                p11 = p1;
            }
            Curve changedCurve = Line.CreateBound(p00, p11);

            return changedCurve;
        }

        #endregion

        #region 将几个洞口的CurveLoop的点向某个Z值平面上投影得到相应的点集合的集合

        /// <summary>
        /// 将几个洞口的CurveLoop的点向某个Z值平面上投影得到相应的点集合的集合
        /// </summary>
        /// <param name="holeOfElement"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static List<List<XYZ>> ProjectPointsOfEveryHole(List<CurveLoop> holeCurveLoops, double z)
        {
            List<List<XYZ>> holePoints = new List<List<XYZ>>();
            foreach (var item in holeCurveLoops)
            {
                List<XYZ> oneHolePoints = new List<XYZ>();
                foreach (Curve curve in item)
                {
                    XYZ p0 = curve.GetEndPoint(0);
                    XYZ p1 = curve.GetEndPoint(1);
                    XYZ p2 = ProjectPoint(p0, z);
                    XYZ p3 = ProjectPoint(p1, z);
                    oneHolePoints.Add(p2);
                    oneHolePoints.Add(p3);
                }
                oneHolePoints = DistinctPoint(oneHolePoints);
                holePoints.Add(oneHolePoints);
            }



            return holePoints;

        }

        #endregion

        #region 将一个点投影到另一个Z值平面上,得到一个新点

        /// <summary>
        /// 将一个点投影到另一个Z值平面上,得到一个新点
        /// </summary>
        /// <param name="originPoint"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static XYZ ProjectPoint(XYZ originPoint, double z)
        {
            XYZ newPoint = new XYZ(originPoint.X, originPoint.Y, z);
            return newPoint;
        }

        #endregion

        #region 给定一个矩形的集合A，和一个矩形B，删掉A中四个端点离B最近的那个矩形

        /// <summary>
        /// 给定一个矩形的集合A，和一个矩形B，删掉A中四个端点离B最近的那个矩形
        /// </summary>
        /// <param name="allRectangles"></param>
        /// <param name="holePoints"></param>
        /// <returns></returns>
        public static List<List<Curve>> DeleteRectangleNearHoleFromRectangles(List<List<Curve>> allRectangles,
            List<XYZ> holePoints)
        {
            //先找到矩形区域四个顶点的中心点
            XYZ p0 = holePoints[0];
            XYZ p1 = holePoints[1];
            XYZ p2 = holePoints[2];
            XYZ p3 = holePoints[3];
            XYZ p4 = new XYZ((p0.X + p1.X)/2, (p0.Y + p1.Y)/2, p0.Z);
            XYZ p5 = new XYZ((p2.X + p3.X)/2, (p2.Y + p3.Y)/2, p2.Z);
            XYZ p6 = new XYZ((p4.X + p5.X)/2, (p4.Y + p5.Y)/2, p2.Z);

            List<List<Curve>> orderedAllRectangles = OrderRectangle(allRectangles, p6);

            orderedAllRectangles.RemoveAt(0);

            return orderedAllRectangles;

        }

        #endregion

        #region 给出一个矩形集合A，和某个点P，按照A中每个矩形四个端点到P的距离之和的大小进行排序

        /// <summary>
        /// 给出一个矩形集合A，和某个点P，按照A中每个矩形四个端点到P的距离之和的大小进行排序
        /// </summary>
        /// <param name="rectangles"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static List<List<Curve>> OrderRectangle(List<List<Curve>> rectangles, XYZ point)
        {
            return rectangles.OrderBy(x => CentroidPointOfARectangle(x).DistanceTo(point)).ToList();
        }

        #endregion

        #region 得到一个矩形的形心，这个矩形表达形式为曲线集合

        /// <summary>
        /// 得到一个矩形的形心，这个矩形表达形式为曲线集合
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public static XYZ CentroidPointOfARectangle(List<Curve> rectangle)
        {
            List<XYZ> points = EndPointOfCurves(rectangle);
            XYZ p1 = new XYZ((points[0].X + points[1].X)/2, (points[0].Y + points[1].Y)/2, (points[0].Z + points[1].Z)/2);
            XYZ p2 = new XYZ((points[2].X + points[3].X)/2, (points[2].Y + points[3].Y)/2, (points[2].Z + points[3].Z)/2);
            XYZ p3 = new XYZ((p1.X + p2.X)/2, (p1.Y + p2.Y)/2, (p1.Z + p2.Z)/2);
            return p3;
        }

        public static List<List<Curve>> AddHoleRectangleToNearestRectangle(List<List<Curve>> allRectangles,
            CurveLoop inBoardHoleCurveLoop, double z)
        {
            //先找到矩形区域四个顶点的中心点
            List<XYZ> projectedinBoardHolePoints = new List<XYZ>();
            foreach (Curve item in inBoardHoleCurveLoop)
            {
                projectedinBoardHolePoints.Add(ProjectPoint(item.GetEndPoint(0), z));
            }
            XYZ p0 = projectedinBoardHolePoints[0];
            XYZ p1 = projectedinBoardHolePoints[1];
            XYZ p2 = projectedinBoardHolePoints[2];
            XYZ p3 = projectedinBoardHolePoints[3];
            XYZ p4 = new XYZ((p0.X + p1.X)/2, (p0.Y + p1.Y)/2, p0.Z);
            XYZ p5 = new XYZ((p2.X + p3.X)/2, (p2.Y + p3.Y)/2, p2.Z);
            XYZ p6 = new XYZ((p4.X + p5.X)/2, (p4.Y + p5.Y)/2, p2.Z);


            List<List<Curve>> orderedAllRectangles = OrderRectangle(allRectangles, p6);

            foreach (Curve item in inBoardHoleCurveLoop)
            {
                orderedAllRectangles[0].Add(item);
            }

            return orderedAllRectangles;


        }

        #endregion

        #region 返回Curves的顶点

        /// <summary>
        /// 返回Curves的顶点
        /// </summary>
        /// <returns></returns>
        public static List<XYZ> EndPointOfCurves(List<Curve> curves)
        {
            List<XYZ> temp = new List<XYZ>();
            List<XYZ> points = new List<XYZ>();
            foreach (Curve item in curves)
            {
                temp.Add(item.GetEndPoint(0));
                temp.Add(item.GetEndPoint(1));
            }
            points.Add(temp[0]);
            temp.ForEach(x => { if (!points.Any(y => y.IsAlmostEqualTo(x))) points.Add(x); }); //去重
            return points;
        }

        #endregion

        #region 得到一个Curve集合curves所有交点，此处称为节点（node），但是对于图1的情况还需要考虑

        /// <summary>
        /// 得到一个Curve集合curves所有交点，此处称为节点（node），但是对于图1的情况还需要考虑
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<XYZ> NodeOfCurves(List<Curve> curves)
        {
            List<XYZ> nodes = new List<XYZ>();
            for (int i = 0; i < curves.Count; i++)
            {
                for (int j = i + 1; j < curves.Count; j++)
                {
                    XYZ point = new XYZ();
                    try
                    {
                        point = GetIntersection(curves[i], curves[j]);
                        nodes.Add(point);
                    }
                    catch (Exception)
                    {
                        point = null;
                    }
                }
            }
            nodes = DistinctPoint(nodes);
            return nodes;
        }

        #endregion

        #region 判断、返回两直线相交的情况.即返回交点

        /// <summary>
        /// 返回两直线相交的情况.
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static XYZ GetIntersection(Curve line1, Curve line2)
        {
            IntersectionResultArray results;
            SetComparisonResult result = line1.Intersect(line2, out results);
            XYZ intersectionPoint;
            if (result != SetComparisonResult.Overlap)
                //throw new InvalidOperationException("Input lines did not intersect.");
                intersectionPoint = null;
            if (results == null || results.Size != 1)
                throw new InvalidOperationException("Could not extract intersection point for lines.");
            IntersectionResult iResult = results.get_Item(0);
            intersectionPoint = iResult.XYZPoint;

            return intersectionPoint;
        }

        #endregion

        #region 找出一系列曲线中最长的一条

        /// <summary>
        /// 找出一系列曲线中最长的一条
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static Curve GetMaxLengthInCurves(List<Curve> curves)
        {
            curves.OrderByDescending(x => x.Length).ToList();
            return curves[0];
        }

        #endregion

        #region 找出一系列曲线中最短的一条

        /// <summary>
        /// 找出一系列曲线中最短的一条
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static Curve GetMinLengthInCurves(List<Curve> curves)
        {
            curves.OrderBy(x => x.Length).ToList();
            return curves[0];
        }

        #endregion

        #region 偏移复制一系列点，沿着一定的方向

        /// <summary>
        /// 偏移复制一系列点，沿着一定的方向
        /// </summary>
        /// <param name="poinsList"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static List<XYZ> OffsetCurves(List<XYZ> poinsList, XYZ direction, double  length)
        {
            List<XYZ> pointsAfterOffset = new List<XYZ>();
            if (Math.Abs(direction.X - 0.0000000) > 0.000000)
            {
                //for (int i = 0; i < poinsList.Count; i ++)
                //{
                //    XYZ point = new XYZ((poinsList[i].X + length*direction.X), poinsList[i].Y, poinsList[i].Z);
                //    pointsAfterOffset.Add(point);
                //}

                foreach (XYZ xyz in poinsList)
                {
                    XYZ point = new XYZ();
                    point = new XYZ((xyz.X + length * direction.X), xyz.Y, xyz.Z);
                    pointsAfterOffset.Add(point);
                }
                
            }
            else if (Math.Abs(direction.Y- 0.0000000) > 0.000000)
            {
                foreach (var xyz in poinsList)
                {
                    XYZ point = new XYZ();
                    point = new XYZ(xyz.X, xyz.Y + length*direction.Y, xyz.Z);
                    poinsList.Add(point);
                }
            }
            else if (Math.Abs(direction.Z - 0.0000000) > 0.000000)
            {
                foreach (var xyz in poinsList)
                {
                    XYZ point = new XYZ();
                    point = new XYZ(xyz.X, xyz.Y, xyz.Z + length*direction.Z);
                    poinsList.Add(point);
                }
            }

            return pointsAfterOffset;

        }

        #endregion

        #region 偏移复制一系列线，沿着一定的方向(这里是Z方向，或者其他平行于坐标轴的方向）

        public static List<Curve> DuplicateCurvesByOffset(List<Curve> curves, XYZ direction, double distance)
        {
            List<Curve> curvesCopy = new List<Curve>();
            XYZ dirvec = direction.Normalize();

            foreach (var curve in curves)
            {
                XYZ startPoint = new XYZ(curve.GetEndPoint(0).X, curve.GetEndPoint(0).Y,
                            curve.GetEndPoint(0).Z + distance * direction.Z);
                XYZ endPoint = new XYZ(curve.GetEndPoint(1).X, curve.GetEndPoint(1).Y,
                            curve.GetEndPoint(1).Z + distance*direction.Z);
                Line line = Line.CreateBound(startPoint, endPoint);
                curvesCopy.Add(line);
            }

            return curvesCopy;
        }
        #endregion

        #region 求空间中一点到一条直线的距离

        /// <summary>
        /// 求空间中一点到一条直线的距离
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Double TheDistanceBetweenPointAndLine(Curve curve, XYZ point)
        {
            double x, y, z;
            double xInCurve, yInCurve, zInCurve;
            double A, B, C, D;

            double distanceBetweenPointLine;

            x = point.X;
            y = point.Y;
            z = point.Z;

            xInCurve = curve.GetEndPoint(0).X;
            yInCurve = curve.GetEndPoint(0).Y;
            zInCurve = curve.GetEndPoint(0).Z;


            Line line = curve as Line;

            A = line.Direction.X;
            B = line.Direction.Y;
            C = line.Direction.Z;
            D = -(A*xInCurve + B*yInCurve + C*zInCurve);

            distanceBetweenPointLine = (Math.Abs(A*x + B*y + C*z + D))/(Math.Sqrt(A*A + B*B + C*C));

            return distanceBetweenPointLine;

        }

        #endregion

        #region 从一个曲线集合中找到一条曲线与已知曲线curve平行，已知曲线可以使曲线集合中的（没有写有包含多条平行线的方法）

        /// <summary>
        /// 从一个曲线集合中找到一条曲线与已知曲线curve平行，已知曲线可以使曲线集合中的（没有写有包含多条平行线的方法）
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static Curve GetCurveParallelCurves(Curve curve, List<Curve> curves)
        {
            Curve curveParallel = null;

            foreach (Curve cv in curves)
            {
                if (General.IsTwoLineParallel(curve, cv))
                {
                    curveParallel = cv;
                }
            }
            return curveParallel;
        }

        #endregion

        #region 返回两直线相交的情况,如果相交返回交点，如果不相交返回null

        /// <summary>
        /// 返回两直线相交的情况,如果相交返回交点，如果不相交返回null
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static XYZ GetIntersectionSet(Curve line1, Curve line2)
        {
            IntersectionResultArray results;
            SetComparisonResult result = line1.Intersect(line2, out results);
            XYZ intersectionPoint = new XYZ();
            if (result != SetComparisonResult.Overlap)//不重叠就是平行，没有交点
                //throw new InvalidOperationException("Input lines did not intersect.");
                intersectionPoint = null;
            if (results == null || results.Size != 1)
                //throw new InvalidOperationException("Could not extract intersection point for lines.");
                intersectionPoint = null;
            if (results != null && results.Size == 1)
            {
                IntersectionResult iResult = results.get_Item(0);
                intersectionPoint = iResult.XYZPoint;
            }
            
            return intersectionPoint;
        }

        #endregion

        #region 判断点是否在直线上,返回真假

        /// <summary>
        /// 判断点是否在直线上
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        public static bool IsPointInCurve(Curve curve, XYZ point)
        {
            XYZ startPoint = curve.GetEndPoint(0);
            XYZ endPoint = curve.GetEndPoint(1);

            XYZ vectorSToP = startPoint - point;
            XYZ vectorEToP = endPoint - point;

            double lengthOfSToP, lengthOfEToP;
            lengthOfSToP = vectorSToP.GetLength();
            lengthOfEToP = vectorEToP.GetLength();

            if (vectorSToP.DotProduct(vectorEToP) < 0 &&
                (Math.Abs(vectorSToP.DotProduct(vectorEToP)) - lengthOfSToP*lengthOfEToP < 0.00000001))
            {
                return true; //返回true，表示点在线上
            }
            else
            {
                return false;
            }
        }

        #endregion

        /// <summary>
        /// 将点按照固定方向偏移一定距离
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static XYZ Offset(XYZ origin, XYZ dirVec, double offset)
        {
            XYZ point = origin + Normalize(dirVec)*offset;
            return new XYZ(point.X, point.Y, point.Z);
        }

        /// <summary>
        /// 归一化
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static XYZ Normalize(XYZ origin)
        {
            double mod = Modulus(origin);
            if (mod < SMALL_NUMBER)
            {
                return new XYZ();
            }
            return new XYZ(origin.X/mod, origin.Y/mod, origin.Z/mod);
        }

        /// <summary>
        /// 向量模
        /// </summary>
        /// <returns></returns>
        public static double Modulus(XYZ origin)
        {
            return Math.Sqrt(Math.Pow(origin.X, 2) + Math.Pow(origin.Y, 2) + Math.Pow(origin.Z, 2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orientation"></param>
        /// <returns></returns>
        public static double TransformBeamOrient(XYZ orientation)
        {
            if (orientation.Y != 0)
            {
                return orientation.Y == -1 ? 0 : Math.PI;
            }
            else if (orientation.Z != 0)
            {
                return orientation.Z == -1 ? 0 : Math.PI;
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oriention"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static double TansformPurlineColumnOrient(XYZ oriention, XYZ normal)
        {
            double angle = oriention.AngleTo(XYZ.BasisZ);
            if (normal.Z < 0)
            {
                if (angle < Math.PI/2)
                {
                    return angle + Math.PI;
                }
                return Math.PI - angle;
            }
            else
            {
                if (angle < Math.PI/2)
                {
                    return Math.PI - angle;
                }
                return Math.PI + angle;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cv"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static double TransformPurlinBraceOrient(Curve cv, XYZ normal)
        {
            var line = cv as Line;
            var v1 = line.Direction.CrossProduct(XYZ.BasisZ);
            var v2 = line.Direction.CrossProduct(v1);
            var v3 = normal.CrossProduct(line.Direction);
            if (normal.Z < 0)
            {
                return v3.AngleTo(v2);

            }
            return -v3.AngleTo(v2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orientation"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static double TransformPurlinBeamOrient(XYZ orientation, XYZ normal)
        {
            double angle = orientation.AngleTo(-XYZ.BasisZ);
            if (normal.Z < 0)
            {
                return angle;
            }
            else
            {
                return 2*Math.PI - angle;
            }
        }

        /// <summary>
        /// 获得竖杆的旋转角度
        /// </summary>
        /// <param name="orientation">竖杆在起点还是终点</param>
        /// <param name="direction">竖杆在空间坐标系下确定的方向</param>
        /// <returns></returns>
        public static double TransformColumnOrient(XYZ orientation, XYZ direction)
        {
            var angle = direction.AngleFrom(new XYZ(1, 0, 0));
            angle = orientation.X == 1 ? angle : (angle + Math.PI)%(2*Math.PI);
            return angle;
        }

        /// <summary>
        /// 获得竖杆旋转角度
        /// </summary>
        /// <param name="direction">竖杆在空间坐标系下的朝向</param>
        /// <returns></returns>
        public static double TransformColumnOrient(XYZ orientation)
        {
            var angle = orientation.AngleFrom(new XYZ(1, 0, 0));
            return angle;
        }

        /// <summary>
        /// 向量逆时针到旋转到终点向量的角度（均为平行于水平面的向量），值域为[0, 2π)（平行于水平面的向量）
        /// </summary>
        /// <param name="v"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double AngleFrom(this XYZ v, XYZ source)
        {
            return v.AngleFrom(source, XYZ.BasisZ);
        }

        /// <summary>
        /// 设定一个参考平面的法向量，向量source逆时针旋转到终点向量的角度，值域为[0, 2π)
        /// </summary>
        /// <param name="v"></param>
        /// <param name="source"></param>
        /// <param name="refNormal"></param>
        /// <returns></returns>
        public static double AngleFrom(this XYZ v, XYZ source, XYZ refNormal)
        {
            var angle = v.AngleTo(source);
            if (Math.Abs(angle - 0) < SMALL_NUMBER)
            {
                return 0;
            }
            if (v.CrossProduct(source).AngleTo(refNormal) < Math.PI/2)
            {
                return 2*Math.PI - angle;
            }
            return angle;
        }
    }

    


    #region 定义只能选择楼板类型的一个类，可以用于申明一个新类（楼板单元过滤）,新类型在使用的时候需要New一个实例

    /// <summary>
    /// 只能选取楼板
    /// </summary>
    public class GroupPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {
            return (e.Category.Id.IntegerValue.Equals((int) BuiltInCategory.OST_Floors));
        }

        public bool AllowReference(Reference r, XYZ p)
        {
            return false;
        }
    }

    #endregion

    #region 定义只能选择梁柱支撑的新类，可以用于申明一个新类（梁柱支撑单元过滤）

    /// <summary>
    /// 只能选取梁柱支撑
    /// </summary>
    public class SelectFilter : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {
            //return (e.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Walls))
            return (e.Category.Id.IntegerValue.Equals((int) BuiltInCategory.OST_StructuralColumns) ||
                    e.Category.Id.IntegerValue.Equals((int) BuiltInCategory.OST_StructuralFraming));
        }

        public bool AllowReference(Reference r, XYZ p)
        {
            return false;
        }
    }

    #endregion


        





}