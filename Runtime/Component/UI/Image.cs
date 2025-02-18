using UnityEngine;

namespace cdc.AssetWorkflow
{
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    [ExecuteInEditMode]
    public class Image : MonoBehaviour
    {
        public AssetRef assetRef;
        private UnityEngine.UI.Image m_uImage;
        private AssetKeeper m_assetKeeper;

        public UnityEngine.UI.Image UImage
        {
            get
            {
                if (m_uImage == null)
                    m_uImage = GetComponent<UnityEngine.UI.Image>();
                return m_uImage;
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
                var asset = await handle.Cast<Sprite>();
                m_assetKeeper = AssetKeeper.Link(assetRef, gameObject, m_assetKeeper);
                UImage.sprite = asset ?? null;
            }
        }

#if UNITY_EDITOR
        [OnAssetRefPathChange]
        private void OnAssetRefChange(string newPath)
        {
            assetRef.UpdatePath(newPath);
            Prepare();
        }
#endif
    }
}