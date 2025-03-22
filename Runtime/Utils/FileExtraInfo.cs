using System;

namespace cdc.AssetWorkflow
{
    public struct FileExtraInfo
    {
        public string fileName;
        public long size;

        public static string ToStorageStr(FileExtraInfo info)
        {
            return info.ToStorageStr();
        }

        public static FileExtraInfo FromStorageStr(string str)
        {
            var info = new FileExtraInfo();
            string[] sections = str.Split(',');
            if (sections.Length < 2)
                throw new System.Exception();
            info.fileName = sections[0];
            info.size = Convert.ToInt64(sections[1]);
            return info;
        }

        public string ToStorageStr()
        {
            return $"{fileName},{size}";
        }
    }
}