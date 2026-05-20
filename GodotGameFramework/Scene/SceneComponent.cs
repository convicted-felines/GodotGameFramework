using GameFramework;
using GameFramework.Resource;
using GameFramework.Scene;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 场景组件。封装 ISceneManager，以 additive 方式加载/卸载 Godot 场景。
    ///
    /// 设计说明：
    ///   GameFramework 的 SceneManager 内部依赖 IResourceManager 的 LoadScene/UnloadScene。
    ///   ResourceComponent 的 GodotResourceManager 已实现 additive 场景加载（挂载到
    ///   SceneRoot 子节点）和 UnloadScene（free 对应节点）。SceneComponent 仅做透传。
    ///
    /// 使用方式：
    ///   var scene = GameEntry.GetComponent＜SceneComponent＞();
    ///   scene.LoadScene("res://Scenes/Game.tscn");
    ///   scene.UnloadScene("res://Scenes/Game.tscn");
    /// </summary>
    public sealed partial class SceneComponent : GameFrameworkComponent
    {
        private ISceneManager m_SceneManager = null;

        // ── 事件透传 ───────────────────────────────────────────────────────────

        public event EventHandler<LoadSceneSuccessEventArgs> LoadSceneSuccess
        {
            add => m_SceneManager.LoadSceneSuccess += value;
            remove => m_SceneManager.LoadSceneSuccess -= value;
        }

        public event EventHandler<LoadSceneFailureEventArgs> LoadSceneFailure
        {
            add => m_SceneManager.LoadSceneFailure += value;
            remove => m_SceneManager.LoadSceneFailure -= value;
        }

        public event EventHandler<LoadSceneUpdateEventArgs> LoadSceneUpdate
        {
            add => m_SceneManager.LoadSceneUpdate += value;
            remove => m_SceneManager.LoadSceneUpdate -= value;
        }

        public event EventHandler<LoadSceneDependencyAssetEventArgs> LoadSceneDependencyAsset
        {
            add => m_SceneManager.LoadSceneDependencyAsset += value;
            remove => m_SceneManager.LoadSceneDependencyAsset -= value;
        }

        public event EventHandler<UnloadSceneSuccessEventArgs> UnloadSceneSuccess
        {
            add => m_SceneManager.UnloadSceneSuccess += value;
            remove => m_SceneManager.UnloadSceneSuccess -= value;
        }

        public event EventHandler<UnloadSceneFailureEventArgs> UnloadSceneFailure
        {
            add => m_SceneManager.UnloadSceneFailure += value;
            remove => m_SceneManager.UnloadSceneFailure -= value;
        }

        // ── 初始化 ─────────────────────────────────────────────────────────────

        public override void _Ready()
        {
            base._Ready();

            m_SceneManager = GameFrameworkEntry.GetModule<ISceneManager>();
            if (m_SceneManager == null)
            {
                GameFrameworkLog.Fatal("Scene manager is invalid.");
                return;
            }

            // 注入资源管理器（ResourceComponent 必须先于 SceneComponent 初始化）
            var resourceManager = ResourceComponent.Instance;
            if (resourceManager == null)
            {
                GameFrameworkLog.Fatal("Resource manager is invalid. Ensure ResourceComponent is added before SceneComponent.");
                return;
            }

            m_SceneManager.SetResourceManager(resourceManager);
        }

        // ── 查询 ───────────────────────────────────────────────────────────────

        public bool SceneIsLoaded(string sceneAssetName) =>
            m_SceneManager.SceneIsLoaded(sceneAssetName);

        public string[] GetLoadedSceneAssetNames() =>
            m_SceneManager.GetLoadedSceneAssetNames();

        public void GetLoadedSceneAssetNames(List<string> results) =>
            m_SceneManager.GetLoadedSceneAssetNames(results);

        public bool SceneIsLoading(string sceneAssetName) =>
            m_SceneManager.SceneIsLoading(sceneAssetName);

        public string[] GetLoadingSceneAssetNames() =>
            m_SceneManager.GetLoadingSceneAssetNames();

        public void GetLoadingSceneAssetNames(List<string> results) =>
            m_SceneManager.GetLoadingSceneAssetNames(results);

        public bool SceneIsUnloading(string sceneAssetName) =>
            m_SceneManager.SceneIsUnloading(sceneAssetName);

        public string[] GetUnloadingSceneAssetNames() =>
            m_SceneManager.GetUnloadingSceneAssetNames();

        public void GetUnloadingSceneAssetNames(List<string> results) =>
            m_SceneManager.GetUnloadingSceneAssetNames(results);

        public bool HasScene(string sceneAssetName) =>
            m_SceneManager.HasScene(sceneAssetName);

        // ── 加载 / 卸载 ────────────────────────────────────────────────────────

        public void LoadScene(string sceneAssetName) =>
            m_SceneManager.LoadScene(sceneAssetName);

        public void LoadScene(string sceneAssetName, int priority) =>
            m_SceneManager.LoadScene(sceneAssetName, priority);

        public void LoadScene(string sceneAssetName, object userData) =>
            m_SceneManager.LoadScene(sceneAssetName, userData);

        public void LoadScene(string sceneAssetName, int priority, object userData) =>
            m_SceneManager.LoadScene(sceneAssetName, priority, userData);

        public void UnloadScene(string sceneAssetName) =>
            m_SceneManager.UnloadScene(sceneAssetName);

        public void UnloadScene(string sceneAssetName, object userData) =>
            m_SceneManager.UnloadScene(sceneAssetName, userData);
    }
}
