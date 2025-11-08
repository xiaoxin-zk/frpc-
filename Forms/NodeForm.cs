using System;
using System.Drawing;
using System.Windows.Forms;
using FrpClientManager.Models;

namespace FrpClientManager.Forms
{
    public partial class NodeForm : Form
    {
        public ServerNode Node { get; private set; }

        public NodeForm(ServerNode node = null)
        {
            InitializeComponent();
            Node = node ?? new ServerNode();
            
            if (node != null)
            {
                Text = "编辑节点";
                LoadNodeData();
            }
            else
            {
                Text = "添加节点";
            }
        }

        private void LoadNodeData()
        {
            txtName.Text = Node.Name;
            txtServerAddress.Text = Node.ServerAddress;
            numServerPort.Value = Node.ServerPort;
            txtToken.Text = Node.Token;
            txtAdminAddress.Text = Node.AdminAddress;
            numAdminPort.Value = Node.AdminPort;
            chkEnabled.Checked = Node.IsEnabled;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("请输入节点名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Node.Name = txtName.Text.Trim();
            Node.ServerAddress = txtServerAddress.Text.Trim();
            Node.ServerPort = (int)numServerPort.Value;
            Node.Token = txtToken.Text;
            Node.AdminAddress = txtAdminAddress.Text.Trim();
            Node.AdminPort = (int)numAdminPort.Value;
            Node.IsEnabled = chkEnabled.Checked;

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
            var lblServer = new Label();
            txtServerAddress = new TextBox();
            numServerPort = new NumericUpDown();
            var lblToken = new Label();
            txtToken = new TextBox();
            var lblAdmin = new Label();
            txtAdminAddress = new TextBox();
            numAdminPort = new NumericUpDown();
            chkEnabled = new CheckBox();
            btnSave = new Button();
            btnCancel = new Button();

            // 设置控件属性
            lblName.Text = "节点名称:";
            lblName.Location = new Point(20, 20);
            lblName.Width = 80;

            txtName.Location = new Point(100, 17);
            txtName.Width = 200;

            lblServer.Text = "服务器地址:";
            lblServer.Location = new Point(20, 50);
            lblServer.Width = 80;

            txtServerAddress.Text = "127.0.0.1";
            txtServerAddress.Location = new Point(100, 47);
            txtServerAddress.Width = 150;

            numServerPort.Minimum = 1;
            numServerPort.Maximum = 65535;
            numServerPort.Value = 7000;
            numServerPort.Location = new Point(260, 47);
            numServerPort.Width = 60;

            lblToken.Text = "认证令牌:";
            lblToken.Location = new Point(20, 80);
            lblToken.Width = 80;

            txtToken.UseSystemPasswordChar = true;
            txtToken.Location = new Point(100, 77);
            txtToken.Width = 200;

            lblAdmin.Text = "管理地址:";
            lblAdmin.Location = new Point(20, 110);
            lblAdmin.Width = 80;

            txtAdminAddress.Text = "127.0.0.1";
            txtAdminAddress.Location = new Point(100, 107);
            txtAdminAddress.Width = 150;

            numAdminPort.Minimum = 1;
            numAdminPort.Maximum = 65535;
            numAdminPort.Value = 7400;
            numAdminPort.Location = new Point(260, 107);
            numAdminPort.Width = 60;

            chkEnabled.Text = "启用节点";
            chkEnabled.Checked = true;
            chkEnabled.Location = new Point(100, 140);

            btnSave.Text = "保存";
            btnSave.Location = new Point(150, 180);
            btnSave.Width = 80;
            btnSave.Click += btnSave_Click;

            btnCancel.Text = "取消";
            btnCancel.Location = new Point(240, 180);
            btnCancel.Width = 80;
            btnCancel.Click += btnCancel_Click;

            // 设置窗体
            Text = "FRP节点配置";
            ClientSize = new Size(350, 230);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            // 添加控件
            Controls.AddRange(new Control[] {
                lblName, txtName,
                lblServer, txtServerAddress, numServerPort,
                lblToken, txtToken,
                lblAdmin, txtAdminAddress, numAdminPort,
                chkEnabled,
                btnSave, btnCancel
            });
        }

        // 控件字段
        private TextBox txtName;
        private TextBox txtServerAddress;
        private NumericUpDown numServerPort;
        private TextBox txtToken;
        private TextBox txtAdminAddress;
        private NumericUpDown numAdminPort;
        private CheckBox chkEnabled;
        private Button btnSave;
        private Button btnCancel;
    }
}