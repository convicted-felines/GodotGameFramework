//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using GameFramework.UI;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// UI 组件。
    /// 封装 IUIManager，提供 UI 界面的打开/关闭/查询/激活等能力。
    /// 在 Godot 场景树中作为 Node 存在，自动注册到 GameEntry。
    ///
    /// 使用步骤：
    ///   1. 在场景树中添加 UIComponent 节点（BaseComponent 之后）。
    ///   2. 在 Inspector 中配置 UIGroupNames 和对应的 UIGroupDepths。
    ///   3. 用户 UI 场景：PackedScene 根节点继承 UIFormLogic，直接拖拽引用。
    ///   4. 调用 OpenUIForm / CloseUIForm 驱动 UI 生命周期。
    /// </summary>
    public sealed partial class UIComponent : GameFrameworkComponent
    {
        // ── Inspector 配置 ─────────────────────────────────────────────────────

        /// <summary>要预先注册的 UI 组名称列表（顺序与 UIGroupDepths 对应）。</summary>
        [Godot.Export] public string[] UIGroupNames = Array.Empty<string>();

        /// <summary>各 UI 组的 CanvasLayer.Layer 深度（数值越大越靠前）。</summary>
        [Godot.Export] public int[] UIGroupDepths = Array.Empty<int>();

        /// <summary>对象池自动释放间隔（秒）。</summary>
        [Godot.Export] public float InstanceAutoReleaseInterval = 60f;

        /// <summary>对象池容量上限。</summary>
        [Godot.Export] public int InstanceCapacity = 16;

        /// <summary>对象池对象过期时间（秒）。</summary>
        [Godot.Export] public float InstanceExpireTime = 60f;

        /// <summary>对象池优先级。</summary>
        [Godot.Export] public int InstancePriority = 0;

        // ── 内部状态 ───────────────────────────────────────────────────────────

        private IUIManager m_UIManager = null;

        // ── 属性代理 ───────────────────────────────────────────────────────────

        public int UIGroupCount => m_UIManager.UIGroupCount;

        public float AutoReleaseInterval
        {
            get => m_UIManager.InstanceAutoReleaseInterval;
            set => m_UIManager.InstanceAutoReleaseInterval = value;
        }

        public int Capacity
        {
            get => m_UIManager.InstanceCapacity;
            set => m_UIManager.InstanceCapacity = value;
        }

        public float ExpireTime
        {
            get => m_UIManager.InstanceExpireTime;
            set => m_UIManager.InstanceExpireTime = value;
        }

        public int Priority
        {
            get => m_UIManager.InstancePriority;
            set => m_UIManager.InstancePriority = value;
        }

        // ── 事件（透传给用户订阅） ────────────────────────────────────────────

        public event EventHandler<OpenUIFormSuccessEventArgs> OpenUIFormSuccess
        {
            add => m_UIManager.OpenUIFormSuccess += value;
            remove => m_UIManager.OpenUIFormSuccess -= value;
        }

        public event EventHandler<OpenUIFormFailureEventArgs> OpenUIFormFailure
        {
            add => m_UIManager.OpenUIFormFailure += value;
            remove => m_UIManager.OpenUIFormFailure -= value;
        }

        public event EventHandler<OpenUIFormUpdateEventArgs> OpenUIFormUpdate
        {
            add => m_UIManager.OpenUIFormUpdate += value;
            remove => m_UIManager.OpenUIFormUpdate -= value;
        }

        public event EventHandler<OpenUIFormDependencyAssetEventArgs> OpenUIFormDependencyAsset
        {
            add => m_UIManager.OpenUIFormDependencyAsset += value;
            remove => m_UIManager.OpenUIFormDependencyAsset -= value;
        }

        public event EventHandler<CloseUIFormCompleteEventArgs> CloseUIFormComplete
        {
            add => m_UIManager.CloseUIFormComplete += value;
            remove => m_UIManager.CloseUIFormComplete -= value;
        }

        // ── 初始化 ─────────────────────────────────────────────────────────────

        public override void _Ready()
        {
            base._Ready();

            m_UIManager = GameFrameworkEntry.GetModule<IUIManager>();
            if (m_UIManager == null)
            {
                GameFrameworkLog.Fatal("UI manager is invalid.");
                return;
            }

            // 注入对象池
            var objectPoolManager = GameFrameworkEntry.GetModule<IObjectPoolManager>();
            if (objectPoolManager == null)
            {
                GameFrameworkLog.Fatal("Object pool manager is invalid.");
                return;
            }
            m_UIManager.SetObjectPoolManager(objectPoolManager);

            // 注入对象池参数
            m_UIManager.InstanceAutoReleaseInterval = InstanceAutoReleaseInterval;
            m_UIManager.InstanceCapacity = InstanceCapacity;
            m_UIManager.InstanceExpireTime = InstanceExpireTime;
            m_UIManager.InstancePriority = InstancePriority;

            // 注入资源管理器（可选）
            var resourceManager = GameFrameworkEntry.GetModule<IResourceManager>();
            if (resourceManager != null)
            {
                m_UIManager.SetResourceManager(resourceManager);
            }

            // 注入 UI 辅助器
            m_UIManager.SetUIFormHelper(new UIFormHelper(this));

            // 注册 Inspector 中配置的 UI 组
            RegisterUIGroupsFromExport();
        }

        // ── UI 组操作 ──────────────────────────────────────────────────────────

        public bool HasUIGroup(string uiGroupName) => m_UIManager.HasUIGroup(uiGroupName);
        public IUIGroup GetUIGroup(string uiGroupName) => m_UIManager.GetUIGroup(uiGroupName);
        public IUIGroup[] GetAllUIGroups() => m_UIManager.GetAllUIGroups();
        public void GetAllUIGroups(List<IUIGroup> results) => m_UIManager.GetAllUIGroups(results);

        /// <summary>
        /// 动态添加 UI 组（运行时调用）。
        /// </summary>
        public bool AddUIGroup(string uiGroupName, int depth = 0)
        {
            if (m_UIManager.HasUIGroup(uiGroupName))
                return false;

            var helper = CreateGroupHelperNode(uiGroupName, depth);
            return m_UIManager.AddUIGroup(uiGroupName, depth, helper);
        }

        // ── 界面查询 ───────────────────────────────────────────────────────────

        public bool HasUIForm(int serialId) => m_UIManager.HasUIForm(serialId);
        public bool HasUIForm(string uiFormAssetName) => m_UIManager.HasUIForm(uiFormAssetName);

        public IUIForm GetUIForm(int serialId) => m_UIManager.GetUIForm(serialId);
        public IUIForm GetUIForm(string uiFormAssetName) => m_UIManager.GetUIForm(uiFormAssetName);
        public IUIForm[] GetUIForms(string uiFormAssetName) => m_UIManager.GetUIForms(uiFormAssetName);
        public void GetUIForms(string uiFormAssetName, List<IUIForm> results) => m_UIManager.GetUIForms(uiFormAssetName, results);

        public IUIForm[] GetAllLoadedUIForms() => m_UIManager.GetAllLoadedUIForms();
        public void GetAllLoadedUIForms(List<IUIForm> results) => m_UIManager.GetAllLoadedUIForms(results);

        public int[] GetAllLoadingUIFormSerialIds() => m_UIManager.GetAllLoadingUIFormSerialIds();
        public void GetAllLoadingUIFormSerialIds(List<int> results) => m_UIManager.GetAllLoadingUIFormSerialIds(results);

        public bool IsLoadingUIForm(int serialId) => m_UIManager.IsLoadingUIForm(serialId);
        public bool IsLoadingUIForm(string uiFormAssetName) => m_UIManager.IsLoadingUIForm(uiFormAssetName);
        public bool IsValidUIForm(IUIForm uiForm) => m_UIManager.IsValidUIForm(uiForm);

        // ── 打开 / 关闭 ────────────────────────────────────────────────────────

        public int OpenUIForm(string uiFormAssetName, string uiGroupName) =>
            m_UIManager.OpenUIForm(uiFormAssetName, uiGroupName);

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority) =>
            m_UIManager.OpenUIForm(uiFormAssetName, uiGroupName, priority);

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm) =>
            m_UIManager.OpenUIForm(uiFormAssetName, uiGroupName, pauseCoveredUIForm);

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, object userData) =>
            m_UIManager.OpenUIForm(uiFormAssetName, uiGroupName, userData);

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm) =>
            m_UIManager.OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm);

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, object userData) =>
            m_UIManager.OpenUIForm(uiFormAssetName, uiGroupName, priority, userData);

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData) =>
            m_UIManager.OpenUIForm(uiFormAssetName, uiGroupName, pauseCoveredUIForm, userData);

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm, object userData) =>
            m_UIManager.OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, userData);

        public void CloseUIForm(int serialId) => m_UIManager.CloseUIForm(serialId);
        public void CloseUIForm(int serialId, object userData) => m_UIManager.CloseUIForm(serialId, userData);
        public void CloseUIForm(IUIForm uiForm) => m_UIManager.CloseUIForm(uiForm);
        public void CloseUIForm(IUIForm uiForm, object userData) => m_UIManager.CloseUIForm(uiForm, userData);

        public void CloseAllLoadedUIForms() => m_UIManager.CloseAllLoadedUIForms();
        public void CloseAllLoadedUIForms(object userData) => m_UIManager.CloseAllLoadedUIForms(userData);
        public void CloseAllLoadingUIForms() => m_UIManager.CloseAllLoadingUIForms();

        // ── 激活 / 对象池控制 ─────────────────────────────────────────────────

        public void RefocusUIForm(IUIForm uiForm) => m_UIManager.RefocusUIForm(uiForm);
        public void RefocusUIForm(IUIForm uiForm, object userData) => m_UIManager.RefocusUIForm(uiForm, userData);

        public void SetUIFormInstanceLocked(object uiFormInstance, bool locked) =>
            m_UIManager.SetUIFormInstanceLocked(uiFormInstance, locked);

        public void SetUIFormInstancePriority(object uiFormInstance, int priority) =>
            m_UIManager.SetUIFormInstancePriority(uiFormInstance, priority);

        // ── 私有辅助 ───────────────────────────────────────────────────────────

        private void RegisterUIGroupsFromExport()
        {
            int count = UIGroupNames?.Length ?? 0;
            for (int i = 0; i < count; i++)
            {
                string groupName = UIGroupNames[i];
                if (string.IsNullOrEmpty(groupName) || m_UIManager.HasUIGroup(groupName))
                    continue;

                int depth = i < UIGroupDepths.Length ? UIGroupDepths[i] : i;
                var helper = CreateGroupHelperNode(groupName, depth);
                m_UIManager.AddUIGroup(groupName, depth, helper);
            }
        }

        /// <summary>为 UI 组创建 CanvasLayer 容器节点并挂到 UIComponent 下。</summary>
        private UIGroupHelper CreateGroupHelperNode(string groupName, int depth)
        {
            var helper = new UIGroupHelper();
            helper.Name = $"UIGroup_{groupName}";
            helper.Layer = depth;
            AddChild(helper);
            return helper;
        }
    }
}
