//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework.DataTable;
using GodotGameFramework;
using System;
using System.Reflection;

namespace GameMain
{
    /// <summary>
    /// 数据表扩展方法。
    /// </summary>
    public static class DataTableExtension
    {
        internal static readonly char[] DataSplitSeparators = { '\t' };
        internal static readonly char[] DataTrimSeparators = { '\"' };

        private const string DataRowClassPrefixName = "GameMain.DR";

        /// <summary>
        /// 按表名从 .bytes 文件加载数据表。
        /// </summary>
        public static void LoadDataTable(this DataTableComponent dataTableComponent, string dataTableName)
        {
            if (string.IsNullOrEmpty(dataTableName))
            {
                throw new ArgumentException("Data table name is invalid.", nameof(dataTableName));
            }

            Type dataRowType = typeof(DataTableExtension).Assembly.GetType(DataRowClassPrefixName + dataTableName);
            if (dataRowType == null)
            {
                throw new InvalidOperationException($"Can not get data row type 'DR{dataTableName}'.");
            }

            string dataAssetPath = AssetUtility.GetByteDataTableAsset(dataTableName);
            MethodInfo loadMethod = typeof(DataTableComponent)
                .GetMethod(nameof(DataTableComponent.LoadDataTable), 1, new[] { typeof(string) })
                ?.MakeGenericMethod(dataRowType);

            if (loadMethod == null)
            {
                throw new InvalidOperationException("Can not resolve DataTableComponent.LoadDataTable method.");
            }

            loadMethod.Invoke(dataTableComponent, new object[] { dataAssetPath });
        }

        /// <summary>
        /// 获取指定编号的数据表行。
        /// </summary>
        public static T GetDataRow<T>(this DataTableComponent dataTableComponent, int id)
            where T : class, IDataRow
        {
            return dataTableComponent.GetDataTable<T>()?.GetDataRow(id);
        }
    }
}
