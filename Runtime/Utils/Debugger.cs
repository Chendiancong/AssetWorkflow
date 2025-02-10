using UnityEngine;

namespace cdc.AssetWorkflow
{
    internal static class Debugger
    {
        public static bool enabled = true;

        public static void Log(string message)
        {
            if (enabled)
                Debug.Log(message);
        }
    }
}