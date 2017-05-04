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
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Mechanical;

namespace FloorCurve
{
    class CommonUtil
    {

        public  static string ChangeNumToChinese(int levelth)
        {
            string target = null;
            switch (levelth)
            {
                case 1:
                    target = "一";
                    break;
                case 2:
                    target = "二";
                    break;
                case 3:
                    target = "三";
                    break;
                case 4:
                    target = "四";
                    break;
                default:
                    target = levelth.ToString();
                    break;
             }
            return target;

        }



        internal static List<AssemblyInstance> GetAssemblyInstance(Document doc, ViewPlan ActiveView)
        {
            throw new NotImplementedException();
        }
    }
}
