//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace GameFramework
{
    /// <summary>
    /// 游戏框架入口。
    /// </summary>
    public static class GameFrameworkEntry
    {
        private static readonly GameFrameworkLinkedList<GameFrameworkModule> s_GameFrameworkModules = new GameFrameworkLinkedList<GameFrameworkModule>();

        /// <summary>
        /// 所有游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public static void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (GameFrameworkModule module in s_GameFrameworkModules)
            {
                module.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理所有游戏框架模块。
        /// </summary>
        public static void Shutdown()
        {
            for (LinkedListNode<GameFrameworkModule> current = s_GameFrameworkModules.Last; current != null; current = current.Previous)
            {
                current.Value.Shutdown();
            }

            s_GameFrameworkModules.Clear();
            ReferencePool.ClearAll();
            Utility.Marshal.FreeCachedHGlobal();
            GameFrameworkLog.SetLogHelper(null);
        }

        /// <summary>
        /// 注册一个已创建的游戏框架模块（用于注入自定义实现替代自动创建的默认模块）。
        /// 若相同类型的模块已存在则替换之。
        /// </summary>
        /// <param name="module">要注册的模块实例。</param>
        public static void RegisterModule(GameFrameworkModule module)
        {
            if (module == null)
            {
                throw new GameFrameworkException("Module is invalid.");
            }

            // 移除已有的同类型模块
            LinkedListNode<GameFrameworkModule> current = s_GameFrameworkModules.First;
            while (current != null)
            {
                if (current.Value.GetType() == module.GetType())
                {
                    s_GameFrameworkModules.Remove(current);
                    break;
                }
                current = current.Next;
            }

            // 按优先级插入
            current = s_GameFrameworkModules.First;
            while (current != null)
            {
                if (module.Priority > current.Value.Priority)
                    break;
                current = current.Next;
            }

            if (current != null)
                s_GameFrameworkModules.AddBefore(current, module);
            else
                s_GameFrameworkModules.AddLast(module);
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架模块类型。</typeparam>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        public static T GetModule<T>() where T : class
        {
            Type interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
            {
                throw new GameFrameworkException(Utility.Text.Format("You must get module by interface, but '{0}' is not.", interfaceType.FullName));
            }

            if (!interfaceType.FullName.StartsWith("GameFramework.", StringComparison.Ordinal))
            {
                throw new GameFrameworkException(Utility.Text.Format("You must get a Game Framework module, but '{0}' is not.", interfaceType.FullName));
            }

            // 先查找已注册的、实现了该接口的模块（允许外部注入替代实现）
            foreach (GameFrameworkModule module in s_GameFrameworkModules)
            {
                if (interfaceType.IsInstanceOfType(module))
                    return module as T;
            }

            string moduleName = Utility.Text.Format("{0}.{1}", interfaceType.Namespace, interfaceType.Name.Substring(1));
            Type moduleType = Type.GetType(moduleName);
            if (moduleType == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find Game Framework module type '{0}'.", moduleName));
            }

            return GetModule(moduleType) as T;
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要获取的游戏框架模块类型。</param>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        private static GameFrameworkModule GetModule(Type moduleType)
        {
            foreach (GameFrameworkModule module in s_GameFrameworkModules)
            {
                if (module.GetType() == moduleType)
                {
                    return module;
                }
            }

            return CreateModule(moduleType);
        }

        /// <summary>
        /// 创建游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要创建的游戏框架模块类型。</param>
        /// <returns>要创建的游戏框架模块。</returns>
        private static GameFrameworkModule CreateModule(Type moduleType)
        {
            GameFrameworkModule module = (GameFrameworkModule)Activator.CreateInstance(moduleType);
            if (module == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not create module '{0}'.", moduleType.FullName));
            }

            LinkedListNode<GameFrameworkModule> current = s_GameFrameworkModules.First;
            while (current != null)
            {
                if (module.Priority > current.Value.Priority)
                {
                    break;
                }

                current = current.Next;
            }

            if (current != null)
            {
                s_GameFrameworkModules.AddBefore(current, module);
            }
            else
            {
                s_GameFrameworkModules.AddLast(module);
            }

            return module;
        }
    }
}
