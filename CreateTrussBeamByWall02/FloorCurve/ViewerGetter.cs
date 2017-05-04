using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloorCurve
{
    class ViewerGetter
    {
        private Autodesk.Revit.DB.Document doc;

        public ViewerGetter(Autodesk.Revit.DB.Document doc)
        {
            // TODO: Complete member initialization
            this.doc = doc;
        }

        internal Autodesk.Revit.DB.ViewPlan GetViewPlan(Autodesk.Revit.DB.ViewFamily viewFamily, Autodesk.Revit.DB.ElementId levelId)
        {
            throw new NotImplementedException();
        }
    }
}
