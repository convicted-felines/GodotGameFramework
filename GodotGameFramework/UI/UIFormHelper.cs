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
    /// 默认 UI 界面辅助器。
    /// 负责将 PackedScene 实例化为 Control 节点，并验证根节点已挂载 UIFormLogic 脚本。
    /// </summary>
    public sealed partial class UIFormHelper : UIFormHelperBase
    {
        public override object InstantiateUIForm(object uiFormAsset)
        {
            if (uiFormAsset is not PackedScene packedScene)
            {
                GameFrameworkLog.Error($"UI form asset '{uiFormAsset}' is not a PackedScene.");
                return null;
            }

            return packedScene.Instantiate();
        }

        public override IUIForm CreateUIForm(object uiFormInstance, IUIGroup uiGroup, object userData)
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

            if (uiGroup?.Helper is UIGroupHelperBase groupHelper)
            {
                groupHelper.AddChild(instanceNode);
            }
            else
            {
                UIRoot.AddChild(instanceNode);
            }

            return uiFormLogic;
        }

        public override void ReleaseUIForm(object uiFormAsset, object uiFormInstance)
        {
            if (uiFormInstance is Node node && GodotObject.IsInstanceValid(node))
            {
                node.QueueFree();
            }
        }
    }
}
