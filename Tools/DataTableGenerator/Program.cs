using System;
using System.IO;

namespace DataTableGenerator
{
    /// <summary>
    /// DataTableGenerator 命令行工具入口。
    ///
    /// 用法:
    ///   DataTableGenerator excel  &lt;excel_file&gt; [--text &lt;dir&gt;]
    ///       将 Excel 文件所有 Sheet 导出为 TSV 文本文件
    ///
    ///   DataTableGenerator bytes  [--text &lt;dir&gt;] [--bytes &lt;dir&gt;]
    ///       将 text 目录下所有 .txt 文件生成 .bytes 二进制文件
    ///
    ///   DataTableGenerator code   [--text &lt;dir&gt;] [--code &lt;dir&gt;] [--namespace &lt;ns&gt;]
    ///       将 text 目录下所有 .txt 文件生成 C# 数据行代码文件
    ///
    ///   DataTableGenerator all    &lt;excel_file&gt; [--text &lt;dir&gt;] [--bytes &lt;dir&gt;] [--code &lt;dir&gt;] [--namespace &lt;ns&gt;]
    ///       一键完成：Excel → TSV → bytes + C# 代码
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 0;
            }

            string command = args[0].ToLowerInvariant();

            // Parse options
            string? excelFile = null;
            for (int i = 1; i < args.Length; i++)
            {
                if (!args[i].StartsWith("--") && excelFile == null)
                {
                    excelFile = args[i];
                }
                else if (args[i] == "--text" && i + 1 < args.Length)
                {
                    DataTableGeneratorUtility.DataTableTextPath = args[++i];
                }
                else if (args[i] == "--bytes" && i + 1 < args.Length)
                {
                    DataTableGeneratorUtility.DataTableBytesPath = args[++i];
                }
                else if (args[i] == "--code" && i + 1 < args.Length)
                {
                    DataTableGeneratorUtility.DataTableCodePath = args[++i];
                }
                else if (args[i] == "--namespace" && i + 1 < args.Length)
                {
                    DataTableGeneratorUtility.CodeNamespace = args[++i];
                }
            }

            try
            {
                switch (command)
                {
                    case "excel":
                        if (string.IsNullOrEmpty(excelFile))
                        {
                            Console.Error.WriteLine("Error: excel command requires <excel_file>.");
                            return 1;
                        }

                        DataTableGeneratorUtility.ExportExcelToText(excelFile);
                        break;

                    case "bytes":
                        Console.WriteLine($"Generating bytes files from '{DataTableGeneratorUtility.DataTableTextPath}'...");
                        DataTableGeneratorUtility.GenerateAllDataFiles();
                        Console.WriteLine("Done.");
                        break;

                    case "code":
                        Console.WriteLine($"Generating code files from '{DataTableGeneratorUtility.DataTableTextPath}'...");
                        DataTableGeneratorUtility.GenerateAllCodeFiles();
                        Console.WriteLine("Done.");
                        break;

                    case "all":
                        if (string.IsNullOrEmpty(excelFile))
                        {
                            Console.Error.WriteLine("Error: all command requires <excel_file>.");
                            return 1;
                        }

                        Console.WriteLine("=== Step 1: Excel → Text ===");
                        DataTableGeneratorUtility.ExportExcelToText(excelFile);

                        Console.WriteLine("=== Step 2: Text → Bytes ===");
                        DataTableGeneratorUtility.GenerateAllDataFiles();

                        Console.WriteLine("=== Step 3: Text → Code ===");
                        DataTableGeneratorUtility.GenerateAllCodeFiles();

                        Console.WriteLine("=== Done ===");
                        break;

                    default:
                        Console.Error.WriteLine($"Unknown command: {command}");
                        PrintUsage();
                        return 1;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }

            return 0;
        }

        private static void PrintUsage()
        {
            Console.WriteLine(
@"DataTableGenerator - 数据表生成工具

用法:
  DataTableGenerator excel  <excel_file> [--text <dir>]
      将 Excel (.xlsx) 文件所有 Sheet 导出为 TSV 文本文件

  DataTableGenerator bytes  [--text <dir>] [--bytes <dir>]
      将 text 目录下所有 .txt 文件生成 .bytes 二进制文件

  DataTableGenerator code   [--text <dir>] [--code <dir>] [--namespace <ns>]
      将 text 目录下所有 .txt 文件生成 C# IDataRow 实现代码

  DataTableGenerator all    <excel_file> [options]
      一键完成：Excel → TSV → .bytes + C# 代码

选项:
  --text <dir>        TSV 文本文件目录      (默认: DataTables/Text)
  --bytes <dir>       二进制输出目录        (默认: DataTables/Bytes)
  --code <dir>        C# 代码输出目录       (默认: DataTables/Code)
  --namespace <ns>    生成代码的命名空间    (默认: GameMain)

示例:
  DataTableGenerator all GameData.xlsx --text Assets/DataTables/Text --bytes Assets/DataTables/Bytes --code Assets/Scripts/DataTable --namespace MyGame

文本格式 (Tab 分隔):
  行 0: 列名（第 0 列为注释列）
  行 1: 列类型 (id / int / string / bool / float / long / ...)
  行 2: 默认值（可留空）
  行 3: 列说明注释
  行 4+: 数据行（首列以 # 开头的行为注释行，跳过）
");
        }
    }
}
