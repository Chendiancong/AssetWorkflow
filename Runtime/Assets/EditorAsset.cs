using System;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    public class EditorAsset : IAssetHandle
    {
        private string m_name;
        private int m_refCount = 0;
        private UnityEngine.Object m_asset;
        private bool m_isValid = false;
        private ManagedAssetState m_state;

        public bool IsValid => m_isValid;

        public string Name => m_name;

        public ManagedAssetState State => m_state;

        public float LoadingProgress => m_state >= ManagedAssetState.Loaded ? 1f : 0f;

        public int RefCount => m_refCount;

        public EditorAsset(string name)
        {
            m_name = name;
            m_refCount = 0;
            m_isValid = true;
        }

        public void AddRef(int diff = 1)
        {
            m_refCount += diff;
        }

        public void DecRef(int diff = 1)
        {
            m_refCount = Math.Max(0, m_refCount - 1);
            if (m_refCount == 0)
            {
                Type t = GetType();
                if (t != typeof(GameObject))
                    Resources.UnloadAsset(m_asset);
                m_asset = null;
                m_isValid = false;
            }
        }

        public ValueTask<AssetType> Cast<AssetType>() where AssetType : UnityEngine.Object
        {
            return new ValueTask<AssetType>((AssetType)InternalGet(typeof(AssetType)));
        }

        public ValueTask<UnityEngine.Object> Get()
        {
            return new ValueTask<UnityEngine.Object>(InternalGet(typeof(UnityEngine.Object)));
        }

        private UnityEngine.Object InternalGet(Type assetType)
        {
#if UNITY_EDITOR
            if (m_state == ManagedAssetState.Initial)
            {
                m_state = ManagedAssetState.Loaded;
                m_asset = UnityEditor.AssetDatabase.LoadAssetAtPath(m_name, assetType);
            }
#else
            throw new Exception("Just valid in editor mode");
#endif
            return m_asset;
        }
    }
}