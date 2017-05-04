using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace FloorCurve
{
    public class SheetInfo
    {
        public SheetInfo(ViewType viewType)
        {
            this.ViewType = viewType;
        }

        public ViewType ViewType { get; set; }

        /// <summary>
        /// 当前图纸对应的参考
        /// </summary>
        public dynamic Reference
        {
            get;
            set;
        }

        public int LevelId { get; set; }

        public int Levelth { get; set; }

        public string Name { get; set; }
    }
}
