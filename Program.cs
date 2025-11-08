using System;
using System.Windows.Forms;

namespace FrpClientManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // 设置高 DPI 模式
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}