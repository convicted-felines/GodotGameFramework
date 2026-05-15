using GameFramework;
using GameFramework.DataTable;
using System;
using System.IO;
using System.Text;

namespace GodotGameFramework
{
    /// <summary>
    /// 默认数据提供者辅助器。
    /// 支持从二进制字节流解析数据表，也支持从 UTF-8 字符串（TSV格式）解析。
    /// </summary>
    public sealed class DefaultDataProviderHelper : IDataProviderHelper<DataTableBase>
    {
        private static readonly Encoding s_Encoding = new UTF8Encoding(false);

        /// <summary>
        /// 读取数据（Asset对象形式，此实现将 byte[] asset 直接解析）。
        /// </summary>
        public bool ReadData(DataTableBase dataProviderOwner, string dataAssetName, object dataAsset, object userData)
        {
            if (dataAsset is byte[] bytes)
            {
                return dataProviderOwner.ParseData(bytes, 0, bytes.Length, userData);
            }

            if (dataAsset is string text)
            {
                return dataProviderOwner.ParseData(text, userData);
            }

            throw new GameFrameworkException($"Data asset type '{dataAsset?.GetType().FullName}' is not supported.");
        }

        /// <summary>
        /// 读取数据（字节流形式）。
        /// </summary>
        public bool ReadData(DataTableBase dataProviderOwner, string dataAssetName, byte[] dataBytes, int startIndex, int length, object userData)
        {
            return dataProviderOwner.ParseData(dataBytes, startIndex, length, userData);
        }

        /// <summary>
        /// 解析数据表字符串（TSV文本格式，每行是一行数据，跳过'#'注释行）。
        /// </summary>
        public bool ParseData(DataTableBase dataProviderOwner, string dataString, object userData)
        {
            if (string.IsNullOrEmpty(dataString))
            {
                return false;
            }

            string[] lines = dataString.Split('\n');
            bool result = true;
            foreach (string line in lines)
            {
                string trimmed = line.TrimEnd('\r');
                if (string.IsNullOrEmpty(trimmed) || trimmed[0] == '#')
                {
                    continue;
                }

                if (!dataProviderOwner.AddDataRow(trimmed, userData))
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 解析数据表二进制流。
        /// 二进制格式：4字节行数 + 每行 [4字节长度 + 行字节数据]
        /// </summary>
        public bool ParseData(DataTableBase dataProviderOwner, byte[] dataBytes, int startIndex, int length, object userData)
        {
            using var memoryStream = new MemoryStream(dataBytes, startIndex, length, false);
            using var binaryReader = new BinaryReader(memoryStream, s_Encoding);

            int rowCount = binaryReader.ReadInt32();
            for (int i = 0; i < rowCount; i++)
            {
                int rowLength = binaryReader.ReadInt32();
                byte[] rowBytes = binaryReader.ReadBytes(rowLength);
                if (!dataProviderOwner.AddDataRow(rowBytes, 0, rowLength, userData))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 释放内容资源（无需操作）。
        /// </summary>
        public void ReleaseDataAsset(DataTableBase dataProviderOwner, object dataAsset)
        {
        }
    }
}
