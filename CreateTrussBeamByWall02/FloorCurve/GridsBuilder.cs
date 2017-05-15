using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using ArgumentException = Autodesk.Revit.Exceptions.ArgumentException;


namespace FloorCurve
{
    public class GridsBuilder
    {
        #region Properities

        private const double GRID_EXTEND_DISTANCE = 1000.0/304.8;

        Document Document { get; set; }

        /// <summary>
        /// 定义水平轴线个数
        /// </summary>
        int HCount { get; set; }

        /// <summary>
        /// 定义竖直轴线的个数
        /// </summary>
        int VCount { get; set; }

        /// <summary>
        /// 默认轴线间距
        /// </summary>
        double Interval { get; set; }

        /// <summary>
        ///  轴线交点的集合
        /// </summary>
        public List<XYZ> CrossPoints { get; set; }

        /// <summary>
        /// 根据轴网根数创建
        /// </summary>
        /// <param name="document"></param>
        /// <param name="hCount"></param>
        /// <param name="vCount"></param>
        /// <param name="interval"></param>
        public GridsBuilder(Document document, int hCount, int vCount, double interval)
        {
            this.Document = document;
            this.HCount = hCount;
            this.VCount = vCount;
            this.Interval = interval;

            CrossPoints = new List<XYZ>();
        }

        #endregion

        /// <summary>
        /// Builder
        /// </summary>
        public void Build()
        {
            BuildVerticals();
            BuildHorizontals();

        }

        private void BuildHorizontals()
        {
            var length = GetHorizontalLength();
            try
            {
                BuildHorizontal(GetDistance(0, HCount), length).Name = "A";
                for (int i = 1; i < HCount-1; i++)
                {
                    BuildHorizontal(GetDistance(i, HCount), length);
                }
                Document.GetElement(BuildHorizontal(GetDistance(HCount - 1, HCount), length).GetTypeId())
                    .get_Parameter(BuiltInParameter.GRID_CENTER_SEGMENT_STYLE)
                    .Set(0);

            }
            catch (ArgumentException Exception)
            {

                MessageBox.Show("若要重复建轴网，请先删除之前的轴网");
            }

        }

        private Grid BuildHorizontal(double  y, double length)
        {
            Grid grid =
                Document.Create.NewGrid(Line.CreateBound(new XYZ(-length/2 - GRID_EXTEND_DISTANCE, y, 0),
                    new XYZ(length/2 + GRID_EXTEND_DISTANCE, y, 0)));

            return grid;
        }



        private double GetHorizontalLength()
        {
            return VCount*Interval;
        }

        void BuildVerticals()
        {
            var length = GetVerticalLength();
            for (int i = 0; i < VCount; i++)
            {
                BuildVertical(GetDistance(i, VCount), length);
            }
        }

        private Grid BuildVertical(double x, double length)
        {
            Grid grid =
                Document.Create.NewGrid(Line.CreateBound(new XYZ(x, -length/2 - GRID_EXTEND_DISTANCE, 0),
                    new XYZ(x, length/2 + GRID_EXTEND_DISTANCE, 0)));

            return grid;
        }

        private double GetVerticalLength()
        {
            return VCount*Interval;
        }

        double GetDistance(int i, int quantity)
        {
            return (i - (quantity - 1)/2.0)*Interval;
        }

    }
}
