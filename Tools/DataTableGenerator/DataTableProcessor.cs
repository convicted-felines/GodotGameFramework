using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DataTableGenerator
{
    /// <summary>
    /// 数据表处理器。解析 Tab 分隔的文本数据表文件并生成二进制和C#代码输出。
    ///
    /// 文本格式（行索引从0开始）：
    ///   行0：列名（第0列固定为注释列）
    ///   行1：列类型（id / int / string / bool / float / ...）
    ///   行2：默认值（可留空）
    ///   行3：列注释说明
    ///   行4+：实际数据
    /// </summary>
    public sealed partial class DataTableProcessor
    {
        private const int NameRow = 0;
        private const int TypeRow = 1;
        private const int DefaultValueRow = 2;
        private const int CommentRow = 3;
        private const int ContentStartRow = 4;
        private const int IdColumn = 1;

        private static readonly Encoding s_Encoding = new UTF8Encoding(false);
        private static readonly Regex s_NameRegex = new Regex(@"^[A-Za-z][A-Za-z0-9_]*$");

        private readonly string[] m_NameRow;
        private readonly string[] m_TypeRow;
        private readonly string[] m_DefaultValueRow;
        private readonly string[] m_CommentRow;
        private readonly string[][] m_RawValues;
        private readonly DataProcessor[] m_DataProcessors;
        private readonly Dictionary<string, int> m_StringIndexes;
        private readonly List<string> m_Strings;

        private string m_CodeTemplate = string.Empty;
        private DataTableCodeGenerator? m_CodeGenerator = null;

        public DataTableProcessor(string dataTableFileName)
        {
            if (!File.Exists(dataTableFileName))
            {
                throw new Exception($"Data table file '{dataTableFileName}' is not found.");
            }

            string[] lines = File.ReadAllLines(dataTableFileName, s_Encoding);
            if (lines.Length < ContentStartRow)
            {
                throw new Exception($"Data table file '{dataTableFileName}' is invalid (need at least {ContentStartRow} rows).");
            }

            // parse header rows
            m_NameRow = SplitLine(lines[NameRow]);
            m_TypeRow = SplitLine(lines[TypeRow]);
            m_DefaultValueRow = lines.Length > DefaultValueRow ? SplitLine(lines[DefaultValueRow]) : Array.Empty<string>();
            m_CommentRow = lines.Length > CommentRow ? SplitLine(lines[CommentRow]) : Array.Empty<string>();

            int columnCount = m_TypeRow.Length;

            // build data processors per column
            m_DataProcessors = new DataProcessor[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                string typeName = i < m_TypeRow.Length ? m_TypeRow[i] : string.Empty;
                m_DataProcessors[i] = DataProcessorUtility.GetDataProcessor(typeName);
            }

            // parse data rows (skip comment rows starting with '#' in first column)
            var rawValueList = new List<string[]>();
            for (int i = ContentStartRow; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd('\r');
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string[] cols = SplitLine(line);
                if (cols.Length > 0 && IsCommentRow(cols))
                {
                    continue;
                }

                rawValueList.Add(cols);
            }

            m_RawValues = rawValueList.ToArray();

            // collect all strings for optimized string pool (frequency order)
            m_StringIndexes = new Dictionary<string, int>(StringComparer.Ordinal);
            m_Strings = new List<string>();

            var stringFrequency = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int row = 0; row < m_RawValues.Length; row++)
            {
                for (int col = 0; col < m_DataProcessors.Length; col++)
                {
                    if (m_DataProcessors[col] is GenericDataProcessor<string>)
                    {
                        string val = GetValue(row, col);
                        if (!stringFrequency.ContainsKey(val))
                        {
                            stringFrequency[val] = 0;
                        }

                        stringFrequency[val]++;
                    }
                }
            }

            // sort by frequency desc so common strings get low indexes (smaller 7-bit encoded index)
            var sorted = new List<KeyValuePair<string, int>>(stringFrequency);
            sorted.Sort((a, b) => b.Value.CompareTo(a.Value));
            foreach (var kv in sorted)
            {
                m_StringIndexes[kv.Key] = m_Strings.Count;
                m_Strings.Add(kv.Key);
            }
        }

        public int RawRowCount => m_RawValues.Length;
        public int RawColumnCount => m_DataProcessors.Length;
        public int StringCount => m_Strings.Count;
        public int ContentStartRowIndex => ContentStartRow;

        public string GetName(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= m_NameRow.Length)
            {
                return string.Empty;
            }

            return m_NameRow[rawColumn];
        }

        public string GetLanguageKeyword(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= m_DataProcessors.Length)
            {
                return string.Empty;
            }

            return m_DataProcessors[rawColumn].LanguageKeyword ?? string.Empty;
        }

        public string GetComment(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= m_CommentRow.Length)
            {
                return string.Empty;
            }

            return m_CommentRow[rawColumn];
        }

        public string GetDefaultValue(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= m_DefaultValueRow.Length)
            {
                return string.Empty;
            }

            return m_DefaultValueRow[rawColumn];
        }

        public string GetValue(int rawRow, int rawColumn)
        {
            if (rawRow < 0 || rawRow >= m_RawValues.Length)
            {
                return string.Empty;
            }

            string[] row = m_RawValues[rawRow];
            if (rawColumn < 0 || rawColumn >= row.Length)
            {
                return GetDefaultValue(rawColumn);
            }

            string val = row[rawColumn];
            return string.IsNullOrEmpty(val) ? GetDefaultValue(rawColumn) : val;
        }

        public string GetString(int index)
        {
            if (index < 0 || index >= m_Strings.Count)
            {
                return string.Empty;
            }

            return m_Strings[index];
        }

        public bool IsIdColumn(int rawColumn) => rawColumn == IdColumn && m_DataProcessors[rawColumn].IsId;

        public bool IsCommentColumn(int rawColumn) => rawColumn < m_DataProcessors.Length && m_DataProcessors[rawColumn].IsComment;

        public bool IsSystem(int rawColumn) => rawColumn < m_DataProcessors.Length && m_DataProcessors[rawColumn].IsSystem;

        public Type? GetType(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= m_DataProcessors.Length)
            {
                return null;
            }

            return m_DataProcessors[rawColumn].Type;
        }

        public DataProcessor GetDataProcessor(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= m_DataProcessors.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rawColumn));
            }

            return m_DataProcessors[rawColumn];
        }

        public bool CheckRawData()
        {
            for (int col = 0; col < RawColumnCount; col++)
            {
                if (IsCommentColumn(col))
                {
                    continue;
                }

                string name = GetName(col);
                if (!s_NameRegex.IsMatch(name))
                {
                    Console.Error.WriteLine($"Column name '{name}' at index {col} is invalid.");
                    return false;
                }
            }

            return true;
        }

        public void SetCodeTemplate(string codeTemplate)
        {
            m_CodeTemplate = codeTemplate ?? string.Empty;
        }

        public void SetCodeGenerator(DataTableCodeGenerator codeGenerator)
        {
            m_CodeGenerator = codeGenerator;
        }

        /// <summary>
        /// 生成二进制数据文件。格式：4字节行数 + 每行 [4字节长度 + 行字节]
        /// </summary>
        public bool GenerateDataFile(string outputFileName)
        {
            try
            {
                string? dir = Path.GetDirectoryName(outputFileName);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using var stream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write);
                using var writer = new BinaryWriter(stream, s_Encoding);

                writer.Write(RawRowCount);
                for (int row = 0; row < RawRowCount; row++)
                {
                    byte[] rowBytes = GetRowBytes(row);
                    writer.Write(rowBytes.Length);
                    writer.Write(rowBytes);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Generate data file '{outputFileName}' failed: {ex}");
                return false;
            }
        }

        /// <summary>
        /// 生成 C# 代码文件。
        /// </summary>
        public bool GenerateCodeFile(string outputFileName, object? userData = null)
        {
            if (string.IsNullOrEmpty(m_CodeTemplate))
            {
                Console.Error.WriteLine("Code template is not set.");
                return false;
            }

            try
            {
                string? dir = Path.GetDirectoryName(outputFileName);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var sb = new StringBuilder(m_CodeTemplate);
                m_CodeGenerator?.Invoke(this, sb, userData);

                File.WriteAllText(outputFileName, sb.ToString(), s_Encoding);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Generate code file '{outputFileName}' failed: {ex}");
                return false;
            }
        }

        private byte[] GetRowBytes(int row)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, s_Encoding);

            for (int col = 0; col < RawColumnCount; col++)
            {
                if (IsCommentColumn(col))
                {
                    continue;
                }

                string value = GetValue(row, col);
                try
                {
                    m_DataProcessors[col].WriteToStream(this, writer, value);
                }
                catch (Exception ex)
                {
                    // try default value fallback
                    string defaultVal = GetDefaultValue(col);
                    try
                    {
                        m_DataProcessors[col].WriteToStream(this, writer, defaultVal);
                        Console.WriteLine($"  Row {row}, col {col} ('{GetName(col)}'): value='{value}' invalid, used default='{defaultVal}'.");
                    }
                    catch
                    {
                        throw new Exception($"Row {row}, col {col} ('{GetName(col)}'): value='{value}', default='{defaultVal}'. Original: {ex.Message}");
                    }
                }
            }

            return stream.ToArray();
        }

        private static bool IsCommentRow(string[] cols)
        {
            return cols.Length > 0 && (cols[0] == "#" || cols[0].StartsWith("#"));
        }

        private static string[] SplitLine(string line)
        {
            return line.Split('\t');
        }
    }
}
