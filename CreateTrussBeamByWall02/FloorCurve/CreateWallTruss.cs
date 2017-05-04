using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace FloorCurve
{

    /// <summary>
    /// 墙体选择器，用于选择墙体
    /// </summary>
    internal class WallSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            bool isWall = (elem.Category.Id.IntegerValue == (int) BuiltInCategory.OST_Walls);
            return isWall;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    /// <summary>
    /// 建模选择的墙体是片墙，分割之后的墙体，用于生成框架
    /// </summary>


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateWallTruss:IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            Document document = uiApp.ActiveUIDocument.Document;
            Selection sel = uiApp.ActiveUIDocument.Selection;

            app = uiApp.Application;

            //事务组
            //TransactionGroup transactionGroup = new TransactionGroup(document);
            //transactionGroup.Start();


            Transaction trans = new Transaction(document, "拾取楼板在边线处放置桁架");
            trans.Start();

            ///拾取墙
            Reference refelem = sel.PickObject(ObjectType.Element, "选取一块楼板 ");
            IList<Reference> references = sel.PickObjects(ObjectType.Element, "拾取多个墙体");

            #region 自己拾取墙体的方法

            ////取得墙体集合
            //List<Wall> walls = new List<Wall>();
            //foreach (var reference in references)
            //{
            //    Element elem = document.GetElement(reference) as Element;
            //    Wall wall = elem as Wall;
            //    walls.Add(wall);
            //}

            #endregion

            #region 拾取墙

            List<Wall> walls = new List<Wall>();
            try
            {
                ISelectionFilter wallSelectionFilter = new WallSelectionFilter();
                IList<Reference> wallRefs = sel.PickObjects(ObjectType.Element, wallSelectionFilter, "拾取墙体");

                foreach (Reference wallRef in wallRefs)
                {
                    Wall wall = document.GetElement(wallRef.ElementId) as Wall;
                    walls.Add(wall);
                }
            }
            catch (Exception)
            {

                return Result.Failed;
            }

            TaskDialog.Show("拾取墙体的个数：", walls.Count.ToString());
            #endregion



            #region //获取洞口

            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof (Opening)).OfCategory(BuiltInCategory.OST_SWallRectOpening);

            #region 找到洞口并取得洞口集合以及洞口边线集合

            ///找到墙上洞口
            //洞口集合的集合
            //List<List<Opening>> openings = new List<List<Opening>>();
            //foreach (Wall wall in walls)
            //{
            //    var wallOpenings = from open in collector
            //        where ((Opening) open).Host.Id == wall.Id
            //        select open;

            //    List<Opening> opens = new List<Opening>();
            //    foreach (Element wallOpening in wallOpenings)
            //    {
            //        Opening op = wallOpening as Opening;
            //        opens.Add(op);
            //    }

            //    openings.Add(opens);
            //}
            ////下面对洞口进行处理，获得洞口边界
            //List<List<List<Curve >>> locationOfOpening = new List<List<List<Curve>>>();//洞口边界集合，第一层是墙，第二层是该墙上多个洞口，第三层是该洞口边线
            //foreach (var opening in openings)
            //{
            //    List<List<Curve >> location2 = new List<List<Curve>>();
            //    foreach (var op in opening)
            //    {
            //        //if (op.IsRectBoundary)
            //        //{
            //        //    XYZ startPoint = op.BoundaryRect[0];
            //        //    XYZ endPoint = op.BoundaryRect[1];
            //        //}
            //        //else
            //        //{
            //        //    foreach (Curve curve in op.BoundaryCurves)
            //        //    {

            //        //    }
            //        //}

            //        //为方便，在这里直接提取所有洞口边线，而不是startPoint、endPoint，因为不确定其指代哪里
            //        List<Curve > location1 = new List<Curve>();
            //        foreach (Curve curve in op.BoundaryCurves)
            //        {
            //            location1.Add(curve);
            //        }
            //        location2.Add(location1);
            //    }
            //    locationOfOpening.Add(location2);
            //}

            #endregion

            #region 找到洞口并获得洞口集合以及洞口边线集合（较好的方法）

            ///找到墙上洞口
            //洞口集合的集合
            //List<List<Opening>> openings = new List<List<Opening>>();//墙体洞口集合
            //List<List<List<Curve>>> locationOfOpening = new List<List<List<Curve>>>();//洞口边界集合，第一层是墙，第二层是该墙上多个洞口，第三层是该洞口边线
            //foreach (Wall wall in walls)
            //{
            //    var wallOpenings = from open in collector
            //        where ((Opening) open).Host.Id == wall.Id
            //        select open;

            //    //List<Opening> opens = new List<Opening>();
            //    //foreach (Element wallOpening in wallOpenings.ToList())
            //    //{
            //    //    Opening op = wallOpening as Opening;
            //    //    opens.Add(op);
            //    //}
            //    //openings.Add(opens);

            //    //直接取得洞口边线, 直接把一面墙上所有洞口放在一个集合
            //    List<List<Curve>> location2 = new List<List<Curve>>();
            //    foreach (Element el in wallOpenings.ToList())
            //    {
            //        Opening opening = el as Opening;
            //        //if (opening.IsRectBoundary)
            //        //{
            //        //    //opening.BoundaryRect;
            //        //    //opening.BoundaryCurves;
            //        //}
            //        List<Curve> location1 = new List<Curve>();
            //        foreach (Curve op in opening.BoundaryCurves)
            //        {
            //            location1.Add(op);
            //        }
            //        location2.Add(location1);
            //    }
            //    locationOfOpening.Add(location2);
            //}

            ////显示洞口个数
            //TaskDialog.Show("洞口总个数：", openings.Count.ToString());

            #endregion

            #endregion

            //如果识别不出洞口，可以先识别门窗是否在墙上，然后获得门窗边界或者根据门窗边界创建洞口
           
            //var wallOpenings = from c in collector where ((Opening)c).Host.Id == new ElementId(618627) select c;

            FilteredElementCollector doorCollector = new FilteredElementCollector(document);
            doorCollector.OfClass(typeof (FamilyInstance)).OfCategory(BuiltInCategory.OST_Doors);

            FilteredElementCollector windowCollector = new FilteredElementCollector(document);
            windowCollector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Windows);



            //墙体所有轮廓线集合（先放门洞口轮廓线，再放置窗洞口轮廓线，其他洞口轮廓线再放置在之后）
            List<List<List<Curve>>> openingOutlines = new List<List<List<Curve>>>();



            ///所有墙体外轮廓线
            List<List<Curve>> walloutlines = new List<List<Curve>>();

            foreach (Wall wall in walls)
            {
                #region 试验获取墙定位线(成功）（通过参数转化的失败）

                LocationCurve wallLine = wall.Location as LocationCurve;
                Curve wallBaseLine = wallLine.Curve as Curve;

                //墙定位线方向
                XYZ directionOfWall = (wallBaseLine as Line).Direction;
                // 墙体高度
                double wallheight = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();

                XYZ wallBottomStart = wallBaseLine.GetEndPoint(0);
                XYZ wallBottomEnd = wallBaseLine.GetEndPoint(1);
                XYZ wallTopStart = new XYZ(wallBottomStart.X , wallBottomStart.Y, wallBottomStart.Z + wallheight );
                XYZ wallTopEnd = new XYZ(wallBottomEnd.X, wallBottomEnd.Y, wallBottomEnd.Z + wallheight);
                List<Curve > walloutline = new List<Curve>();
                walloutline.Add(Line.CreateBound(wallBottomStart, wallTopStart));
                walloutline.Add(Line.CreateBound(wallBottomEnd, wallTopEnd));
                walloutline.Add(Line.CreateBound(wallTopStart, wallTopEnd));
                walloutline.Add(Line.CreateBound(wallBottomStart,wallBottomEnd));
                walloutlines.Add(walloutline);
                //var wallLocationline = wall.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Element;
                //Location walLocation = wallLocationline.Location;

                #endregion



                //获得墙上门
                List<Element> doorInWall = GetDoorsIn(wall, doorCollector);
                //获得墙上窗
                List<Element> windowInWall = GetWindowsIn(wall, windowCollector);


                //门洞口轮廓线集合
                List<List<Curve> > outlineOfDoor = new List<List<Curve>>();
                foreach (var elem in doorInWall)
                {
                    //获得门窗洞口高度宽度参数
                    double height = GetHeight(elem);
                    double width = GetWidth(elem);

                    LocationPoint locationPoint = elem.Location as LocationPoint;
                    List<Curve> outlines = OutLines(locationPoint.Point, directionOfWall, height, width);
                    outlineOfDoor.Add(outlines);
                }
                openingOutlines.Add(outlineOfDoor);

                //窗洞口轮廓线集合
                List<List<Curve>> outlineOfWindow = new List<List<Curve>>();
                foreach (var elem in windowInWall)
                {
                    //获得门窗洞口高度宽度参数
                    double height = GetHeight(elem);
                    double width = GetWidth(elem);

                    LocationPoint locationPoint = elem.Location as LocationPoint;
                    List<Curve> outlines = OutLines(locationPoint.Point, directionOfWall, height, width);
                    outlineOfWindow.Add(outlines);
                }
                openingOutlines.Add(outlineOfWindow);

                #region 获取门宽度(成功）

                ///获取门宽度
                /// 
                //List<double > widths = new List<double>();

                //List<FamilySymbol> doorSymbol = new List<FamilySymbol>();
                //foreach (Element element in doorInWall)
                //{
                //    FamilySymbol doSymbol = element as FamilySymbol;
                //    double width = doSymbol.LookupParameter("宽度").AsDouble();
                //    TaskDialog.Show("门类型宽度：", width.ToString());
                //    doorSymbol.Add(doSymbol);

                //}

                //foreach (FamilyInstance  elem in doorInWall)
                //{
                //    double width = elem.Symbol.get_Parameter(BuiltInParameter.CASEWORK_WIDTH).AsDouble();

                //    TaskDialog.Show("门宽度", width.ToString());
                //}

                #endregion

                #region 获取门窗定位点（成功） 

                /////获取门窗定位点
                ///// 

                //List<XYZ> doorLocations = new List<XYZ>();
                //foreach (Element elem in doorInWall)
                //{
                //    LocationPoint doorPoint = elem.Location as LocationPoint;
                //    XYZ point = doorPoint.Point as XYZ;
                //    doorLocations.Add(point);
                //}

                #endregion

                #region 

                /////利用FamilySymbol获取族类型参数
                //foreach (FamilySymbol door in doorInWall)
                //{
                //    double width = door.LookupParameter("宽度").AsDouble();
                //    TaskDialog.Show("门宽度", width.ToString());
                //    double height = door.LookupParameter("高度").AsDouble();
                //    TaskDialog.Show("门高度", height.ToString());
                //}

                #endregion
            }

            //对墙体轮廓线进行拆分，分为水平和竖直
            List<List<List<Curve>>> wallLines = SplitWallOutLine(walloutlines);
            //对洞口轮廓线进行拆分，分为水平和竖直
            List<List<List<List<Curve>>>> openingLines = SplitOpeningOutline(openingOutlines);

            #region 

            //List<List<List<Curve>> > wallLines = new List<List<List<Curve>>>();
            //foreach (var walloutline in walloutlines)
            //{
            //    List<List<Curve>> wallsList = new List<List<Curve>>();//先放竖向再放水平
            //    List<Curve> parallelZCurves = new List<Curve>();
            //    List<Curve> parallelXyCurves = new List<Curve>();
            //    foreach (var curve in walloutline)
            //    {
            //        Line parallelZ = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(1, 0, 0));
            //        if (General.IsTwoLineParallel(curve, (Curve)parallelZ))
            //        {
            //            parallelZCurves.Add(curve);
            //        }
            //        else
            //        {
            //            parallelXyCurves.Add(curve);
            //        }
            //    }
            //    wallsList.Add(parallelZCurves);
            //    wallsList.Add(parallelXyCurves);
            //    wallLines.Add(wallsList);
            //}

            #endregion

            #region 试创建族实例
            ////使用族类型创建
            bool symbolFound = true;
            //梁族类型
            string beamTypeName = "楼板-上下弦";
            FamilySymbol trussBeamType = GetBeamFamilySymbol(document,beamTypeName);
            //柱族类型
            string columnTypeName = "墙体&楼板-竖杆";
            FamilySymbol trussColumnType = GetColumnFamilySymbol(document, columnTypeName);

            if (symbolFound)
            {
                foreach (var wallLine in wallLines)
                {
                    //求面法线方向
                    XYZ normal = new XYZ();
                    List<Curve > walloutline = new List<Curve>();
                    foreach (var line in wallLine)
                    {
                        foreach (var curve in line)
                        {
                            walloutline.Add(curve);
                        }
                    }
                    for (int i = 1; i < walloutline.Count; i++)
                    {
                        if (!General.IsTwoLineParallel(walloutline[0], walloutline[i]))
                        {
                            normal = (walloutline[0] as Line).Direction.CrossProduct((walloutline[i] as Line).Direction);
                            //normal = (curve[0] as Line).Direction.CrossProduct((curve[1] as Line).Direction);
                            break;
                        }
                    }

                    //创建桁架构件，先竖杆后横梁
                    foreach (var line in wallLine)
                    {
                        foreach (var curve in line)
                        {
                            string levelName = "标高 2";
                            Level level = GetLevel(document, levelName);
                            if (General.IsTwoDirectionParallel((curve as Line).Direction, XYZ.BasisZ))
                            {
                                FamilyInstance wallColumn = document.Create.NewFamilyInstance(curve.GetEndPoint(0),
                                    trussColumnType, level, StructuralType.Column);
                                if (General.IsHalfOfPI(normal, OrientationZ(wallColumn)))
                                {
                                    wallColumn.Location.Rotate((curve as Line), Math.PI);
                                }
                            }
                            else
                            {
                                FamilyInstance wallColumn = document.Create.NewFamilyInstance(curve, trussBeamType, level,
                                StructuralType.Beam);
                                StructuralFramingUtils.DisallowJoinAtEnd(wallColumn, 0);
                                StructuralFramingUtils.DisallowJoinAtEnd(wallColumn, 1);
                            }
                        }
                    }
                }
            }

            //继续绘制洞口构件
                

            #endregion


            //对墙轮廓线进行处理，将其分为横杆和竖杆

            //去重，短线在长线上之时，将短线删除，避免实例重复


            trans.Commit();

            return Result.Succeeded;
        }

        /// <summary>
        /// 将墙曲线集合集合的集合拆分（水平竖直）
        /// </summary>
        /// <param name="linesList"></param>
        /// <returns></returns>
        public List<List<List<Curve>>> SplitWallOutLine(List<List<Curve>> linesList)
        {
            List<List<List<Curve>>> lines = new List<List<List<Curve>>>();
            foreach (var line in linesList)
            {
                List<List<Curve>> lists = new List<List<Curve>>();//先放竖向再放水平
                List<Curve> parallelZCurves = new List<Curve>();
                List<Curve> parallelXyCurves = new List<Curve>();
                foreach (var curve in line)
                {
                    Line parallelZ = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(1, 0, 0));
                    if (General.IsTwoLineParallel(curve, (Curve)parallelZ))
                    {
                        parallelZCurves.Add(curve);
                    }
                    else
                    {
                        parallelXyCurves.Add(curve);
                    }
                }
                lists.Add(parallelZCurves);
                lists.Add(parallelXyCurves);
                lines.Add(lists);
            }

            return lines;
        }

        /// <summary>
        /// 将洞口曲线集合集合的集合拆分（水平竖直） 
        /// </summary>
        /// <param name="liness"></param>
        /// <returns></returns>
        public List<List<List<List<Curve>>>> SplitOpeningOutline(List<List<List<Curve>>> liness)
        {
            List<List<List<List<Curve>>>> linesList = new List<List<List<List<Curve>>>>();
            foreach (var lines in liness)
            {
                foreach (var line in lines)
                {
                    List<List<List<Curve>>> lines1 = new List<List<List<Curve>>>();
                    foreach (var curve in line)
                    {
                        List<List<Curve>> lists = new List<List<Curve>>();//先放竖向再放水平
                        List<Curve> parallelZCurves = new List<Curve>();
                        List<Curve> parallelXyCurves = new List<Curve>();
                        Line parallelZ = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(1, 0, 0));
                        if (General.IsTwoLineParallel(curve, (Curve)parallelZ))
                        {
                            parallelZCurves.Add(curve);
                        }
                        else
                        {
                            parallelXyCurves.Add(curve);
                        }
                        lists.Add(parallelZCurves);
                        lists.Add(parallelXyCurves);
                        lines1.Add(lists);
                    }
                    linesList.Add(lines1);
                }
            }
            return linesList;
        }

        /// <summary>
        /// 将族实例按照所给的旋转角和旋转轴旋转一定角度（用于杆件绕自身轴线选装）
        /// </summary>
        /// <param name="familyInstance">待旋转族实例</param>
        /// <param name="angle">旋转角</param>
        /// <param name="rotationAxis">旋转轴</param>
        /// <returns></returns>
        public FamilyInstance Rotate(FamilyInstance familyInstance, double angle, Curve rotationAxis)
        {
            familyInstance.Location.Rotate((Line)rotationAxis, angle);
            return familyInstance;
        }

        /// <summary>
        /// 返回杆件的开口方向
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns></returns>
        public XYZ OrientationZ(FamilyInstance familyInstance)
        {
            XYZ orientationZ = new XYZ();
            orientationZ = familyInstance.GetTransform().BasisZ;
            return orientationZ;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns></returns>
        public XYZ OrientationY(FamilyInstance familyInstance)
        {
            XYZ orientationY = new XYZ();
            orientationY = familyInstance.GetTransform().BasisY;
            return orientationY;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns></returns>
        public XYZ OrientationX(FamilyInstance familyInstance)
        {
            XYZ orientationX = new XYZ();
            orientationX = familyInstance.GetTransform().BasisX;
            return orientationX;
        }

        /// <summary>
        /// 通过族实例创建柱构件（实际上采用的是创建梁的方法）
        /// 柱通过定位点创建族实例
        /// </summary>
        /// <param name="document"></param>
        /// <param name="cv"></param>
        /// <param name="columnType"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public FamilyInstance BuildColumn(Document document, Curve cv, FamilySymbol columnType, Level level)
        {
            //
            FamilyInstance columnFrame = document.Create.NewFamilyInstance(cv.GetEndPoint(0), columnType, level,
                                 StructuralType.Column);

            //确定是否连接端部
            StructuralFramingUtils.DisallowJoinAtEnd(columnFrame, 0);
            StructuralFramingUtils.DisallowJoinAtEnd(columnFrame, 1);
            //将创建的族实例以中性轴为旋转轴，旋转一定的角度
            //wallOutlinesTruss.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(Math.PI);

            return columnFrame;
        }

        /// <summary>
        /// 通过族实例创建柱构件，有端部缩进
        /// 柱通过定位点创建族实例
        /// </summary>
        /// <param name="document"></param>
        /// <param name="cv"></param>
        /// <param name="columnType"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public FamilyInstance BuildColumn(Document document, Curve cv, FamilySymbol columnType, Level baseLevel, Level topLevel,
            double angle, double startExtension, double endExtension)
        {
            //
            FamilyInstance columnFrame = document.Create.NewFamilyInstance(cv.GetEndPoint(0), columnType, baseLevel,
                                 StructuralType.Column);

            //重新设置标高
            columnFrame.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM).Set(baseLevel.Id);
            columnFrame.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).Set(topLevel.Id);

            //通过定义旋转轴线和角度将柱旋转
            columnFrame.Location.Rotate((Line) cv, angle);

            //
            double baseOffset = cv.GetEndPoint(0).Z - baseLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble() +
                                startExtension/304.5;
            double topOffset = cv.GetEndPoint(1).Z - topLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble() + 
                                endExtension/304.5;

            //偏移族实例
            columnFrame.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).Set(baseOffset);
            columnFrame.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(topOffset);

            //确定是否连接端部
            //StructuralFramingUtils.DisallowJoinAtEnd(columnFrame, 0);
            //StructuralFramingUtils.DisallowJoinAtEnd(columnFrame, 1);

            return columnFrame;

        }


        /// <summary>
        /// 通过族实例创建柱构件，有端部缩进
        /// 柱通过定位点创建族实例
        /// </summary>
        /// <param name="document"></param>
        /// <param name="cv"></param>
        /// <param name="columnType"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public FamilyInstance BuildColumn(Document document, Curve cv, FamilySymbol columnType, Level baseLevel, Level topLevel,
            double angle)
        {
            //
            FamilyInstance columnFrame = document.Create.NewFamilyInstance(cv.GetEndPoint(0), columnType, baseLevel,
                                 StructuralType.Column);

            //重新设置标高
            columnFrame.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM).Set(baseLevel.Id);
            columnFrame.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).Set(topLevel.Id);

            //通过定义旋转轴线和角度将柱旋转
            columnFrame.Location.Rotate((Line)cv, angle);

            //
            double baseOffset = cv.GetEndPoint(0).Z - baseLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble();//同时还可以再加上端部缩进
            double topOffset = cv.GetEndPoint(1).Z - topLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble();

            //偏移族实例
            columnFrame.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).Set(baseOffset);
            columnFrame.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(topOffset);

            //确定是否连接端部
            //StructuralFramingUtils.DisallowJoinAtEnd(columnFrame, 0);
            //StructuralFramingUtils.DisallowJoinAtEnd(columnFrame, 1);

            return columnFrame;

        }

        /// <summary>
        ///  给定门窗定位点，高度和宽度，绘制门窗轮廓线(还有墙体定位线方向，这样可以知道门窗平行于那个方向）
        /// </summary>
        /// <param name="locationPoint"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public List<Curve> OutLines(XYZ locationPoint, XYZ dirVec, double height, double width)
        {
            //将定位点沿着方向正反偏移两个点，偏移距离为宽度的一半
            XYZ bottomPointLeft = General.Offset(locationPoint, -dirVec, width/2);
            XYZ bottomPointRight = General.Offset(locationPoint, dirVec, width/2);
            XYZ topPointLeft = new XYZ(bottomPointLeft.X, bottomPointLeft.Y, bottomPointLeft.Z + height);
            XYZ topPointRight = new XYZ(bottomPointRight.X, bottomPointRight.Y, bottomPointRight.Z + height);
            
            //绘制边线（顺时针）
            List<Curve> outlines = new List<Curve>();
            outlines.Add(Line.CreateBound(bottomPointLeft,topPointLeft));
            outlines.Add(Line.CreateBound(topPointLeft, topPointRight));
            outlines.Add(Line.CreateBound(topPointRight, bottomPointRight));
            outlines.Add(Line.CreateBound(bottomPointRight, bottomPointLeft));

            //return outlines;
            //可以再加一个排序
            List<Curve> sortList = General.SortCurves(outlines);
            return sortList;
        }

        /// <summary>
        /// 获取门
        /// </summary>
        /// <param name="wall"></param>
        /// <returns></returns>
        public List<Element> GetDoorsIn(Wall wall, FilteredElementCollector doorCollector)
        {
            var doorInWall = from door in doorCollector
                             where ((FamilyInstance)door).Host.Id == wall.Id
                             select door;

            //TaskDialog.Show("该面墙上门的个数：", doorInWall.Count().ToString());
            List<Element> doors = new List<Element>();
            foreach (var element in doorInWall)
            {
                doors.Add(element);
            }
            return doors;
        }

        /// <summary>
        /// 获取窗
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="windowCollector"></param>
        /// <returns></returns>
        public List<Element> GetWindowsIn(Wall wall, FilteredElementCollector windowCollector)
        {
            var windowInWall = from window in windowCollector
                               where ((FamilyInstance)window).Host.Id == wall.Id
                               select window;

            //TaskDialog.Show("该面墙上门的个数：", doorInWall.Count().ToString());
            List<Element> windows = new List<Element>();
            foreach (var element in windowInWall)
            {
                windows.Add(element);
            }
            return windows;
        }

        /// <summary>
        /// 获取族实例单元参数
        /// </summary>
        /// <param name="elem"></param>
        public void GetHeight(List<Element> elem)
        {

            #region 获取门高度，通过API获取低高度和顶高度相减得形式获得

            ///利用FamilyInstance获取族实例参数
            //关于如何获取对象参数的问题利用revit API
            foreach (FamilyInstance door in elem)
            {
                double bottomHeight = door.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).AsDouble();
                double topheight = door.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM).AsDouble();
                double heightOfWall = topheight - bottomHeight;

                //double doorWidth = door.LookupParameter("面积").AsDouble();

                //FamilySymbol doorSymbol = 

                //TaskDialog.Show("门高度：", heightOfWall.ToString());
                //TaskDialog.Show("门宽度：", doorWidth.ToString());
            }

            #endregion
        }

        /// <summary>
        /// 获取门窗等FamilyInstance单元的高度
        /// </summary>
        /// <param name="elem"></param>
        public double  GetHeight(Element elem)
        {
            //顶高度与底高度相减得方式
            double bottomHeight = elem.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).AsDouble();
            double topheight = elem.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM).AsDouble();
            double heightOfWall = topheight - bottomHeight;
            //return heightOfWall;
            //直接通过FamilyInstance的Symbol获取

            double height = (elem as FamilyInstance).Symbol.get_Parameter(BuiltInParameter.CASEWORK_HEIGHT).AsDouble();

            return height;
        }

        /// <summary>
        /// 获取门窗等FamilyInstance单元的宽度
        /// </summary>
        /// <param name="elem"></param>
        public double GetWidth(Element elem)
        {
            
            //直接通过FamilyInstance的Symbol获取

            double width = (elem as FamilyInstance).Symbol.get_Parameter(BuiltInParameter.CASEWORK_WIDTH).AsDouble();

            return width;
        }

        public Autodesk.Revit.DB.Wall RevitWall
        {
            private set;
            get;
        }

        public Document Doc
        {
            get;
            private set;
        }

        /// <summary>
        /// 提取洞口信息
        /// </summary>
        public void ExtractOpening()
        {
            IList<ElementId> openingElementIds = RevitWall.FindInserts(true, false, false, false);//找到墙上附着的门、窗和洞口等

            foreach (var id in openingElementIds)
            {
                Element instance = Doc.GetElement(id);
                if (instance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                {
                    //加门到门集合
                }
                else if (instance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows)
                {
                    //加窗到窗集合
                }
                else if (instance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_SWallRectOpening)
                {
                    //加洞到洞口集合
                }
                else
                {
                    throw new Exception("id.IntegerValue,暂时无法处理这种类型的洞口");
                }
            }
        }

        //为了获取墙的几何信息，必须先创建一个几何选项
        public Options GetGeometryOption(Application app)
        {
            Options option = app.Create.NewGeometryOptions();
            option.ComputeReferences = true;//打开计算几何引用 
            option.DetailLevel = ViewDetailLevel.Fine;//视图详细程度为最好
            return option;
        }

        /// <summary>
        /// 获取墙体的几何数据（面和边）
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="uiapp"></param>
        /// <param name="aWall"></param>
        public void GetWallGeometry(Document doc, UIApplication uiapp, Wall aWall)
        {
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Options option = GetGeometryOption(app);

            GeometryElement geomElement = aWall.get_Geometry(option);
            foreach (GeometryObject geomObj in geomElement)
            {
                Solid geomSolid = geomObj as Solid;
                if (null != geomSolid)
                {
                    foreach (Face geomFace in geomSolid.Faces)
                    {
                        //得到墙面
                    }
                    foreach (Edge geomEdge in geomSolid.Edges)
                    {
                        //得到墙的边
                    }
                }
            }
        }

        /// <summary>
        /// 获得梁FamilySymbol
        /// </summary>
        /// <param name="document"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public FamilySymbol GetBeamFamilySymbol(Document document, string Name)
        {
            string trussTypeName = Name;
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

            return trussType;
            
        }
        /// <summary>
        /// 获得柱FamilySymbol
        /// </summary>
        /// <param name="document"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public FamilySymbol GetColumnFamilySymbol(Document document, string Name)
        {
            string trussTypeName = Name;
            FamilySymbol trussType = null;
            ElementFilter trussCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Columns);
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

            return trussType;

        }


        /// <summary>
        ///  找到某一个标高
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public Level GetLevel(Document document, string name)
        {
            Level level = null;
            FilteredElementCollector lvlFilter = new FilteredElementCollector(document);
            lvlFilter.OfClass(typeof(Level)).OfCategory(BuiltInCategory.OST_Levels);
            var elementss = from ele in lvlFilter
                            let instance = ele as Level
                            where instance.Name == name
                            select ele;
            List<Level> levels = elementss.Cast<Level>().ToList(); //把LINQ方法找到的Level单元放到Levels集合中
            if (levels.Count >= 1)
            {
                level = levels.First();
            }
            else
            {
                throw new Exception("没有找到name标高");
            }
            if (levels.Count >= 2)
            {
                Level ll = levels[1]; //获取Levels集合中第二个元素的方法
            }

            return level;
        }

        public List<Level> GetLevel(Document document)
        {
            //Level level = null;
            FilteredElementCollector lvlFilter = new FilteredElementCollector(document);
            lvlFilter.OfClass(typeof(Level)).OfCategory(BuiltInCategory.OST_Levels);
            var elementss = from ele in lvlFilter
                            let instance = ele as Level
                            where instance.Name == "标高 2"
                            select ele;
            List<Level> levels = elementss.Cast<Level>().ToList(); //把LINQ方法找到的Level单元放到Levels集合中
            //if (levels.Count >= 1)
            //{
            //    level = levels.First();
            //}
            //else
            //{
            //    throw new Exception("没有找到F2标高");
            //}
            //if (levels.Count >= 2)
            //{
            //    Level ll = levels[1]; //获取Levels集合中第二个元素的方法
            //}

            return levels;
        }



    }
}
