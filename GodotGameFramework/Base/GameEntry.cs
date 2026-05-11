//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 游戏入口，对应 Unity 版本的 GameEntry。
    /// 作为静态服务定位器，管理所有 GameFrameworkComponent。
    /// </summary>
    public static class GameEntry
    {
        private static readonly GameFrameworkLinkedList<GameFrameworkComponent> s_GameFrameworkComponents = new GameFrameworkLinkedList<GameFrameworkComponent>();

        /// <summary>
        /// 获取游戏框架组件。
        /// </summary>
        public static T GetComponent<T>() where T : GameFrameworkComponent
        {
            return (T)GetComponent(typeof(T));
        }

        /// <summary>
        /// 获取游戏框架组件。
        /// </summary>
        public static GameFrameworkComponent GetComponent(Type type)
        {
            LinkedListNode<GameFrameworkComponent> current = s_GameFrameworkComponents.First;
            while (current != null)
            {
                if (current.Value.GetType() == type)
                    return current.Value;
                current = current.Next;
            }
            return null;
        }

        /// <summary>
        /// 获取游戏框架组件。
        /// </summary>
        public static GameFrameworkComponent GetComponent(string typeName)
        {
            LinkedListNode<GameFrameworkComponent> current = s_GameFrameworkComponents.First;
            while (current != null)
            {
                Type type = current.Value.GetType();
                if (type.FullName == typeName || type.Name == typeName)
                    return current.Value;
                current = current.Next;
            }
            return null;
        }

        /// <summary>
        /// 关闭游戏框架。
        /// </summary>
        public static void Shutdown(ShutdownType shutdownType)
        {
            switch (shutdownType)
            {
                case ShutdownType.None:
                    break;
                case ShutdownType.Restart:
                    // 由 BaseComponent 负责重启场景
                    break;
                case ShutdownType.Quit:
                    break;
            }
        }

        /// <summary>
        /// 注册游戏框架组件（由 GameFrameworkComponent._Ready 调用）。
        /// </summary>
        internal static void RegisterComponent(GameFrameworkComponent gameFrameworkComponent)
        {
            if (gameFrameworkComponent == null)
                throw new GameFrameworkException("Game framework component is invalid.");

            Type type = gameFrameworkComponent.GetType();
            LinkedListNode<GameFrameworkComponent> current = s_GameFrameworkComponents.First;
            while (current != null)
            {
                if (current.Value.GetType() == type)
                    throw new GameFrameworkException($"Game framework component type '{type.FullName}' is already exist.");
                current = current.Next;
            }

            s_GameFrameworkComponents.AddLast(gameFrameworkComponent);
        }

        internal static void UnregisterComponent(GameFrameworkComponent gameFrameworkComponent)
        {
            if (gameFrameworkComponent == null)
                return;
            s_GameFrameworkComponents.Remove(gameFrameworkComponent);
        }
    }
}
