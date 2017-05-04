using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

            IList<XYZ> xyz1 = new List<XYZ>();
            IList<XYZ> xyz2 = new List<XYZ>();

            IList<Curve> lines1 = new List<Curve>();
            IList<Curve> lines2 = new List<Curve>();

            IList<CurveLoop> curvesUp = faceUp.GetEdgesAsCurveLoops();//楼
            IList<CurveLoop> curvesUnder = faceUnder.GetEdgesAsCurveLoops();

            //取得楼板上表面轮廓线集合
            IList<Curve> edgesLineUp = new List<Curve>();
            edgesLineUp = General.GetCurvesInFace(faceUp);

            //取得楼板下表面轮廓线集合
            IList<Curve> edgesLineUnder = new List<Curve>();
            edgesLineUnder = General.GetCurvesInFace(faceUnder);

            //获得上表面轮廓线中与X轴平行的边线，放入到集合
            IList<Curve> edgeLinesUpParallelX = new List<Curve>();
            foreach (Curve cv in edgesLineUp)
            {
                Curve lineList = General.GetLineParallelZX(cv);
                if (lineList != null)
                {
                    edgeLinesUpParallelX.Add(lineList);
                }
            }
            foreach (Curve cv in edgesLineUp)
            {
                Curve lineList = General.GetLineParallelFX(cv);
                if (lineList != null)
                {
                    edgeLinesUpParallelX.Add(lineList);
                }
            }
            TaskDialog.Show("上表面轮廓边线集合中平行于X轴的边线个数：", edgeLinesUpParallelX.Count.ToString());
            //获得下表面轮廓线中与X轴平行的边线，放入到集合
            IList<Curve> edgeLinesUuderParallelX = new List<Curve>();
            foreach (Curve cv in edgesLineUnder)
            {
                Curve lineList = General.GetLineParallelZX(cv);
                if (lineList != null)
                {
                    edgeLinesUuderParallelX.Add(lineList);
                }
            }
            foreach (Curve cv in edgesLineUnder)
            {
                Curve lineList = General.GetLineParallelFX(cv);
                if (lineList != null)
                {
                    edgeLinesUuderParallelX.Add(lineList);
                }
            }
            TaskDialog.Show("下表面轮廓边线集合中平行于X轴的边线个数：", edgeLinesUuderParallelX.Count.ToString());

            //根据选择出的边线绘制定位线，自己绘制的好处是可以保证方向一致
            IList<Curve> locationLinesUp = new List<Curve>();
            for (int i = 0; i < edgeLinesUpParallelX.Count; i++)
            {
                //string strInfo0 = string.Format("{0}, {1}, {2} ", edgeLinesUpParallelX[i].GetEndPoint(0).X,
                //    edgeLinesUpParallelX[i].GetEndPoint(0).Y, edgeLinesUpParallelX[i].GetEndPoint(0).Z);
                //TaskDialog.Show("0点坐标：", strInfo0);
                //string strInfo1 = string.Format("{0}, {1}, {2} ", edgeLinesUpParallelX[i].GetEndPoint(1).X,
                //    edgeLinesUpParallelX[i].GetEndPoint(1).Y, edgeLinesUpParallelX[i].GetEndPoint(1).Z);
                //TaskDialog.Show("0点坐标：", strInfo1);

                XYZ dirVec = edgeLinesUpParallelX[i].GetEndPoint(1).Subtract(edgeLinesUpParallelX[i].GetEndPoint(0));
                if (Math.Abs(dirVec.AngleTo(new XYZ(1, 0, 0)))<0.000000000001)
                {
                    Line line = Line.CreateBound(edgeLinesUpParallelX[i].GetEndPoint(0),
                    edgeLinesUpParallelX[i].GetEndPoint(1));
                    if (line != null)
                    {
                        locationLinesUp.Add(line);
                    }
                    string strInfo = string.Format("{0}, {1}, {2}, {3}, {4}, {5}", 
                        line.GetEndPoint(0).X, line.GetEndPoint(0).Y, line.GetEndPoint(0).Z,
                        line.GetEndPoint(1).X, line.GetEndPoint(1).Y, line.GetEndPoint(1).Z);
                    TaskDialog.Show("线的起始点坐标：", strInfo);
                }
                if(Math.Abs(dirVec.AngleTo(new XYZ(-1, 0, 0))) < 0.000000000001)
                {
                    Line line = Line.CreateBound(edgeLinesUpParallelX[i].GetEndPoint(1),
                    edgeLinesUpParallelX[i].GetEndPoint(0));
                    if (line != null)
                    {
                        locationLinesUp.Add(line);
                    }
                    string strInfo = string.Format("{0}, {1}, {2}, {3}, {4}, {5}",
                        line.GetEndPoint(0).X, line.GetEndPoint(0).Y, line.GetEndPoint(0).Z,
                        line.GetEndPoint(1).X, line.GetEndPoint(1).Y, line.GetEndPoint(1).Z);
                    TaskDialog.Show("线的起始点坐标：", strInfo);
                }
                
                
            }
            TaskDialog.Show("上表面定位线个数：", locationLinesUp.Count.ToString());


            IList<Curve> locationLinesUnder = new List<Curve>();
            for (int i = (edgeLinesUuderParallelX.Count-1); i >= 0; i--)
            {
                //string strInfo0 = string.Format("{0}, {1}, {2} ", edgeLinesUuderParallelX[i].GetEndPoint(0).X,
                //    edgeLinesUuderParallelX[i].GetEndPoint(0).Y, edgeLinesUuderParallelX[i].GetEndPoint(0).Z);
                //TaskDialog.Show("0点坐标：", strInfo0);
                //string strInfo1 = string.Format("{0}, {1}, {2} ", edgeLinesUuderParallelX[i].GetEndPoint(1).X,
                //    edgeLinesUuderParallelX[i].GetEndPoint(1).Y, edgeLinesUuderParallelX[i].GetEndPoint(1).Z);
                //TaskDialog.Show("0点坐标：", strInfo1);

                XYZ dirVec = edgeLinesUuderParallelX[i].GetEndPoint(1).Subtract(edgeLinesUuderParallelX[i].GetEndPoint(0));
                if (Math.Abs(dirVec.AngleTo(new XYZ(-1, 0, 0))) < 0.000000000001)
                {
                    Line line = Line.CreateBound(edgeLinesUuderParallelX[i].GetEndPoint(1),
                    edgeLinesUuderParallelX[i].GetEndPoint(0));
                    if (line != null)
                    {
                        locationLinesUnder.Add(line);
                    }
                    string strInfo = string.Format("{0}, {1}, {2}, {3}, {4}, {5}",
                        line.GetEndPoint(0).X, line.GetEndPoint(0).Y, line.GetEndPoint(0).Z,
                        line.GetEndPoint(1).X, line.GetEndPoint(1).Y, line.GetEndPoint(1).Z);
                    TaskDialog.Show("线的起始点坐标：", strInfo);
                }
                if (Math.Abs(dirVec.AngleTo(new XYZ(1, 0, 0))) < 0.000000000001)
                {
                    Line line = Line.CreateBound(edgeLinesUuderParallelX[i].GetEndPoint(0),
                    edgeLinesUuderParallelX[i].GetEndPoint(1));
                    if (line != null)
                    {
                        locationLinesUnder.Add(line);
                    }
                    string strInfo = string.Format("{0}, {1}, {2}, {3}, {4}, {5}",
                        line.GetEndPoint(0).X, line.GetEndPoint(0).Y, line.GetEndPoint(0).Z,
                        line.GetEndPoint(1).X, line.GetEndPoint(1).Y, line.GetEndPoint(1).Z);
                    TaskDialog.Show("线的起始点坐标：", strInfo);
                }

            }
            TaskDialog.Show("下表面定位线个数：", locationLinesUnder.Count.ToString());
            
            ////将定位线以一定的间距进行划分，取得分点
            
            //定义划分间距
            const double distance = 5;
            //上表面定位线划分
            List<XYZ> dividePointsUp = new List<XYZ>();
            //List<List<XYZ> > sss=new List<List<XYZ>>();
            //sss.Add(dividePointsUp);
            
            string strInfoTest = null;
            foreach (Curve curve in locationLinesUp)
            {
                IList<XYZ> points = new List<XYZ>();
                points = General.GetPointsByDivideWithDistance(curve, distance);
                dividePointsUp.AddRange(points);
                strInfoTest += string.Format("{0}, {1}, {2}", points.First().X, points.First().Y, points.First().Z);
            }
            //下表面定位线划分
            List<XYZ> dividePointsUuder = new List<XYZ>();
            //List<List<XYZ> > sss=new List<List<XYZ>>();
            //sss.Add(dividePointsUp);
            foreach (Curve curve in locationLinesUnder)
            {
                IList<XYZ> points = new List<XYZ>();
                points = General.GetPointsByDivideWithDistance(curve, distance);
                dividePointsUuder.AddRange(points);
                strInfoTest += string.Format("{0}, {1}, {2}", points.First().X, points.First().Y, points.First().Z);
            }
            TaskDialog.Show("线集合上提出出来的点：", strInfoTest);
            TaskDialog.Show("上表面定位线划分点个数：", dividePointsUp.Count.ToString());
            TaskDialog.Show("下表面定位线划分点个数：", dividePointsUuder.Count.ToString());


            ////创建桁架两端竖杆
            List<Curve> columnTruss = new List<Curve>();
            foreach (XYZ xyzup in dividePointsUp)
            {
                foreach (XYZ xyzunder in dividePointsUuder)
                {
                    if ((Math.Abs(xyzup.X - xyzunder.X) < 0.00000001) &&(Math.Abs(xyzup.Y - xyzunder.Y) < 0.0000001))
                    {
                        Line line = Line.CreateBound(xyzup, xyzunder);
                        columnTruss.Add(line);
                    }
                }
            }
            TaskDialog.Show("竖杆个数：", columnTruss.Count.ToString());


            #region 创建桁架竖杆放置在刚刚生成的线上

            ////创建桁架竖杆放置在刚刚生成的线上

            //在文档中找到名字为"楼板-上下弦"的结构框架类型
            string trussTypeName = "楼板-上下弦";
            FamilySymbol trussType = null;
            ElementFilter trussCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
            ElementFilter familySymbolFilter = new ElementClassFilter(typeof (FamilySymbol));
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
            lvlFilter.OfClass(typeof (Level)).OfCategory(BuiltInCategory.OST_Levels);
            var elementss = from ele in lvlFilter
                let instance = ele as Level
                where instance.Name == "F2"
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

            //使用族类型创建trussBeam
            if (symbolFound)
            {
                for (int i = 0; i < (columnTruss.Count / 2); i++)
                {
                    FamilyInstance colTruss = document.Create.NewFamilyInstance(columnTruss[i], trussType, level,
                        StructuralType.Beam);
                    //将创建的族实例以中性轴为旋转轴，旋转一定的角度
                    colTruss.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(Math.PI*3 / 2);
                }
                for (int i = (columnTruss.Count / 2 - 1); i < columnTruss.Count; i++)
                {
                    FamilyInstance colTruss = document.Create.NewFamilyInstance(columnTruss[i], trussType, level,
                        StructuralType.Beam);
                    colTruss.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(Math.PI / 2);
                }
            }
            #endregion


            #region 创建桁架上下弦杆

            ////创建桁架上下弦杆
            List<Curve> chordTruss = new List<Curve>();
            foreach (XYZ xyzup in dividePointsUp)
            {
                foreach (XYZ xyzunder in dividePointsUp)
                {
                    if (xyzup != xyzunder)
                    {
                        if ((Math.Abs(xyzup.X - xyzunder.X) < 0.0000000001) && (Math.Abs(xyzup.Z - xyzunder.Z) < 0.0000000001))
                        {
                            Line line = Line.CreateBound(xyzup, xyzunder);
                            chordTruss.Add(line);
                        }
                    }
                }
            }
            foreach (XYZ xyzup in dividePointsUuder)
            {
                foreach (XYZ xyzunder in dividePointsUuder)
                {
                    if (xyzup != xyzunder)
                    {
                        if ((Math.Abs(xyzup.X - xyzunder.X) < 0.0000000001) && (Math.Abs(xyzup.Z - xyzunder.Z) < 0.0000000001))
                        {
                            Line line = Line.CreateBound(xyzup, xyzunder);
                            chordTruss.Add(line);
                        }
                    }
                }
            }
            TaskDialog.Show("上下弦杆个数：", chordTruss.Count.ToString());
            for (int i = 0; i < (chordTruss.Count / 2); i++)
            {
                FamilyInstance choTruss = document.Create.NewFamilyInstance(chordTruss[i], trussType, level,
                    StructuralType.Beam);
                //将创建的族实例以中性轴为旋转轴，旋转一定的角度
                //choTruss.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(Math.PI * 3 / 2);
            }
            for (int i = (chordTruss.Count / 2 -1); i < chordTruss.Count; i++)
            {
                FamilyInstance choTruss = document.Create.NewFamilyInstance(chordTruss[i], trussType, level,
                    StructuralType.Beam);
                //将创建的族实例以中性轴为旋转轴，旋转一定的角度
                choTruss.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(Math.PI);
            }

            #endregion

            




            trans.Commit();
            return Result.Succeeded;

        }
    }
}