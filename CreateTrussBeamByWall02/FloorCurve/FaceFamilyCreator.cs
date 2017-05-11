using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

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
        /// <summary>
        /// 当前的文档对象
        /// </summary>
        public UIApplication Application { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Document Doc { get; set; }

        /// <summary>
        /// 构造函数，初始化当前面族创建
        /// </summary>
        /// <param name="app"></param>
        public FaceFamilyCreator(UIApplication app)
        {
            this.Application = app;
            this.Doc = this.Application.ActiveUIDocument.Document;
        }

        public FamilySymbol Create()
        {
            Transaction trans = new Transaction(Doc);
            trans.Start("创建面族");
            Document familyDoc =
                Application.Application.NewFamilyDocument(
                    @"C:\ProgramData\Autodesk\RVT 2015\Family Templates\Chinese\公制常规模型.rft");
            FamilyManager manage = familyDoc.FamilyManager;
            //创建拉伸平面
            CurveArrArray curveArray = this.CreateCurves();
            //创建参考平面
            SketchPlane skplane = this.GetSketchPlane(familyDoc);
            //创建平面拉伸

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
    }
}
