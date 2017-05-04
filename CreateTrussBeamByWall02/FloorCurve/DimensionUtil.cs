using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using Autodesk.Revit.DB;

namespace FloorCurve
{
    class DimensionUtil
    {
        private Autodesk.Revit.DB.Document doc;
        private Autodesk.Revit.DB.ViewPlan ActiveView;
        public DimensionType CurrentDimensionType;

        public DimensionUtil(Autodesk.Revit.DB.Document doc, Autodesk.Revit.DB.ViewPlan ActiveView)
        {
            // TODO: Complete member initialization
            this.doc = doc;
            this.ActiveView = ActiveView;
        }

        public Autodesk.Revit.DB.ReferenceArray CreateReferenceArray(List<Autodesk.Revit.DB.XYZ> firstlocationpoints, Autodesk.Revit.DB.Curve DimensionMainLine)
        {
            throw new NotImplementedException();
        }

        public Dimension CreateDimension(Autodesk.Revit.DB.ReferenceArray array, Autodesk.Revit.DB.Curve curve, OffsetDirection offsetType, double offsetDistance)
        {
            Line line = DetermineDimensionLocationLine(CurrrentView, curve, offsetType, offsetDistance);//调整

            if (array.Size >= 2)
            {
                Dimension newDimension = Doc.Create.NewDimension(CurrrentView, line, array, CurrentDimensionType);

                //文字引线
                Parameter para = newDimension.get_Parameter(BuiltInParameter.DIM_LEADER);
                if (para != null && !para.IsReadOnly)
                {
                    para.Set(0);//0表示无引线
                }

                return newDimension;
            }
            return null;
        }

      

        /// <summary>
        /// 确定标注尺寸线位置
        /// </summary>
        /// <param name="CurrrentView"></param>
        /// <param name="curve"></param>
        /// <param name="offsetType"></param>
        /// <param name="offsetDistance"></param>
        /// <returns></returns>
        private Line DetermineDimensionLocationLine(View view, Curve curve, OffsetDirection type, double offsetDistance)
        {
            XYZ upDirection = view.UpDirection;
            XYZ viewDirection = view.ViewDirection;
            XYZ rightDirection = view.RightDirection;

            Line line = null;
            Line dimenLine = null;
            XYZ direction = ((Line) curve).Direction;

            if (direction.IsAlmostEqualTo(upDirection) || direction.IsAlmostEqualTo(-upDirection))
            {
                direction = XYZ.Zero;
            }
            else
            {
                line = AdjustlocationCurve(curve, view);
                //获得的方向是偏上的
                direction = viewDirection.CrossProduct(line.Direction);
            }

            if (type == OffsetDirection.Up)
            {
                
            }
            else if (type == OffsetDirection.Down)
            {
                direction = -direction;
            }
            else if (type == OffsetDirection.Left)
            {
                if (direction.IsAlmostEqualTo(XYZ.Zero))
                {
                    direction = -rightDirection;
                }
                else if (direction.AngleTo(rightDirection) <= Math.PI /2 + 1e-6)
                {
                    direction = -direction;
                }
            }
            else if (type == OffsetDirection.Right)
            {
                if (direction.IsAlmostEqualTo(XYZ.Zero))
                {
                    direction = rightDirection;
                }
                else if (direction.AngleTo(rightDirection) >= Math.PI /2 + 1e-6)
                {
                    direction = -direction;
                }
            }
            XYZ startPoint = curve.GetEndPoint(0) + (offsetDistance/304.8)*(direction.Normalize());
            XYZ endPoint = curve.GetEndPoint(1) + (offsetDistance / 304.8) * (direction.Normalize());
            dimenLine = Line.CreateBound(startPoint, endPoint);

            return dimenLine;


        }

        /// <summary>
        /// 调整locationcurve的起点和终点，使其x坐标小的为起点，如果x相同，使其y坐标小的为起点
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        private Line AdjustlocationCurve(Curve curve, View view)
        {
            Line line = null;

            Transform transform = view.CropBox.Transform.Inverse;

            XYZ startPoint = curve.GetEndPoint(0);
            XYZ endPoint = curve.GetEndPoint(1);

            XYZ pp0 = transform.OfPoint(startPoint);
            XYZ pp1 = transform.OfPoint(endPoint);

            line = Line.CreateBound(startPoint, endPoint);
            if (Math.Abs(pp0.X - pp1.X ) < 1e-6)
            {
                if (pp0.Y > pp1.Y )
                {
                    line = Line.CreateBound(endPoint, startPoint);

                }
            }
            else if (pp0.X > pp1.X )
            {
                line = Line.CreateBound(endPoint, startPoint);
            }

            return line;
        }

        public View CurrrentView { get; set; }

        private Document Doc { get; set; }
    }

    public enum OffsetDirection
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right =3,

    }
}
