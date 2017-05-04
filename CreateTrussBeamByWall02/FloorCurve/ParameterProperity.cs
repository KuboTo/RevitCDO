using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloorCurve
{
    class ParameterProperity
    {
        public readonly string ParaNumber;//对参数进行定义，包括名称、参数类型（长度、文字等等）
        public readonly string ParaAssemblyCategory;//对参数进行定义，包括名称、参数类型（长度、文字等等）

        public static ParameterProperity Instance { get; set; }
    }
}
