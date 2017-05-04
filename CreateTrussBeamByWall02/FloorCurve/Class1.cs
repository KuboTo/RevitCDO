using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;
using Autodesk.Revit.UI.Selection;

namespace FloorCurve
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class cmdFloorCurve:IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            Document document = uiApp.ActiveUIDocument.Document;
            Selection sel = uiApp.ActiveUIDocument.Selection;

            Transaction trans = new Transaction(document, "拾取楼板在边线处放置桁架");
            trans.Start();
            Reference refelem = sel.PickObject(ObjectType.Element, "选取一块楼板 ");
            Floor floor = document.GetElement(refelem) as Floor;
            Face face = FindFloorFace(floor);
            XYZ testPoint = new XYZ();
            string edgeInfo = null;
            int i=0, j;
            double[][] centerPoint = new double[4][];
            if(null != face)
            {
                EdgeArrayArray edgeArrays = face.EdgeLoops;
                foreach(EdgeArray edges in edgeArrays)
                {
                        foreach (Edge edge in edges)
                        {
                            i++;
                            //get one test point
                            testPoint = edge.Evaluate(0.5);
                            centerPoint[i][1] = testPoint.X;
                            centerPoint[i][2] = testPoint.Y;
                            centerPoint[i][3] = testPoint.Z;
                            edgeInfo += string.Format("Point on edge: ({0},{1},{2})", testPoint.X, testPoint.Y, testPoint.Z + "\n");
                        }
                        TaskDialog.Show("Edge", edgeInfo);
                 }
                    
       
                    XYZ point1 = new XYZ(centerPoint[1][1], centerPoint[1][2], centerPoint[1][3]);
                    XYZ point2 = new XYZ(centerPoint[2][1], centerPoint[2][2], centerPoint[2][3]);
                    XYZ point3 = new XYZ(centerPoint[3][1], centerPoint[3][2], centerPoint[3][3]);
                    XYZ point4 = new XYZ(centerPoint[4][1], centerPoint[4][2], centerPoint[4][3]);

                    string pointInfo = null;
                    pointInfo += string.Format("point1 is : ({X},{Y},{Z})", point1.X, point1.Y, point1.Z);
                    TaskDialog.Show("point1", pointInfo);
                    //Transaction trans = new Transaction(document, "拾取楼板在边线处放置桁架");
                    //trans.Start();
                    //Line line1 = Line.CreateBound(point1, point2);
                    //ElementId levelId = new ElementId(497364);
                    //Wall wall = Wall.Create(document, line1, levelId, false);
                    //trans.Commit();


                }
            trans.Commit();

            return Result.Succeeded;
            }

        public Face FindFloorFace(Floor floor)
        {
            Face normalFace = null;
            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Medium;
            GeometryElement e = floor.get_Geometry(opt);
            foreach (GeometryObject obj in e)
            {
                Solid solid = obj as Solid;
                if (solid != null && solid.Faces.Size > 0)
                {
                    foreach(Face face in solid.Faces)
                    {
                        PlanarFace pf = face as PlanarFace;
                        if(null != pf)
                        {
                            if(pf.Normal.AngleTo(new XYZ(0,0,-1)) < 0.001)
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
    }
}
