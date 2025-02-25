using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace llcom
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }

        //重载锁，防止逻辑卡死
        private static bool fileLoading = false;
        //上次打开文件名
        private static string lastLuaFile = "";
        /// <summary>
        /// 加载lua脚本文件
        /// </summary>
        /// <param name="fileName">文件名，不带.lua</param>
        private void loadLuaFile(string fileName)
        {
            //检查文件是否存在
            if (!File.Exists(Tools.Global.ProfilePath + $"user_script_send_convert/{fileName}.lua"))
            {
                Tools.Global.setting.sendScript = "默认";
                if (!File.Exists(Tools.Global.ProfilePath + $"user_script_send_convert/{Tools.Global.setting.sendScript}.lua"))
                {
                    File.Create(Tools.Global.ProfilePath + $"user_script_send_convert/{Tools.Global.setting.sendScript}.lua").Close();
                }
            }
            else
            {
                Tools.Global.setting.sendScript = fileName;
            }

            //文件内容显示出来
            textEditor.Text = File.ReadAllText(Tools.Global.ProfilePath + $"user_script_send_convert/{Tools.Global.setting.sendScript}.lua");

            //刷新文件列表
            DirectoryInfo luaFileDir = new DirectoryInfo(Tools.Global.ProfilePath + "user_script_send_convert/");
            FileSystemInfo[] luaFiles = luaFileDir.GetFileSystemInfos();
            fileLoading = true;
            luaFileList.Items.Clear();
            for (int i = 0; i < luaFiles.Length; i++)
            {
                FileInfo file = luaFiles[i] as FileInfo;
                //是文件
                if (file != null && file.Name.IndexOf(".lua") == file.Name.Length - (".lua").Length)
                {
                    luaFileList.Items.Add(file.Name.Substring(0, file.Name.Length - 4));
                }
            }
            luaFileList.Text = lastLuaFile = Tools.Global.setting.sendScript;
            fileLoading = false;
        }

        /// <summary>
        /// 保存lua文件
        /// </summary>
        /// <param name="fileName">文件名，不带.lua</param>
        private void saveLuaFile(string fileName)
        {
            File.WriteAllText(Tools.Global.ProfilePath + $"user_script_send_convert/{fileName}.lua", textEditor.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = Tools.Global.setting;

            //重写关闭响应代码
            this.Closing += SettingWindow_Closing;

            //置顶显示以免被挡住
            this.Topmost = true;

            //初始化下拉框参数
            dataBitsComboBox.SelectedIndex = Tools.Global.setting.dataBits - 5;
            stopBitComboBox.SelectedIndex = Tools.Global.setting.stopBit - 1;
            dataCheckComboBox.SelectedIndex = Tools.Global.setting.parity;

            //快速搜索
            SearchPanel.Install(textEditor.TextArea);
            string name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".Lua.xshd";
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (System.IO.Stream s = assembly.GetManifestResourceStream(name))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    var xshd = HighlightingLoader.LoadXshd(reader);
                    textEditor.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
                }
            }
            //加载上次打开的文件
            loadLuaFile(Tools.Global.setting.sendScript);

            //加载编码
            var el = Encoding.GetEncodings();
            List<EncodingInfo> encodingList = new List<EncodingInfo>(el);
            //先排个序，美观点
            encodingList.Sort((x, y) => x.CodePage - y.CodePage);
            foreach (var en in encodingList)
            {
                ComboBoxItem c = new ComboBoxItem();
                c.Content = $"[{en.CodePage}] {en.Name}";
                c.Tag = en.CodePage;
                int index = encodingComboBox.Items.Add(c);
                if (Tools.Global.setting.encoding == en.CodePage)//现在用的编码
                    encodingComboBox.SelectedIndex = index;
            }
        }

        private void SettingWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //自动保存脚本
            if (lastLuaFile != "")
                saveLuaFile(lastLuaFile);
            if (Tools.Global.isMainWindowsClosed)
            {
                //说明软件关了
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;//取消这次关闭事件
                Hide();//隐藏窗口，以便下次调用show
            }
        }

        private void ApiDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Tools.Global.apiDocumentUrl);
        }

        private void OpenScriptFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Tools.Global.GetTrueProfilePath() + "user_script_send_convert");
        }

        private void DataBitsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dataBitsComboBox.SelectedItem != null)
            {
                Tools.Global.setting.dataBits = dataBitsComboBox.SelectedIndex + 5;
            }
        }

        private void StopBitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (stopBitComboBox.SelectedItem != null)
            {
                Tools.Global.setting.stopBit = stopBitComboBox.SelectedIndex + 1;
            }
        }

        private void DataCheckComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataCheckComboBox.SelectedItem != null)
            {
                Tools.Global.setting.parity = dataCheckComboBox.SelectedIndex;
                //MessageBox.Show((dataCheckComboBox.SelectedItem as ComboBoxItem).Content.ToString());
            }
        }

        private void NewScriptButton_Click(object sender, RoutedEventArgs e)
        {
            luaTestWrapPanel.Visibility = Visibility.Collapsed;
            newLuaFileWrapPanel.Visibility = Visibility.Visible;
        }

        private void TestScriptButton_Click(object sender, RoutedEventArgs e)
        {
            newLuaFileWrapPanel.Visibility = Visibility.Collapsed;
            luaTestWrapPanel.Visibility = Visibility.Visible;
        }

        private void LuaFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (luaFileList.SelectedItem != null && !fileLoading)
            {
                if (lastLuaFile != "")
                    saveLuaFile(lastLuaFile);
                string fileName = luaFileList.SelectedItem as string;
                loadLuaFile(fileName);
            }
        }

        private void NewLuaFileCancelbutton_Click(object sender, RoutedEventArgs e)
        {
            newLuaFileWrapPanel.Visibility = Visibility.Collapsed;
        }

        private void NewLuaFilebutton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(newLuaFileNameTextBox.Text))
            {
                MessageBox.Show(FindResource("LuaNoName") as string);
                return;
            }
            if (File.Exists(Tools.Global.ProfilePath + $"user_script_send_convert/{newLuaFileNameTextBox.Text}.lua"))
            {
                MessageBox.Show(FindResource("LuaExist") as string);
                return;
            }

            try
            {
                File.Create(Tools.Global.ProfilePath + $"user_script_send_convert/{newLuaFileNameTextBox.Text}.lua").Close();
                loadLuaFile(newLuaFileNameTextBox.Text);
            }
            catch
            {
                MessageBox.Show(FindResource("LuaCreateFail") as string);
                return;
            }
            newLuaFileWrapPanel.Visibility = Visibility.Collapsed;
        }

        private void LuaTestbutton_Click(object sender, RoutedEventArgs e)
        {
            if (luaFileList.SelectedItem != null && !fileLoading)
            {
                try
                {
                    string r = LuaEnv.LuaLoader.Run($"{luaFileList.SelectedItem as string}.lua",
                                        new System.Collections.ArrayList{"uartData",
                                           Tools.Global.String2Hex(luaTestTextBox.Text,"")});
                    MessageBox.Show($"{FindResource("SettingLuaRunResult") as string}\r\nHEX：" + r +
                        $"\r\n{FindResource("SettingLuaRawText") as string}" + Tools.Global.Hex2String(r));
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"{FindResource("ErrorScript") as string}\r\n" + ex.ToString());
                }

            }
        }

        private void LuaTestCancelbutton_Click(object sender, RoutedEventArgs e)
        {
            luaTestWrapPanel.Visibility = Visibility.Collapsed;
        }

        private void TextEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            //自动保存脚本
            if (lastLuaFile != "")
                saveLuaFile(lastLuaFile);
        }

        private void OpenLogButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Tools.Global.GetTrueProfilePath() + "logs");
        }

        private void encodingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox c = sender as ComboBox;
            if ((int)((ComboBoxItem)c.SelectedItem).Tag == Tools.Global.setting.encoding)
                return;
            Tools.Global.setting.encoding = (int)((ComboBoxItem)c.SelectedItem).Tag;
        }
    }
}
