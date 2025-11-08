using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FrpClientManager.Forms;
using FrpClientManager.Models;
using FrpClientManager.Services;

namespace FrpClientManager
{
    public partial class MainForm : Form
    {
        private readonly ConfigService _configService;
        private readonly FrpService _frpService;
        private List<ServerNode> _nodes;
        private List<TunnelConfig> _tunnels;
        private ServerNode _currentNode;

        // 控件字段
        private ListView lstNodes;
        private ListView lstTunnels;
        private TextBox txtLog;
        private Button btnAddNode;
        private Button btnEditNode;
        private Button btnDeleteNode;
        private Button btnAddTunnel;
        private Button btnEditTunnel;
        private Button btnDeleteTunnel;
        private Button btnStart;
        private Button btnStop;
        private Label lblStatus;
        private Label lblConnectionInfo;
        private ListView lstConnectionAddresses;
        private Panel pnlConnectionAddresses;
        private Button btnCopyAddress;
        private Button btnSettings;

        public MainForm()
        {
            InitializeComponent();
            
            _configService = new ConfigService();
            _frpService = new FrpService(_configService);
            _frpService.OnOutputReceived += OnFrpOutput;
            _frpService.OnErrorReceived += OnFrpError;
            
            LoadData();
            UpdateUI();
        }

        private void LoadData()
        {
            _nodes = _configService.LoadNodes();
            _tunnels = _configService.LoadTunnels();
            
            // 诊断加载的数据
            DebugLoadedData();
            
            RefreshNodeList();
            RefreshTunnelList();
        }

        /// <summary>
        /// 诊断加载的数据
        /// </summary>
        private void DebugLoadedData()
        {
            Console.WriteLine("=== 数据加载诊断 ===");
            Console.WriteLine($"加载的节点数量: {_nodes.Count}");
            foreach (var node in _nodes)
            {
                Console.WriteLine($"  节点: '{node.Name}' -> 服务器地址: '{node.ServerAddress}'");
            }
            Console.WriteLine($"加载的隧道数量: {_tunnels.Count}");
            Console.WriteLine("===================");
        }

        private void SaveData()
        {
            _configService.SaveNodes(_nodes);
            _configService.SaveTunnels(_tunnels);
        }

        private void RefreshNodeList()
        {
            lstNodes.Items.Clear();
            foreach (var node in _nodes)
            {
                var item = new ListViewItem(node.Name)
                {
                    SubItems = 
                    {
                        node.ServerAddress,
                        node.ServerPort.ToString(),
                        node.IsEnabled ? "是" : "否"
                    },
                    Tag = node
                };
                lstNodes.Items.Add(item);
            }
        }

        private void RefreshTunnelList()
        {
            lstTunnels.Items.Clear();
            foreach (var tunnel in _tunnels)
            {
                var node = _nodes.Find(n => n.Id == tunnel.NodeId);
                
                // 修复：使用统一的 GetFullRemoteAddress 方法
                var remoteInfo = GetFullRemoteAddress(node, tunnel);

                var item = new ListViewItem(tunnel.Name)
                {
                    SubItems = 
                    {
                        node?.Name ?? "未知节点",
                        tunnel.Protocol,
                        $"{tunnel.LocalIp}:{tunnel.LocalPort}",
                        remoteInfo,
                        tunnel.IsEnabled ? "是" : "否"
                    },
                    Tag = tunnel
                };
                lstTunnels.Items.Add(item);
            }
        }

        private void UpdateUI()
        {
            var isRunning = _frpService.IsRunning();
            btnStart.Enabled = !isRunning && _currentNode != null;
            btnStop.Enabled = isRunning;
            lblStatus.Text = isRunning ? "🟢 运行中" : "🔴 已停止";
            lblStatus.ForeColor = isRunning ? Color.Green : Color.Red;

            // 显示或隐藏连接地址区域
            pnlConnectionAddresses.Visible = isRunning && _currentNode != null;

            // 更新连接信息
            if (_currentNode != null && isRunning)
            {
                var activeTunnels = _configService.GetActiveTunnelsForNode(_currentNode, _tunnels);
                lblConnectionInfo.Text = $"连接到: {_currentNode.ServerAddress}:{_currentNode.ServerPort} | 活动隧道: {activeTunnels.Count}";
                lblConnectionInfo.ForeColor = Color.Blue;
                
                // 更新连接地址列表
                UpdateConnectionAddresses();
            }
            else if (_currentNode != null)
            {
                lblConnectionInfo.Text = $"就绪: {_currentNode.ServerAddress}:{_currentNode.ServerPort}";
                lblConnectionInfo.ForeColor = Color.Gray;
                lstConnectionAddresses.Items.Clear();
                pnlConnectionAddresses.Visible = false;
            }
            else
            {
                lblConnectionInfo.Text = "请选择一个节点";
                lblConnectionInfo.ForeColor = Color.Gray;
                lstConnectionAddresses.Items.Clear();
                pnlConnectionAddresses.Visible = false;
            }
        }

        /// <summary>
        /// 更新连接地址列表
        /// </summary>
        private void UpdateConnectionAddresses()
        {
            if (_currentNode == null) return;
            
            lstConnectionAddresses.Items.Clear();
            var activeTunnels = _configService.GetActiveTunnelsForNode(_currentNode, _tunnels);
            
            if (activeTunnels.Count == 0)
            {
                var item = new ListViewItem("暂无隧道");
                item.SubItems.Add("请确保有启用的隧道");
                lstConnectionAddresses.Items.Add(item);
                return;
            }
            
            foreach (var tunnel in activeTunnels)
            {
                string fullAddress = GetFullRemoteAddress(_currentNode, tunnel);
                
                // 根据协议类型使用不同的图标
                string protocolIcon = tunnel.Protocol.ToLower() switch
                {
                    "tcp" => "🔗",
                    "udp" => "📡", 
                    "http" => "🌐",
                    "https" => "🔒",
                    _ => "📌"
                };

                var item = new ListViewItem($"{protocolIcon} {tunnel.Name}");
                item.SubItems.Add(fullAddress);
                item.Tag = fullAddress; // 将完整地址存储在Tag中，便于复制

                lstConnectionAddresses.Items.Add(item);
            }

            // 自动调整列宽
            AdjustConnectionAddressColumns();
        }

        /// <summary>
        /// 获取完整的远程连接地址 - 修复版本（统一逻辑）
        /// </summary>
        private string GetFullRemoteAddress(ServerNode node, TunnelConfig tunnel)
        {
            if (node == null)
            {
                Console.WriteLine("❌ 错误: 节点为空");
                return "节点不存在";
            }

            if (tunnel == null)
            {
                Console.WriteLine("❌ 错误: 隧道为空");
                return "隧道不存在";
            }

            // 调试信息：显示节点和隧道数据
            Console.WriteLine($"🔍 GetFullRemoteAddress 调试:");
            Console.WriteLine($"   节点名称: '{node.Name}'");
            Console.WriteLine($"   服务器地址: '{node.ServerAddress}'");
            Console.WriteLine($"   隧道名称: '{tunnel.Name}'");
            Console.WriteLine($"   远程端口: {tunnel.RemotePort}");
            Console.WriteLine($"   子域名: '{tunnel.SubDomain}'");
            Console.WriteLine($"   自定义域名: '{tunnel.CustomDomain}'");

            // 确保使用正确的服务器地址
            string serverAddress = node.ServerAddress?.Trim();
            
            if (string.IsNullOrEmpty(serverAddress))
            {
                Console.WriteLine($"❌ 错误: 节点 '{node.Name}' 的服务器地址为空");
                return "服务器地址未配置";
            }

            // 根据隧道类型构建完整的远程地址
            if (tunnel.RemotePort > 0)
            {
                // TCP/UDP 隧道：服务器IP:远程端口
                string address = $"{serverAddress}:{tunnel.RemotePort}";
                Console.WriteLine($"🔗 TCP/UDP隧道地址: {address}");
                return address;
            }
            else if (!string.IsNullOrEmpty(tunnel.CustomDomain))
            {
                // 自定义域名
                string address = tunnel.CustomDomain.Trim();
                Console.WriteLine($"🌐 自定义域名地址: {address}");
                return address;
            }
            else if (!string.IsNullOrEmpty(tunnel.SubDomain))
            {
                // 子域名（需要服务器配置根域名）
                string address = $"{tunnel.SubDomain.Trim()}.{serverAddress}";
                Console.WriteLine($"🔗 子域名地址: {address}");
                return address;
            }
            else
            {
                Console.WriteLine($"❌ 错误: 隧道 '{tunnel.Name}' 未配置有效的远程地址");
                return "未配置远程地址";
            }
        }

        /// <summary>
        /// 诊断当前节点数据
        /// </summary>
        private void DebugCurrentNode()
        {
            if (_currentNode != null)
            {
                Console.WriteLine("=== 当前节点诊断 ===");
                Console.WriteLine($"节点名称: '{_currentNode.Name}'");
                Console.WriteLine($"服务器地址: '{_currentNode.ServerAddress}'");
                Console.WriteLine($"服务器端口: {_currentNode.ServerPort}");
                Console.WriteLine($"类型比较:");
                Console.WriteLine($"  名称类型: {_currentNode.Name?.GetType()}");
                Console.WriteLine($"  地址类型: {_currentNode.ServerAddress?.GetType()}");
                Console.WriteLine($"  值是否相等: {_currentNode.Name == _currentNode.ServerAddress}");
                Console.WriteLine("===================");
            }
            else
            {
                Console.WriteLine("当前没有选中节点");
            }
        }

        private void OnFrpOutput(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(OnFrpOutput), message);
                return;
            }
            
            try
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                txtLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
                
                // 确保日志自动滚动到底部
                txtLog.SelectionStart = txtLog.Text.Length;
                txtLog.ScrollToCaret();
                
                // 更新连接信息显示
                UpdateConnectionInfoFromLog(message);
            }
            catch (Exception ex)
            {
                // 防止日志更新异常
                Console.WriteLine($"日志更新异常: {ex.Message}");
            }
        }

        private void OnFrpError(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(OnFrpError), message);
                return;
            }
            
            try
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                txtLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
                
                // 确保日志自动滚动到底部
                txtLog.SelectionStart = txtLog.Text.Length;
                txtLog.ScrollToCaret();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误日志更新异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 从日志消息中提取并更新连接信息
        /// </summary>
        private void UpdateConnectionInfoFromLog(string message)
        {
            if (message.Contains("start proxy success") || message.Contains("完整连接地址"))
            {
                UpdateUI();
            }
        }

        private void btnAddNode_Click(object sender, EventArgs e)
        {
            var form = new NodeForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                _nodes.Add(form.Node);
                SaveData();
                RefreshNodeList();
                
                // 诊断新添加的节点
                Console.WriteLine($"✅ 添加新节点: '{form.Node.Name}' -> '{form.Node.ServerAddress}'");
            }
        }

        private void btnEditNode_Click(object sender, EventArgs e)
        {
            if (lstNodes.SelectedItems.Count == 0) return;
            
            var node = (ServerNode)lstNodes.SelectedItems[0].Tag;
            var form = new NodeForm(node);
            if (form.ShowDialog() == DialogResult.OK)
            {
                SaveData();
                RefreshNodeList();
                RefreshTunnelList();
                
                // 诊断编辑后的节点
                Console.WriteLine($"✅ 编辑节点: '{form.Node.Name}' -> '{form.Node.ServerAddress}'");
            }
        }

        private void btnDeleteNode_Click(object sender, EventArgs e)
        {
            if (lstNodes.SelectedItems.Count == 0) return;
            
            var node = (ServerNode)lstNodes.SelectedItems[0].Tag;
            if (MessageBox.Show($"确定要删除节点 '{node.Name}' 吗？", "确认删除", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _nodes.Remove(node);
                SaveData();
                RefreshNodeList();
                RefreshTunnelList();
                
                Console.WriteLine($"🗑️ 删除节点: '{node.Name}'");
            }
        }

        private void btnAddTunnel_Click(object sender, EventArgs e)
        {
            if (_nodes.Count == 0)
            {
                MessageBox.Show("请先添加至少一个节点", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var form = new TunnelForm(_nodes);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _tunnels.Add(form.Tunnel);
                SaveData();
                RefreshTunnelList();
                
                Console.WriteLine($"✅ 添加新隧道: '{form.Tunnel.Name}'");
            }
        }

        private void btnEditTunnel_Click(object sender, EventArgs e)
        {
            if (lstTunnels.SelectedItems.Count == 0) return;
            
            var tunnel = (TunnelConfig)lstTunnels.SelectedItems[0].Tag;
            var form = new TunnelForm(_nodes, tunnel);
            if (form.ShowDialog() == DialogResult.OK)
            {
                SaveData();
                RefreshTunnelList();
                
                Console.WriteLine($"✅ 编辑隧道: '{form.Tunnel.Name}'");
            }
        }

        private void btnDeleteTunnel_Click(object sender, EventArgs e)
        {
            if (lstTunnels.SelectedItems.Count == 0) return;
            
            var tunnel = (TunnelConfig)lstTunnels.SelectedItems[0].Tag;
            if (MessageBox.Show($"确定要删除隧道 '{tunnel.Name}' 吗？", "确认删除", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _tunnels.Remove(tunnel);
                SaveData();
                RefreshTunnelList();
                
                Console.WriteLine($"🗑️ 删除隧道: '{tunnel.Name}'");
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (_currentNode == null) return;
            
            var nodeTunnels = _tunnels.FindAll(t => t.NodeId == _currentNode.Id && t.IsEnabled);
            if (nodeTunnels.Count == 0)
            {
                MessageBox.Show("当前节点没有启用的隧道", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            txtLog.Clear();
            OnFrpOutput($"🚀 正在启动 FRP 客户端连接到 {_currentNode.Name}...");
            
            // 诊断启动前的节点数据
            Console.WriteLine($"🔍 启动前节点诊断:");
            Console.WriteLine($"   节点名称: '{_currentNode.Name}'");
            Console.WriteLine($"   服务器地址: '{_currentNode.ServerAddress}'");
            
            var success = await _frpService.StartFrpClient(_currentNode, nodeTunnels);
            if (success)
            {
                OnFrpOutput("✅ FRP 客户端启动成功");
            }
            else
            {
                OnFrpError("❌ FRP 客户端启动失败");
            }
            
            UpdateUI();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _frpService.StopFrpClient();
            UpdateUI();
        }

        private void lstNodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstNodes.SelectedItems.Count > 0)
            {
                _currentNode = (ServerNode)lstNodes.SelectedItems[0].Tag;
                DebugCurrentNode(); // 添加诊断
            }
            else
            {
                _currentNode = null;
            }
            UpdateUI();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _frpService.StopFrpClient();
        }

        /// <summary>
        /// 调整连接地址列表的列宽
        /// </summary>
        private void AdjustConnectionAddressColumns()
        {
            if (lstConnectionAddresses != null && pnlConnectionAddresses != null)
            {
                var availableWidth = pnlConnectionAddresses.Width - 30; // 减去边距和滚动条宽度
                
                if (availableWidth > 200)
                {
                    // 设置第一列宽度为150，第二列使用剩余宽度
                    lstConnectionAddresses.Columns[0].Width = 150;
                    lstConnectionAddresses.Columns[1].Width = availableWidth - 155;
                }
            }
        }

        // 清空日志按钮事件
        private void btnClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }

        // 复制连接地址按钮事件
        private void btnCopyAddress_Click(object sender, EventArgs e)
        {
            CopySelectedAddress();
        }

        /// <summary>
        /// 复制选中的地址
        /// </summary>
        private void CopySelectedAddress()
        {
            if (lstConnectionAddresses.SelectedItems.Count > 0)
            {
                var selectedItem = lstConnectionAddresses.SelectedItems[0];
                string address = selectedItem.Tag as string; // 从Tag中获取地址

                if (!string.IsNullOrEmpty(address) && address != "未配置远程地址")
                {
                    try
                    {
                        Clipboard.SetText(address);
                        OnFrpOutput($"📋 已复制连接地址到剪贴板: {address}");

                        // 显示复制成功的提示
                        MessageBox.Show($"已复制连接地址:\n{address}", "复制成功", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        OnFrpError($"❌ 复制到剪贴板失败: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("选中的项目没有有效的连接地址", "提示", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("请先选择一个连接地址", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // 设置按钮点击事件
        private void btnSettings_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(_configService);
            settingsForm.ShowDialog();
        }

        private void InitializeComponent()
        {
            // 设置窗体基本属性
            Text = "FRP客户端管理器 v1.0";
            ClientSize = new Size(1100, 750); // 增加窗口宽度
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 650);
            
            // 创建主选项卡控件
            var tabControl = new TabControl { Dock = DockStyle.Fill };
            
            // 节点管理选项卡
            var tabNodes = new TabPage { Text = "节点管理" };
            var pnlNodes = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            btnAddNode = new Button { Text = "添加节点", Location = new Point(10, 10), Size = new Size(80, 30) };
            btnEditNode = new Button { Text = "编辑节点", Location = new Point(100, 10), Size = new Size(80, 30) };
            btnDeleteNode = new Button { Text = "删除节点", Location = new Point(190, 10), Size = new Size(80, 30) };
            
            lstNodes = new ListView { 
                Location = new Point(10, 50),
                Size = new Size(pnlNodes.Width - 20, pnlNodes.Height - 60),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            lstNodes.Columns.Add("节点名称", 120);
            lstNodes.Columns.Add("服务器地址", 150);
            lstNodes.Columns.Add("服务器端口", 80);
            lstNodes.Columns.Add("是否启用", 80);
            
            pnlNodes.Controls.AddRange(new Control[] { btnAddNode, btnEditNode, btnDeleteNode, lstNodes });
            tabNodes.Controls.Add(pnlNodes);
            
            // 隧道管理选项卡
            var tabTunnels = new TabPage { Text = "隧道管理" };
            var pnlTunnels = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            btnAddTunnel = new Button { Text = "添加隧道", Location = new Point(10, 10), Size = new Size(80, 30) };
            btnEditTunnel = new Button { Text = "编辑隧道", Location = new Point(100, 10), Size = new Size(80, 30) };
            btnDeleteTunnel = new Button { Text = "删除隧道", Location = new Point(190, 10), Size = new Size(80, 30) };
            
            lstTunnels = new ListView { 
                Location = new Point(10, 50),
                Size = new Size(pnlTunnels.Width - 20, pnlTunnels.Height - 60),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            lstTunnels.Columns.Add("隧道名称", 100);
            lstTunnels.Columns.Add("所属节点", 100);
            lstTunnels.Columns.Add("协议", 60);
            lstTunnels.Columns.Add("本地地址", 120);
            lstTunnels.Columns.Add("远程地址", 200);
            lstTunnels.Columns.Add("是否启用", 80);
            
            pnlTunnels.Controls.AddRange(new Control[] { btnAddTunnel, btnEditTunnel, btnDeleteTunnel, lstTunnels });
            tabTunnels.Controls.Add(pnlTunnels);
            
            // 控制台选项卡 - 重新设计布局，确保地址完整显示且控件不重叠
            var tabConsole = new TabPage { Text = "控制台" };
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            // 第一行：控制按钮区域 - 增加高度并重新布局
            var pnlControl = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 70, // 增加高度以避免重叠
                Padding = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            lblStatus = new Label 
            { 
                Text = "🔴 已停止", 
                Location = new Point(10, 20), 
                AutoSize = true, 
                ForeColor = Color.Red, 
                Font = new Font("Microsoft YaHei", 10, FontStyle.Bold) 
            };
            
            lblConnectionInfo = new Label 
            { 
                Text = "请选择一个节点", 
                Location = new Point(120, 20), 
                AutoSize = true, 
                ForeColor = Color.Gray,
                Font = new Font("Microsoft YaHei", 9),
                MaximumSize = new Size(400, 0) // 限制最大宽度
            };
            
            btnStart = new Button 
            { 
                Text = "🚀 启动服务", 
                Location = new Point(550, 15), 
                Size = new Size(100, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Microsoft YaHei", 9, FontStyle.Bold)
            };
            
            btnStop = new Button 
            { 
                Text = "🛑 停止服务", 
                Location = new Point(660, 15), 
                Size = new Size(100, 35), 
                Enabled = false,
                BackColor = Color.LightCoral,
                Font = new Font("Microsoft YaHei", 9, FontStyle.Bold)
            };
            
            var btnClearLog = new Button 
            { 
                Text = "🗑️ 清空日志", 
                Location = new Point(770, 15), 
                Size = new Size(100, 35) 
            };
            
            // 新增设置按钮
            btnSettings = new Button 
            { 
                Text = "⚙️ 设置", 
                Location = new Point(880, 15), 
                Size = new Size(100, 35) 
            };
            
            pnlControl.Controls.AddRange(new Control[] { 
                lblStatus, lblConnectionInfo, btnStart, btnStop, btnClearLog, btnSettings
            });
            
            // 第二行：连接地址区域 - 使用 ListView 确保地址完整显示
            pnlConnectionAddresses = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 220, // 增加高度以容纳更多地址
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                Margin = new Padding(0, 10, 0, 0) // 添加上边距
            };
            
            var lblAddressTitle = new Label 
            { 
                Text = "🌐 远程连接地址（启动服务后可用）",
                Location = new Point(10, 10),
                AutoSize = true, 
                Font = new Font("Microsoft YaHei", 10, FontStyle.Bold),
                ForeColor = Color.Blue
            };
            
            // 使用 ListView 替代 ListBox，支持更好的水平滚动和列显示
            lstConnectionAddresses = new ListView
            {
                Location = new Point(10, 40),
                Size = new Size(pnlConnectionAddresses.Width - 25, 140),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 9),
                MultiSelect = false
            };
            
            // 添加列
            lstConnectionAddresses.Columns.Add("隧道名称", 150);
            lstConnectionAddresses.Columns.Add("连接地址", pnlConnectionAddresses.Width - 175); // 动态宽度
            
            // 双击复制功能
            lstConnectionAddresses.DoubleClick += (sender, e) =>
            {
                CopySelectedAddress();
            };
            
            btnCopyAddress = new Button 
            { 
                Text = "📋 复制选中地址", 
                Location = new Point(10, 185),
                Size = new Size(pnlConnectionAddresses.Width - 25, 25)
            };
            
            pnlConnectionAddresses.Controls.AddRange(new Control[] { 
                lblAddressTitle, lstConnectionAddresses, btnCopyAddress 
            });
            
            // 第三行：日志区域
            var pnlLog = new Panel 
            { 
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 10, 0, 0) // 添加上边距
            };
            
            var lblLogTitle = new Label 
            { 
                Text = "📝 日志输出",
                Location = new Point(5, 5),
                AutoSize = true, 
                Font = new Font("Microsoft YaHei", 9, FontStyle.Bold)
            };
            
            txtLog = new TextBox 
            { 
                Location = new Point(5, 30),
                Size = new Size(pnlLog.Width - 15, pnlLog.Height - 40),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.Black,
                ForeColor = Color.LightGreen,
                WordWrap = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            
            pnlLog.Controls.AddRange(new Control[] { lblLogTitle, txtLog });
            
            // 将各区域添加到主面板
            mainPanel.Controls.Add(pnlLog);
            mainPanel.Controls.Add(pnlConnectionAddresses);
            mainPanel.Controls.Add(pnlControl);
            
            tabConsole.Controls.Add(mainPanel);
            
            // 添加选项卡
            tabControl.TabPages.AddRange(new TabPage[] { tabNodes, tabTunnels, tabConsole });
            
            // 设置主窗体
            Controls.Add(tabControl);
            
            // 事件绑定
            btnAddNode.Click += btnAddNode_Click;
            btnEditNode.Click += btnEditNode_Click;
            btnDeleteNode.Click += btnDeleteNode_Click;
            btnAddTunnel.Click += btnAddTunnel_Click;
            btnEditTunnel.Click += btnEditTunnel_Click;
            btnDeleteTunnel.Click += btnDeleteTunnel_Click;
            btnStart.Click += btnStart_Click;
            btnStop.Click += btnStop_Click;
            btnClearLog.Click += btnClearLog_Click;
            btnCopyAddress.Click += btnCopyAddress_Click;
            btnSettings.Click += btnSettings_Click; // 设置按钮事件
            lstNodes.SelectedIndexChanged += lstNodes_SelectedIndexChanged;
            FormClosing += MainForm_FormClosing;
            
            // 添加窗体调整大小事件
            this.Resize += (sender, e) => 
            {
                // 调整连接地址列表的列宽
                AdjustConnectionAddressColumns();
                
                // 调整连接地址列表和复制按钮的大小
                if (lstConnectionAddresses != null && pnlConnectionAddresses != null)
                {
                    int availableWidth = pnlConnectionAddresses.Width - 25;
                    lstConnectionAddresses.Width = availableWidth;
                    btnCopyAddress.Width = availableWidth;
                    
                    // 调整列宽
                    AdjustConnectionAddressColumns();
                }
                
                // 调整日志文本框大小
                if (txtLog != null && pnlLog != null)
                {
                    txtLog.Width = pnlLog.Width - 15;
                    txtLog.Height = pnlLog.Height - 40;
                }
                
                // 调整第一行控件位置，确保不重叠
                if (pnlControl != null)
                {
                    // 计算可用空间并调整控件位置
                    int availableWidth = pnlControl.Width - 20;
                    
                    // 确保按钮有足够空间
                    int buttonStartX = availableWidth - 440; // 四个按钮总宽度 + 间距
                    if (buttonStartX > 500) // 确保按钮不会太靠左
                    {
                        btnStart.Left = buttonStartX;
                        btnStop.Left = buttonStartX + 110;
                        btnClearLog.Left = buttonStartX + 220;
                        btnSettings.Left = buttonStartX + 330;
                    }
                    
                    // 限制连接信息标签的最大宽度，避免与按钮重叠
                    int maxLabelWidth = btnStart.Left - lblStatus.Right - 20;
                    if (maxLabelWidth > 100)
                    {
                        lblConnectionInfo.MaximumSize = new Size(maxLabelWidth, 0);
                    }
                }
            };
        }
    }
}