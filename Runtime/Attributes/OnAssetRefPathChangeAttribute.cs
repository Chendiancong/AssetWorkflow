using System;

namespace cdc.AssetWorkflow
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OnAssetRefPathChangeAttribute : Attribute { }
}