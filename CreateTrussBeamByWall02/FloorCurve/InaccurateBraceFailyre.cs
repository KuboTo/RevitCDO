using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace FloorCurve
{
    /// <summary>
    /// 杆件偏离了轴线
    /// </summary>
    public class InaccurateBraceFailyre : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            throw new NotImplementedException();
        }
    }
}
