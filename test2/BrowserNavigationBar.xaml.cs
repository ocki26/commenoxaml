using CefSharp;
using CefSharp.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace test2
{
    public partial class BrowserNavigationBar : UserControl
    {
        // Property để tham chiếu đến ChromiumWebBrowser của tab hiện tại
        public ChromiumWebBrowser CurrentBrowser { get; set; }

        public BrowserNavigationBar()
        {
            InitializeComponent();
        }

        // --- Event Handlers cho các nút điều hướng ---

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentBrowser?.Back();
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentBrowser?.Forward();
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentBrowser?.Reload();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Điều hướng về trang chủ mặc định (Google)
            NavigateTo("https://www.google.com");
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateFromAddressBar();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Thêm logic cho nút Menu (ví dụ: hiển thị menu ngữ cảnh)
            MessageBox.Show("Menu button clicked!");
        }


        // --- Logic điều hướng từ Address Bar ---

        private void AddressTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NavigateFromAddressBar();
            }
        }

        private void NavigateFromAddressBar()
        {
            string url = AddressTextBox.Text;
            if (!string.IsNullOrWhiteSpace(url))
            {
                NavigateTo(url);
            }
        }

        public void NavigateTo(string url)
        {
            if (CurrentBrowser != null)
            {
                // Kiểm tra và thêm scheme nếu cần
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url; // Mặc định dùng HTTPS
                }
                CurrentBrowser.Address = url;
            }
        }

        // --- Cập nhật Address Bar và trạng thái ---

        public void UpdateAddressBar(string url)
        {
            AddressTextBox.Text = url;
        }

        public void SetSecurityIcon(bool isSecure)
        {
            if (isSecure)
            {
                // TODO: Thay đổi icon khóa thành màu xanh hoặc một icon khóa đóng
                SecurityIcon.Fill = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                // TODO: Thay đổi icon khóa thành màu đỏ hoặc icon khóa mở/cảnh báo
                SecurityIcon.Fill = System.Windows.Media.Brushes.OrangeRed;
            }
        }

        public void SetNavigationButtonsState(bool canGoBack, bool canGoForward)
        {
            BackButton.IsEnabled = canGoBack;
            ForwardButton.IsEnabled = canGoForward;
        }
    }
}