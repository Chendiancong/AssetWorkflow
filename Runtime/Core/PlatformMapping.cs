using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    public enum AssetPlatform
    {
        Unknown,
        Windows,
        OSX,
        Linux,
        PS4,
        Switch,
        XboxOne,
        WebGL,
        iOS,
        Android,
        WindowsUniversal
    }
    public static class PlatformMapping
    {
#if UNITY_EDITOR
        internal static readonly Dictionary<BuildTarget, AssetPlatform> m_buildTargetMapping =
            new Dictionary<BuildTarget, AssetPlatform>()
            {
                { BuildTarget.XboxOne, AssetPlatform.XboxOne },
                { BuildTarget.Switch, AssetPlatform.Switch },
                { BuildTarget.PS4, AssetPlatform.PS4 },
                { BuildTarget.iOS, AssetPlatform.iOS },
                { BuildTarget.Android, AssetPlatform.Android },
                { BuildTarget.WebGL, AssetPlatform.WebGL },
                { BuildTarget.StandaloneWindows, AssetPlatform.Windows },
                { BuildTarget.StandaloneWindows64, AssetPlatform.Windows },
                { BuildTarget.StandaloneOSX, AssetPlatform.OSX },
                { BuildTarget.StandaloneLinux64, AssetPlatform.Linux },
                { BuildTarget.WSAPlayer, AssetPlatform.WindowsUniversal }
            };
#endif
        internal static readonly Dictionary<RuntimePlatform, AssetPlatform> m_runtimePlatformMapping =
            new Dictionary<RuntimePlatform, AssetPlatform>()
            {
                { RuntimePlatform.XboxOne, AssetPlatform.XboxOne },
                { RuntimePlatform.Switch, AssetPlatform.Switch },
                { RuntimePlatform.PS4, AssetPlatform.PS4 },
                { RuntimePlatform.IPhonePlayer, AssetPlatform.iOS },
                { RuntimePlatform.Android, AssetPlatform.Android },
                { RuntimePlatform.WebGLPlayer, AssetPlatform.WebGL },
                { RuntimePlatform.WindowsPlayer, AssetPlatform.Windows },
                { RuntimePlatform.OSXPlayer, AssetPlatform.OSX },
                { RuntimePlatform.LinuxPlayer, AssetPlatform.Linux },
                { RuntimePlatform.WindowsEditor, AssetPlatform.Windows },
                { RuntimePlatform.OSXEditor, AssetPlatform.OSX },
                { RuntimePlatform.LinuxEditor, AssetPlatform.Linux },
                { RuntimePlatform.WSAPlayerARM, AssetPlatform.WindowsUniversal },
                { RuntimePlatform.WSAPlayerX64, AssetPlatform.WindowsUniversal },
                { RuntimePlatform.WSAPlayerX86, AssetPlatform.WindowsUniversal }
            };

#if UNITY_EDITOR
        public static AssetPlatform GetAssetPlatform(BuildTarget buildTarget)
        {
            if (m_buildTargetMapping.ContainsKey(buildTarget))
                return m_buildTargetMapping[buildTarget];
            return AssetPlatform.Unknown;
        }
#endif
        public static AssetPlatform GetAssetPlatform(RuntimePlatform platform)
        {
            if (m_runtimePlatformMapping.ContainsKey(platform))
                return m_runtimePlatformMapping[platform];
            return AssetPlatform.Unknown;
        }
    }
}