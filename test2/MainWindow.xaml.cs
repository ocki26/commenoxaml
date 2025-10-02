using CefSharp;
using CefSharp.Wpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace test2
{
    public partial class MainWindow : Window
    {
        // Khai báo thông tin proxy (có thể đọc từ cấu hình hoặc UI)
        private const string ProxyServer = "142.111.124.238:6258";
        private const string ProxyUsername = "pvbubstg";
        private const string ProxyPassword = "87asjfv371b9";

        public MainWindow()
        {
            // 1. Cấu hình CefSettings cho Proxy
            CefSettings settings = new CefSettings();
            settings.CefCommandLineArgs.Add("enable-media-stream", "1");
            settings.CachePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test2\\CefSharpCache");

            // Cấu hình proxy nếu cần
            if (!string.IsNullOrEmpty(ProxyServer))
            {
                settings.CefCommandLineArgs.Add("proxy-server", ProxyServer);
                // Nếu proxy cần xác thực, CustomRequestHandler sẽ xử lý sau
            }
            // else {  settings.CefCommandLineArgs.Add("no-proxy-server", "1"); // Tùy chọn: Tắt hoàn toàn proxy nếu không muốn dùng }

            Cef.Initialize(settings);

            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            AddTab("https://www.google.com");
        }

        // --- Window Control Handlers ---
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeRestoreButton_Click(sender, e);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // --- Tab Management ---

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddTab("https://www.google.com");
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            Button closeButton = sender as Button;
            if (closeButton != null)
            {
                TabItem tabItem = FindParent<TabItem>(closeButton);
                if (tabItem != null)
                {
                    // Lấy BrowserTab control ra khỏi ContentHost trước khi dispose
                    BrowserTab browserTab = tabItem.Tag as BrowserTab; // Sử dụng Tag để lưu BrowserTab
                    if (browserTab != null)
                    {
                        if (BrowserContentHost.Children.Contains(browserTab))
                        {
                            BrowserContentHost.Children.Remove(browserTab);
                        }
                        browserTab.Browser.Dispose(); // Dispose CefSharp browser
                    }

                    BrowserTabControl.Items.Remove(tabItem);

                    if (BrowserTabControl.Items.Count == 0)
                    {
                        Close();
                    }
                    else
                    {
                        if (BrowserTabControl.SelectedIndex == -1)
                        {
                            BrowserTabControl.SelectedIndex = 0;
                        }
                        else if (BrowserTabControl.SelectedIndex >= BrowserTabControl.Items.Count)
                        {
                            BrowserTabControl.SelectedIndex = BrowserTabControl.Items.Count - 1;
                        }
                    }
                }
            }
        }


        private void AddTab(string url)
        {
            BrowserTab newBrowserTab = new BrowserTab();

            // 2. Gán RequestHandler cho mỗi ChromiumWebBrowser
            // Mỗi ChromiumWebBrowser cần một RequestHandler riêng để xử lý xác thực
            newBrowserTab.Browser.RequestHandler = new CustomRequestHandler(ProxyUsername, ProxyPassword);

            newBrowserTab.Navigate(url);

            TabItem newTabItem = new TabItem
            {
                Header = "Loading...",
                Tag = newBrowserTab
            };

            newTabItem.SetBinding(TabItem.DataContextProperty, new Binding(".") { Source = newBrowserTab });

            BrowserTabControl.Items.Add(newTabItem);
            BrowserTabControl.SelectedItem = newTabItem;
        }

        private void BrowserTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BrowserContentHost.Children.Clear();

            if (BrowserTabControl.SelectedItem is TabItem selectedTabItem)
            {
                BrowserTab selectedBrowserTab = selectedTabItem.Tag as BrowserTab;
                if (selectedBrowserTab != null)
                {
                    BrowserContentHost.Children.Add(selectedBrowserTab);
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (TabItem item in BrowserTabControl.Items)
            {
                if (item.Tag is BrowserTab browserTab)
                {
                    browserTab.Browser.Dispose();
                }
            }
            Cef.Shutdown();
            base.OnClosed(e);
        }
    }
}