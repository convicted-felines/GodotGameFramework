//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework.UI;
using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// UI 组辅助器基类。继承此类实现自定义 UI 组辅助器，在 UIComponent 的 Inspector 中填写完整类名即可生效。
    /// 每个 UI 组会创建一个对应的辅助器实例（CanvasLayer）挂载到 UIComponent 下。
    /// </summary>
    public abstract partial class UIGroupHelperBase : CanvasLayer, IUIGroupHelper
    {
        public abstract void SetDepth(int depth);
    }
}
