using System;

namespace FrpClientManager.Models
{
    public class TunnelConfig
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "新隧道";
        public string NodeId { get; set; } = "";
        public string Protocol { get; set; } = "tcp";
        public string LocalIp { get; set; } = "127.0.0.1";
        public int LocalPort { get; set; } = 80;
        public int RemotePort { get; set; } = 0;
        public string CustomDomain { get; set; } = "";
        public string SubDomain { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime ModifiedTime { get; set; } = DateTime.Now;
        
        public string GetConfigString()
        {
            var config = $"[{Name}]\n" +
                        $"type = {Protocol}\n" +
                        $"local_ip = {LocalIp}\n" +
                        $"local_port = {LocalPort}\n";
            
            if (RemotePort > 0)
                config += $"remote_port = {RemotePort}\n";
            
            if (!string.IsNullOrEmpty(CustomDomain))
                config += $"custom_domain = {CustomDomain}\n";
            
            if (!string.IsNullOrEmpty(SubDomain))
                config += $"subdomain = {SubDomain}\n";
                
            return config;
        }

        /// <summary>
        /// 验证隧道数据的完整性
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;
                
            if (string.IsNullOrWhiteSpace(NodeId))
                return false;
                
            if (string.IsNullOrWhiteSpace(LocalIp))
                return false;
                
            if (LocalPort <= 0 || LocalPort > 65535)
                return false;
                
            // 验证至少有一种远程地址配置
            if (RemotePort <= 0 && 
                string.IsNullOrEmpty(CustomDomain) && 
                string.IsNullOrEmpty(SubDomain))
                return false;
                
            return true;
        }

        /// <summary>
        /// 复制隧道数据
        /// </summary>
        public TunnelConfig Clone()
        {
            return new TunnelConfig
            {
                Id = this.Id,
                Name = this.Name,
                NodeId = this.NodeId,
                Protocol = this.Protocol,
                LocalIp = this.LocalIp,
                LocalPort = this.LocalPort,
                RemotePort = this.RemotePort,
                CustomDomain = this.CustomDomain,
                SubDomain = this.SubDomain,
                IsEnabled = this.IsEnabled,
                CreatedTime = this.CreatedTime,
                ModifiedTime = this.ModifiedTime
            };
        }
    }
}