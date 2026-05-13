//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.UI;
using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// UI 界面辅助器。
    /// 负责将 PackedScene 实例化为 Control 节点，并验证根节点已挂载 UIFormLogic 脚本。
    ///
    /// 与 EntityHelper 的关键区别：
    ///   不做递归查找，要求 PackedScene 根节点本身必须是 UIFormLogic 的子类。
    ///   这是因为 UI 场景中大量使用编辑器拖拽绑定（[Export] 字段），
    ///   根节点就是脚本宿主，才能在编辑器中正确序列化引用。
    /// </summary>
    public sealed class UIFormHelper : IUIFormHelper
    {
        private readonly Node m_UIRoot;

        /// <param name="uiRoot">UI 根节点（UIComponent 自身），用于兜底挂载。</param>
        public UIFormHelper(Node uiRoot)
        {
            m_UIRoot = uiRoot;
        }

        /// <summary>
        /// 实例化 PackedScene，返回根节点 Node。
        /// </summary>
        public object InstantiateUIForm(object uiFormAsset)
        {
            if (uiFormAsset is not PackedScene packedScene)
            {
                GameFrameworkLog.Error($"UI form asset '{uiFormAsset}' is not a PackedScene.");
                return null;
            }

            return packedScene.Instantiate();
        }

        /// <summary>
        /// 从实例化后的 Node 中取得 IUIForm。
        /// 约定：根节点必须继承 UIFormLogic，否则报错并释放节点。
        /// 将节点挂到 UIGroupHelper（CanvasLayer）下使其正确分层。
        /// </summary>
        public IUIForm CreateUIForm(object uiFormInstance, IUIGroup uiGroup, object userData)
        {
            if (uiFormInstance is not Node instanceNode)
            {
                GameFrameworkLog.Error($"UI form instance '{uiFormInstance}' is not a Node.");
                return null;
            }

            if (instanceNode is not UIFormLogic uiFormLogic)
            {
                GameFrameworkLog.Error(
                    $"UI form '{instanceNode.Name}' root node is not UIFormLogic. " +
                    "Please attach a UIFormLogic-derived script directly to the PackedScene root.");
                instanceNode.QueueFree();
                return null;
            }

            // 挂到组对应的 CanvasLayer，使深度分层生效
            if (uiGroup?.Helper is UIGroupHelper groupHelper)
            {
                groupHelper.AddChild(instanceNode);
            }
            else
            {
                m_UIRoot.AddChild(instanceNode);
            }

            return uiFormLogic;
        }

        /// <summary>
        /// 释放界面节点。对象池决定何时真正调用此方法。
        /// </summary>
        public void ReleaseUIForm(object uiFormAsset, object uiFormInstance)
        {
            if (uiFormInstance is Node node && GodotObject.IsInstanceValid(node))
            {
                node.QueueFree();
            }
        }
    }
}
