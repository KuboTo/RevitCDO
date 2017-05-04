using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloorCurve
{
    public class AnalyticalRoof
    {
        public List<RoofTruss> RoofTrusses { get; set; }
        public List<RoofPurlin> RoofPurlins { get; set; }

        public IEnumerable<Member> Members { get; set; }
    }
}
