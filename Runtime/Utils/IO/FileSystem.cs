using System;
using System.IO;
using System.Threading.Tasks;

namespace cdc.AssetWorkflow
{
    internal static class FileSystem
    {
        public static void ReadFileLineByLine(string filePath, Action<string> lineHandler)
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
}