//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// 游戏框架组件抽象类，对应 Unity 版本的 GameFrameworkComponent (MonoBehaviour)。
    /// 所有框架组件 Node 均继承此类，在 _Ready 时自动注册到 GameEntry。
    /// </summary>
    public abstract partial class GameFrameworkComponent : Node
    {
        public override void _Ready()
        {
            GameEntry.RegisterComponent(this);
        }
    }
}
