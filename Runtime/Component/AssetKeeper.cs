using UnityEngine;

namespace cdc.AssetWorkflow
{
    public class AssetKeeper : MonoBehaviour
    {
        private IAssetHandle m_handle;

        public IAssetHandle CurHandle => m_handle;

        public void SetHandle(IAssetHandle handle)
        {
            if (handle != m_handle)
            {
                m_handle?.DecRef();
                m_handle = null;
            }
            handle.AddRef();
            m_handle = handle;
        }

        private void OnDestroy()
        {
            m_handle?.DecRef();
            m_handle = null;
        }

        /// <summary>
        /// Associate specific AssetHandles with GameObjects, let them to be released  with the lifecycle of the GameObject
        /// </summary>
        /// <returns></returns>
        public static AssetKeeper Link(IAssetHandle handle, GameObject obj, AssetKeeper curKeeper = null)
        {
            if (curKeeper == null)
                curKeeper = obj.AddComponent<AssetKeeper>();
            curKeeper.SetHandle(handle);
            return curKeeper;
        }
    }
}