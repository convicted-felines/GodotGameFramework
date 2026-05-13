//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework.UI;
using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// UI 界面逻辑基类。
    /// 与 EntityLogic 不同：用户的 UI 脚本必须直接挂在 PackedScene 的根节点上，
    /// 因为 UI 场景通常有大量节点引用通过编辑器拖拽绑定，不能在运行时动态查找。
    /// 继承此类后在编辑器中拖拽好所有 [Export] 引用，框架自动管理生命周期和对象池。
    ///
    /// 约定：PackedScene 的根节点类型继承 UIFormLogic（Control 或其子类均可）。
    /// </summary>
    public abstract partial class UIFormLogic : Control, IUIForm
    {
        private int m_SerialId;
        private string m_UIFormAssetName;
        private IUIGroup m_UIGroup;
        private bool m_PauseCoveredUIForm;
        private int m_DepthInUIGroup;

        // ── IUIForm 属性 ──────────────────────────────────────────────────────

        public int SerialId => m_SerialId;
        public string UIFormAssetName => m_UIFormAssetName;
        public object Handle => this;
        public IUIGroup UIGroup => m_UIGroup;
        public int DepthInUIGroup => m_DepthInUIGroup;
        public bool PauseCoveredUIForm => m_PauseCoveredUIForm;

        // ── IUIForm 生命周期（由 UIManager 调用，不要手动调用）──────────────────

        void IUIForm.OnInit(int serialId, string uiFormAssetName, IUIGroup uiGroup, bool pauseCoveredUIForm, bool isNewInstance, object userData)
        {
            m_SerialId = serialId;
            m_UIFormAssetName = uiFormAssetName;
            m_UIGroup = uiGroup;
            m_PauseCoveredUIForm = pauseCoveredUIForm;
            m_DepthInUIGroup = 0;
            Hide();
            OnInit(isNewInstance, userData);
        }

        void IUIForm.OnRecycle()
        {
            m_SerialId = 0;
            OnRecycle();
        }

        void IUIForm.OnOpen(object userData)
        {
            Show();
            OnOpen(userData);
        }

        void IUIForm.OnClose(bool isShutdown, object userData)
        {
            Hide();
            OnClose(isShutdown, userData);
        }

        void IUIForm.OnPause()
        {
            OnPause();
        }

        void IUIForm.OnResume()
        {
            OnResume();
        }

        void IUIForm.OnCover()
        {
            OnCover();
        }

        void IUIForm.OnReveal()
        {
            OnReveal();
        }

        void IUIForm.OnRefocus(object userData)
        {
            OnRefocus(userData);
        }

        void IUIForm.OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            OnUpdate(elapseSeconds, realElapseSeconds);
        }

        void IUIForm.OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            m_DepthInUIGroup = depthInUIGroup;
            OnDepthChanged(uiGroupDepth, depthInUIGroup);
        }

        // ── 供子类重写的虚方法 ────────────────────────────────────────────────

        /// <summary>界面初始化（首次从对象池取出时调用）。</summary>
        protected virtual void OnInit(bool isNewInstance, object userData) { }

        /// <summary>界面回收（已放回对象池）。</summary>
        protected virtual void OnRecycle() { }

        /// <summary>界面打开（每次显示时调用，对应 OnShow）。</summary>
        protected virtual void OnOpen(object userData) { }

        /// <summary>界面关闭（每次隐藏时调用，回池前）。</summary>
        protected virtual void OnClose(bool isShutdown, object userData) { }

        /// <summary>界面被上层界面遮挡且配置了 pauseCovered 时暂停。</summary>
        protected virtual void OnPause() { }

        /// <summary>遮挡解除后恢复。</summary>
        protected virtual void OnResume() { }

        /// <summary>界面被上层界面遮挡（未暂停）。</summary>
        protected virtual void OnCover() { }

        /// <summary>遮挡解除（未暂停）。</summary>
        protected virtual void OnReveal() { }

        /// <summary>界面被重新激活（已显示时再次打开同一界面）。</summary>
        protected virtual void OnRefocus(object userData) { }

        /// <summary>界面每帧更新。</summary>
        protected virtual void OnUpdate(float elapseSeconds, float realElapseSeconds) { }

        /// <summary>界面在组内深度发生变化时调用（可用于调整 ZIndex）。</summary>
        protected virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            ZIndex = depthInUIGroup;
        }
    }
}
