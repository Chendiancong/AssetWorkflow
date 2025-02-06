using System;
using System.Deployment.Internal;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    internal static partial class AsyncLoader
    {
        /// <summary>
        /// 从本地文件加载AssetBundle
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        public static void LoadBundle(string localPath)
        {
            throw new NotImplementedException();
        }
    }

    internal static partial class AsyncLoader
    {
        public struct AssetBundleDefer
        {
            private AssetBundleCreateRequest m_req;
        }
    }
}