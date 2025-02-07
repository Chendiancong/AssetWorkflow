using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace cdc.AssetWorkflow
{
    internal class StreamingFileDownloadHandler : DownloadHandlerScript
    {
        private string m_filePath;
        private FileStream m_fileStream;

        public StreamingFileDownloadHandler(string filePath)
            : base(new byte[1024 * 1024]) // 使用1MB缓冲区
        {
            string dirName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            m_filePath = filePath;
            m_fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0)
                return false;

            // write to stream
            m_fileStream.Write(data, 0, dataLength);
            return true;
        }

        protected override void CompleteContent()
        {
            m_fileStream.Close();
            Debug.Log($"File download complete:{m_filePath}");
        }

        public override void Dispose()
        {
            m_fileStream?.Dispose();
            base.Dispose();
        }

    }
}