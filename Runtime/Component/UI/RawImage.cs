using UnityEngine;

namespace cdc.AssetWorkflow
{
    [RequireComponent(typeof(UnityEngine.UI.RawImage))]
    [ExecuteInEditMode]
    public class RawImage : MonoBehaviour
    {
        public AssetRef assetRef;
        private UnityEngine.UI.RawImage m_urImage;
        private AssetKeeper m_assetKeeper;

        public UnityEngine.UI.RawImage URawImage
        {
            get
            {
                if (m_urImage == null)
                    m_urImage = GetComponent<UnityEngine.UI.RawImage>();
                return m_urImage;
            }
        }

        private void Awake()
        {
            Prepare();
        }

        private async void Prepare()
        {
            var handle = assetRef?.Handle;
            if (handle != null)
            {
                var asset = await handle.Cast<Texture>();
                m_assetKeeper = AssetKeeper.Link(assetRef, gameObject, m_assetKeeper);
                URawImage.texture = asset;
            }
        }

#if UNITY_EDITOR
        private void OnAssetRefChange(string newPath)
        {
            assetRef.UpdatePath(newPath);
            Prepare();
        }
#endif
    }
}