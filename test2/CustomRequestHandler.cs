using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
// using CefSharp.Handler; // Chỉ cần nếu bạn dùng các lớp trong namespace này

namespace test2
{
    public class CustomRequestHandler : IRequestHandler
    {
        private readonly string _proxyUsername;
        private readonly string _proxyPassword;

        public CustomRequestHandler(string username, string password)
        {
            _proxyUsername = username;
            _proxyPassword = password;
        }

        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return null; // Trả về null để sử dụng xử lý mặc định cho Resource Requests
        }

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            return false; // Cho phép điều hướng bình thường
        }

        // Đã cập nhật phương thức OnRenderProcessTerminated với chữ ký mới
        public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status, int exitCode, string exitMessage)
        {
            // Logic xử lý khi quá trình render kết thúc (ví dụ: ghi log lỗi)
            // System.Diagnostics.Debug.WriteLine($"Render process terminated. Status: {status}, Exit Code: {exitCode}, Message: {exitMessage}");
        }

        // Đã thêm phương thức OnRenderViewReady
        public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            // Logic xử lý khi khung hiển thị của renderer đã sẵn sàng
            // (Ví dụ: inject JavaScript ngay sau khi render view sẵn sàng)
            // System.Diagnostics.Debug.WriteLine("Render View Ready!");
        }

        public IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response) { return null; }
        public bool OnResourceResponse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response) { return false; }
        public CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback) { return CefReturnValue.Continue; }
        public void OnResourceRedirect(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl) { }
        public bool OnQuotaRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, long newSize, IRequestCallback callback) { return false; }
        public void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength) { }
        public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback) { return false; }
        public void OnPluginReady(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string pluginPath, bool success) { }
        public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture) { return false; }
        public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser) { }
        public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            return false;
        }

        // Phương thức quan trọng cho xác thực
        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            if (isProxy)
            {
                if (!string.IsNullOrEmpty(_proxyUsername) && !string.IsNullOrEmpty(_proxyPassword))
                {
                    callback.Continue(_proxyUsername, _proxyPassword);
                    return true;
                }
            }
            callback.Dispose();
            return false;
        }
    }
}