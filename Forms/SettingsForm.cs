using System;
using System.Drawing;
using System.Windows.Forms;
using FrpClientManager.Services;

namespace FrpClientManager.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly ConfigService _configService;
        private TextBox txtDataPath;
        private Button btnBrowse;
        private Button btnSave;
        private Button btnCancel;
        private Label lblCurrentPath;
        private Label lblTip;

        public SettingsForm(ConfigService configService)
        {
            _configService = configService;
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            txtDataPath.Text = _configService.GetCurrentDataDirectory();
            lblCurrentPath.Text = $"当前数据目录: {_configService.GetCurrentDataDirectory()}";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择数据存储目录";
                folderDialog.SelectedPath = txtDataPath.Text;
                
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtDataPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDataPath.Text))
            {
                MessageBox.Show("请选择数据存储目录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool success = _configService.ChangeDataDirectory(txtDataPath.Text.Trim());
                if (success)
                {
                    MessageBox.Show("设置保存成功！重启应用后生效。", "提示", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("保存失败，请检查路径是否有效。", "错误", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存设置时出错：{ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void InitializeComponent()
        {
            // 控件声明
            var lblTitle = new Label();
            lblCurrentPath = new Label();
            var lblPath = new Label();
            txtDataPath = new TextBox();
            btnBrowse = new Button();
            btnSave = new Button();
            btnCancel = new Button();
            lblTip = new Label();

            // 设置控件属性
            lblTitle.Text = "应用设置";
            lblTitle.Location = new Point(20, 20);
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Microsoft YaHei", 12, FontStyle.Bold);

            lblCurrentPath.Text = "当前数据目录: ";
            lblCurrentPath.Location = new Point(20, 50);
            lblCurrentPath.AutoSize = true;
            lblCurrentPath.ForeColor = Color.Blue;

            lblPath.Text = "新数据目录:";
            lblPath.Location = new Point(20, 80);
            lblPath.Width = 100;

            txtDataPath.Location = new Point(120, 77);
            txtDataPath.Width = 250;
            txtDataPath.ReadOnly = true;

            btnBrowse.Text = "浏览...";
            btnBrowse.Location = new Point(380, 75);
            btnBrowse.Size = new Size(60, 25);
            btnBrowse.Click += btnBrowse_Click;

            lblTip.Text = "💡 更改数据目录后需要重启应用才能生效";
            lblTip.Location = new Point(20, 110);
            lblTip.AutoSize = true;
            lblTip.ForeColor = Color.DarkGreen;
            lblTip.Font = new Font("Microsoft YaHei", 9);

            btnSave.Text = "保存";
            btnSave.Location = new Point(200, 140);
            btnSave.Size = new Size(80, 30);
            btnSave.Click += btnSave_Click;

            btnCancel.Text = "取消";
            btnCancel.Location = new Point(290, 140);
            btnCancel.Size = new Size(80, 30);
            btnCancel.Click += btnCancel_Click;

            // 设置窗体
            Text = "应用设置";
            ClientSize = new Size(460, 190);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            // 添加控件
            Controls.AddRange(new Control[] {
                lblTitle, lblCurrentPath, lblPath, txtDataPath, btnBrowse, 
                lblTip, btnSave, btnCancel
            });
        }
    }
}