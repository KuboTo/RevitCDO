using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Document = Autodesk.Revit.DB.Document;

namespace FloorCurve
{
    /*
     * 定义一个族创建器，创建面族
     */

    /// <summary>
    /// 用于创建一个常规模型
    /// </summary>
    public class FaceFamilyCreator
    {
        private double familylen = 0;

        /// <summary>
        /// 当前的文档对象
        /// </summary>
        public UIApplication Application { get; set; }

        /// <summary>
        /// 当前文档
        /// </summary>
        public Autodesk.Revit.DB.Document Doc { get; set; }

        /// <summary>
        /// 当前族的文档
        /// </summary>
        public Autodesk.Revit.DB.Document FamilyDoc { get; set; }

        /// <summary>
        /// 当前族的类别
        /// </summary>
        public FamilyType FamType { get; set; }



        /// <summary>
        /// 构造函数，初始化当前面族创建
        /// </summary>
        /// <param name="app"></param>
        public FaceFamilyCreator(UIApplication app)
        {
            this.Application = app;
            this.Doc = this.Application.ActiveUIDocument.Document;
        }

        /// <summary>
        /// 构造函数，初始化当前面族创建
        /// </summary>
        /// <param name="doc"></param>
        public FaceFamilyCreator(Autodesk.Revit.DB.Document doc)
        {
            this.Doc = doc;
        }

        #region 未完成

        ///// <summary>
        ///// 面族的创建
        ///// </summary>
        ///// <returns></returns>
        //public FamilySymbol Create()
        //{
        //    Transaction trans = new Transaction(Doc);
        //    trans.Start("创建面族");
        //    Document familyDoc =
        //        Application.Application.NewFamilyDocument(
        //            @"C:\ProgramData\Autodesk\RVT 2015\Family Templates\Chinese\公制常规模型.rft");
        //    FamilyManager manage = familyDoc.FamilyManager;
        //    //创建拉伸平面
        //    CurveArrArray curveArray = this.CreateCurves();
        //    //创建参考平面
        //    SketchPlane skplane = this.GetSketchPlane(familyDoc);
        //    //创建平面拉伸

        //}

        #endregion

        /// <summary>
        /// 面族创建
        /// </summary>
        /// <returns></returns>
        public Family Create()
        {
            string familyTempPath = Doc.Application.FamilyTemplatePath;
            //模型样板文件
            string modelTempPath = familyTempPath + "\\基于线的公制常规模型.rft";
            //要创建的族
            Family family = null;

            FamilyDoc = Doc.Application.NewFamilyDocument(modelTempPath);
            Transaction trans = new Transaction(FamilyDoc);
            trans.Start("创建面参考族");

            //执行当前创建
            try
            {
                FamilyManager manager = FamilyDoc.FamilyManager;

                //创建族类别
                this.CreateFamilyType(manager);
                //获取族创建器
                FamilyItemFactory familyCreate = FamilyDoc.FamilyCreate;
                //创建拉伸体
                Extrusion extrusion = this.CreateExtrusion(manager, familyCreate);

                //声明参考对象
                Reference leftRef = null,
                    rightRel = null,
                    topRef = null,
                    bottomRef = null,
                    frontRef = null,
                    backRef = null;
                //创建参考对象
                this.CreateReference(extrusion, out leftRef, out rightRel, out topRef, out bottomRef, out frontRef, out backRef); 
                
                //判断当前所有的参考面信息
                if (leftRef != null && rightRel != null && topRef != null && bottomRef != null && frontRef != null && backRef != null )
                {
                    //创建标尺详细
                    this.CreateLengthParameter(manager, extrusion, leftRef, rightRel);
                    this.CreateWidthParameter(manager, extrusion, frontRef, backRef);
                    this.CreateHeightParameter(manager, extrusion, bottomRef, topRef);
                }
                //事务提交
                trans.Commit();
                //向文档中加载族实例
                family = FamilyDoc.LoadFamily(Doc);

                //保存选项
                SaveAsOptions saveAsOptions = new SaveAsOptions();
                //重写存在的文档
                saveAsOptions.OverwriteExistingFile = true;
                string directoryName = this.CreateExportFile("face");
                //保存族文件
                FamilyDoc.SaveAs(directoryName + "face.rft", saveAsOptions);
                //关闭族文档
                FamilyDoc.Close();
            }
            catch (Exception ex)
            {

                string message = ex.Message;
                trans.RollBack();
            }
            return family;
        }

        /// <summary>
        /// 创建导出目录
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string CreateExportFile(string filename)
        {
            string currentPathName = "";
            if (Doc.PathName != null && Doc.PathName != "")
            {
                currentPathName = Path.GetDirectoryName(Doc.PathName);
                currentPathName = currentPathName + "\\" + Path.GetFileNameWithoutExtension(Doc.PathName);
            }
            else
            {
                currentPathName = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                currentPathName = currentPathName + "\\temp";

            }
            DirectoryInfo dir = null;
            int i = 1;
            string originPathName = currentPathName;

            ////直到不存在
            while (Directory.Exists(currentPathName))
            {
                currentPathName = originPathName + i;
                i++;

            }
            Directory.CreateDirectory(currentPathName);
            string path = currentPathName + "\\" + filename + "\\";
            dir = Directory.CreateDirectory(path);

            //获取当前的目录信息
            return dir.FullName;
        }

        /// <summary>
        /// 创建高度参数
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="extrusion"></param>
        /// <param name="bottomRef"></param>
        /// <param name="topRef"></param>
        private void CreateHeightParameter(FamilyManager manager, Extrusion extrusion, Reference bottomRef, Reference topRef)
        {
            //创建中心位置参考面
            XYZ bottomPnt = new XYZ(0, 0, -familylen/2);
            XYZ topPnt = new XYZ(0, 0, familylen/2);
            //对象引用数组
            ReferenceArray rfArray = new ReferenceArray();
            rfArray.Append(bottomRef);
            rfArray.Append(topRef); 
            //添加参照标高面
            View currentView = this.GetView("参照标高");
            Line dimension = Line.CreateBound(bottomPnt, topPnt);
            Dimension lengthDim = FamilyDoc.FamilyCreate.NewLinearDimension(currentView, dimension, rfArray);
            lengthDim.IsLocked = true;
            //添加参数
            FamilyParameter para = manager.AddParameter("heigth", BuiltInParameterGroup.PG_GEOMETRY, ParameterType.Length, true);
            //关联参数和标尺
            AssociateParameterToDimension(lengthDim, para);
        }

        /// <summary>
        /// 创建宽度参数
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="extrusion"></param>
        /// <param name="frontRef"></param>
        /// <param name="backRef"></param>
        private void CreateWidthParameter(FamilyManager manager, Extrusion extrusion, Reference frontRef, Reference backRef)
        {
            //创建中心位置参考面
            XYZ frontPnt = new XYZ(familylen, -familylen, 0);
            XYZ backPnt = new XYZ(familylen, familylen, 0);
            //对象引用数组
            ReferenceArray rfArray = new ReferenceArray();
            rfArray.Append(frontRef);
            rfArray.Append(backRef);
            //添加参照标高面
            View currentView = this.GetView("参照标高");
            Line dimension = Line.CreateBound(frontPnt, backPnt);
            Dimension lengthDim = FamilyDoc.FamilyCreate.NewLinearDimension(currentView, dimension, rfArray);
            lengthDim.IsLocked = true;
            //添加参数
            FamilyParameter para = manager.AddParameter("width", BuiltInParameterGroup.PG_GEOMETRY, ParameterType.Length, true);
            //关联参数和标尺
            AssociateParameterToDimension(lengthDim, para);

        }

        /// <summary>
        /// 创建长度参数
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="extrusion"></param>
        /// <param name="leftRef"></param>
        /// <param name="rightRel"></param>
        private void CreateLengthParameter(FamilyManager manager, Extrusion extrusion, Reference leftRef, Reference rightRel)
        {
            // 创建中心位置的参考面
            XYZ leftPnt = new XYZ(-familylen, familylen, 0);
            XYZ rightPnt = new XYZ(familylen, familylen, 0);
            //对象引用数组
            ReferenceArray rfArray = new ReferenceArray();
            rfArray.Append(leftRef);
            rfArray.Append(rightRel);
            //添加参照标高面
            View currentView = this.GetView("参照标高");
            Line dimension = Line.CreateBound(leftPnt, rightPnt);
            Dimension lengthDim = FamilyDoc.FamilyCreate.NewLinearDimension(currentView, dimension, rfArray);
            lengthDim.IsLocked = true;
            //添加参数
            FamilyParameter para = manager.AddParameter("length", BuiltInParameterGroup.PG_GEOMETRY, ParameterType.Length, true);
            //关联参数和标尺
            AssociateParameterToDimension(lengthDim, para);

        }

        private void AssociateParameterToDimension(Dimension lengthDim, FamilyParameter para)
        {
            FamilyParameter currentParam = lengthDim.FamilyLabel;
            if (currentParam != null)
            {
                if (currentParam.Definition.ParameterType != para.Definition.ParameterType)
                {
                    return;
                }
            }
            lengthDim.FamilyLabel = para;
        }

        /// <summary>
        /// 获得相应的视图
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        private View GetView(string viewName)
        {
            FilteredElementCollector viewCollector = new FilteredElementCollector(FamilyDoc);
            viewCollector.OfClass(typeof (View));
            View view = viewCollector.First(x => x.Name == viewName) as View;
            return view;
        }

        /// <summary>
        /// 创建约束
        /// </summary>
        /// <param name="extrusion"></param>
        /// <param name="leftRef"></param>
        /// <param name="rightRel"></param>
        /// <param name="topRef"></param>
        /// <param name="bottomRef"></param>
        /// <param name="frontRef"></param>
        /// <param name="backRef"></param>
        private void CreateReference(Extrusion extrusion, out Reference leftRef, out Reference rightRel, out Reference topRef, out Reference bottomRef, out Reference frontRef, out Reference backRef)
        {
            leftRef = null;
            rightRel = null;
            topRef = null;
            bottomRef = null;
            frontRef = null;
            backRef = null;
            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement gelm = extrusion.get_Geometry(opt);
            foreach (GeometryObject gob in gelm)
            {
                if (gob is Solid)
                {
                    Solid s = gob as Solid;
                    foreach (Face face in s.Faces)
                    {
                        if (face.ComputeNormal(new UV()).IsAlmostEqualTo(new XYZ(-1,0,0)))
                        {
                            leftRef = face.Reference;
                            continue;
                            
                        }
                        if (face.ComputeNormal(new UV()).IsAlmostEqualTo(new XYZ(1, 0, 0)))
                        {
                            rightRel = face.Reference;
                            continue;

                        }
                        if (face.ComputeNormal(new UV()).IsAlmostEqualTo(new XYZ(0, 0, 1)))
                        {
                            topRef = face.Reference;
                            continue;

                        }
                        if (face.ComputeNormal(new UV()).IsAlmostEqualTo(new XYZ(0, 0, -1)))
                        {
                            bottomRef = face.Reference;
                            continue;

                        }
                        if (face.ComputeNormal(new UV()).IsAlmostEqualTo(new XYZ(0, 1, 0)))
                        {
                            frontRef = face.Reference;
                            continue;

                        }
                        if (face.ComputeNormal(new UV()).IsAlmostEqualTo(new XYZ(0, -1, 0)))
                        {
                            backRef = face.Reference;
                            continue;

                        }
                        
                    }
                }
            }
        }

        /// <summary>
        /// 创建一个拉伸体
        /// </summary>
        /// <param name="fm"></param>
        /// <param name="familyCreate"></param>
        /// <returns></returns>
        private Extrusion CreateExtrusion(FamilyManager fm, FamilyItemFactory familyCreate)
        {
            FamilyParameter fp = fm.get_Parameter("长度");
            if (fp.StorageType == StorageType.Double)
            {
                familylen = FamType.AsDouble(fp).Value;

            }
            //获取参照面
            XYZ v1 = new XYZ(0,0,0);
            XYZ v2 = new XYZ(0,familylen,0);
            XYZ v3 = new XYZ(familylen, familylen, 0);
            XYZ v4 = new XYZ(familylen,0,0);
            Line l1 = Line.CreateBound(v1, v2);
            Line l2 = Line.CreateBound(v2, v3);
            Line l3 = Line.CreateBound(v3, v4);
            Line l4 = Line.CreateBound(v4, v1);
            CurveArrArray arry = new CurveArrArray();
            CurveArray ary = new CurveArray();
            ary.Append(l1);
            ary.Append(l2);
            ary.Append(l3);
            ary.Append(l4);
            arry.Append(ary);
            //创建参考平面
            SketchPlane skplane = this.GetSketchPlane(FamilyDoc);
            //创建拉伸体
            Extrusion extrusion = familyCreate.NewExtrusion(true, arry, skplane, familylen);

            return extrusion;

        }

        /// <summary>
        /// 用于创建族的类别
        /// </summary>
        /// <param name="manager"></param>
        private void CreateFamilyType(FamilyManager manager)
        {
            FamilyType ft = manager.CurrentType;
            if (ft.Name == null)
            {
                FamType = manager.NewType("面参照");
            }
            else if (ft.Name == " ")
            {
                manager.RenameCurrentType("面参照");
            }
            else
            {
                FamType = ft;
            }
        }

        /// <summary>
        /// 获取参考面
        /// </summary>
        /// <returns></returns>
        private SketchPlane GetSketchPlane(Document familyDoc)
        {
            //由于常规模型族只有一个参考平面，则过滤出当前的参考面
            FilteredElementCollector filtered = new FilteredElementCollector(familyDoc);
            filtered.OfClass(typeof (SketchPlane));
            SketchPlane sketchPlane = filtered.First(x => x.Name == "参考标高") as SketchPlane;
            return sketchPlane;
        }
        
        /// <summary>
        /// 创建一个拉伸平面
        /// </summary>
        /// <returns></returns>
        private CurveArrArray CreateCurves()
        {
            double lenPlane = 300/304.8;
            XYZ v1 = new XYZ(-lenPlane,-lenPlane, 0);
            XYZ v2 = new XYZ(lenPlane, -lenPlane, 0);
            XYZ v3 = new XYZ(lenPlane, lenPlane, 0);
            XYZ v4 = new XYZ(-lenPlane, lenPlane, 0);
            Line l1 = Line.CreateBound(v1, v2);
            Line l2 = Line.CreateBound(v2, v3);
            Line l3 = Line.CreateBound(v3, v4);
            Line l4 = Line.CreateBound(v4, v1);
            CurveArrArray arry = new CurveArrArray();
            CurveArray ary = new CurveArray();
            ary.Append(l1);
            ary.Append(l2);
            ary.Append(l3);
            ary.Append(l4);
            arry.Append(ary);
            return arry;
        }

        /// <summary>
        /// 获得指定的参考
        /// </summary>
        /// <param name="view"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private ReferencePlane GetCenterReference(View view, string name)
        {
            FilteredElementCollector filtered = new FilteredElementCollector(FamilyDoc);
            filtered.OfClass(typeof(ReferencePlane));
            ReferencePlane referencePlane = filtered.First(x => x.Name == name) as ReferencePlane;
            return referencePlane;
        }
    }
}
