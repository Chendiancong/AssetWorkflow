using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace cdc.AssetWorkflow.Editor
{
    internal class AssetServer : IDisposable
    {
        private readonly HttpListener m_listener;
        private readonly string m_rootDirectory;
        private bool m_internalRunning;

#region static
        private static AssetServer m_inst;
        public static AssetServer Inst => m_inst;
        private static AssetServerInfo m_info = new AssetServerInfo
        {
            state = AssetServerState.Undefined
        };
        public static AssetServerInfo Info;
        public static AssetServerState State
        {
            get => m_info.state;
            private set => m_info.state = value;
        }

        public static AssetServerState ConfirmServerState(int port)
        {
            if (State == AssetServerState.Undefined)
            {
                var (inUse, isRunning) = CheckServerPort(port);
                if (inUse)
                {
                    if (!isRunning)
                        State = AssetServerState.Occupied;
                    else
                        State = AssetServerState.Running;
                }
                else
                {
                    State = AssetServerState.Stop;
                }
            }

            return State;
        }

        public static void StartServer(string rootDirectory, int port)
        {
            if (m_inst is not null)
            {
                m_inst.Stop();
                m_inst = null;
            }
        }

        public static void StopServer(int port)
        {
        }

        private static (bool inUse, bool isRunning) CheckServerPort(int port)
        {
            (bool, bool) result = (false, false);

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    result.Item1 = true;
                    break;
                }
            }

            if (!result.Item1)
                return result;

            try
            {
                using (var client = new TcpClient("localhost", port))
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine("GET / HTTP/1.1");
                    writer.WriteLine("Host: localhost");
                    writer.WriteLine("X-AssetServer-Check: true");
                    writer.WriteLine();
                    writer.Flush();

                    using (var reader = new StreamReader(stream))
                    {
                        var response = reader.ReadLine();
                        if (response != null && response.Contains("AssetServer"))
                        {
                            result.Item2 = true;
                        }
                    }
                }
            }
            catch
            {
                // Connection failed or invalid response
            }

            return result;
        }
#endregion

        protected AssetServer(string rootDirectory)
        {
            if (!Directory.Exists(rootDirectory))
            {
                throw new DirectoryNotFoundException($"The specified directory does not exist: {rootDirectory}");
            }

            m_rootDirectory = Path.GetFullPath(rootDirectory);
            m_listener = new HttpListener();
        }

        public void Start(int port = 8080)
        {
            if (m_internalRunning) return;

            m_listener.Prefixes.Add($"http://localhost:{port}/");
            m_listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            m_listener.Start();
            m_internalRunning = true;

            Task.Run(ListenForRequests);
        }

        public void Stop()
        {
            if (!m_internalRunning) return;

            m_listener.Stop();
            m_internalRunning = false;
        }

        private async Task ListenForRequests()
        {
            while (m_internalRunning)
            {
                try
                {
                    var context = await m_listener.GetContextAsync();
                    ProcessRequest(context);
                }
                catch (HttpListenerException)
                {
                    // Listener was stopped
                    break;
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                if (request.Headers["X-AssetServer-Check"] == "true")
                {
                    response.StatusCode = 200;
                    response.StatusDescription = "OK";
                    response.ContentType = "text/plain";
                    using (var writer = new StreamWriter(response.OutputStream))
                    {
                        writer.WriteLine("AssetServer");
                    }
                    return;
                }

                if (request.Headers["X-AssetServer-Shutdown"] == "true")
                {
                    response.StatusCode = 200;
                    response.StatusDescription = "OK";
                    response.OutputStream.Close();
                    Task.Run(Stop);
                    return;
                }

                // Get the relative path and map to local file system
                var relativePath = request.Url.LocalPath.TrimStart('/');
                var fullPath = Path.Combine(m_rootDirectory, relativePath);

                if (File.Exists(fullPath))
                {
                    var fileBytes = File.ReadAllBytes(fullPath);
                    response.ContentType = GetContentType(fullPath);
                    response.ContentLength64 = fileBytes.Length;
                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                }
                else
                {
                    response.StatusCode = 404;
                    response.StatusDescription = "Not Found";
                }

                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "Internal Server Error";
                context.Response.OutputStream.Close();
                Console.WriteLine($"Error processing request: {ex.Message}");
            }
        }

        private static string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }

        public void Dispose()
        {
            Stop();
            m_listener.Close();
        }
    }

    internal enum AssetServerState
    {
        /// <summary>
        /// 未定义
        /// </summary>
        Undefined,
        /// <summary>
        /// 已停止
        /// </summary>
        Stop,
        /// <summary>
        /// 端口被占用
        /// </summary>
        Occupied,
        /// <summary>
        /// 运行中
        /// </summary>
        Running,
    }

    internal struct AssetServerInfo
    {
        /// <summary>
        /// 根目录
        /// </summary>
        public string root;
        /// <summary>
        /// 端口
        /// </summary>
        public int port;
        /// <summary>
        /// 服务器状态
        /// </summary>
        public AssetServerState state;
    }
}