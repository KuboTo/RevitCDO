using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace FloorCurve
{
    [Serializable]
    public enum MemberType
    {U, N, D, S, B, K}

    [Serializable]
    public class Member
    {
        /// <summary>
        /// 分析后得到杆件的位置信息
        /// </summary>
        public Line line
        { get; set; }

        /// <summary>
        /// 杆件开口方向
        /// </summary>
        public XYZ Orientation
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double StartExtension
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double EndExtension
        { get; set; }

        /// <summary>
        /// 当前杆件的基础标高
        /// </summary>
        public int BaseLevelId
        { get; set; }

        /// <summary>
        /// 当前杆件的顶部标高
        /// </summary>
        public int TopLevelId
        { get; set; }

        /// <summary>
        /// 每个杆件的编号，例如S1、U1
        /// </summary>
        public string Name
        { get; set; }

        /// <summary>
        /// 构件所属于部件的编号，例如1W!
        /// </summary>
        public string AnalyticalName
        { get; set; }

        /// <summary>
        /// 构件的类型信息
        /// </summary>
        public MemberType Type
        { get; set; }

        /// <summary>
        /// 构件的编号
        /// </summary>
        public int Number
        { get; set; }

        /// <summary>
        /// 构件的反转信息
        /// </summary>
        public bool Flipped
        { get; set; }

        /// <summary>
        /// 构件的ID信息
        /// </summary>
        public int ID
        { get; set; }

        /// <summary>
        /// 中心线，导出机器加工文件时用到
        /// </summary>
        public Line CenterLine
        { get; set; }

        /// <summary>
        /// 腹板边缘线
        /// </summary>
        public Line WebLine
        { get; set; }

        /// <summary>
        /// 用于判断倒角的腹板边缘线
        /// </summary>
        public Line WebChamferLine
        { get; set; }

        /// <summary>
        /// 唇边线
        /// </summary>
        public Line LipLine
        { get; set; }

        /// <summary>
        /// 螺栓点
        /// </summary>
        public List<XYZ> BoltLocation
        { get; set; }

        /// <summary>
        /// 当前的螺栓开口
        /// </summary>
        public Member()
        {
            BoltLocation = new List<XYZ>();
        }



        public object Line { get; set; }
    }
}
