// FileMerger.csproj
using System;
using System.IO;
using System.Windows.Forms;

namespace FileMerger
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            btnMerge.Click += (s, e) => MergeFiles();
        }

        private void MergeFiles()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "选择两个文件进行合并",
                Multiselect = true,
                Filter = "所有文件|*.*"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FileNames.Length != 2)
                {
                    MessageBox.Show("请选择两个文件");
                    return;
                }

                try
                {
                    var outputDir = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, 
                        "Data");
                    
                    Directory.CreateDirectory(outputDir);
                    
                    var outputPath = Path.Combine(
                        outputDir, 
                        $"merged_{DateTime.Now:yyyyMMddHHmmss}.txt");
                    
                    File.WriteAllText(outputPath, 
                        $"{File.ReadAllText(dialog.FileNames[0])}\n" +
                        $"{File.ReadAllText(dialog.FileNames[1])}");
                    
                    MessageBox.Show($"文件已保存到：{outputPath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"操作失败：{ex.Message}");
                }
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
