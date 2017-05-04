using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using Autodesk.Revit.DB;

namespace FloorCurve
{
    /// <summary>
    /// 部品创建类，用于创建部品
    /// </summary>
    public class AssemblyInstanceCreator
    {
        public Document Doc
        {
            private set;
            get;
            
        }

        private string assemblyName = "标记";

        /// <summary>
        /// 构造函数，初始化当前部品创建类
        /// </summary>
        /// <param name="Doc"></param>
        public AssemblyInstanceCreator(Document Doc)
        {
            // TODO: Complete member initialization
            this.Doc = Doc;
        }
        /// <summary>
        /// 创建一个墙的部品并命名
        /// </summary>
        /// <param name="elementIdInOneWallFrame"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        public AssemblyInstance CteateWallAssemblyInstance(ICollection<ElementId> elementIdInOneWallFrame,
            string strName)
        {
            AssemblyInstance assemblyInstance = null;
            using (Transaction transaction = new Transaction(Doc, "创建部件"))
            {
                //获取当前集合中的一个类别
                ElementId categoryId = Doc.GetElement(elementIdInOneWallFrame.First()).Category.Id;

                //验证当前类别的Id信息
                if (AssemblyInstance.IsValidNamingCategory(Doc, categoryId, elementIdInOneWallFrame))
                {
                    //创建部件
                    transaction.Start();
                    //开始创建一个部品
                    assemblyInstance = AssemblyInstance.Create(Doc, elementIdInOneWallFrame, categoryId);
                    //提交创建
                    transaction.Commit();

                    //判断事务运行状态
                    if (transaction.GetStatus() == TransactionStatus.Committed)
                    {
                        //如果要修改部件的名称，一定要新建一个事务，否则报错
                        transaction.Start();
                        //修改部品名称
                        this.SetAssemblyInstanceName(assemblyInstance, strName);

                        transaction.Commit();
                    }

                }
            }
            return assemblyInstance;
        }

        /// <summary>
        /// 创建一个屋架部品
        /// </summary>
        /// <param name="elementIdInOneWallFrame"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        public AssemblyInstance CreateRoofAssemblyInstance(ICollection<ElementId> elementIdInOneWallFrame,
            string strName)
        {
            AssemblyInstance assemblyInstance = null;
            using (Transaction transaction = new Transaction(Doc, "创建部件"))
            {
                //获取当前集合中的一个类别
                ElementId categoryId = Doc.GetElement(elementIdInOneWallFrame.First()).Category.Id;

                //验证当前类别的Id信息
                if (AssemblyInstance.IsValidNamingCategory(Doc, categoryId, elementIdInOneWallFrame))
                {
                    //创建部件
                    transaction.Start();
                    //开始创建一个部品
                    assemblyInstance = AssemblyInstance.Create(Doc, elementIdInOneWallFrame, categoryId);
                    //提交创建
                    transaction.Commit();

                    //判断事务运行状态
                    if (transaction.GetStatus() == TransactionStatus.Committed)
                    {
                        //如果要修改部件的名称，一定要新建一个事务，否则报错
                        transaction.Start();
                        //修改部品名称
                        this.SetAssemblyInstanceName(assemblyInstance, strName);

                        transaction.Commit();
                    }

                }
            }
            return assemblyInstance;
        }

        /// <summary>
        /// 创建一个楼板桁架
        /// </summary>
        /// <param name="elementIdInOneWallFrame"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        public AssemblyInstance CreateFloorAssemblyInstance(ICollection<ElementId> elementIdInOneWallFrame,
            string strName)
        {
            AssemblyInstance assemblyInstance = null;
            using (Transaction transaction = new Transaction(Doc, "创建部件"))
            {
                //获取当前集合中的一个类别
                ElementId categoryId = Doc.GetElement(elementIdInOneWallFrame.First()).Category.Id;

                //验证当前类别的Id信息
                if (AssemblyInstance.IsValidNamingCategory(Doc, categoryId, elementIdInOneWallFrame))
                {
                    //创建部件
                    transaction.Start();
                    //开始创建一个部品
                    assemblyInstance = AssemblyInstance.Create(Doc, elementIdInOneWallFrame, categoryId);
                    //提交创建
                    transaction.Commit();

                    //判断事务运行状态
                    if (transaction.GetStatus() == TransactionStatus.Committed)
                    {
                        //如果要修改部件的名称，一定要新建一个事务，否则报错
                        transaction.Start();
                        //修改部品名称
                        this.SetAssemblyInstanceName(assemblyInstance, strName);

                        transaction.Commit();
                    }

                }
            }
            return assemblyInstance;
        }

        /// <summary>
        /// 设置部品名称
        /// </summary>
        /// <param name="assemblyInstance"></param>
        /// <param name="strName"></param>
        private void SetAssemblyInstanceName(AssemblyInstance assemblyInstance, string strName)
        {
            try
            {
                assemblyInstance.AssemblyTypeName = strName;
                assemblyInstance.LookupParameter(assemblyName).Set(strName);
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                
                char[] chars = new char[] {'W', 'N', 'R'};
                string[] ss = strName.Split(chars);

                char targetChar = 'A';
                for (int i = 0; i < chars.Length; i++)
                {
                    if (strName.Contains(chars[i]))
                    {
                        targetChar = chars[i];
                        break;
                    }
                }

                string newName = ss[0] + targetChar.ToString() + (Convert.ToInt32(ss[1]) + 1).ToString();
                SetAssemblyInstanceName(assemblyInstance, newName);

            }
        }


        /// <summary>
        /// 创建一个钢筋部品
        /// </summary>
        /// <param name="rebars"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        public AssemblyInstance CreateRebarAssemblyInstance(ICollection<ElementId> rebars,
            string strName)
        {
            AssemblyInstance assemblyInstance = null;
            using (Transaction transaction = new Transaction(Doc, "创建部件"))
            {
                //获取当前集合中的一个类别
                ElementId categoryId = Doc.GetElement(rebars.First()).Category.Id;

                //验证当前类别的Id信息
                if (AssemblyInstance.IsValidNamingCategory(Doc, categoryId, rebars))
                {
                    //创建部件
                    transaction.Start();
                    //开始创建一个部品
                    assemblyInstance = AssemblyInstance.Create(Doc, rebars, categoryId);
                    //提交创建
                    transaction.Commit();

                    //判断事务运行状态
                    if (transaction.GetStatus() == TransactionStatus.Committed)
                    {
                        //如果要修改部件的名称，一定要新建一个事务，否则报错
                        transaction.Start();
                        //修改部品名称
                        this.SetAssemblyInstanceName(assemblyInstance, strName);

                        transaction.Commit();
                    }

                }
                if (assemblyInstance.AllowsAssemblyViewCreation())
                {
                    Transaction trans = new Transaction(Doc);
                    trans.Start();
                    View detailView = AssemblyViewUtils.CreateDetailSection(Doc, assemblyInstance.Id,
                        AssemblyDetailViewOrientation.ElevationTop);
                    trans.Commit();
                }
            }
            return assemblyInstance;
        }

        /// <summary>
        /// 设置钢筋部品名称
        /// </summary>
        /// <param name="assemblyInstance"></param>
        /// <param name="strName"></param>
        private void SetRebarAssemblyInstanceName(AssemblyInstance assemblyInstance, string strName)
        {
            try
            {
                assemblyInstance.AssemblyTypeName = strName;
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                
                throw;
            }
        }
    }
}
