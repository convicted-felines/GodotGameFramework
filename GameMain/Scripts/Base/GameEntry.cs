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
        }
    }
}
