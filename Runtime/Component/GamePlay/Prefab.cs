using System;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    [ExecuteInEditMode]
    public class Prefab : MonoBehaviour
    {
        public AssetRef assetRef;
        public bool autoCreate = true;
        private GameObject m_prefabRoot;
        private GameObject m_prefabTarget;
        private AssetKeeper m_assetKeeper;
        private TaskCompletionSource<GameObject> m_createPending;
        private Action<GameObject> m_onTargetLoaded;

        public GameObject Root => m_prefabRoot;
        public GameObject Target => m_prefabTarget;
        public event Action<GameObject> OnTargetLoaded
        {
            add
            {
                if (m_prefabTarget != null)
                    value.Invoke(m_prefabTarget);
                else
                    m_onTargetLoaded += value;
            }
            remove => m_onTargetLoaded -= value;
        }

        public (GameObject prefabRoot, Task createTask) InstantiatePrefab()
        {
            if (!m_prefabRoot)
            {
                m_prefabRoot = new GameObject("PrefabRoot");
                m_prefabRoot.transform.SetParent(transform);
                m_prefabRoot.transform.position = Vector3.zero;
            }
            if (m_createPending == null)
            {
                m_createPending = new TaskCompletionSource<GameObject>();
                InstantiatePrefabAsync(m_prefabRoot, m_createPending);
            }
            return (m_prefabRoot, m_createPending.Task);
        }

        private void Start()
        {
            if (autoCreate)
                InstantiatePrefab();
        }

        private async void InstantiatePrefabAsync(GameObject prefabRoot, TaskCompletionSource<GameObject> pending)
        {
            var handle = assetRef?.Handle;
            if (handle == null)
                return;
            m_assetKeeper = AssetKeeper.Link(assetRef, prefabRoot, m_assetKeeper);
            var target = Instantiate(await handle.Cast<GameObject>());
            target.transform.SetParent(prefabRoot.transform);
            pending.SetResult(target);
            m_onTargetLoaded?.Invoke(target);
        }
    }
}