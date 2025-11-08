using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FrpClientManager.Models;

namespace FrpClientManager.Forms
{
    public partial class TunnelForm : Form
    {
        public TunnelConfig Tunnel { get; private set; }
        private readonly List<ServerNode> _nodes;

        public TunnelForm(List<ServerNode> nodes, TunnelConfig tunnel = null)
        {
            _nodes = nodes;
            Tunnel = tunnel ?? new TunnelConfig();
            
            InitializeComponent();
            
            if (tunnel != null)
            {
                Text = "编辑隧道";
                LoadTunnelData();
            }
            else
            {
                Text = "添加隧道";
            }
            
            LoadNodes();
        }

        private void LoadNodes()
        {
            cmbNode.Items.Clear();
            foreach (var node in _nodes)
            {
                cmbNode.Items.Add(node);
            }
            
            if (cmbNode.Items.Count > 0)
                cmbNode.SelectedIndex = 0;
        }

        private void LoadTunnelData()
        {
            txtName.Text = Tunnel.Name;
            
            // 选择对应的节点
            for (int i = 0; i < cmbNode.Items.Count; i++)
            {
                if (cmbNode.Items[i] is ServerNode node && node.Id == Tunnel.NodeId)
                {
                    cmbNode.SelectedIndex = i;
                    break;
                }
            }
            
            cmbProtocol.SelectedItem = Tunnel.Protocol;
            txtLocalIp.Text = Tunnel.LocalIp;
            numLocalPort.Value = Tunnel.LocalPort;
            numRemotePort.Value = Tunnel.RemotePort;
            txtCustomDomain.Text = Tunnel.CustomDomain;
            txtSubDomain.Text = Tunnel.SubDomain;
            chkEnabled.Checked = Tunnel.IsEnabled;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("请输入隧道名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbNode.SelectedItem == null)
            {
                MessageBox.Show("请选择节点", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Tunnel.Name = txtName.Text.Trim();
            Tunnel.NodeId = ((ServerNode)cmbNode.SelectedItem).Id;
            Tunnel.Protocol = cmbProtocol.SelectedItem?.ToString() ?? "tcp";
            Tunnel.LocalIp = txtLocalIp.Text.Trim();
            Tunnel.LocalPort = (int)numLocalPort.Value;
            Tunnel.RemotePort = (int)numRemotePort.Value;
            Tunnel.CustomDomain = txtCustomDomain.Text.Trim();
            Tunnel.SubDomain = txtSubDomain.Text.Trim();
            Tunnel.IsEnabled = chkEnabled.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void InitializeComponent()
        {
            // 控件声明
            var lblName = new Label();
            txtName = new TextBox();
            var lblNode = new Label();
            cmbNode = new ComboBox();
            var lblProtocol = new Label();
            cmbProtocol = new ComboBox();
            var lblLocal = new Label();
            txtLocalIp = new TextBox();
            numLocalPort = new NumericUpDown();
            var lblRemote = new Label();
            numRemotePort = new NumericUpDown();
            var lblDomain = new Label();
            txtCustomDomain = new TextBox();
            var lblSubDomain = new Label();
            txtSubDomain = new TextBox();
            chkEnabled = new CheckBox();
            btnSave = new Button();
            btnCancel = new Button();

            // 设置控件属性
            lblName.Text = "隧道名称:";
            lblName.Location = new Point(20, 20);
            lblName.Width = 80;

            txtName.Location = new Point(100, 17);
            txtName.Width = 200;

            lblNode.Text = "所属节点:";
            lblNode.Location = new Point(20, 50);
            lblNode.Width = 80;

            cmbNode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbNode.Location = new Point(100, 47);
            cmbNode.Width = 200;

            lblProtocol.Text = "协议类型:";
            lblProtocol.Location = new Point(20, 80);
            lblProtocol.Width = 80;

            cmbProtocol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProtocol.Location = new Point(100, 77);
            cmbProtocol.Width = 100;
            cmbProtocol.Items.AddRange(new object[] { "tcp", "udp", "http", "https" });
            cmbProtocol.SelectedIndex = 0;

            lblLocal.Text = "本地地址:";
            lblLocal.Location = new Point(20, 110);
            lblLocal.Width = 80;

            txtLocalIp.Text = "127.0.0.1";
            txtLocalIp.Location = new Point(100, 107);
            txtLocalIp.Width = 100;

            numLocalPort.Minimum = 1;
            numLocalPort.Maximum = 65535;
            numLocalPort.Value = 80;
            numLocalPort.Location = new Point(210, 107);
            numLocalPort.Width = 60;

            lblRemote.Text = "远程端口:";
            lblRemote.Location = new Point(20, 140);
            lblRemote.Width = 80;

            numRemotePort.Minimum = 0;
            numRemotePort.Maximum = 65535;
            numRemotePort.Value = 0;
            numRemotePort.Location = new Point(100, 137);
            numRemotePort.Width = 60;

            lblDomain.Text = "自定义域名:";
            lblDomain.Location = new Point(20, 170);
            lblDomain.Width = 80;

            txtCustomDomain.Location = new Point(100, 167);
            txtCustomDomain.Width = 200;

            lblSubDomain.Text = "子域名:";
            lblSubDomain.Location = new Point(20, 200);
            lblSubDomain.Width = 80;

            txtSubDomain.Location = new Point(100, 197);
            txtSubDomain.Width = 200;

            chkEnabled.Text = "启用隧道";
            chkEnabled.Checked = true;
            chkEnabled.Location = new Point(100, 230);

            btnSave.Text = "保存";
            btnSave.Location = new Point(150, 270);
            btnSave.Width = 80;
            btnSave.Click += btnSave_Click;

            btnCancel.Text = "取消";
            btnCancel.Location = new Point(240, 270);
            btnCancel.Width = 80;
            btnCancel.Click += btnCancel_Click;

            // 设置窗体
            Text = "隧道配置";
            ClientSize = new Size(350, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            // 添加控件
            Controls.AddRange(new Control[] {
                lblName, txtName,
                lblNode, cmbNode,
                lblProtocol, cmbProtocol,
                lblLocal, txtLocalIp, numLocalPort,
                lblRemote, numRemotePort,
                lblDomain, txtCustomDomain,
                lblSubDomain, txtSubDomain,
                chkEnabled,
                btnSave, btnCancel
            });
        }

        // 控件字段
        private TextBox txtName;
        private ComboBox cmbNode;
        private ComboBox cmbProtocol;
        private TextBox txtLocalIp;
        private NumericUpDown numLocalPort;
        private NumericUpDown numRemotePort;
        private TextBox txtCustomDomain;
        private TextBox txtSubDomain;
        private CheckBox chkEnabled;
        private Button btnSave;
        private Button btnCancel;
    }
}