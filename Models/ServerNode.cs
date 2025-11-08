using System;

namespace FrpClientManager.Models
{
    public class ServerNode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        // 节点显示名称
        public string Name { get; set; } = "新节点";
        
        // 服务器实际地址（IP或域名）
        public string ServerAddress { get; set; } = "127.0.0.1";
        
        public int ServerPort { get; set; } = 7000;
        public string Token { get; set; } = "";
        public string AdminAddress { get; set; } = "127.0.0.1";
        public int AdminPort { get; set; } = 7400;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime ModifiedTime { get; set; } = DateTime.Now;
        public bool IsEnabled { get; set; } = true;
        
        public override string ToString()
        {
            return $"{Name} ({ServerAddress}:{ServerPort})";
        }

        /// <summary>
        /// 验证节点数据的完整性
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;
                
            if (string.IsNullOrWhiteSpace(ServerAddress))
                return false;
                
            if (ServerPort <= 0 || ServerPort > 65535)
                return false;
                
            if (string.IsNullOrWhiteSpace(AdminAddress))
                return false;
                
            if (AdminPort <= 0 || AdminPort > 65535)
                return false;
                
            return true;
        }

        /// <summary>
        /// 复制节点数据
        /// </summary>
        public ServerNode Clone()
        {
            return new ServerNode
            {
                Id = this.Id,
                Name = this.Name,
                ServerAddress = this.ServerAddress,
                ServerPort = this.ServerPort,
                Token = this.Token,
                AdminAddress = this.AdminAddress,
                AdminPort = this.AdminPort,
                CreatedTime = this.CreatedTime,
                ModifiedTime = this.ModifiedTime,
                IsEnabled = this.IsEnabled
            };
        }
    }
}