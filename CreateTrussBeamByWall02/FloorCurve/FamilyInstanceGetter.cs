using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace FloorCurve
{
    /// <summary>
    /// 族实例获取类
    /// </summary>
    abstract class FamilyInstanceGetter
    {
        /// <summary>
        /// 当前的文档对象
        /// </summary>
        protected Autodesk.Revit.DB.Document Document
        {
            get;
            private set;
        }
        /// <summary>
        /// 墙类型
        /// </summary>
        public WallType WallType { get; set; }
        /// <summary>
        /// 楼板类型
        /// </summary>
        public FloorType FloorType { get; set; }
        /// <summary>
        /// 族的实例对象
        /// </summary>
        public FamilySymbol FamilySymbol
        {
            get;
            set;
        }
        /// <summary>
        /// 系统族的名称
        /// </summary>
        protected string RevitTypeName
        {
            get;
            set;
        }
        /// <summary>
        /// 族类的名称
        /// </summary>
        protected string FamilyName
        {
            get;
            set;
        }
        /// <summary>
        /// 当前的类型信息
        /// </summary>
        protected string TypeName
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        public FamilyInstanceGetter(Autodesk.Revit.DB.Document document)
        {
            Document = document;
        }

        public abstract FamilyInstance CreateInstance(Level level, Curve curve, double angle);
        public abstract FamilyInstance CreateInstance(Level level, Curve curve, double angle, double startExtension, double endExtension);
        public abstract FamilyInstance CreateInstance(Level baseLevel, Level topLevel, Curve curve, double angle);
        public abstract FamilyInstance CreateInstance(Level level, Curve curve, double angle, int zjustification);
        public abstract FamilyInstance CreateInstance(Level level, Curve curve, double angle, int zjustification, double startExtension, double endExtension);
        public abstract FamilyInstance CreateInstance(Level baseLevel, Level topLevel, Curve curve, double angle, double startExtension, double endExtension);


        /// <summary>
        /// 获取指定族对象
        /// </summary>
        /// <param name="familyName"></param>
        /// <param name="typeName"></param>
        /// <param name="category"></param>
        public void GetFamilySymbol(string familyName, string typeName, BuiltInCategory category)
        {
            this.FamilyName = familyName;
            this.TypeName = typeName;
            var collector = new FilteredElementCollector(Document).OfClass(typeof (FamilySymbol));
            collector.OfCategory(category);

            var targetElems = from element in collector
                where
                    element.Name.Equals(typeName) &&
                    element.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString().Equals(familyName)
                select element;

            IList<Element> elems = targetElems.ToList();
            if (elems.Count == 0)
            {
                throw new Exception("没有载入指定的族，族名：" + familyName + "，类型名：" + typeName);
            }
            FamilySymbol = elems[0] as FamilySymbol;

            if (FamilySymbol.IsActive)
            {
                FamilySymbol.Activate();
            }
        }

        /// <summary>
        /// 获取指定墙类型，并复制一份修改类型名
        /// </summary>
        /// <param name="revitTypeName"></param>
        /// <param name="typeName"></param>
        /// <param name="category"></param>
        protected void GetFamilySymbol2(string revitTypeName, string typeName, BuiltInCategory category)
        {
            WallType wallType = null;
            List<WallType> collectors =
                new FilteredElementCollector(Document).OfClass(typeof (WallType))
                    .OfCategory(category)
                    .Cast<WallType>()
                    .ToList();
            List<WallType> useType = collectors.FindAll(z => z.Name == revitTypeName);
            bool isTargetWallTypeExist = collectors.Any(z => z.Name == typeName);
            if (!isTargetWallTypeExist)
            {
                WallType sourceType = useType.FirstOrDefault();
                if (sourceType == null)
                {
                    throw new Exception("没有找到指定类型名的墙族，" + "族类型名：" + revitTypeName);
                }
                wallType = useType.FirstOrDefault().Duplicate(typeName) as WallType;
            }
            else
            {
                wallType = collectors.Find(z => z.Name == typeName);
            }
            WallType = wallType;
        }

        /// <summary>
        /// 获取指定楼板类型，并复制一份修改类型名
        /// </summary>
        /// <param name="revitTypeName"></param>
        /// <param name="typeName"></param>
        /// <param name="category"></param>
        protected void GetFamilySymbol3(string revitTypeName, string typeName, BuiltInCategory category)
        {
            FloorType floorType = null;
            List<FloorType> collectors =
                new FilteredElementCollector(Document).OfClass(typeof (FloorType))
                    .OfCategory(category)
                    .Cast<FloorType>()
                    .ToList();
            bool isTargetFloorTypeExist = collectors.Any(z => z.Name == typeName);
            if (!isTargetFloorTypeExist)
            {
                floorType = collectors.FirstOrDefault().Duplicate(typeName) as FloorType;
            }
            else
            {
                floorType = collectors.Find(z => z.Name == typeName);
            }
            FloorType = floorType;
        }
    }
}
