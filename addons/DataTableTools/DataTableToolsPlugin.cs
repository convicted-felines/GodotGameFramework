#if TOOLS
using Godot;
using GodotGameFramework.Editor.DataTableTools;

namespace GodotGameFramework.Editor
{
    /// <summary>
    /// 配置表编辑器菜单，对应 Unity 的 DataTables/A 与 DataTables/B。
    /// 入口：编辑器顶部菜单「项目」→「工具」→「DataTables/...」
    /// </summary>
    [Tool]
    public partial class DataTableToolsPlugin : EditorPlugin
    {
        private const string MenuExportExcel = "DataTables/A.Excel导出数据表文本";
        private const string MenuGenerate = "DataTables/B.生成数据表代码和byte文件";
        private const string MenuAll = "DataTables/一键导出并生成";

        public override void _EnterTree()
        {
            AddToolMenuItem(MenuExportExcel, Callable.From(ExportExcelToText));
            AddToolMenuItem(MenuGenerate, Callable.From(GenerateDataTables));
            AddToolMenuItem(MenuAll, Callable.From(ExportAndGenerateAll));
        }

        public override void _ExitTree()
        {
            RemoveToolMenuItem(MenuExportExcel);
            RemoveToolMenuItem(MenuGenerate);
            RemoveToolMenuItem(MenuAll);
        }

        private void ExportExcelToText()
        {
            RunTool("excel-all", "Excel 导出完成");
        }

        private void GenerateDataTables()
        {
            RunTool("generate", "数据表生成完成");
        }

        private void ExportAndGenerateAll()
        {
            RunTool("all", "一键导出并生成完成");
        }

        private void RunTool(string command, string doneMessage)
        {
            string projectRoot = ProjectSettings.GlobalizePath("res://");
            string log = DataTableToolRunner.Run(projectRoot, command, out bool success);

            GD.Print(log);

            if (success)
            {
                GetEditorInterface().GetResourceFilesystem().Scan();
                GD.Print($"[DataTableTools] {doneMessage}");
            }
            else
            {
                GD.PrintErr($"[DataTableTools] 操作失败，请查看上方日志。");
            }
        }
    }
}
#endif
