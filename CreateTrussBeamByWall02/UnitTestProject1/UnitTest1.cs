using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FloorCurve;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            List<Curve> curvesList = new List<Curve>();

            XYZ point1 = new XYZ(0, 0, 0);
            XYZ point2 = new XYZ(0, 1, 0);
            XYZ point3 = new XYZ(0, 2, 0);

            Line line1 = Line.CreateBound(point1, point2);
            Line line2 = Line.CreateBound(point1, point2);
            Line line3 = Line.CreateBound(point1, point3);
            curvesList.Add(line1);
            curvesList.Add(line2);
            curvesList.Add(line3);

            List<Curve> curvesDistinct = General.DistinctCurve(curvesList, new XYZ(0, 1, 0));

            Assert.AreEqual(2, curvesDistinct.Count);

            TaskDialog.Show("去重后个数：", curvesDistinct.Count.ToString());
        }
    }
}
