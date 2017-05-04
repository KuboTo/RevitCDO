using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace FloorCurve
{
    class ColumnInstanceGetter:FamilyInstanceGetter
    {
        public ColumnInstanceGetter(Document document, string ColumnfamilyName, string familyTypeName)
            : base(document)
        {
            this.GetFamilySymbol(ColumnfamilyName, familyTypeName, BuiltInCategory.OST_StructuralColumns);
        }

        public override Autodesk.Revit.DB.FamilyInstance CreateInstance(Autodesk.Revit.DB.Level baseLevel, Autodesk.Revit.DB.Curve curve, double angle)
        {
            FamilyInstance column = Document.Create.NewFamilyInstance(curve.GetEndPoint(0), FamilySymbol, baseLevel,
                StructuralType.Column);
            column.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM).Set(baseLevel.Id);
            column.Location.Rotate((Line) curve, angle);
            double baseOffset = curve.GetEndPoint(0).Z - baseLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble();
            column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).Set(baseOffset);

            return column;
        }

        public override Autodesk.Revit.DB.FamilyInstance CreateInstance(Autodesk.Revit.DB.Level baseLevel, Autodesk.Revit.DB.Curve curve, double angle, double startExtension, double endExtension)
        {
            FamilyInstance column = Document.Create.NewFamilyInstance(curve.GetEndPoint(0), FamilySymbol, baseLevel,
                StructuralType.Column);
            column.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM).Set(baseLevel.Id);
            column.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).Set(baseLevel.Id);
            column.Location.Rotate((Line)curve, angle);
            double baseOffset = curve.GetEndPoint(0).Z - baseLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble() + startExtension/WallProperity.Instance.InchToMins;
            double topOffset = curve.GetEndPoint(1).Z - baseLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble() + endExtension / WallProperity.Instance.InchToMins;
            column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).Set(baseOffset);
            column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(topOffset);

            return column;

        }

        public override Autodesk.Revit.DB.FamilyInstance CreateInstance(Autodesk.Revit.DB.Level baseLevel, Autodesk.Revit.DB.Level topLevel, Autodesk.Revit.DB.Curve curve, double angle)
        {
            FamilyInstance column = Document.Create.NewFamilyInstance(curve.GetEndPoint(0), FamilySymbol, baseLevel,
                StructuralType.Column);
            column.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM).Set(baseLevel.Id);
            column.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).Set(topLevel.Id);
            column.Location.Rotate((Line)curve, angle);
            double baseOffset = curve.GetEndPoint(0).Z - baseLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble(); //+ startExtension / WallProperity.Instance.InchToMins;
            double topOffset = curve.GetEndPoint(1).Z - topLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble(); //+ endExtension / WallProperity.Instance.InchToMins;
            column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).Set(baseOffset);
            column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(topOffset);

            return column;
        }

        public override Autodesk.Revit.DB.FamilyInstance CreateInstance(Autodesk.Revit.DB.Level baseLevel, Autodesk.Revit.DB.Curve curve, double angle, int zjustification)
        {
            FamilyInstance column = Document.Create.NewFamilyInstance(curve.GetEndPoint(0), FamilySymbol, baseLevel,
                StructuralType.Column);
            column.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM).Set(baseLevel.Id);
            column.Location.Rotate((Line)curve, angle);
            double baseOffset = curve.GetEndPoint(0).Z - baseLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble();
            column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).Set(baseOffset);

            return column;
        }

        public override Autodesk.Revit.DB.FamilyInstance CreateInstance(Autodesk.Revit.DB.Level level, Autodesk.Revit.DB.Curve curve, double angle, int zjustification, double startExtension, double endExtension)
        {
            throw new NotImplementedException();
        }

        public override Autodesk.Revit.DB.FamilyInstance CreateInstance(Autodesk.Revit.DB.Level baseLevel, Autodesk.Revit.DB.Level topLevel, Autodesk.Revit.DB.Curve curve, double angle, double startExtension, double endExtension)
        {
            FamilyInstance column = Document.Create.NewFamilyInstance(curve.GetEndPoint(0), FamilySymbol, baseLevel,
                StructuralType.Column);
            column.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM).Set(baseLevel.Id);
            column.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).Set(topLevel.Id);
            column.Location.Rotate((Line)curve, angle);
            double baseOffset = curve.GetEndPoint(0).Z - baseLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble() + startExtension / WallProperity.Instance.InchToMins;
            double topOffset = curve.GetEndPoint(1).Z - topLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble() + endExtension / WallProperity.Instance.InchToMins;
            column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).Set(baseOffset);
            column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(topOffset);

            return column;
        }
    }
}
