using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloorCurve
{
    public class RoofTruss
    {
        public int GlobalNumber;

        public Autodesk.Revit.DB.XYZ TrussDirection { get; set; }

        public List<Member> Members { get; set; }

        public string Name { get; set; }
    }
}
