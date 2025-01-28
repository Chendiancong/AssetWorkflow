using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace cdc.BundleWorkFlow.Editor
{
    public static class CachingTest
    {
        [MenuItem("BundleWorkFlow/One")]
        private static void One()
        {
            // string cachingPath = Path.Combine(Application.streamingAssetsPath, DateTime.Now.ToString());
            string cachingPath = DateTime.Today.ToLongDateString();
            if (!Directory.Exists(cachingPath))
                Directory.CreateDirectory(cachingPath);
            var newCache = Caching.AddCache(cachingPath);
            if (newCache.valid)
            {
                Caching.currentCacheForWriting = newCache;
            }
        }

        [MenuItem("BundleWorkFlow/Two")]
        private static void Two()
        {
            Debug.Log(Caching.currentCacheForWriting.path);
        }

        [MenuItem("BundleWorkFlow/Three")]
        private static void Three()
        {
            Debug.Log(Application.temporaryCachePath);
            Debug.Log(Application.persistentDataPath);
        }
    }
}