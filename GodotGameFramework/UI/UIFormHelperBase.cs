//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework.UI;
using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// UI 界面辅助器基类。继承此类实现自定义 UI 辅助器，在 UIComponent 的 Inspector 中填写完整类名即可生效。
    /// 辅助器会被作为子节点挂载到 UIComponent 下，可通过 UIRoot 访问父节点。
    /// </summary>
    public abstract partial class UIFormHelperBase : Node, IUIFormHelper
    {
        /// <summary>UI 根节点（即 UIComponent 自身）。</summary>
        protected Node UIRoot => GetParent();

        public abstract object InstantiateUIForm(object uiFormAsset);
        public abstract IUIForm CreateUIForm(object uiFormInstance, IUIGroup uiGroup, object userData);
        public abstract void ReleaseUIForm(object uiFormAsset, object uiFormInstance);
    }
}
