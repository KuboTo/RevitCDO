using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
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
    public class RoofBuildercs
    {
        public Document Doc { get; set; }

        private BeamInstanceGetter BeamManager { get; set; }

        private ColumnInstanceGetter ColumnManager { get; set; }

        private BraceInstanceGetter BraceManager { get; set; }

        private XYZ Direction;

        public ICollection<ElementId> ElementIdsInOneRoofPurline { get; set; } 

        private Random random = null;

        private AssemblyInstanceCreator assemblyInstances { get; set; }

        private string ChordFamilyName = RoofProperity.Instance.ChordFamilyName;

        private Autodesk.Revit.UI.UIDocument UiDocument;

        /// <summary>
        /// 屋顶的构建
        /// </summary>
        /// <param name="doc"></param>
        public RoofBuildercs(Document doc)
        {
            this.Doc = doc;
            UiDocument = new UIDocument(Doc);
            assemblyInstances = new AssemblyInstanceCreator(Doc);
        }

        public void Build(House house)
        {
            //屋架个数
            double roofTrussCount = 0;
            double purlinCount = 0;
            List<AnalyticalRoof> analyticalRoofs = new List<AnalyticalRoof>();

            house.Roofs.ForEach(x =>
            {
                analyticalRoofs.AddRange(x.AnalyticalRoofs);

                x.AnalyticalRoofs.ForEach(y =>
                {
                    roofTrussCount += y.RoofTrusses.Count;
                    y.RoofPurlins.ForEach(z =>
                    {
                        purlinCount += z.Purlins.Count;
                    });
                });

            });
            //生成一个随机变量，通过修改杆件的微小长度来区别部件
            random = new Random();


            //初始化族
            InitialFamilySymbol();

            int num = 0;
            int nn = 0;

            foreach (AnalyticalRoof roof in analyticalRoofs)
            {
                foreach (RoofTruss roofTruss in roof.RoofTrusses)
                {
                    StatusBarHandler.SetText(string.Format("屋架 ：{0} 已创建：{1}", roofTrussCount, num + 1));
                    num ++;
                    roofTruss.GlobalNumber = num;
                    this.Build(roofTruss);
                }

                //构建当前檩条
                foreach (RoofPurlin roofPurlin in roof.RoofPurlins)
                {
                    foreach (Purlin purlin in roofPurlin.Purlins)
                    {
                        StatusBarHandler.SetText(string.Format("檩条 ：{0} 已创建：{1}", roofTrussCount, num + 1));
                        num++;
                        purlin.GlobalNumber = num;
                        this.Build(purlin);
                    }
                    
                }

                using (Transaction transaction = new Transaction(Doc))
                {
                    //创建平面外支撑
                    transaction.Start("创建平面外支撑");
                    foreach (Member member in roof.Members)
                    {
                        FamilyInstance fi = BuildOutPlaneBraces(member);

                        if (fi != null)
                        {
                            SharedParameter.AddMemberSharedParameter(fi, member);
                        }
                    }
                    transaction.Commit();

                }

            }

        }

        private FamilyInstance BuildOutPlaneBraces(Member member)
        {
            try
            {
                Autodesk.Revit.DB.Curve bracCurve = CoordConverter.Line3d2Curve(member.Line);
                double orientation = CoordConverter.TransformBraceOrientation(member.Orientation, -member.Line.Direction);
                FamilyInstance braceInstance = BraceManager.CreateInstance(GetRoofBaseLevel(member.BaseLevelId),
                    bracCurve, orientation, member.StartExtension, member.EndExtension);

                member.ID = braceInstance.Id.IntegerValue;
                return braceInstance;
            }
            catch (Exception ex)
            {
                
                throw new Exception("杆件创建失败" + ex.Message);
            }
        }

        private Level GetRoofBaseLevel(int p)
        {
            ElementId levelId = new ElementId(p);
            Level roofLevel = Doc.GetElement(levelId) as Level;
            ;
            return roofLevel;
        }

        /// <summary>
        /// 构建一个檩条
        /// </summary>
        /// <param name="purlin"></param>
        private void Build(Purlin purlin)
        {
            throw new NotImplementedException();
        }

        private void Build(RoofTruss roofTruss)
        {
            //事务
            Transaction transaction = new Transaction(Doc);
            //创建每一品屋架
            transaction.Start("创建每一品屋架中的杆件");
            FailureHandlingOptions failOption = transaction.GetFailureHandlingOptions();
            failOption.SetFailuresPreprocessor(new InaccurateBraceFailyre());
            transaction.SetFailureHandlingOptions(failOption);

            ElementIdsInOneRoofPurline = new List<ElementId>();
            Direction = roofTruss.TrussDirection;
            foreach (Member mb in roofTruss.Members)
            {
                FamilyInstance fi = null;
                if (mb is Beam)
                {
                    fi = this.BuildBeams(mb);
                }
                else if (mb is Column)
                {
                    fi = this.BuildColumns(mb);
                }
                else if (mb is Brace)
                {
                    fi = this.BuildBraces(mb);
                }

                if (fi != null)
                {
                    SharedParameter.AddMemberSharedParameter(fi, mb);
                    ElementIdsInOneRoofPurline.Add(fi.Id);

                }
            }
            RefreshModel.Refresh(Doc);
            transaction.Commit();

            //创建部件
            if (ElementIdsInOneRoofPurline.Count > 0)
            {
                string strName = roofTruss.Name;
                AssemblyInstance assemblyInstance =
                    assemblyInstances.CreateRoofAssemblyInstance(ElementIdsInOneRoofPurline, strName);
                CommonMethod.AppendAssemblyInfo(Doc, assemblyInstance, strName);
                SheetCreator.GetInstance(Doc)
                    .CreateAssemblySheet(Doc, roofTruss, assemblyInstance, this.ChordFamilyName);

            }


        }

        private FamilyInstance BuildBraces(Member mb)
        {
            try
            {
                Curve braceCurve = CoordConverter.Line3d2Curve(mb.Line);
                double orientation = CoordConverter.TransformBraceOrientation(mb.Orientation);
                FamilyInstance braceInstance = BraceManager.CreateInstance(GetRoofBaseLevel(mb.BaseLevelId), braceCurve,
                    orientation, mb.StartExtension, mb.EndExtension);

                mb.ID = braceInstance.Id.IntegerValue;
                return braceInstance;
            }
            catch (Exception ex)
            {

                throw new Exception("杆件创建失败" + ex.Message);
            }
        }

        private FamilyInstance BuildColumns(Member mb)
        {
            Curve columnCurve = CoordConverter.Line3d2Curve(mb.Line);
            double orientation = CoordConverter.TransformBraceOrientation(mb.Orientation);
            FamilyInstance columnInstance = null;
            if (mb.TopLevelId <=0)
            {
                columnInstance = ColumnManager.CreateInstance(GetRoofBaseLevel(mb.BaseLevelId), columnCurve, orientation,
                    mb.StartExtension, mb.EndExtension);

            }
            else
            {
                columnInstance = ColumnManager.CreateInstance(GetRoofBaseLevel(mb.BaseLevelId),
                    GetRoofBaseLevel(mb.TopLevelId), columnCurve, orientation, mb.StartExtension, mb.EndExtension);

            }
            mb.ID = columnInstance.Id.IntegerValue;
            return columnInstance;
        }

        private FamilyInstance BuildBeams(Member mb)
        {
            try
            {
                Curve beamCurve = CoordConverter.Line3d2Curve(mb.Line)

                Double rand = random.NextDouble()/100/304.8;
                XYZ direction = ((Line) beamCurve).Direction;
                beamCurve = Line.CreateBound(beamCurve.GetEndPoint(0) + rand*direction, beamCurve.GetEndPoint(1));

                double orientation = CoordConverter.TransformBraceOrientation(mb.Orientation);
                FamilyInstance beamInstance = BeamManager.CreateInstance(GetRoofBaseLevel(mb.BaseLevelId), beamCurve,
                    orientation, mb.StartExtension, mb.EndExtension);

                mb.ID = beamInstance.Id.IntegerValue;
                return beamInstance;
            }
            catch (Exception ex)
            {
                
                throw new Exception("杆件创建失败"+ex.Message);
            }
        }

        private void InitialFamilySymbol()
        {
            Transaction transaction = new Transaction(Doc);
            transaction.Start("族激活");
            this.BeamManager = new BeamInstanceGetter(Doc, RoofProperity.Instance.ChordFamilyName, RoofProperity.Instance.FamilyTypeName);
            this.ColumnManager = new ColumnInstanceGetter(Doc, RoofProperity.Instance.ColumnFamilyName, RoofProperity.Instance.FamilyTypeName);
            this.BraceManager = new BraceInstanceGetter(Doc, RoofProperity.Instance.WebFamilyName, RoofProperity.Instance.FamilyTypeName);
            transaction.Commit();
        }




    }

    public class SheetCreator
    {
        public static SheetCreator GetInstance(Document Doc)
        {
            throw new NotImplementedException();
        }

        public void CreateAssemblySheet(Document Doc, RoofTruss roofTruss, AssemblyInstance assemblyInstance, string p)
        {
            throw new NotImplementedException();
        }
    }

    public class CommonMethod
    {
        internal static void AppendAssemblyInfo(Document Doc, AssemblyInstance assemblyInstance, string strName)
        {
            throw new NotImplementedException();
        }
    }

    public class RefreshModel
    {
        internal static void Refresh(Document Doc)
        {
            throw new NotImplementedException();
        }
    }

    public class SharedParameter
    {
        internal static void AddMemberSharedParameter(FamilyInstance fi, Member member)
        {
            throw new NotImplementedException();
        }
    }

    public class CoordConverter
    {
        internal static Curve Line3d2Curve(object p)
        {
            throw new NotImplementedException();
        }

        public static double TransformBraceOrientation(XYZ orientation, XYZ  xyz)
        {
            throw new NotImplementedException();
        }

        internal static double TransformBraceOrientation(XYZ xYZ)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusBarHandler
    {
        internal static void SetText(string p)
        {
            throw new NotImplementedException();
        }
    }

    public class RoofProperity
    {
        public string FamilyTypeName;
        public static RoofProperity Instance { get; set; }
        public string ChordFamilyName;
        public string ColumnFamilyName;
        public string WebFamilyName;

    }
}
