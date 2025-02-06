using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    public class AssetMgrHelper
    {
        public Dictionary<string, string> versions = new Dictionary<string, string>();

        public AssetMgrConfig LoadSetting(IAssetManager mgr)
        {
            string settingPath = Path.Combine(
                Application.streamingAssetsPath,
                "Setting.json"
            );
            string jsonString;
            using (StreamReader reader = new StreamReader(settingPath))
            {
                jsonString = reader.ReadToEnd();
            }
            AssetMgrConfig setting = JsonUtility.FromJson<AssetMgrConfig>(jsonString);
            return setting;
        }

        public async ValueTask HotUpdate(IAssetManager mgr)
        {
            await Task.Delay(100);
        }
    }
}