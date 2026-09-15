using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DataTableGenerator
{
    /// <summary>
    /// 数据表生成器委托，用于将自定义代码填充到模板中。
    /// </summary>
    public delegate void DataTableCodeGenerator(DataTableProcessor dataTableProcessor, StringBuilder codeContent, object? userData);

    /// <summary>
    /// 数据表生成器。
    /// 功能：
    ///   1. 读取 Excel (.xlsx) 文件，将每个 Sheet 导出为 TSV 文本文件
    ///   2. 将 TSV 文本文件转换为二进制 .bytes 文件
    ///   3. 根据 TSV 文本文件生成 C# IDataRow 实现代码
    /// </summary>
    public static class DataTableGeneratorUtility
    {
        private static readonly Regex s_NumberedColumnRegex = new Regex(@"^(.+?)(\d+)$");

        // ── 路径默认值（由 Program.cs 覆盖）────────────────────────────────────

        public static string DataTableTextPath = "MainGame/DataTables/Text";
        public static string DataTableBytesPath = "MainGame/DataTables/Bytes";
        public static string DataTableCodePath = "MainGame/Scripts/DataTable";
        public static string DataTableExcelPath = "MainGame/DataTables/Excel";
        public static string DataTableNamesFilePath = "MainGame/DataTables/DataTableNames.txt";
        public static string CodeNamespace = "GameMain";
        public static string CodeBaseClass = "DataTableRowBase";
        public static string ProjectRoot = ".";

        // ── 1. Excel → TSV ──────────────────────────────────────────────────────

        /// <summary>
        /// 将 Excel 目录下所有 .xlsx/.xlsm 文件导出为 TSV（对应 Unity 菜单 A）。
        /// </summary>
        public static int ExportAllExcelFromFolder(string? excelDir = null, string? outputDir = null)
        {
            excelDir ??= ResolvePath(DataTableExcelPath);
            outputDir ??= ResolvePath(DataTableTextPath);

            if (!Directory.Exists(excelDir))
            {
                throw new Exception($"Excel folder not found: {excelDir}");
            }

            Directory.CreateDirectory(outputDir);

            string[] excelFiles = Directory.GetFiles(excelDir)
                .Where(f =>
                {
                    string name = Path.GetFileName(f);
                    if (name.StartsWith("~$", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return f.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".xlsm", StringComparison.OrdinalIgnoreCase);
                })
                .ToArray();

            if (excelFiles.Length == 0)
            {
                Console.WriteLine($"No Excel files found in '{excelDir}'.");
                return 0;
            }

            var sheetNameToFiles = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (string excelFile in excelFiles)
            {
                try
                {
                    foreach (string sheetName in GetSheetNamesFromExcel(excelFile))
                    {
                        if (!sheetNameToFiles.TryGetValue(sheetName, out List<string>? files))
                        {
                            files = new List<string>();
                            sheetNameToFiles[sheetName] = files;
                        }

                        files.Add(excelFile);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(FormatExcelOpenError(excelFile, ex));
                }
            }

            var duplicateSheetNames = new HashSet<string>(
                sheetNameToFiles.Where(kv => kv.Value.Count > 1).Select(kv => kv.Key),
                StringComparer.Ordinal);

            if (duplicateSheetNames.Count > 0)
            {
                Console.WriteLine($"Duplicate sheet names detected, will add file suffix: {string.Join(", ", duplicateSheetNames)}");
            }

            int exportedCount = 0;
            int failedFileCount = 0;
            foreach (string excelFile in excelFiles)
            {
                try
                {
                    exportedCount += ExportSheetsFromExcel(excelFile, outputDir, duplicateSheetNames);
                }
                catch (Exception ex)
                {
                    failedFileCount++;
                    Console.Error.WriteLine(FormatExcelOpenError(excelFile, ex));
                }
            }

            if (failedFileCount > 0 && exportedCount == 0)
            {
                throw new Exception($"Excel export failed: {failedFileCount} file(s) could not be opened.");
            }

            Console.WriteLine($"Excel export complete: {exportedCount} sheet(s) -> {outputDir}");
            if (failedFileCount > 0)
            {
                Console.WriteLine($"Warning: {failedFileCount} Excel file(s) were skipped due to errors.");
            }

            return exportedCount;
        }

        /// <summary>
        /// 将 Excel 文件所有 Sheet 导出为 TSV 文本文件。
        /// </summary>
        /// <param name="excelPath">Excel 文件路径。</param>
        /// <param name="outputDir">输出目录（默认使用 DataTableTextPath）。</param>
        public static void ExportExcelToText(string excelPath, string? outputDir = null)
        {
            outputDir ??= ResolvePath(DataTableTextPath);
            Directory.CreateDirectory(outputDir);

            Console.WriteLine($"Exporting Excel: {excelPath}");
            ExportSheetsFromExcel(excelPath, outputDir, null);
        }

        private static int ExportSheetsFromExcel(string excelPath, string outputDir, HashSet<string>? duplicateSheetNames)
        {
            int count = 0;
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(excelPath);

            using SpreadsheetDocument document = OpenExcelReadOnly(excelPath);
            var workbookPart = document.WorkbookPart
                ?? throw new Exception("Excel workbook part is null.");

            var sharedStrings = ReadSharedStrings(workbookPart);
            var sheets = workbookPart.Workbook.Descendants<Sheet>();

            foreach (Sheet sheet in sheets)
            {
                string sheetName = sheet.Name?.Value ?? "Sheet";
                string outputFileName = duplicateSheetNames != null && duplicateSheetNames.Contains(sheetName)
                    ? $"{sheetName}_{fileNameWithoutExt}.txt"
                    : $"{sheetName}.txt";
                string outputFile = Path.Combine(outputDir, outputFileName);

                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!.Value!);
                string tsv = ReadSheetAsTsv(worksheetPart, sharedStrings);

                File.WriteAllText(outputFile, tsv, new UTF8Encoding(false));
                Console.WriteLine($"  Exported sheet '{sheetName}' -> {outputFile}");
                count++;
            }

            return count;
        }

        private static List<string> GetSheetNamesFromExcel(string excelPath)
        {
            using SpreadsheetDocument document = OpenExcelReadOnly(excelPath);
            var workbookPart = document.WorkbookPart
                ?? throw new Exception("Excel workbook part is null.");

            return workbookPart.Workbook.Descendants<Sheet>()
                .Select(s => s.Name?.Value ?? "Sheet")
                .ToList();
        }

        /// <summary>
        /// 以共享只读方式打开 Excel，允许 Excel/WPS 正在编辑时读取。
        /// </summary>
        private static SpreadsheetDocument OpenExcelReadOnly(string excelPath)
        {
            var stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return SpreadsheetDocument.Open(stream, false);
        }

        private static string FormatExcelOpenError(string excelPath, Exception ex)
        {
            string fileName = Path.GetFileName(excelPath);
            if (IsFileLockedException(ex))
            {
                return $"Error: Cannot read '{fileName}' because it is open in another program. Please close Excel/WPS and try again.";
            }

            return $"Error: Failed to read '{fileName}': {ex.Message}";
        }

        private static bool IsFileLockedException(Exception ex)
        {
            for (Exception? current = ex; current != null; current = current.InnerException)
            {
                if (current is IOException ioEx &&
                    (ioEx.Message.Contains("being used by another process", StringComparison.OrdinalIgnoreCase) ||
                     ioEx.Message.Contains("used by another process", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        // ── 2. TSV → .bytes ─────────────────────────────────────────────────────

        /// <summary>
        /// 读取数据表名称列表（对应 ProcedurePreload.DataTableNames）。
        /// </summary>
        public static string[] LoadDataTableNames(string? namesFile = null)
        {
            namesFile ??= ResolvePath(DataTableNamesFilePath);
            if (!File.Exists(namesFile))
            {
                throw new Exception($"Data table names file not found: {namesFile}");
            }

            return File.ReadAllLines(namesFile, Encoding.UTF8)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith("#", StringComparison.Ordinal))
                .ToArray();
        }

        /// <summary>
        /// 为指定名称列表生成 .bytes 和 C# 代码（对应 Unity 菜单 B）。
        /// </summary>
        public static void GenerateDataTables(string[] dataTableNames, string? textDir = null, string? bytesDir = null, string? codeDir = null)
        {
            textDir ??= ResolvePath(DataTableTextPath);
            bytesDir ??= ResolvePath(DataTableBytesPath);
            codeDir ??= ResolvePath(DataTableCodePath);

            foreach (string tableName in dataTableNames)
            {
                string txtFile = Path.Combine(textDir, tableName + ".txt");
                if (!File.Exists(txtFile))
                {
                    Console.Error.WriteLine($"Text file not found for table '{tableName}': {txtFile}");
                    continue;
                }

                GenerateDataFile(txtFile, bytesDir);
                GenerateCodeFile(txtFile, codeDir);
            }
        }

        /// <summary>
        /// 为名称列表文件中的表生成 .bytes 和 C# 代码。
        /// </summary>
        public static void GenerateDataTablesFromNamesFile(string? namesFile = null, string? textDir = null, string? bytesDir = null, string? codeDir = null)
        {
            GenerateDataTables(LoadDataTableNames(namesFile), textDir, bytesDir, codeDir);
        }

        // ── 2. TSV → .bytes ─────────────────────────────────────────────────────

        /// <summary>
        /// 将指定目录下所有 .txt TSV 数据表文件生成对应的 .bytes 二进制文件。
        /// </summary>
        /// <param name="textDir">TSV 文本目录（默认 DataTableTextPath）。</param>
        /// <param name="bytesDir">二进制输出目录（默认 DataTableBytesPath）。</param>
        public static void GenerateAllDataFiles(string? textDir = null, string? bytesDir = null)
        {
            textDir ??= ResolvePath(DataTableTextPath);
            bytesDir ??= ResolvePath(DataTableBytesPath);
            Directory.CreateDirectory(bytesDir);

            foreach (string txtFile in Directory.GetFiles(textDir, "*.txt"))
            {
                GenerateDataFile(txtFile, bytesDir);
            }
        }

        /// <summary>
        /// 将单个 TSV 文件转换为 .bytes 文件。
        /// </summary>
        public static void GenerateDataFile(string txtFile, string? bytesDir = null)
        {
            bytesDir ??= ResolvePath(DataTableBytesPath);
            Directory.CreateDirectory(bytesDir);

            string tableName = Path.GetFileNameWithoutExtension(txtFile);
            string outputFile = Path.Combine(bytesDir, tableName + ".bytes");

            Console.WriteLine($"  Generating bytes: {tableName}");
            var processor = new DataTableProcessor(txtFile);

            if (!processor.CheckRawData())
            {
                Console.Error.WriteLine($"    CheckRawData failed for '{tableName}', skipped.");
                return;
            }

            if (!processor.GenerateDataFile(outputFile))
            {
                Console.Error.WriteLine($"    GenerateDataFile failed for '{tableName}'.");
            }
            else
            {
                Console.WriteLine($"    → {outputFile}");
            }
        }

        // ── 3. TSV → C# 代码 ─────────────────────────────────────────────────────

        /// <summary>
        /// 为指定目录下所有 .txt TSV 数据表生成 C# IDataRow 实现文件。
        /// </summary>
        /// <param name="textDir">TSV 文本目录（默认 DataTableTextPath）。</param>
        /// <param name="codeDir">代码输出目录（默认 DataTableCodePath）。</param>
        public static void GenerateAllCodeFiles(string? textDir = null, string? codeDir = null)
        {
            textDir ??= ResolvePath(DataTableTextPath);
            codeDir ??= ResolvePath(DataTableCodePath);
            Directory.CreateDirectory(codeDir);

            foreach (string txtFile in Directory.GetFiles(textDir, "*.txt"))
            {
                GenerateCodeFile(txtFile, codeDir);
            }
        }

        /// <summary>
        /// 为单个 TSV 文件生成 C# 代码。
        /// </summary>
        public static void GenerateCodeFile(string txtFile, string? codeDir = null)
        {
            codeDir ??= ResolvePath(DataTableCodePath);
            Directory.CreateDirectory(codeDir);

            string tableName = Path.GetFileNameWithoutExtension(txtFile);
            string outputFile = Path.Combine(codeDir, "DR" + tableName + ".cs");

            Console.WriteLine($"  Generating code: DR{tableName}");
            var processor = new DataTableProcessor(txtFile);

            if (!processor.CheckRawData())
            {
                Console.Error.WriteLine($"    CheckRawData failed for '{tableName}', skipped.");
                return;
            }

            string template = BuildCodeTemplate(tableName);
            processor.SetCodeTemplate(template);
            processor.SetCodeGenerator(FillCodeContent);

            if (!processor.GenerateCodeFile(outputFile))
            {
                Console.Error.WriteLine($"    GenerateCodeFile failed for '{tableName}'.");
            }
            else
            {
                Console.WriteLine($"    → {outputFile}");
            }
        }

        // ── Code generation ──────────────────────────────────────────────────────

        private static string BuildCodeTemplate(string tableName)
        {
            return
$@"//------------------------------------------------------------------------------
// <auto-generated>
//   此代码由 DataTableGenerator 工具自动生成，请勿手动修改。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using GameFramework.DataTable;

namespace {CodeNamespace}
{{
    /// <summary>
    /// {tableName} 数据表行。
    /// </summary>
    public sealed class DR{tableName} : IDataRow
    {{
        //@PROPERTIES@

        public int Id {{ get; private set; }}

        //@PROPERTY_ARRAY@

        public bool ParseDataRow(string dataRowString, object userData)
        {{
            string[] columnStrings = dataRowString.Split('\t');
            //@PARSE_STRING@
            return true;
        }}

        public bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {{
            using var stream = new MemoryStream(dataRowBytes, startIndex, length, false);
            using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8);
            //@PARSE_BINARY@
            return true;
        }}
    }}
}}
";
        }

        private static void FillCodeContent(DataTableProcessor processor, StringBuilder code, object? userData)
        {
            var properties = new StringBuilder();
            var propertyArrays = new StringBuilder();
            var parseString = new StringBuilder();
            var parseBinary = new StringBuilder();

            // Track numbered column groups (e.g. Item1, Item2, Item3 → Items[])
            var numberedGroups = new Dictionary<string, (string keyword, int maxIndex)>(StringComparer.Ordinal);

            // First pass: collect numbered column groups
            for (int col = 0; col < processor.RawColumnCount; col++)
            {
                if (processor.IsCommentColumn(col))
                {
                    continue;
                }

                string name = processor.GetName(col);
                var match = s_NumberedColumnRegex.Match(name);
                if (match.Success)
                {
                    string baseName = match.Groups[1].Value;
                    int index = int.Parse(match.Groups[2].Value);
                    string keyword = processor.GetLanguageKeyword(col);

                    if (!numberedGroups.TryGetValue(baseName, out var existing) || existing.maxIndex < index)
                    {
                        numberedGroups[baseName] = (keyword, index);
                    }
                }
            }

            // Second pass: generate properties and parse methods
            // string col index tracking for ParseDataRow(string)
            int stringColumnIndex = 0;

            for (int col = 0; col < processor.RawColumnCount; col++)
            {
                if (processor.IsCommentColumn(col))
                {
                    if (!processor.IsIdColumn(col))
                    {
                        stringColumnIndex++;
                    }

                    continue;
                }

                string name = processor.GetName(col);
                string keyword = processor.GetLanguageKeyword(col);
                string comment = processor.GetComment(col);

                bool isId = processor.IsIdColumn(col);
                bool isNumbered = s_NumberedColumnRegex.IsMatch(name);

                // Property (skip Id and numbered columns, those are handled separately)
                if (!isId && !isNumbered)
                {
                    if (!string.IsNullOrEmpty(comment))
                    {
                        properties.AppendLine($"        /// <summary>{comment}</summary>");
                    }

                    properties.AppendLine($"        public {keyword} {name} {{ get; private set; }}");
                    properties.AppendLine();
                }

                // String parsing
                if (isId)
                {
                    parseString.AppendLine($"            Id = int.Parse(columnStrings[{stringColumnIndex}]);");
                }
                else if (!isNumbered)
                {
                    parseString.AppendLine($"            {name} = {GetParseStringExpression(keyword, stringColumnIndex)};");
                }
                else
                {
                    // numbered column — will be handled in array fill block
                    var match = s_NumberedColumnRegex.Match(name);
                    string baseName = match.Groups[1].Value;
                    int idx = int.Parse(match.Groups[2].Value) - 1; // 0-based
                    parseString.AppendLine($"            {baseName}s[{idx}] = {GetParseStringExpression(keyword, stringColumnIndex)};");
                }

                stringColumnIndex++;

                // Binary parsing
                if (isId)
                {
                    parseBinary.AppendLine($"            Id = reader.Read7BitEncodedInt32();");
                }
                else if (!isNumbered)
                {
                    parseBinary.AppendLine($"            {name} = {GetReadBinaryExpression(keyword)};");
                }
                else
                {
                    var match = s_NumberedColumnRegex.Match(name);
                    string baseName = match.Groups[1].Value;
                    int idx = int.Parse(match.Groups[2].Value) - 1;
                    parseBinary.AppendLine($"            {baseName}s[{idx}] = {GetReadBinaryExpression(keyword)};");
                }
            }

            // Generate array properties and initializers
            foreach (var kvp in numberedGroups)
            {
                string baseName = kvp.Key;
                string keyword = kvp.Value.keyword;
                int count = kvp.Value.maxIndex;

                propertyArrays.AppendLine($"        public {keyword}[] {baseName}s {{ get; }} = new {keyword}[{count}];");
                propertyArrays.AppendLine();
            }

            code.Replace("//@PROPERTIES@", properties.ToString().TrimEnd());
            code.Replace("//@PROPERTY_ARRAY@", propertyArrays.ToString().TrimEnd());
            code.Replace("//@PARSE_STRING@", parseString.ToString().TrimEnd());
            code.Replace("//@PARSE_BINARY@", parseBinary.ToString().TrimEnd());
        }

        private static string GetParseStringExpression(string keyword, int colIndex)
        {
            return keyword switch
            {
                "int" => $"int.Parse(columnStrings[{colIndex}])",
                "long" => $"long.Parse(columnStrings[{colIndex}])",
                "short" => $"short.Parse(columnStrings[{colIndex}])",
                "ushort" => $"ushort.Parse(columnStrings[{colIndex}])",
                "uint" => $"uint.Parse(columnStrings[{colIndex}])",
                "ulong" => $"ulong.Parse(columnStrings[{colIndex}])",
                "byte" => $"byte.Parse(columnStrings[{colIndex}])",
                "sbyte" => $"sbyte.Parse(columnStrings[{colIndex}])",
                "bool" => $"bool.Parse(columnStrings[{colIndex}])",
                "float" => $"float.Parse(columnStrings[{colIndex}])",
                "double" => $"double.Parse(columnStrings[{colIndex}])",
                "string" => $"columnStrings[{colIndex}]",
                _ => $"columnStrings[{colIndex}]",
            };
        }

        private static string GetReadBinaryExpression(string keyword)
        {
            return keyword switch
            {
                "int" => "reader.Read7BitEncodedInt32()",
                "long" => "reader.Read7BitEncodedInt64()",
                "short" => "reader.ReadInt16()",
                "ushort" => "reader.ReadUInt16()",
                "uint" => "(uint)reader.Read7BitEncodedInt32()",
                "ulong" => "(ulong)reader.Read7BitEncodedInt64()",
                "byte" => "reader.ReadByte()",
                "sbyte" => "reader.ReadSByte()",
                "bool" => "reader.ReadBoolean()",
                "float" => "reader.ReadSingle()",
                "double" => "reader.ReadDouble()",
                "string" => "reader.ReadString()",
                _ => "reader.ReadString()",
            };
        }

        // ── Excel helpers ────────────────────────────────────────────────────────

        private static List<string> ReadSharedStrings(WorkbookPart workbookPart)
        {
            var result = new List<string>();
            var sharedStringPart = workbookPart.SharedStringTablePart;
            if (sharedStringPart == null)
            {
                return result;
            }

            foreach (SharedStringItem item in sharedStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                result.Add(item.InnerText);
            }

            return result;
        }

        private static string ReadSheetAsTsv(WorksheetPart worksheetPart, List<string> sharedStrings)
        {
            var sb = new StringBuilder();
            var rows = worksheetPart.Worksheet.Descendants<Row>();

            uint lastRowIndex = 0;
            foreach (Row row in rows)
            {
                uint rowIndex = row.RowIndex!.Value;

                // fill in empty rows
                while (lastRowIndex + 1 < rowIndex)
                {
                    sb.AppendLine();
                    lastRowIndex++;
                }

                lastRowIndex = rowIndex;

                int lastColIndex = -1;
                var cells = row.Elements<Cell>();
                bool first = true;

                foreach (Cell cell in cells)
                {
                    int colIndex = CellReferenceToColumnIndex(cell.CellReference!.Value!);

                    // fill empty columns
                    while (lastColIndex + 1 < colIndex)
                    {
                        if (!first)
                        {
                            sb.Append('\t');
                        }

                        first = false;
                        lastColIndex++;
                    }

                    if (!first)
                    {
                        sb.Append('\t');
                    }

                    first = false;
                    lastColIndex = colIndex;

                    string cellValue = GetCellValue(cell, sharedStrings);
                    sb.Append(cellValue);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string GetCellValue(Cell cell, List<string> sharedStrings)
        {
            if (cell.CellValue == null)
            {
                return string.Empty;
            }

            string raw = cell.CellValue.Text;

            if (cell.DataType?.Value == CellValues.SharedString)
            {
                int index = int.Parse(raw);
                return index < sharedStrings.Count ? sharedStrings[index] : string.Empty;
            }

            return raw;
        }

        private static int CellReferenceToColumnIndex(string cellRef)
        {
            // cellRef e.g. "A1", "BC12"
            int col = 0;
            foreach (char c in cellRef)
            {
                if (c < 'A' || c > 'Z')
                {
                    break;
                }

                col = col * 26 + (c - 'A' + 1);
            }

            return col - 1; // 0-based
        }

        private static string ResolvePath(string relativePath)
        {
            if (Path.IsPathRooted(relativePath))
            {
                return relativePath;
            }

            return Path.GetFullPath(Path.Combine(ProjectRoot, relativePath));
        }
    }
}
