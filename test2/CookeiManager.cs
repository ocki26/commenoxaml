using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace test2
{
    // Lớp này implement interface ICookieManager của CefSharp
    // => Dùng để quản lý cookie trong Chromium (xóa, thêm, duyệt cookies)
    internal class CookeiManager : ICookieManager
    {
        private bool disposedValue;
        private readonly List<Cookie> _CookeiMenmory;
        private readonly bool _isPersisted;
        public CookeiManager(bool isPersisted)
        {
            _isPersisted = isPersisted;
            _CookeiMenmory = new List<Cookie>();
        }


        // Thuộc tính kiểm tra object đã dispose chưa
        // Hiện tại chưa implement, chỉ throw Exception
        public bool IsDisposed => disposedValue;

        // Xóa cookie theo url + name, có callback sau khi hoàn tất
        public bool DeleteCookies(string url = null, string name = null, IDeleteCookiesCallback callback = null)
        {
            int initialCount = _CookeiMenmory.Count;
            _CookeiMenmory.RemoveAll(c =>
               (string.IsNullOrEmpty(url) || c.Domain.Contains(new Uri(url).Host)) && // Đơn giản hóa so khớp domain
               (string.IsNullOrEmpty(name) || c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
           );
            bool Deleted = _CookeiMenmory.Count < initialCount;
            callback?.OnComplete(Deleted ? 1:0);
            return Deleted;
        }

        // FlushStore: ép lưu cookie xuống disk (thường dùng khi đóng app)
        public bool FlushStore(ICompletionCallback callback)
        {
            if (_isPersisted)
            {
                // TODO: Triển khai logic để ghi _cookiesInMemory xuống đĩa cho hồ sơ này
                // Ví dụ: Serialize _cookiesInMemory sang JSON và lưu vào file
                Console.WriteLine("Flushing cookies to persistent storage...");
                callback?.OnComplete(); // Giả sử luôn thành công
            }
            else
            {
                // Không làm gì cả, vì cookie chỉ ở trong RAM
                Console.WriteLine("Not flushing cookies (Incognito/In-memory mode).");
                callback?.OnComplete();
            }
            return true;
        }

        // SetCookie: thêm cookie mới vào 1 url
        public bool SetCookie(string url, Cookie cookie, ISetCookieCallback callback = null)
        {
           _CookeiMenmory.RemoveAll(c => c.Name.Equals(cookie.Name, StringComparison.OrdinalIgnoreCase) && c.Domain.Equals(cookie.Domain, StringComparison.OrdinalIgnoreCase) && c.Path.Equals(cookie.Path, StringComparison.OrdinalIgnoreCase));
              _CookeiMenmory.Add(cookie);
              callback?.OnComplete(true);
                return true;
        }

        // Duyệt tất cả cookies hiện có
        public bool VisitAllCookies(ICookieVisitor visitor)
        {
            for(int i = 0; i < _CookeiMenmory.Count; i++)
            {
                Cookie curentCookei = _CookeiMenmory[i];
                bool deleteCookei = false;
                bool continueVisiting = visitor.Visit(curentCookei, i, _CookeiMenmory.Count, ref deleteCookei);
                if (deleteCookei)
                {
                    _CookeiMenmory.RemoveAt(i);
                    i--; // Giảm i để không bỏ qua phần tử kế tiếp
                }
                  if(!continueVisiting)
                  {
                      break;
                }

            }
            visitor.Dispose();
            return true;
        }

        // Duyệt cookies theo url (có thể filter HttpOnly)
        public bool VisitUrlCookies(string url, bool includeHttpOnly, ICookieVisitor visitor)
        {
            Uri uri = new Uri(url);
            string host = uri.Host;
            Func<Cookie, bool> domainMatches = c => c.Domain.Contains(host) || host.Contains(c.Domain);
            List<Cookie> matchingCookies = _CookeiMenmory.Where(c => domainMatches(c) && (includeHttpOnly || !c.HttpOnly)).ToList();
            for (int i = 0; i < matchingCookies.Count; i++)
            {
                Cookie currentCookie = matchingCookies[i];
                bool deleteCookie = false;
                bool continueVisiting = visitor.Visit(currentCookie, i, matchingCookies.Count, ref deleteCookie);
                if (deleteCookie)
                {
                    _CookeiMenmory.Remove(currentCookie);
                }
                if (!continueVisiting)
                {
                    break;
                }
            }
            visitor.Dispose();
            return true;
        }

        // Dispose pattern chuẩn .NET
        // => đảm bảo giải phóng resource khi object bị hủy
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_isPersisted)
                    {
                        // Thực hiện FlushStore để đảm bảo lưu trữ cuối cùng
                        // Hoặc bạn có thể gọi nó ở đây nếu muốn đồng bộ hơn
                        // Tuy nhiên, FlushStore là async, nên gọi nó cần xử lý
                        // hoặc đảm bảo nó đã hoàn thành trước khi Dispose thực sự kết thúc.
                        // Đơn giản nhất là giả định các thao tác trước đó đã được Flush (hoặc sẽ được Flush ngay sau đây)
                        Console.WriteLine("Disposing persistent CookeiManager. Cookies might be flushed if not already.");
                    }
                    else
                    {
                        // Xóa hoàn toàn cookie khỏi RAM khi đối tượng bị dispose
                        _CookeiMenmory.Clear();
                        Console.WriteLine("Disposing in-memory CookeiManager. All cookies cleared from RAM.");
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // Hàm Dispose công khai
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this); // Ngăn GC gọi finalizer nữa
        }
    }
}
