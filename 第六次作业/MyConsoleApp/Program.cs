using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace SimpleFileBrowser
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
            LoadUserDirectories();
        }

        private void InitializeUI()
        {
            // 设置窗体
            this.Text = "简单文件浏览器";
            this.Size = new System.Drawing.Size(800, 600);

            // 创建菜单栏
            MenuStrip menuStrip = new MenuStrip();
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("文件(&F)");
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("退出(&X)", null, (s, e) => Application.Exit());
            fileMenu.DropDownItems.Add(exitMenuItem);
            menuStrip.Items.Add(fileMenu);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            // 创建工具栏
            ToolStrip toolStrip = new ToolStrip();
            ToolStripButton refreshButton = new ToolStripButton("刷新", null, (s, e) => LoadUserDirectories());
            toolStrip.Items.Add(refreshButton);
            this.Controls.Add(toolStrip);

            // 创建分割容器
            SplitContainer splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.SplitterDistance = 250;
            this.Controls.Add(splitContainer);

            // 左侧树形视图
            treeView = new TreeView();
            treeView.Dock = DockStyle.Fill;
            treeView.ImageList = new ImageList();
            treeView.ImageList.Images.Add(SystemIcons.Folder);
            treeView.ImageList.Images.Add(SystemIcons.Application);
            treeView.AfterSelect += TreeView_AfterSelect;
            splitContainer.Panel1.Controls.Add(treeView);

            // 右侧列表视图
            listView = new ListView();
            listView.Dock = DockStyle.Fill;
            listView.View = View.Details;
            listView.Columns.Add("名称", 200);
            listView.Columns.Add("类型", 100);
            listView.Columns.Add("修改日期", 150);
            listView.Columns.Add("大小", 100);
            listView.DoubleClick += ListView_DoubleClick;
            splitContainer.Panel2.Controls.Add(listView);

            // 调整控件层级
            splitContainer.BringToFront();
            toolStrip.BringToFront();
        }

        private TreeView treeView;
        private ListView listView;

        private void LoadUserDirectories()
        {
            treeView.Nodes.Clear();
            
            // 获取用户目录
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            TreeNode rootNode = new TreeNode(Path.GetFileName(userPath), 0, 0);
            rootNode.Tag = userPath;
            treeView.Nodes.Add(rootNode);

            // 加载子目录
            LoadSubDirectories(rootNode);
        }

        private void LoadSubDirectories(TreeNode parentNode)
        {
            string path = parentNode.Tag as string;
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                foreach (string dir in Directory.GetDirectories(path))
                {
                    TreeNode node = new TreeNode(Path.GetFileName(dir), 0, 0);
                    node.Tag = dir;
                    parentNode.Nodes.Add(node);
                    
                    // 预加载一级子目录以显示展开图标
                    try
                    {
                        if (Directory.GetDirectories(dir).Length > 0)
                        {
                            node.Nodes.Add(new TreeNode()); // 添加空节点以显示+号
                        }
                    }
                    catch { }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 忽略无权限访问的目录
            }
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            listView.Items.Clear();
            string path = e.Node.Tag as string;
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                // 添加子目录
                foreach (string dir in Directory.GetDirectories(path))
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    ListViewItem item = new ListViewItem(di.Name, 0);
                    item.SubItems.Add("文件夹");
                    item.SubItems.Add(di.LastWriteTime.ToString());
                    item.SubItems.Add("");
                    item.Tag = dir;
                    listView.Items.Add(item);
                }

                // 添加文件
                foreach (string file in Directory.GetFiles(path))
                {
                    FileInfo fi = new FileInfo(file);
                    ListViewItem item = new ListViewItem(fi.Name, 1);
                    item.SubItems.Add(fi.Extension);
                    item.SubItems.Add(fi.LastWriteTime.ToString());
                    item.SubItems.Add(fi.Length.ToString("N0"));
                    item.Tag = file;
                    listView.Items.Add(item);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show($"无法访问: {path}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0) return;
            
            string path = listView.SelectedItems[0].Tag as string;
            if (string.IsNullOrEmpty(path)) return;

            // 如果是目录，在树形视图中展开
            if (Directory.Exists(path))
            {
                ExpandPathInTreeView(path);
                return;
            }

            // 如果是XML文件，解析并显示内容
            if (Path.GetExtension(path).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                ShowXmlContent(path);
            }
            else
            {
                // 其他文件尝试用默认程序打开
                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法打开文件: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExpandPathInTreeView(string path)
        {
            TreeNode node = FindNodeByPath(treeView.Nodes, path);
            if (node != null)
            {
                treeView.SelectedNode = node;
                node.Expand();
            }
        }

        private TreeNode FindNodeByPath(TreeNodeCollection nodes, string path)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag != null && node.Tag.ToString().Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    return node;
                }

                TreeNode found = FindNodeByPath(node.Nodes, path);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private void ShowXmlContent(string xmlFilePath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFilePath);

                Form xmlViewer = new Form();
                xmlViewer.Text = $"XML查看器 - {Path.GetFileName(xmlFilePath)}";
                xmlViewer.Size = new System.Drawing.Size(600, 400);

                TreeView xmlTree = new TreeView();
                xmlTree.Dock = DockStyle.Fill;
                xmlViewer.Controls.Add(xmlTree);

                // 加载XML内容到树形视图
                TreeNode rootNode = new TreeNode(doc.DocumentElement.Name);
                xmlTree.Nodes.Add(rootNode);
                AddXmlNodeToTree(doc.DocumentElement, rootNode);

                xmlViewer.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析XML文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddXmlNodeToTree(XmlNode xmlNode, TreeNode treeNode)
        {
            // 添加属性
            if (xmlNode.Attributes != null)
            {
                foreach (XmlAttribute attr in xmlNode.Attributes)
                {
                    TreeNode attrNode = new TreeNode($"@{attr.Name} = {attr.Value}");
                    treeNode.Nodes.Add(attrNode);
                }
            }

            // 添加子节点
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element)
                {
                    TreeNode childTreeNode = new TreeNode(childNode.Name);
                    treeNode.Nodes.Add(childTreeNode);
                    AddXmlNodeToTree(childNode, childTreeNode);
                }
                else if (childNode.NodeType == XmlNodeType.Text)
                {
                    if (!string.IsNullOrWhiteSpace(childNode.Value))
                    {
                        treeNode.Nodes.Add(new TreeNode($"文本: {childNode.Value.Trim()}"));
                    }
                }
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}

