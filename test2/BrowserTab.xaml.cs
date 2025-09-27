using CefSharp; // Cần dùng CefSharp để truy cập ILoadingStateChangeEventArgs
using System;
using System.Windows;
using System.Windows.Controls;

namespace test2
{
    public partial class BrowserTab : UserControl
    {
        public string TabTitle
        {
            get { return (string)GetValue(TabTitleProperty); }
            set { SetValue(TabTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TabTitle.  This enables animation, styling, binding, etc...
        public static readonly System.Windows.DependencyProperty TabTitleProperty =
            System.Windows.DependencyProperty.Register("TabTitle", typeof(string), typeof(BrowserTab), new PropertyMetadata("New Tab"));


        public BrowserTab()
        {
            InitializeComponent();
            NavigationBar.CurrentBrowser = Browser; // Gán trình duyệt cho thanh điều hướng
        }

        private void Browser_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Có thể thực hiện các khởi tạo ban đầu sau khi trình duyệt được tải
        }

        private void Browser_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // Cập nhật thanh địa chỉ khi địa chỉ thay đổi
                NavigationBar.UpdateAddressBar(Browser.Address);

                // Cập nhật icon bảo mật dựa trên giao thức
                NavigationBar.SetSecurityIcon(Browser.Address.StartsWith("https://"));
            });
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // Cập nhật trạng thái nút Back/Forward
                NavigationBar.SetNavigationButtonsState(e.CanGoBack, e.CanGoForward);

                // Nếu đang tải trang, có thể thay đổi icon Reload thành Stop
                // Nếu tải xong, thay đổi lại thành Reload
                // TODO: Thực hiện logic này nếu muốn
            });
        }

        private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Browser.IsBrowserInitialized)
            {
                // Lấy URL mặc định từ thanh địa chỉ và điều hướng khi trình duyệt được khởi tạo
                NavigationBar.NavigateTo(NavigationBar.AddressTextBox.Text);
            }
        }

        private void Browser_TitleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Cập nhật tiêu đề tab
            Dispatcher.Invoke(() =>
            {
                TabTitle = Browser.Title;
            });
        }

        // Phương thức để điều hướng từ bên ngoài tab
        public void Navigate(string url)
        {
            NavigationBar.NavigateTo(url);
        }

        private void NavigationBar_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}