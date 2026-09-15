//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using Godot;

namespace GameMain
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry : Node
    {
        public override void _Ready()
        {
            InitBuiltinComponents();
            InitCustomComponents();

            // 所有 GameFrameworkComponent 已在子节点 _Ready 中注册完毕，此处启动流程 FSM。
            Procedure?.StartProcedures();
        }
    }
}
