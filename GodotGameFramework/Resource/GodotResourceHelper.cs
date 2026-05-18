//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.Resource;
using Godot;
using System.Threading.Tasks;

namespace GodotGameFramework
{
    /// <summary>
    /// 资源辅助器：负责平台级别的文件读取、场景卸载和资源释放。
    /// </summary>
    public sealed class GodotResourceHelper : IResourceHelper
    {
        // ── IResourceHelper ────────────────────────────────────────────────────

        /// <summary>
        /// 从 fileUri 异步读取原始字节，完成后触发回调。
        /// fileUri 支持 res:// / user:// / 绝对磁盘路径。
        /// </summary>
        public void LoadBytes(string fileUri, LoadBytesCallbacks loadBytesCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(fileUri))
            {
                loadBytesCallbacks.LoadBytesFailureCallback?.Invoke(fileUri, "File URI is invalid.", userData);
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    byte[] bytes = ReadAllBytes(fileUri);
                    if (bytes == null)
                    {
                        loadBytesCallbacks.LoadBytesFailureCallback?.Invoke(
                            fileUri, $"Cannot open file: {fileUri}", userData);
                        return;
                    }
                    loadBytesCallbacks.LoadBytesSuccessCallback(fileUri, bytes, 0f, userData);
                }
                catch (System.Exception ex)
                {
                    loadBytesCallbacks.LoadBytesFailureCallback?.Invoke(fileUri, ex.Message, userData);
                }
            });
        }

        /// <summary>
        /// 卸载场景：在 Godot 中使用 SceneTree.UnloadCurrentScene 或
        /// 移除已加载为子节点的 additive 场景节点。
        /// </summary>
        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            if (GodotResourceManager.TryGetAdditiveScene(sceneAssetName, out var sceneNode))
            {
                sceneNode.QueueFree();
                GodotResourceManager.RemoveAdditiveScene(sceneAssetName);
                unloadSceneCallbacks.UnloadSceneSuccessCallback(sceneAssetName, userData);
            }
            else
            {
                unloadSceneCallbacks.UnloadSceneFailureCallback?.Invoke(sceneAssetName, userData);
            }
        }

        /// <summary>
        /// 释放资源：Godot 使用引用计数，将引用置空即可让 GC 回收。
        /// </summary>
        public void Release(object objectToRelease)
        {
            // Godot Resource 对象是引用计数的，调用方将持有的引用赋 null 后自动回收。
            // 此处无需显式操作；保留空实现供未来扩展（如卸载 PCK 包）。
        }

        // ── 内部工具 ───────────────────────────────────────────────────────────

        internal static byte[] ReadAllBytes(string path)
        {
            // res:// 和 user:// 路径使用 Godot FileAccess
            if (path.StartsWith("res://") || path.StartsWith("user://"))
            {
                using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                if (file == null)
                    return null;
                return file.GetBuffer((long)file.GetLength());
            }

            // 绝对磁盘路径使用 System.IO
            if (!System.IO.File.Exists(path))
                return null;
            return System.IO.File.ReadAllBytes(path);
        }
    }
}
