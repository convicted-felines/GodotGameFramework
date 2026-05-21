//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework.Resource;

namespace GodotGameFramework
{
    /// <summary>
    /// 资源辅助器基类。继承此类实现自定义资源辅助器，在 ResourceComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class ResourceHelperBase : IResourceHelper
    {
        public abstract void LoadBytes(string fileUri, LoadBytesCallbacks loadBytesCallbacks, object userData);
        public abstract void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData);
        public abstract void Release(object objectToRelease);
    }
}
