using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace FloorCurve
{
    class FloorSheet
    {
        private SheetInfo sheetInfo;

        private Document doc;

        private ViewPlan ActiveView { get; set; }

        private Floor EntityFloor { get; set; }

        private List<Curve> AllTrussLocatonLine { get; set; }
 
        private List<FamilyInstance> Members { get; set; }
 
        private List<Curve> OutermostlocationLine { get; set; } 

        private List<Curve> GDLocationLine { get; set; }
 
        private List<Curve> OpeningOutLine { get; set; }

        private List<AssemblyInstance> FloorTrussAssemblies;

        private FamilySymbol DetailIndexType { get; set; }

        private XYZ PointOnHoleEdge = null;

        public FloorSheet(Document Doc, SheetInfo sheetInfo)
        {
            this.doc = Doc;
            this.sheetInfo = sheetInfo;
            this.EntityFloor = sheetInfo.Reference as Floor;
            AllTrussLocatonLine = new List<Curve>();
            OutermostlocationLine = new List<Curve>();
            GDLocationLine = new List<Curve>();
            OpeningOutLine = new List<Curve>();
            Members = new List<FamilyInstance>();
        }

        internal void CreatFloorPlan()
        {
            //获取平面图基本信息
            GetBaseInfo();

        }

        private void GetBaseInfo()
        {
            //获取视图
            Transaction transaction = new Transaction(doc, "创建平面视图");
            transaction.Start();
            ElementId levelId = new ElementId(sheetInfo.LevelId);
            ViewerGetter viewGetter = new ViewerGetter(doc);
            ViewPlan viewPlan = viewGetter.GetViewPlan(ViewFamily.FloorPlan, levelId);

            viewPlan.Name = CommonUtil.ChangeNumToChinese(sheetInfo.Levelth) + "层梁结构平面图";
            sheetInfo.Name = viewPlan.Name;
            ActiveView = viewPlan;

            //获取部件、钢带并分类

            List<AssemblyInstance> assemblyInstances = CommonUtil.GetAssemblyInstance(doc, ActiveView);
            FloorTrussAssemblies = new List<AssemblyInstance>();

            List<FamilyInstance> familyInstances = GetFamilyInstanceInView(ActiveView);
            List<ElementId> hideElementIds = new List<ElementId>();
            assemblyInstances.ForEach(x =>
            {
                List<ElementId> elementIds = x.GetMemberIds().ToList();
                bool isOwner = false;
               elementIds.ForEach(y =>
               {
                   FamilyInstance fi = doc.GetElement(y) as FamilyInstance;
                   if (fi != null)
                   {
                       Parameter p = fi.LookupParameter(ParameterProperity.Instance.ParaAssemblyCategory);
                       if (p != null && p.AsString() != string.Empty)
                       {
                           if (fi.LookupParameter(ParameterProperity.Instance.ParaAssemblyCategory)
                               .AsString().Contains(sheetInfo.Levelth.ToString() + "F") ||
                               fi.LookupParameter(ParameterProperity.Instance.ParaAssemblyCategory)
                               .AsString().Contains(sheetInfo.Levelth.ToString() + "J"))
                           {
                               isOwner = true;
                           }
                           else
                           {
                               hideElementIds.Add(fi.Id);
                           }
                       }
                       else
                       {
                           isOwner = false;
                       }
                   }
               });
                FamilyInstance member = null;
                //循环所实例
                if (isOwner)
                {
                    FloorTrussAssemblies.Add(x);

                    foreach (var id in elementIds)
                    {
                        
                        //获取当前实例
                        FamilyInstance fi = doc.GetElement(id) as FamilyInstance;
                        string memberText = fi.LookupParameter(ParameterProperity.Instance.ParaNumber).AsString();
                        string type = memberText.Substring(0, 1);
                        if (fi.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_StructuralFraming) && type == "U")
                        {
                            member = fi;
                            break;
                            
                        }
                    }
                    LocationCurve locCurve = member.Location as LocationCurve;
                    Curve curve = locCurve.Curve;
                    Members.Add(member);
                    AllTrussLocatonLine.Add(curve);
                    if (member.LookupParameter(ParameterProperity.Instance.ParaAssemblyCategory).AsString().Contains("F"))
                    {
                        LocationCurve locCurve1 = member.Location as LocationCurve;
                        Curve curve1 = locCurve1.Curve;
                        OutermostlocationLine.Add(curve1);
                    }
                }
            });
            if (hideElementIds.Count > 0)
            {
                ActiveView.HideElements(hideElementIds);
            }

            familyInstances.ForEach(x =>
            {
                XYZ viewerDirection = ActiveView.ViewDirection;
                LocationCurve locCurve = x.Location as LocationCurve;
                Curve curve = locCurve.Curve;
                GDLocationLine.Add(curve);
            });
            //获取洞口轮廓线
            if (EntityFloor.OpeningsOuterLines.Count > 0)
            {
                List<Polygon2D> floorHoles = new List<Polygon2D>();
                List<Polygon2D> lowplateHoles = new List<Polygon2D>();
                floorHoles = EntityFloor.AnalyticalFloor.OpeningOuterlines;
                floorHoles.ForEach(x =>
                {
                    if (EntityFloor.AnalyticalFloor.LowerPlates.Any(y => y.OuterLines.Area.AreEqual(x.Area)))
                    {
                        lowplateHoles.Add(x);
                    }
                });
                if (lowplateHoles.Count > 0)
                {
                    lowplateHoles.ForEach(x => floorHoles.Remove(x));
                }
                if (floorHoles.Count > 0)
                {
                    floorHoles.First().Edges.ForEach(x => OpeningOutLine.Add(EntityFloor.AnalyticalFloor));
                }
            }
            transaction.Commit();

        }

        private List<FamilyInstance> GetFamilyInstanceInView(ViewPlan ActiveView)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType();
            List<FamilyInstance> instances = collector.Cast<FamilyInstance>().ToList();
            List<FamilyInstance> needFamilyInstances = new List<FamilyInstance>();
            instances.ForEach(x =>
            {
                if (x.LookupParameter(ParameterProperity.Instance.ParaNumber) != null)
                {
                    string name = x.LookupParameter(ParameterProperity.Instance.ParaNumber).AsString();

                    if (name != null)
                    {
                        if (name.Contains("GD"))
                        {
                            needFamilyInstances.Add(x);
                        }
                    }
                }
            });
            return needFamilyInstances;
        }

        private void AddDimensionToView()
        {
            //当前视图的方向
            XYZ upDirection = ActiveView.UpDirection;
            XYZ rightDirection = ActiveView.RightDirection;
            XYZ viewerDirection = ActiveView.ViewDirection;
            //获得当前视图的轴网
            List<Grid> grids = GetGridsInView();
            //短边方向桁架的集合
            List<Curve> mainTruss = new List<Curve>();
            //长边方向桁架的集合
            List<Curve> minorTruss = new List<Curve>();
            //临时比较的集合
            List<Curve> evaluate = new List<Curve>();
            evaluate.Add(AllTrussLocatonLine[0]);
            List<Curve> evaluate2 = new List<Curve>();
            //最外侧短边方向桁架集合
            List<Curve> outerMainTruss = new List<Curve>();
            List<Curve> outerMinorTruss = new List<Curve>();
            //洞口短边方向的桁架集合
            List<Curve> openingMianTruss = new List<Curve>();
            List<Curve> openingMinorTruss = new List<Curve>();

            //尺寸标注的基线
            Curve DimensionMainLine = null;
            Curve DimensionMainLineOpposite = null;
            Curve DimensionMinorLine = null;
            Curve DimensionMinorLineOpposite = null;
            Curve DimensionOpeningBaseLine = null;
            //尺寸标注的基准参考
            List<XYZ> firstlocationpoints = new List<XYZ>();
            List<XYZ> secondlocationpoints = new List<XYZ>();
            List<XYZ> thirdlocationpoints = new List<XYZ>();
            List<XYZ> Alocationpoints = new List<XYZ>();
            List<XYZ> Blocationpoints = new List<XYZ>();
            List<Curve> gridMainTruss = new List<Curve>();
            List<Curve> gridMinorTruss = new List<Curve>();
            List<XYZ> openingLocationPoints = new List<XYZ>();

            //尺寸标准的偏移方向
            OffsetDirection type1 = OffsetDirection.Left;
            OffsetDirection type1Opposite = OffsetDirection.Right;
            OffsetDirection type2 = OffsetDirection.Down;
            OffsetDirection type2Opposite = OffsetDirection.Up;
            OffsetDirection type3 = OffsetDirection.Left;

            AllTrussLocatonLine.ForEach(x =>
            {
                XYZ dir1 = (x as Line).Direction.Normalize();
                XYZ dir2 = (evaluate[0] as Line).Direction.Normalize();
                if (dir1.IsAlmostEqualTo(dir2) || dir1.IsAlmostEqualTo(-dir2))
                {
                    evaluate.Add(x);
                 }
                else
                {
                    evaluate2.Add(x);
                }
            });
            if (evaluate.Count > evaluate2.Count)
            {
                mainTruss = evaluate;
                minorTruss = evaluate2;
            }
            else
            {
                mainTruss = evaluate2;
                minorTruss = evaluate;
            }

            XYZ mainTrussDirection = (mainTruss[0] as Line).Direction.Normalize();
            OutermostlocationLine.ForEach(x =>
            {
                XYZ dirc = (x as Line).Direction.Normalize();
                if (dirc.IsAlmostEqualTo(mainTrussDirection) || dirc.IsAlmostEqualTo(-mainTrussDirection))
                {
                    outerMainTruss.Add(x);
                }
                else
                {
                    outerMinorTruss.Add(x);
                }
            });
            if (OpeningOutLine.Count >0)
            {
                OpeningOutLine.ForEach(x =>
                {
                    XYZ dirc = (x as Line).Direction;
                    if (dirc.IsAlmostEqualTo(mainTrussDirection) || dirc.IsAlmostEqualTo(-mainTrussDirection))
                    {
                        openingMianTruss.Add(x);
                    }
                    else
                    {
                        openingMinorTruss.Add(x);
                    }
                });
            }

            openingMinorTruss.ForEach(x => openingLocationPoints.Add(x.GetEndPoint(0)));
            mainTruss = DistinctCollinear(mainTruss);
            mainTruss.ForEach(x => firstlocationpoints.Add(x.GetEndPoint(0)));
            grids.ForEach(x =>
            {
                XYZ direction = (x.Curve as Line).Direction.Normalize();
                if (direction.IsAlmostEqualTo(mainTrussDirection) || direction.IsAlmostEqualTo(-mainTrussDirection))
                {
                    gridMinorTruss.Add(x.Curve);
                }
                else
                {
                    gridMinorTruss.Add(x.Curve);
                }
            });
            if (mainTrussDirection.IsAlmostEqualTo(rightDirection) || mainTrussDirection.IsAlmostEqualTo(-rightDirection))
            {
                if (openingMinorTruss.Count > 0)
                {
                    openingMianTruss = openingMianTruss.OrderBy(x => x.Evaluate(0.5, true).Y).ToList();
                    DimensionOpeningBaseLine = openingMianTruss.First();
                }
                outerMainTruss = outerMainTruss.OrderBy(x => x.Evaluate(0.5, true).Y).ToList();
                DimensionMinorLine = outerMainTruss.First();
                DimensionMinorLineOpposite = outerMainTruss.Last();
                outerMinorTruss = outerMinorTruss.OrderBy(x => x.Evaluate(0.5, true).X).ToList();
                DimensionMainLine = outerMinorTruss.First();
                DimensionMainLineOpposite = outerMinorTruss.Last();
                gridMainTruss = gridMainTruss.OrderBy(x => x.Evaluate(0.5, true).Y).ToList();
                gridMinorTruss = gridMinorTruss.OrderBy(x => x.Evaluate(0.5, true).X).ToList();
                type1 = OffsetDirection.Left;
                type1Opposite = OffsetDirection.Right;
                type2 = OffsetDirection.Down;
                type2Opposite = OffsetDirection.Up;
                type3 = OffsetDirection.Up;
            }
            else
            {
                if (openingMinorTruss.Count > 0)
                {
                    openingMianTruss = openingMianTruss.OrderBy(x => x.Evaluate(0.5, true).Y).ToList();
                    DimensionOpeningBaseLine = openingMianTruss.First();
                }
                outerMainTruss = outerMainTruss.OrderBy(x => x.Evaluate(0.5, true).Y).ToList();
                DimensionMinorLine = outerMainTruss.Last();
                DimensionMinorLineOpposite = outerMainTruss.First();
                outerMinorTruss = outerMinorTruss.OrderBy(x => x.Evaluate(0.5, true).X).ToList();
                gridMainTruss = gridMainTruss.OrderBy(x => x.Evaluate(0.5, true).X).ToList();
                gridMinorTruss = gridMinorTruss.OrderBy(x => x.Evaluate(0.5, true).Y).ToList();
                DimensionMainLine = outerMinorTruss.Last();
                DimensionMainLineOpposite = outerMinorTruss.First();
                type1 = OffsetDirection.Up;
                type1Opposite = OffsetDirection.Down;
                type2 = OffsetDirection.Right;
                type2Opposite = OffsetDirection.Left;
                type3 = OffsetDirection.Right;
            }

            //添加洞口表示符
            if (openingMianTruss.Count !=0)
            {
                Curve targetOutLine =
                    outerMinorTruss.OrderBy(x => x.Distance(DimensionOpeningBaseLine.Evaluate(0.5, true))).First();
                List<XYZ > points1 = new List<XYZ>();
                List<XYZ> points2 = new List<XYZ>();
                PointOnHoleEdge = DimensionOpeningBaseLine.GetEndPoint(0);
                points1.Add(DimensionOpeningBaseLine.GetEndPoint(0));
                points2.Add(DimensionOpeningBaseLine.GetEndPoint(1));
                points1 = points1.OrderBy(x => targetOutLine.Distance(x)).ToList();
                Curve line1 = Line.CreateBound(points1[0], points1[1]);
                XYZ point = line1.Evaluate(0.75, true);
                List<Curve > openingArrangeDirCopy = new List<Curve>(openingMianTruss);
                openingArrangeDirCopy.Remove(DimensionOpeningBaseLine);
                points2.Add(openingArrangeDirCopy[0].GetEndPoint(0));
                points2.Add(openingArrangeDirCopy[0].GetEndPoint(1));
                points2 = points2.OrderBy(x => targetOutLine.Distance(x)).ToList();
                Line line2 = Line.CreateBound(points1[1], points2[1]);
                point = point + line2.Direction*line2.Length/4;
                Curve needLine1 = Line.CreateBound(points1[0], point);
                Curve needLine2 = Line.CreateBound(point, points2[1]);
                DetailCurve detailCurve1 = doc.Create.NewDetailCurve(ActiveView, needLine1);
                DetailCurve detailCurve2 = doc.Create.NewDetailCurve(ActiveView, needLine2);

            }
            //轴网尺寸标注
            gridMainTruss.ForEach(x => secondlocationpoints.Add(x.Evaluate(0.5,true)));
            thirdlocationpoints.Add(gridMainTruss.First().Evaluate(0.5,true));
            thirdlocationpoints.Add(gridMainTruss.Last().Evaluate(0.5, true));

            gridMinorTruss.ForEach(x => Alocationpoints.Add(x.Evaluate(0.5, true)));
            Blocationpoints.Add(gridMinorTruss.First().Evaluate(0.5, true));
            Blocationpoints.Add(gridMinorTruss.Last().Evaluate(0.5,true));

            DimensionUtil util = new DimensionUtil(doc, ActiveView);
            ReferenceArray array = util.CreateReferenceArray(firstlocationpoints, DimensionMainLine);
            ReferenceArray secondArray = util.CreateReferenceArray(secondlocationpoints, DimensionMainLine);
            ReferenceArray thirdArray = util.CreateReferenceArray(thirdlocationpoints, DimensionMainLine);
            ReferenceArray secondArrayOppsite = util.CreateReferenceArray(secondlocationpoints, DimensionMainLineOpposite);
            ReferenceArray thirdArrayOpposite = util.CreateReferenceArray(thirdlocationpoints, DimensionMainLineOpposite);

            ReferenceArray AArray = util.CreateReferenceArray(Alocationpoints, DimensionMinorLine);
            ReferenceArray BArray = util.CreateReferenceArray(Blocationpoints, DimensionMinorLine);
            ReferenceArray AArrayOpposite = util.CreateReferenceArray(Alocationpoints, DimensionMinorLineOpposite);
            ReferenceArray BArrayOpposite = util.CreateReferenceArray(Blocationpoints, DimensionMinorLineOpposite);

            util.CreateDimension(array, DimensionMainLine, type1, 1600);
            util.CreateDimension(secondArray, DimensionMainLine, type1, 2400);
            util.CreateDimension(thirdArray, DimensionMainLine, type1, 3200);
            util.CreateDimension(secondArrayOppsite, DimensionMainLineOpposite, type1Opposite, 2400);
            util.CreateDimension(thirdArrayOpposite, DimensionMainLineOpposite, type1Opposite, 3200);

            util.CreateDimension(AArray, DimensionMinorLine, type2, 2400);
            util.CreateDimension(BArray, DimensionMinorLine, type2, 3200);
            util.CreateDimension(AArrayOpposite, DimensionMinorLineOpposite, type2Opposite, 2400);
            util.CreateDimension(BArrayOpposite, DimensionMinorLineOpposite, type2Opposite, 3200);
            if (DimensionOpeningBaseLine != null)
            {
                ReferenceArray openingArray = util.CreateReferenceArray(openingLocationPoints, DimensionOpeningBaseLine);
                util.CreateDimension(openingArray, DimensionOpeningBaseLine, type3, 600);
            }

        }

 

        private List<Curve> DistinctCollinear(List<Curve> source)
        {
            List<Curve> remove = new List<Curve>();
            for (int i = 0; i < source.Count-2; i++)
            {
                for (int j = i+1; j < source.Count-1; j++)
                {
                    Curve target1 = source[i];
                    Curve target2 = source[j];
                    if (target2.Distance(target1.GetEndPoint(0)) < 1e-2 || target2.Distance(target1.GetEndPoint(1)) < 1e-2)
                    {
                        remove.Add(target1);
                    }
                }
            }
            remove.ForEach(x => source.Remove(x));
            return remove;
        }

        private List<Grid> GetGridsInView()
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(Grid)).WhereElementIsNotElementType();
            List<Grid> grids = collector.Cast<Grid>().ToList();
            return grids;
        }


        private void GetFamilyInstanceLocation(FamilyInstance x, ref XYZ textLocation, ref XYZ direction,
            ref string text)
        {
            XYZ viewDirection = ActiveView.ViewDirection;
            LocationCurve locCurve = x.Location as LocationCurve;
            Curve curve = locCurve.Curve;
            XYZ p1 = curve.GetEndPoint(0);
            XYZ p2 = curve.GetEndPoint(1);
            direction =  new XYZ(p2.X - p1.X , p2.Y - p1.Y ,0).Normalize();
            XYZ offsetDirection = direction.CrossProduct(viewDirection);
            text = x.LookupParameter(ParameterProperity.Instance.ParaNumber).AsString();
            textLocation = curve.Evaluate(0.5, true) + offsetDirection*20/304.8;
        }


        private void SetViewCropBox(ViewPlan viewPlan, List<Curve> curves)
        {
            Transaction trans = new Transaction(doc, "ssss");
            trans.Start();
            List<XYZ > nodes = new List<XYZ>();
            List<XYZ> localNodes = new List<XYZ>();
            List<XYZ > orderedNodes = new List<XYZ>();
            foreach (var curve in curves)
            {
                nodes.Add(curve.GetEndPoint(0));
                nodes.Add(curve.GetEndPoint(1));
            }
            Transform transform = viewPlan.CropBox.Transform.Inverse;
            nodes.ForEach(x => localNodes.Add(transform.OfPoint(x)));
            List<double > xArray = new List<double>();
            List<double > yArray = new List<double>();
            foreach (var node in localNodes)
            {
                
                xArray.Add(node.X);
                yArray.Add(node.Y);
            }
            xArray = xArray.OrderBy(x => x).ToList();
            yArray = yArray.OrderBy(x => x).ToList();
            double minX = xArray.First();
            double maxX = xArray.Last();
            double minY = yArray.First();
            double maxY = yArray.Last();
            BoundingBoxXYZ boungBoxXyz = viewPlan.CropBox;
            viewPlan.CropBoxActive = true;

            boungBoxXyz.Max = new XYZ(maxX + 2400/304.8, maxY + 2400/304.8, boungBoxXyz.Max.Z);
            boungBoxXyz.Min = new XYZ(minX - 2400 / 304.8, minY - 2400 / 304.8, boungBoxXyz.Min.Z);
            viewPlan.CropBox = boungBoxXyz;
            viewPlan.CropBoxVisible = false;
            trans.Commit();
        }

    }

    
}
