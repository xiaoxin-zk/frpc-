using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FrpClientManager.Models;

namespace FrpClientManager.Services
{
    public class FrpService
    {
        private Process _frpProcess;
        private readonly ConfigService _configService;
        private string _currentConfigPath;

        public FrpService(ConfigService configService)
        {
            _configService = configService;
        }

        public async Task<bool> StartFrpClient(ServerNode node, List<TunnelConfig> tunnels)
        {
            try
            {
                // 停止现有进程
                StopFrpClient();

                // 调试信息：检查节点数据
                OnOutputReceived?.Invoke($"🔍 调试信息 - 节点数据检查:");
                OnOutputReceived?.Invoke($"   节点名称: {node.Name}");
                OnOutputReceived?.Invoke($"   服务器地址: {node.ServerAddress}");
                OnOutputReceived?.Invoke($"   服务器端口: {node.ServerPort}");

                // 生成配置文件
                var configContent = _configService.GenerateFrpConfig(node, tunnels);
                _currentConfigPath = Path.Combine(Path.GetTempPath(), $"frpc_{DateTime.Now:yyyyMMddHHmmss}.ini");
                await File.WriteAllTextAsync(_currentConfigPath, configContent, Encoding.UTF8);

                // 输出配置信息到日志 - 直接显示完整连接地址
                OnOutputReceived?.Invoke("=== FRP 配置信息 ===");
                OnOutputReceived?.Invoke($"服务器: {node.ServerAddress}:{node.ServerPort}");
                OnOutputReceived?.Invoke($"管理界面: {node.AdminAddress}:{node.AdminPort}");
                OnOutputReceived?.Invoke($"隧道数量: {tunnels.Count}");
                
                // 输出每个隧道的完整连接地址
                foreach (var tunnel in tunnels)
                {
                    if (tunnel.NodeId == node.Id && tunnel.IsEnabled)
                    {
                        string fullRemoteAddress = GetFullRemoteAddress(node, tunnel);
                        OnOutputReceived?.Invoke($"🌐 {tunnel.Name}: {tunnel.LocalIp}:{tunnel.LocalPort} → {fullRemoteAddress}");
                        
                        // 特别突出显示完整连接地址
                        OnOutputReceived?.Invoke($"📍 完整连接地址: {fullRemoteAddress}");
                    }
                }
                OnOutputReceived?.Invoke("====================");

                // 启动frp客户端
                var frpExePath = GetFrpExePath();
                if (!File.Exists(frpExePath))
                {
                    OnErrorReceived?.Invoke("❌ frpc.exe 未找到，请确保它位于应用程序目录中");
                    return false;
                }

                OnOutputReceived?.Invoke($"🚀 启动 FRP 客户端: {Path.GetFileName(frpExePath)}");

                _frpProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = frpExePath,
                        Arguments = $"-c \"{_currentConfigPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                _frpProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        // 解析并美化 frpc 输出
                        var formattedMessage = FormatFrpOutput(e.Data);
                        OnOutputReceived?.Invoke(formattedMessage);
                    }
                };

                _frpProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        OnErrorReceived?.Invoke($"❌ 错误: {e.Data}");
                    }
                };

                var started = _frpProcess.Start();
                if (started)
                {
                    _frpProcess.BeginOutputReadLine();
                    _frpProcess.BeginErrorReadLine();
                    
                    // 等待进程就绪
                    await Task.Delay(2000);
                    
                    // 检查进程是否仍在运行
                    if (_frpProcess.HasExited)
                    {
                        OnErrorReceived?.Invoke("❌ FRP 客户端启动后立即退出，请检查配置");
                        return false;
                    }
                    
                    OnOutputReceived?.Invoke("✅ FRP 客户端启动成功");
                    
                    // 再次显示完整的连接地址，方便用户复制
                    OnOutputReceived?.Invoke("=== 可用的连接地址 ===");
                    foreach (var tunnel in tunnels)
                    {
                        if (tunnel.NodeId == node.Id && tunnel.IsEnabled)
                        {
                            string fullRemoteAddress = GetFullRemoteAddress(node, tunnel);
                            OnOutputReceived?.Invoke($"📋 {tunnel.Name}: {fullRemoteAddress}");
                        }
                    }
                    OnOutputReceived?.Invoke("====================");
                    
                    return true;
                }
                else
                {
                    OnErrorReceived?.Invoke("❌ 无法启动 FRP 客户端进程");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnErrorReceived?.Invoke($"❌ 启动FRP客户端失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取完整的远程连接地址 - 修复版本
        /// </summary>
        private string GetFullRemoteAddress(ServerNode node, TunnelConfig tunnel)
        {
            if (node == null)
            {
                OnErrorReceived?.Invoke("❌ 错误: 节点对象为空");
                return "节点不存在";
            }

            // 调试信息：检查节点和隧道数据
            bool hasDebugInfo = false;
            if (!hasDebugInfo)
            {
                OnOutputReceived?.Invoke($"🔍 隧道调试信息:");
                OnOutputReceived?.Invoke($"   隧道: {tunnel.Name}");
                OnOutputReceived?.Invoke($"   节点ID: {tunnel.NodeId}");
                OnOutputReceived?.Invoke($"   远程端口: {tunnel.RemotePort}");
                OnOutputReceived?.Invoke($"   子域名: {tunnel.SubDomain}");
                OnOutputReceived?.Invoke($"   自定义域名: {tunnel.CustomDomain}");
                OnOutputReceived?.Invoke($"   服务器地址: {node.ServerAddress}");
                hasDebugInfo = true;
            }

            // 确保使用正确的服务器地址
            string serverAddress = node.ServerAddress?.Trim();
            
            if (string.IsNullOrEmpty(serverAddress))
            {
                OnErrorReceived?.Invoke($"❌ 错误: 节点 {node.Name} 的服务器地址为空");
                return "服务器地址未配置";
            }

            // 根据隧道类型构建完整的远程地址
            if (tunnel.RemotePort > 0)
            {
                // TCP/UDP 隧道：服务器IP:远程端口
                string address = $"{serverAddress}:{tunnel.RemotePort}";
                OnOutputReceived?.Invoke($"🔗 TCP/UDP隧道地址: {address}");
                return address;
            }
            else if (!string.IsNullOrEmpty(tunnel.CustomDomain))
            {
                // 自定义域名
                OnOutputReceived?.Invoke($"🌐 自定义域名地址: {tunnel.CustomDomain}");
                return tunnel.CustomDomain.Trim();
            }
            else if (!string.IsNullOrEmpty(tunnel.SubDomain))
            {
                // 子域名（需要服务器配置根域名）
                string address = $"{tunnel.SubDomain.Trim()}.{serverAddress}";
                OnOutputReceived?.Invoke($"🔗 子域名地址: {address}");
                return address;
            }
            else
            {
                OnErrorReceived?.Invoke($"❌ 错误: 隧道 {tunnel.Name} 未配置有效的远程地址");
                return "未配置远程地址";
            }
        }

        /// <summary>
        /// 验证节点数据的完整性
        /// </summary>
        private bool ValidateNodeData(ServerNode node, List<TunnelConfig> tunnels)
        {
            if (node == null)
            {
                OnErrorReceived?.Invoke("❌ 错误: 节点对象为null");
                return false;
            }

            if (string.IsNullOrEmpty(node.ServerAddress))
            {
                OnErrorReceived?.Invoke("❌ 错误: 服务器地址为空");
                return false;
            }

            if (node.ServerPort <= 0 || node.ServerPort > 65535)
            {
                OnErrorReceived?.Invoke("❌ 错误: 服务器端口无效");
                return false;
            }

            // 检查是否有启用的隧道
            var enabledTunnels = tunnels.FindAll(t => t.NodeId == node.Id && t.IsEnabled);
            if (enabledTunnels.Count == 0)
            {
                OnErrorReceived?.Invoke("❌ 错误: 没有启用的隧道");
                return false;
            }

            // 验证每个隧道的配置
            foreach (var tunnel in enabledTunnels)
            {
                if (string.IsNullOrEmpty(tunnel.LocalIp))
                {
                    OnErrorReceived?.Invoke($"❌ 错误: 隧道 {tunnel.Name} 的本地IP为空");
                    return false;
                }

                if (tunnel.LocalPort <= 0 || tunnel.LocalPort > 65535)
                {
                    OnErrorReceived?.Invoke($"❌ 错误: 隧道 {tunnel.Name} 的本地端口无效");
                    return false;
                }

                // 验证远程地址配置
                if (tunnel.RemotePort <= 0 && 
                    string.IsNullOrEmpty(tunnel.CustomDomain) && 
                    string.IsNullOrEmpty(tunnel.SubDomain))
                {
                    OnErrorReceived?.Invoke($"❌ 错误: 隧道 {tunnel.Name} 未配置远程地址");
                    return false;
                }
            }

            return true;
        }

        public void StopFrpClient()
        {
            try
            {
                if (_frpProcess != null && !_frpProcess.HasExited)
                {
                    OnOutputReceived?.Invoke("🛑 正在停止 FRP 客户端...");
                    _frpProcess.Kill();
                    _frpProcess.WaitForExit(5000);
                    _frpProcess.Dispose();
                    _frpProcess = null;
                    OnOutputReceived?.Invoke("✅ FRP 客户端已停止");
                }

                // 清理临时配置文件
                if (File.Exists(_currentConfigPath))
                {
                    try 
                    { 
                        File.Delete(_currentConfigPath); 
                        OnOutputReceived?.Invoke("🗑️ 临时配置文件已清理");
                    } 
                    catch 
                    {
                        OnOutputReceived?.Invoke("⚠️ 无法删除临时配置文件");
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorReceived?.Invoke($"❌ 停止FRP客户端失败: {ex.Message}");
            }
        }

        public bool IsRunning()
        {
            return _frpProcess != null && !_frpProcess.HasExited;
        }

        private string GetFrpExePath()
        {
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var frpExePath = Path.Combine(exeDir, "frpc.exe");
            
            // 调试信息
            OnOutputReceived?.Invoke($"🔍 FRP客户端路径: {frpExePath}");
            OnOutputReceived?.Invoke($"🔍 文件存在: {File.Exists(frpExePath)}");
            
            return frpExePath;
        }

        /// <summary>
        /// 格式化 FRP 输出，使其更易读
        /// </summary>
        private string FormatFrpOutput(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            // 移除ANSI颜色代码
            message = System.Text.RegularExpressions.Regex.Replace(message, @"\x1B\[[0-9;]*[a-zA-Z]", "");

            // 移除时间戳（如果有）
            if (message.Length > 20 && (message.Contains("2025-") || message.Contains("] [")))
            {
                var timeEnd = message.IndexOf(']');
                if (timeEnd > 0 && timeEnd + 2 < message.Length)
                {
                    message = message.Substring(timeEnd + 2).Trim();
                }
            }

            // 美化常见的 FRP 消息
            if (message.Contains("start proxy success"))
            {
                // 提取代理名称
                var startIndex = message.IndexOf('[');
                var endIndex = message.IndexOf(']');
                if (startIndex >= 0 && endIndex > startIndex)
                {
                    var proxyName = message.Substring(startIndex + 1, endIndex - startIndex - 1);
                    return $"✅ 隧道 [{proxyName}] 启动成功，可以开始连接";
                }
                return $"✅ {message}";
            }

            if (message.Contains("login to server success"))
                return $"🔗 成功连接到FRP服务器";

            if (message.Contains("new proxy"))
                return $"🔄 {message}";

            if (message.Contains("control writer is closing"))
                return $"🔒 {message}";

            if (message.Contains("reconnect to server"))
                return $"🔄 {message}";

            if (message.Contains("port already used"))
                return $"❌ 端口被占用: {message}";

            if (message.Contains("connection refused"))
                return $"❌ 连接被拒绝: {message}";

            if (message.Contains("timeout"))
                return $"⏰ 连接超时: {message}";

            if (message.Contains("error"))
                return $"❌ 错误: {message}";

            if (message.Contains("warning"))
                return $"⚠️ 警告: {message}";

            if (message.Contains("ini format is deprecated"))
                return $"ℹ️ 提示: INI格式已过时，建议使用YAML/JSON格式";

            // 默认返回原始消息
            return $"📝 {message}";
        }

        public event Action<string> OnOutputReceived;
        public event Action<string> OnErrorReceived;
    }
}