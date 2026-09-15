using System;
using System.IO;
using System.Linq;

namespace DataTableGenerator
{
    /// <summary>
    /// DataTableGenerator 命令行工具入口。
    ///
    /// 用法:
    ///   DataTableGenerator excel-all [--excel &lt;dir&gt;] [--text &lt;dir&gt;] [--root &lt;dir&gt;]
    ///       将 Excel 目录下所有 .xlsx/.xlsm 导出为 TSV（对应 Unity 菜单 A）
    ///
    ///   DataTableGenerator excel &lt;excel_file&gt; [--text &lt;dir&gt;] [--root &lt;dir&gt;]
    ///       将单个 Excel 文件所有 Sheet 导出为 TSV
    ///
    ///   DataTableGenerator generate [--names &lt;a,b,c&gt;] [--names-file &lt;file&gt;] [--text &lt;dir&gt;] [--bytes &lt;dir&gt;] [--code &lt;dir&gt;]
    ///       将指定名称的 txt 生成 .bytes + C# 代码（对应 Unity 菜单 B）
    ///
    ///   DataTableGenerator bytes  [--text &lt;dir&gt;] [--bytes &lt;dir&gt;]
    ///       将 text 目录下所有 .txt 文件生成 .bytes
    ///
    ///   DataTableGenerator code   [--text &lt;dir&gt;] [--code &lt;dir&gt;] [--namespace &lt;ns&gt;]
    ///       将 text 目录下所有 .txt 文件生成 C# 数据行代码
    ///
    ///   DataTableGenerator all    [--root &lt;dir&gt;]
    ///       一键完成：Excel 目录导出 → 指定表生成 bytes + 代码
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
            string? excelFile = null;
            string? namesArg = null;
            string? namesFile = null;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--root" && i + 1 < args.Length)
                {
                    DataTableGeneratorUtility.ProjectRoot = Path.GetFullPath(args[++i]);
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
                else if (args[i] == "--excel" && i + 1 < args.Length)
                {
                    DataTableGeneratorUtility.DataTableExcelPath = args[++i];
                }
                else if (args[i] == "--namespace" && i + 1 < args.Length)
                {
                    DataTableGeneratorUtility.CodeNamespace = args[++i];
                }
                else if (args[i] == "--names" && i + 1 < args.Length)
                {
                    namesArg = args[++i];
                }
                else if (args[i] == "--names-file" && i + 1 < args.Length)
                {
                    namesFile = args[++i];
                }
                else if (!args[i].StartsWith("--") && excelFile == null)
                {
                    excelFile = args[i];
                }
            }

            try
            {
                switch (command)
                {
                    case "excel-all":
                        Console.WriteLine("=== Excel -> Text (all files) ===");
                        DataTableGeneratorUtility.ExportAllExcelFromFolder();
                        break;

                    case "excel":
                        if (string.IsNullOrEmpty(excelFile))
                        {
                            Console.Error.WriteLine("Error: excel command requires <excel_file>.");
                            return 1;
                        }

                        DataTableGeneratorUtility.ExportExcelToText(excelFile);
                        break;

                    case "generate":
                        Console.WriteLine("=== Text -> Bytes + Code (named tables) ===");
                        if (!string.IsNullOrEmpty(namesArg))
                        {
                            string[] names = namesArg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            DataTableGeneratorUtility.GenerateDataTables(names);
                        }
                        else
                        {
                            DataTableGeneratorUtility.GenerateDataTablesFromNamesFile(namesFile);
                        }

                        Console.WriteLine("Done.");
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
                        Console.WriteLine("=== Step 1: Excel -> Text ===");
                        DataTableGeneratorUtility.ExportAllExcelFromFolder();

                        Console.WriteLine("=== Step 2: Text -> Bytes + Code ===");
                        DataTableGeneratorUtility.GenerateDataTablesFromNamesFile(namesFile);

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
                return 1;
            }

            return 0;
        }

        private static void PrintUsage()
        {
            Console.WriteLine(
@"DataTableGenerator - 数据表生成工具

用法:
  DataTableGenerator excel-all [--excel <dir>] [--text <dir>] [--root <dir>]
      将 Excel 目录下所有 .xlsx/.xlsm 导出为 TSV（Unity 菜单 A）

  DataTableGenerator excel <excel_file> [--text <dir>] [--root <dir>]
      将单个 Excel 文件所有 Sheet 导出为 TSV

  DataTableGenerator generate [--names Scene,Entity,...] [--names-file <file>]
      将指定名称的 txt 生成 .bytes + C# 代码（Unity 菜单 B）

  DataTableGenerator bytes  [--text <dir>] [--bytes <dir>] [--root <dir>]
      将 text 目录下所有 .txt 生成 .bytes

  DataTableGenerator code   [--text <dir>] [--code <dir>] [--namespace <ns>]
      将 text 目录下所有 .txt 生成 C# IDataRow 代码

  DataTableGenerator all    [--root <dir>] [--names-file <file>]
      一键完成：Excel 目录导出 -> 指定表生成 bytes + 代码

选项:
  --root <dir>        项目根目录              (默认: 当前目录)
  --excel <dir>       Excel 源目录            (默认: MainGame/DataTables/Excel)
  --text <dir>        TSV 文本目录            (默认: MainGame/DataTables/Text)
  --bytes <dir>       二进制输出目录          (默认: MainGame/DataTables/Bytes)
  --code <dir>        C# 代码输出目录         (默认: MainGame/Scripts/DataTable)
  --names <list>      逗号分隔的数据表名称
  --names-file <file> 数据表名称列表文件      (默认: MainGame/DataTables/DataTableNames.txt)
  --namespace <ns>    生成代码的命名空间      (默认: GameMain)

示例:
  DataTableGenerator excel-all --root D:/GodotProject/GodotFramework
  DataTableGenerator generate --root D:/GodotProject/GodotFramework
  DataTableGenerator all --root D:/GodotProject/GodotFramework

文本格式 (Tab 分隔, 与 Unity GameFramework 一致):
  行 0: 标题注释
  行 1: 列名（第 0 列为 #）
  行 2: 列类型 (id / int / string / bool / float / ...)
  行 3: 列说明
  行 4+: 数据行（首列以 # 开头的行为注释行，跳过）
");
        }
    }
}
