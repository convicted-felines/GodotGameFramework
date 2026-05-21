//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework.UI;
using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// UI 界面组辅助器。
    /// 每个 UI 组对应一个 CanvasLayer，通过 Layer 属性控制渲染深度（前后遮挡顺序）。
    /// UIComponent 会为每个界面组自动创建此节点并挂到自身下方。
    /// </summary>
    public sealed partial class UIGroupHelper : UIGroupHelperBase
    {
        public override void SetDepth(int depth)
        {
            Layer = depth;
        }
    }
}
