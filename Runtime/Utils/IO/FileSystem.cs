using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cdc.AssetWorkflow
{
    internal static class FileSystem
    {
        public static async ValueTask ReadFileLineByLine(string filePath, Action<string> lineHandler)
        {
            // Android和WebGL平台的streamingAssetsPath是链接的形式，此处有可能传递这种形式的文件路径
            // 所以一旦发现链接形式的文件路径，就直接转为下载的方式
            if (IsUrlLike(filePath))
            {
                byte[] bytes = await Network.DownloadToMemoryAsync(filePath);
                string content = Encoding.UTF8.GetString(bytes);
                foreach (string line in content.Split('\n'))
                {
                    if (!string.IsNullOrEmpty(line))
                        lineHandler(line);
                }
            }
            else
            {
                if (!File.Exists(filePath))
                    return;
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        lineHandler(line);
                    }
                }
            }
        }

        public static async ValueTask SaveToFile(string contents, string filePath)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(contents);
            await SaveToFile(bytes, filePath);
        }
        public static async ValueTask SaveToFile(byte[] contents, string filePath)
        {
            await File.WriteAllBytesAsync(filePath, contents);
        }

        private static string[] m_sizeTags = new string[]
        {
            "B", "kB", "MB", "GB"
        };
        public static string ConvertFileSizeToString(long size)
        {
            int order = 0;
            while (size >= 1024 && order < m_sizeTags.Length - 1)
            {
                order++;
                size = size / 1024;
            }

            return $"{size:0.##} {m_sizeTags[order]}";
        }

        private static Regex m_urlLike = new Regex(@"^[^/]+://.+$");
        private static bool IsUrlLike(string path) =>
            m_urlLike.IsMatch(path);
    }
}