using GameFramework;
using GameFramework.DataTable;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 数据表组件。
    /// </summary>
    public sealed partial class DataTableComponent : GameFrameworkComponent
    {
        private IDataTableManager m_DataTableManager = null;

        /// <summary>数据表辅助器完整类名。</summary>
        [Export] public string DataTableHelperTypeName = "GodotGameFramework.DefaultDataTableHelper";

        /// <summary>数据提供辅助器完整类名。</summary>
        [Export] public string DataProviderHelperTypeName = "GodotGameFramework.DefaultDataProviderHelper";

        public int Count => m_DataTableManager.Count;

        public int CachedBytesSize => m_DataTableManager.CachedBytesSize;

        public override void _Ready()
        {
            base._Ready();

            m_DataTableManager = GameFrameworkEntry.GetModule<IDataTableManager>();
            if (m_DataTableManager == null)
            {
                GameFrameworkLog.Fatal("Data table manager is invalid.");
                return;
            }

            var dataProviderHelperType = GameFramework.Utility.Assembly.GetType(DataProviderHelperTypeName);
            if (dataProviderHelperType == null || Activator.CreateInstance(dataProviderHelperType) is not IDataProviderHelper<DataTableBase> dataProviderHelper)
            {
                GameFrameworkLog.Fatal($"Can not create data provider helper '{DataProviderHelperTypeName}'.");
                return;
            }
            m_DataTableManager.SetDataProviderHelper(dataProviderHelper);

            var dataTableHelperType = GameFramework.Utility.Assembly.GetType(DataTableHelperTypeName);
            if (dataTableHelperType == null || Activator.CreateInstance(dataTableHelperType) is not DataTableHelperBase dataTableHelper)
            {
                GameFrameworkLog.Fatal($"Can not create data table helper '{DataTableHelperTypeName}'.");
                return;
            }
            m_DataTableManager.SetDataTableHelper(dataTableHelper);
        }

        public void SetResourceManager(GameFramework.Resource.IResourceManager resourceManager)
        {
            m_DataTableManager.SetResourceManager(resourceManager);
        }

        public void EnsureCachedBytesSize(int ensureSize)
        {
            m_DataTableManager.EnsureCachedBytesSize(ensureSize);
        }

        public void FreeCachedBytes()
        {
            m_DataTableManager.FreeCachedBytes();
        }

        public bool HasDataTable<T>() where T : IDataRow
        {
            return m_DataTableManager.HasDataTable<T>();
        }

        public bool HasDataTable<T>(string name) where T : IDataRow
        {
            return m_DataTableManager.HasDataTable<T>(name);
        }

        public IDataTable<T> GetDataTable<T>() where T : IDataRow
        {
            return m_DataTableManager.GetDataTable<T>();
        }

        public IDataTable<T> GetDataTable<T>(string name) where T : IDataRow
        {
            return m_DataTableManager.GetDataTable<T>(name);
        }

        public DataTableBase[] GetAllDataTables()
        {
            return m_DataTableManager.GetAllDataTables();
        }

        public void GetAllDataTables(List<DataTableBase> results)
        {
            m_DataTableManager.GetAllDataTables(results);
        }

        /// <summary>
        /// 创建并立即从二进制文件加载数据表。
        /// </summary>
        /// <param name="dataAssetPath">二进制文件路径（user:// 或 res://）。</param>
        public IDataTable<T> LoadDataTable<T>(string dataAssetPath) where T : class, IDataRow, new()
        {
            return LoadDataTable<T>(string.Empty, dataAssetPath);
        }

        /// <summary>
        /// 创建并立即从二进制文件加载数据表。
        /// </summary>
        /// <param name="name">数据表名称。</param>
        /// <param name="dataAssetPath">二进制文件路径（user:// 或 res://）。</param>
        public IDataTable<T> LoadDataTable<T>(string name, string dataAssetPath) where T : class, IDataRow, new()
        {
            IDataTable<T> dataTable = m_DataTableManager.CreateDataTable<T>(name);
            byte[] bytes = LoadBytesFromPath(dataAssetPath);
            if (bytes == null)
            {
                throw new GameFrameworkException($"Load data table bytes from '{dataAssetPath}' failed.");
            }

            // DataTableBase.ParseData is accessible via the cast
            ((DataTableBase)dataTable).ParseData(bytes);
            return dataTable;
        }

        public IDataTable<T> CreateDataTable<T>() where T : class, IDataRow, new()
        {
            return m_DataTableManager.CreateDataTable<T>();
        }

        public IDataTable<T> CreateDataTable<T>(string name) where T : class, IDataRow, new()
        {
            return m_DataTableManager.CreateDataTable<T>(name);
        }

        public bool DestroyDataTable<T>() where T : IDataRow
        {
            return m_DataTableManager.DestroyDataTable<T>();
        }

        public bool DestroyDataTable<T>(string name) where T : IDataRow
        {
            return m_DataTableManager.DestroyDataTable<T>(name);
        }

        public bool DestroyDataTable<T>(IDataTable<T> dataTable) where T : IDataRow
        {
            return m_DataTableManager.DestroyDataTable(dataTable);
        }

        private static byte[] LoadBytesFromPath(string path)
        {
            if (path.StartsWith("res://") || path.StartsWith("user://"))
            {
                using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    return null;
                }

                return file.GetBuffer((long)file.GetLength());
            }
            else
            {
                if (!System.IO.File.Exists(path))
                {
                    return null;
                }

                return System.IO.File.ReadAllBytes(path);
            }
        }
    }
}
