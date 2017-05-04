using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace FloorCurve
{
    class BeamInstanceGetter:FamilyInstanceGetter
    {
        public BeamInstanceGetter(Document document, string beamFamilyName, string familyTypeName
            ) : base(document)
        {
            GetFamilySymbol(beamFamilyName, familyTypeName, BuiltInCategory.OST_StructuralFraming);
        }

        public override FamilyInstance CreateInstance(Level level, Curve curve, double angle)
        {
            FamilyInstance beam = Document.Create.NewFamilyInstance(curve, FamilySymbol, level, StructuralType.Beam);
            StructuralFramingUtils.DisallowJoinAtEnd(beam, 0);
            StructuralFramingUtils.DisallowJoinAtEnd(beam, 1);

            beam.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
            beam.get_Parameter(BuiltInParameter.START_EXTENSION)
                .Set(WallProperity.Instance.BeamStartExtension/WallProperity.Instance.InchToMins);
            beam.get_Parameter(BuiltInParameter.END_EXTENSION)
                .Set(WallProperity.Instance.BeamEndExtension / WallProperity.Instance.InchToMins);
            beam.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(0);//调整Z轴以顶点对正

            return beam;

        }

        public override FamilyInstance CreateInstance(Level level, Curve curve, double angle, double startExtension, double endExtension)
        {
            FamilyInstance beam = Document.Create.NewFamilyInstance(curve, FamilySymbol, level, StructuralType.Beam);
            StructuralFramingUtils.DisallowJoinAtEnd(beam, 0);
            StructuralFramingUtils.DisallowJoinAtEnd(beam, 1);

            beam.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
            beam.get_Parameter(BuiltInParameter.START_EXTENSION)
                .Set(startExtension / WallProperity.Instance.InchToMins);
            beam.get_Parameter(BuiltInParameter.END_EXTENSION)
                .Set(endExtension / WallProperity.Instance.InchToMins);
            beam.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(0);//调整Z轴以顶点对正

            return beam;
        }

        public override FamilyInstance CreateInstance(Level baseLevel, Level topLevel, Curve curve, double angle)
        {
            FamilyInstance beam = Document.Create.NewFamilyInstance(curve, FamilySymbol, baseLevel, StructuralType.Beam);
            StructuralFramingUtils.DisallowJoinAtEnd(beam, 0);
            StructuralFramingUtils.DisallowJoinAtEnd(beam, 1);

            beam.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
            beam.get_Parameter(BuiltInParameter.START_EXTENSION)
                .Set(WallProperity.Instance.BeamStartExtension / WallProperity.Instance.InchToMins);
            beam.get_Parameter(BuiltInParameter.END_EXTENSION)
                .Set(WallProperity.Instance.BeamEndExtension / WallProperity.Instance.InchToMins);
            //beam.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(0);//调整Z轴以顶点对正

            return beam;
        }

        public override FamilyInstance CreateInstance(Level level, Curve curve, double angle, int zjustification)
        {
            FamilyInstance beam = Document.Create.NewFamilyInstance(curve, FamilySymbol, level, StructuralType.Beam);
            StructuralFramingUtils.DisallowJoinAtEnd(beam, 0);
            StructuralFramingUtils.DisallowJoinAtEnd(beam, 1);

            beam.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
            beam.get_Parameter(BuiltInParameter.START_EXTENSION)
                .Set(WallProperity.Instance.BeamStartExtension / WallProperity.Instance.InchToMins);
            beam.get_Parameter(BuiltInParameter.END_EXTENSION)
                .Set(WallProperity.Instance.BeamEndExtension / WallProperity.Instance.InchToMins);
            beam.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(zjustification);//调整Z轴以顶点对正

            return beam;
        }

        public override FamilyInstance CreateInstance(Level level, Curve curve, double angle, int zjustification, double startExtension, double endExtension)
        {
            FamilyInstance beam = Document.Create.NewFamilyInstance(curve, FamilySymbol, level, StructuralType.Beam);
            StructuralFramingUtils.DisallowJoinAtEnd(beam, 0);
            StructuralFramingUtils.DisallowJoinAtEnd(beam, 1);

            beam.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
            beam.get_Parameter(BuiltInParameter.START_EXTENSION)
                .Set(startExtension / WallProperity.Instance.InchToMins);
            beam.get_Parameter(BuiltInParameter.END_EXTENSION)
                .Set(endExtension / WallProperity.Instance.InchToMins);
            beam.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(zjustification);//调整Z轴以顶点对正

            return beam;
        }

        public override FamilyInstance CreateInstance(Level baseLevel, Level topLevel, Curve curve, double angle, double startExtension, double endExtension)
        {
            return null;
        }
    }

    public class WallProperity
    {
        public static WallProperity Instance { get; set; }

        public readonly double InchToMins = 304.8;
        public readonly double BeamStartExtension = -3;
        public readonly double BeamEndExtension = -3;
        public readonly double ScrewDiameter;//螺钉直径
    }
}
