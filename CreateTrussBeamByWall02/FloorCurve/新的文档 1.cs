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

					