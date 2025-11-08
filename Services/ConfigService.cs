using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using FrpClientManager.Models;

namespace FrpClientManager.Services
{
    public class ConfigService
    {
        private string _configDir;
        private readonly string _nodesFile;
        private readonly string _tunnelsFile;
        private readonly string _appConfigFile;

        public ConfigService()
        {
            // 默认配置目录
            string defaultConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FrpClientManager");
            
            // 应用配置文件路径
            _appConfigFile = Path.Combine(defaultConfigDir, "appsettings.json");
            
            // 初始化时加载配置
            LoadAppConfig();
            
            _nodesFile = Path.Combine(_configDir, "nodes.json");
            _tunnelsFile = Path.Combine(_configDir, "tunnels.json");
            
            if (!Directory.Exists(_configDir))
                Directory.CreateDirectory(_configDir);
        }

        /// <summary>
        /// 加载应用配置
        /// </summary>
        private void LoadAppConfig()
        {
            try
            {
                // 如果配置文件不存在，使用默认路径并创建配置文件
                if (!File.Exists(_appConfigFile))
                {
                    _configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FrpClientManager");
                    SaveAppConfig();
                    return;
                }

                var json = File.ReadAllText(_appConfigFile, Encoding.UTF8);
                var config = JsonConvert.DeserializeObject<AppConfig>(json);
                
                if (config != null && !string.IsNullOrEmpty(config.DataDirectory))
                {
                    _configDir = config.DataDirectory;
                }
                else
                {
                    _configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FrpClientManager");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载应用配置失败: {ex.Message}");
                _configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FrpClientManager");
            }
        }

        /// <summary>
        /// 保存应用配置
        /// </summary>
        public void SaveAppConfig()
        {
            try
            {
                var defaultConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FrpClientManager");
                if (!Directory.Exists(defaultConfigDir))
                    Directory.CreateDirectory(defaultConfigDir);

                var config = new AppConfig { DataDirectory = _configDir };
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_appConfigFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存应用配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更改数据存储目录
        /// </summary>
        public bool ChangeDataDirectory(string newPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPath))
                    return false;

                // 确保新目录存在
                if (!Directory.Exists(newPath))
                    Directory.CreateDirectory(newPath);

                string newNodesFile = Path.Combine(newPath, "nodes.json");
                string newTunnelsFile = Path.Combine(newPath, "tunnels.json");

                // 如果旧配置文件存在，则复制到新位置
                if (File.Exists(_nodesFile))
                    File.Copy(_nodesFile, newNodesFile, true);
                
                if (File.Exists(_tunnelsFile))
                    File.Copy(_tunnelsFile, newTunnelsFile, true);

                // 更新配置
                _configDir = newPath;
                SaveAppConfig();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更改数据目录失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取当前数据目录
        /// </summary>
        public string GetCurrentDataDirectory()
        {
            return _configDir;
        }

        // 修复：增强数据加载和保存方法，添加调试信息
        public List<ServerNode> LoadNodes()
        {
            try
            {
                if (File.Exists(_nodesFile))
                {
                    var json = File.ReadAllText(_nodesFile, Encoding.UTF8);
                    var nodes = JsonConvert.DeserializeObject<List<ServerNode>>(json) ?? new List<ServerNode>();
                    
                    // 调试信息：检查加载的节点数据
                    Console.WriteLine($"🔍 加载节点数据: 共 {nodes.Count} 个节点");
                    foreach (var node in nodes)
                    {
                        Console.WriteLine($"   节点: {node.Name}, 服务器地址: {node.ServerAddress}, 端口: {node.ServerPort}");
                    }
                    
                    return nodes;
                }
                else
                {
                    Console.WriteLine($"⚠️ 节点配置文件不存在: {_nodesFile}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 加载节点配置失败: {ex.Message}");
            }
            return new List<ServerNode>();
        }

        public void SaveNodes(List<ServerNode> nodes)
        {
            try
            {
                // 调试信息：检查要保存的节点数据
                Console.WriteLine($"💾 保存节点数据: 共 {nodes.Count} 个节点");
                foreach (var node in nodes)
                {
                    Console.WriteLine($"   节点: {node.Name}, 服务器地址: {node.ServerAddress}, 端口: {node.ServerPort}");
                    
                    // 验证数据完整性
                    if (string.IsNullOrEmpty(node.ServerAddress))
                    {
                        Console.WriteLine($"⚠️ 警告: 节点 '{node.Name}' 的服务器地址为空!");
                    }
                    else if (node.ServerAddress == node.Name)
                    {
                        Console.WriteLine($"⚠️ 警告: 节点 '{node.Name}' 的服务器地址与节点名称相同!");
                    }
                }
                
                var json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
                File.WriteAllText(_nodesFile, json, Encoding.UTF8);
                Console.WriteLine($"✅ 节点数据已保存到: {_nodesFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 保存节点配置失败: {ex.Message}");
            }
        }

        public List<TunnelConfig> LoadTunnels()
        {
            try
            {
                if (File.Exists(_tunnelsFile))
                {
                    var json = File.ReadAllText(_tunnelsFile, Encoding.UTF8);
                    var tunnels = JsonConvert.DeserializeObject<List<TunnelConfig>>(json) ?? new List<TunnelConfig>();
                    
                    // 调试信息
                    Console.WriteLine($"🔍 加载隧道数据: 共 {tunnels.Count} 个隧道");
                    foreach (var tunnel in tunnels)
                    {
                        Console.WriteLine($"   隧道: {tunnel.Name}, 节点ID: {tunnel.NodeId}, 远程端口: {tunnel.RemotePort}");
                    }
                    
                    return tunnels;
                }
                else
                {
                    Console.WriteLine($"⚠️ 隧道配置文件不存在: {_tunnelsFile}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 加载隧道配置失败: {ex.Message}");
            }
            return new List<TunnelConfig>();
        }

        public void SaveTunnels(List<TunnelConfig> tunnels)
        {
            try
            {
                // 调试信息
                Console.WriteLine($"💾 保存隧道数据: 共 {tunnels.Count} 个隧道");
                
                var json = JsonConvert.SerializeObject(tunnels, Formatting.Indented);
                File.WriteAllText(_tunnelsFile, json, Encoding.UTF8);
                Console.WriteLine($"✅ 隧道数据已保存到: {_tunnelsFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 保存隧道配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 生成FRP配置文件 - 修复版本，添加验证
        /// </summary>
        public string GenerateFrpConfig(ServerNode node, List<TunnelConfig> tunnels)
        {
            // 验证节点数据
            if (node == null)
                throw new ArgumentNullException(nameof(node), "节点不能为空");
                
            if (string.IsNullOrEmpty(node.ServerAddress))
                throw new ArgumentException("服务器地址不能为空", nameof(node));
                
            if (node.ServerPort <= 0 || node.ServerPort > 65535)
                throw new ArgumentException("服务器端口无效", nameof(node));

            var config = new StringBuilder();
            
            // 公共配置 - 增加详细日志设置
            config.AppendLine($"[common]");
            config.AppendLine($"server_addr = {node.ServerAddress}");
            config.AppendLine($"server_port = {node.ServerPort}");
            
            // 添加token支持（如果节点有token）
            var nodeType = node.GetType();
            var tokenProperty = nodeType.GetProperty("Token");
            if (tokenProperty != null)
            {
                var tokenValue = tokenProperty.GetValue(node) as string;
                if (!string.IsNullOrEmpty(tokenValue))
                    config.AppendLine($"token = {tokenValue}");
            }
            
            config.AppendLine($"admin_addr = {node.AdminAddress}");
            config.AppendLine($"admin_port = {node.AdminPort}");
            
            // 增加详细的日志配置
            config.AppendLine($"log_file = console");
            config.AppendLine($"log_level = info");
            config.AppendLine($"log_max_days = 3");
            config.AppendLine($"disable_log_color = false");
            
            // 增加连接和性能配置
            config.AppendLine($"tcp_mux = true");
            config.AppendLine($"pool_count = 5");
            config.AppendLine($"user_conn_timeout = 10");
            config.AppendLine();
            
            // 隧道配置
            int tunnelCount = 0;
            foreach (var tunnel in tunnels)
            {
                if (tunnel.NodeId == node.Id && tunnel.IsEnabled)
                {
                    try
                    {
                        // 验证隧道配置
                        if (string.IsNullOrEmpty(tunnel.LocalIp))
                            throw new ArgumentException($"隧道 '{tunnel.Name}' 的本地IP不能为空");
                            
                        if (tunnel.LocalPort <= 0 || tunnel.LocalPort > 65535)
                            throw new ArgumentException($"隧道 '{tunnel.Name}' 的本地端口无效");
                            
                        // 验证远程地址配置
                        if (tunnel.RemotePort <= 0 && 
                            string.IsNullOrEmpty(tunnel.CustomDomain) && 
                            string.IsNullOrEmpty(tunnel.SubDomain))
                        {
                            throw new ArgumentException($"隧道 '{tunnel.Name}' 未配置有效的远程地址");
                        }
                        
                        config.AppendLine(tunnel.GetConfigString());
                        config.AppendLine();
                        tunnelCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ 隧道配置验证失败: {ex.Message}");
                        // 继续处理其他隧道
                    }
                }
            }
            
            Console.WriteLine($"🔧 生成FRP配置: 包含 {tunnelCount} 个隧道");
            return config.ToString();
        }

        /// <summary>
        /// 获取当前活动的隧道配置
        /// </summary>
        public List<TunnelConfig> GetActiveTunnelsForNode(ServerNode node, List<TunnelConfig> allTunnels)
        {
            var activeTunnels = allTunnels.FindAll(t => t.NodeId == node.Id && t.IsEnabled);
            Console.WriteLine($"🔍 节点 {node.Name} 的活动隧道: {activeTunnels.Count} 个");
            return activeTunnels;
        }

        /// <summary>
        /// 调试方法：打印当前配置状态
        /// </summary>
        public void PrintConfigStatus()
        {
            Console.WriteLine("=== 配置状态 ===");
            Console.WriteLine($"配置目录: {_configDir}");
            Console.WriteLine($"节点文件: {_nodesFile} (存在: {File.Exists(_nodesFile)})");
            Console.WriteLine($"隧道文件: {_tunnelsFile} (存在: {File.Exists(_tunnelsFile)})");
            Console.WriteLine($"应用配置: {_appConfigFile} (存在: {File.Exists(_appConfigFile)})");
            
            var nodes = LoadNodes();
            var tunnels = LoadTunnels();
            
            Console.WriteLine($"已加载节点: {nodes.Count} 个");
            Console.WriteLine($"已加载隧道: {tunnels.Count} 个");
            Console.WriteLine("================");
        }
    }

    /// <summary>
    /// 应用配置类
    /// </summary>
    public class AppConfig
    {
        public string DataDirectory { get; set; } = "";
    }
}