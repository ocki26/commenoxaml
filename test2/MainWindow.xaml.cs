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
        public MainWindow()
        {
            CefSettings settings = new CefSettings();
            settings.CefCommandLineArgs.Add("enable-media-stream", "1");
            // RẤT QUAN TRỌNG: Thiết lập đường dẫn cache để CefSharp lưu trữ dữ liệu
            // Điều này giúp trình duyệt hoạt động ổn định hơn và nhớ trạng thái giữa các lần mở
            settings.CachePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test2\\CefSharpCache");
            // Không nên dùng "no-sandbox" trong sản phẩm thực tế
            // settings.CefCommandLineArgs.Add("no-sandbox", "1");

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
                        // Chọn lại tab cuối cùng nếu tab hiện tại bị đóng
                        // Hoặc chọn tab kế bên nếu tab bị đóng không phải là tab cuối cùng
                        if (BrowserTabControl.SelectedIndex == -1)
                        {
                            BrowserTabControl.SelectedIndex = 0; // Chọn tab đầu tiên nếu không có gì được chọn
                        }
                        else if (BrowserTabControl.SelectedIndex >= BrowserTabControl.Items.Count)
                        {
                            BrowserTabControl.SelectedIndex = BrowserTabControl.Items.Count - 1;
                        }
                        // Trigger selection changed manually if needed to update UI
                        // BrowserTabControl_SelectionChanged(BrowserTabControl, null);
                    }
                }
            }
        }


        private void AddTab(string url)
        {
            BrowserTab newBrowserTab = new BrowserTab();
            newBrowserTab.Navigate(url);

            TabItem newTabItem = new TabItem
            {
                Header = "Loading...", // Tiêu đề ban đầu
                Tag = newBrowserTab // Lưu BrowserTab trong Tag của TabItem
            };

            // Thiết lập DataContext cho Header nếu bạn muốn binding phức tạp hơn
            // (Hiện tại HeaderTemplate đã tự động binding đến Content.TabTitle)
            newTabItem.SetBinding(TabItem.DataContextProperty, new Binding(".") { Source = newBrowserTab });

            BrowserTabControl.Items.Add(newTabItem);
            BrowserTabControl.SelectedItem = newTabItem; // Chọn tab vừa thêm
        }

        private void BrowserTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Xóa tất cả các BrowserTab cũ khỏi ContentHost
            BrowserContentHost.Children.Clear();

            if (BrowserTabControl.SelectedItem is TabItem selectedTabItem)
            {
                // Lấy BrowserTab từ Tag của TabItem
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
            // Dispose tất cả các BrowserTab khi đóng cửa sổ
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