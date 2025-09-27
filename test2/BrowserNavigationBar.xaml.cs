using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace test2
{
    public partial class BrowserNavigationBar : UserControl
    {
        // Property để tham chiếu đến ChromiumWebBrowser của tab hiện tại
        public ChromiumWebBrowser CurrentBrowser { get; set; }
        private static readonly HttpClient _httpClient = new HttpClient();
        private System.Threading.CancellationTokenSource _autocompleteCancellationTokenSource;

        public BrowserNavigationBar()
        {
            InitializeComponent(); // Dòng này sẽ khởi tạo các điều khiển từ XAML
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
                if (SuggestionsPopup != null)
                {
                    SuggestionsPopup.IsOpen = false; // Đóng popup khi nhấn Enter
                }
            }
            else if (e.Key == Key.Down)
            {
                if (SuggestionsListBox.Items.Count > 0)
                {
                    SuggestionsListBox.Focus();
                    SuggestionsListBox.SelectedIndex = 0;
                    ListBoxItem item = (ListBoxItem)SuggestionsListBox.ItemContainerGenerator.ContainerFromIndex(0);
                    item?.Focus();
                }
            }
        }
        private async void AddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = AddressTextBox.Text;

            // Hủy yêu cầu autocomplete trước đó nếu có
            _autocompleteCancellationTokenSource?.Cancel();
            _autocompleteCancellationTokenSource = new System.Threading.CancellationTokenSource();
            var cancellationToken = _autocompleteCancellationTokenSource.Token;

            if (SuggestionsPopup != null && (string.IsNullOrWhiteSpace(query) || IsValidUrl(query)))
            {
                SuggestionsPopup.IsOpen = false;
                return;
            }
            // ..

            try
            {
                // Độ trễ nhỏ để tránh gửi quá nhiều yêu cầu khi người dùng đang gõ
                await Task.Delay(300, cancellationToken);

                string encodedQuery = Uri.EscapeDataString(query);
                string apiUrl = $"http://suggestqueries.google.com/complete/search?client=firefox&q={encodedQuery}";

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl, cancellationToken);
                response.EnsureSuccessStatusCode(); // Ném ngoại lệ nếu mã trạng thái không thành công

                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Phân tích JSON
                JArray jsonArray = JArray.Parse(jsonResponse);
                JArray suggestionsArray = jsonArray[1] as JArray; // Lấy mảng thứ 2 chứa các gợi ý

                List<string> suggestions = new List<string>();
                if (suggestionsArray != null)
                {
                    foreach (var item in suggestionsArray)
                    {
                        suggestions.Add(item.ToString());
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    SuggestionsListBox.ItemsSource = suggestions;
                    if (SuggestionsPopup != null)
                    {
                        SuggestionsPopup.IsOpen = suggestions.Count > 0;
                    }
                });
            }
            catch (OperationCanceledException)
            {
                // Yêu cầu đã bị hủy, bỏ qua
            }
            catch (Exception ex)
            {
                // Xử lý lỗi (ví dụ: ghi log)
                Console.WriteLine($"Error fetching suggestions: {ex.Message}");
                SuggestionsPopup.IsOpen = false;
            }
        }

        private void AddressTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Đóng popup khi AddressTextBox mất focus, nhưng có thể cần trì hoãn
            // để cho phép click vào ListBoxItem.
            // Hoặc xử lý trong PreviewMouseUp của ListBoxItem.
            // Để đơn giản, ban đầu có thể đóng luôn, sau này điều chỉnh nếu gặp lỗi UX.
            // SuggestionsPopup.IsOpen = false; // Đã thêm logic vào PreviewMouseUp của ListBox.
        }

        private void AddressTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Khi AddressTextBox nhận focus, nếu có gợi ý, mở lại popup
            if (SuggestionsListBox.Items.Count > 0)
            {
                SuggestionsPopup.IsOpen = true;
            }
            // Trigger lại TextChanged để hiển thị gợi ý cũ nếu có
            AddressTextBox_TextChanged(sender, null);
        }

        private void SuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuggestionsListBox.SelectedItem is string selectedSuggestion)
            {
                AddressTextBox.Text = selectedSuggestion;
                AddressTextBox.CaretIndex = AddressTextBox.Text.Length; // Đặt con trỏ ở cuối
                NavigateFromAddressBar(); // Điều hướng khi chọn gợi ý

                // Thêm kiểm tra null ở đây
                if (SuggestionsPopup != null)
                {
                    SuggestionsPopup.IsOpen = false; // Đóng popup
                }
                else
                {
                    // Đây là nơi bạn có thể đặt một breakpoint hoặc log để kiểm tra
                    System.Diagnostics.Debug.WriteLine("SuggestionsPopup is null in SelectionChanged!");
                }
            }
        }

        private void SuggestionsListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Dùng PreviewMouseUp để đảm bảo sự kiện SelectionChanged được xử lý trước
            // và sau đó đóng popup.
            // Điều này giải quyết vấn đề LostFocus của TextBox đóng Popup quá nhanh.
            SuggestionsPopup.IsOpen = false;
        }



        private void NavigateFromAddressBar()
        {
            string input = AddressTextBox.Text;
            if (string.IsNullOrWhiteSpace(input)) return;

            if (IsValidUrl(input))
            {
                NavigateTo(input);
            }
            else
            {
                string encodedQuery = Uri.EscapeDataString(input);
                string searchUrl = $"https://www.google.com/search?q={encodedQuery}";
                NavigateTo(searchUrl);
            }
        }
        private bool IsValidUrl(string input)
        {
            return Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public void NavigateTo(string url)
        {
            if (CurrentBrowser != null)
            {
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }
                CurrentBrowser.Address = url;
            }
        }

        // --- Cập nhật Address Bar và trạng thái ---

        public void UpdateAddressBar(string url)
        {
            // Chỉ cập nhật nếu URL hiện tại không giống với nội dung AddressTextBox
            // Tránh việc ghi đè lên khi người dùng đang gõ
            if (AddressTextBox.Text != url)
            {
                AddressTextBox.Text = url;
            }
        }

        public void SetSecurityIcon(bool isSecure)
        {
            if (isSecure)
            {
                SecurityIcon.Fill = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
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