using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace FloorCurve
{
    class BraceInstanceGetter:FamilyInstanceGetter
    {
        public BraceInstanceGetter(Document document, string BeamfamilyName, string familyTypeName)
            : base(document)
        {
            this.GetFamilySymbol(BeamfamilyName, familyTypeName, BuiltInCategory.OST_StructuralFraming);
        }

        public override FamilyInstance CreateInstance(Level level, Curve curve, double angle)
        {
            FamilyInstance brace = Document.Create.NewFamilyInstance(curve, FamilySymbol, level, StructuralType.Brace);
            StructuralFramingUtils.DisallowJoinAtEnd(brace, 0);
            StructuralFramingUtils.DisallowJoinAtEnd(brace, 1);

            brace.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
            brace.get_Parameter(BuiltInParameter.START_EXTENSION)
                .Set(WallProperity.Instance.BeamStartExtension / WallProperity.Instance.InchToMins);
            brace.get_Parameter(BuiltInParameter.END_EXTENSION)
                .Set(WallProperity.Instance.BeamEndExtension / WallProperity.Instance.InchToMins);
            brace.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(2);//调整Z轴以顶点对正

            return brace;
        }

        public override FamilyInstance CreateInstance(Level level, Curve curve, double angle, double startExtension, double endExtension)
        {
            FamilyInstance brace = Document.Create.NewFamilyInstance(curve, FamilySymbol, level, StructuralType.Brace);
            StructuralFramingUtils.DisallowJoinAtEnd(brace, 0);
            StructuralFramingUtils.DisallowJoinAtEnd(brace, 1);

            brace.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
            brace.get_Parameter(BuiltInParameter.START_EXTENSION)
                .Set(startExtension/ WallProperity.Instance.InchToMins);
            brace.get_Parameter(BuiltInParameter.END_EXTENSION)
                .Set(endExtension / WallProperity.Instance.InchToMins);
            brace.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(2);//调整Z轴以顶点对正

            return brace;
        }

        public override FamilyInstance CreateInstance(Level baseLevel, Level topLevel, Curve curve, double angle)
        {
            FamilyInstance brace = Document.Create.NewFamilyInstance(curve, FamilySymbol, baseLevel, StructuralType.Brace);
            StructuralFramingUtils.DisallowJoinAtEnd(brace, 0);
            StructuralFramingUtils.DisallowJoinAtEnd(brace, 1);

            brace.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
            brace.get_Parameter(BuiltInParameter.START_EXTENSION)
                .Set(WallProperity.Instance.BeamStartExtension / WallProperity.Instance.InchToMins);
            brace.get_Parameter(BuiltInParameter.END_EXTENSION)
                .Set(WallProperity.Instance.BeamEndExtension / WallProperity.Instance.InchToMins);

            return brace;
        }

        public override FamilyInstance CreateInstance(Level level, Curve curve, double angle, int zjustification)
        {
            FamilyInstance brace = Document.Create.NewFamilyInstance(curve, FamilySymbol, level, StructuralType.Brace);
            StructuralFramingUtils.DisallowJoinAtEnd(brace, 0);
            StructuralFramingUtils.DisallowJoinAtEnd(brace, 1);

            brace.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
            brace.get_Parameter(BuiltInParameter.START_EXTENSION)
                .Set(WallProperity.Instance.ScrewDiameter * 2 / WallProperity.Instance.InchToMins);
            brace.get_Parameter(BuiltInParameter.END_EXTENSION)
                .Set(WallProperity.Instance.ScrewDiameter * 2 / WallProperity.Instance.InchToMins);
            brace.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(zjustification);//调整Z轴以顶点对正

            return brace;
        }

        public override FamilyInstance CreateInstance(Level level, Curve curve, double angle, int zjustification, double startExtension, double endExtension)
        {
            throw new NotImplementedException();
        }

        public override FamilyInstance CreateInstance(Level baseLevel, Level topLevel, Curve curve, double angle, double startExtension, double endExtension)
        {
            return null;
        }
    }
}
