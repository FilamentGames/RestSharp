using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine.Networking;

namespace RestSharp 
{
    public class UnityWebRequestWrapper : IHttpWebRequest {

        private UnityWebRequest mRequest;
        private UnityWebRequestAsyncOperation mOperation;
        private MemoryStream mRequestStream;

        public UnityWebRequestWrapper(Uri url) {
            mRequest = new UnityWebRequest(url);
            CookieContainer = new CookieContainer();
            Headers = new WebHeaderCollection();
            ServicePoint = ServicePointManager.FindServicePoint(url);
        }

        public string ContentType {
            get => Headers.Get("Content-Type");
            set => Headers.Set("Content-Type", value);
        }
        public string Accept {
            get => Headers.Get("Accept");
            set => Headers.Set("Accept", value);
        }
        public DateTime Date {
            get => DateTime.ParseExact(Headers.Get("Date"), CultureInfo.CurrentCulture.DateTimeFormat.RFC1123Pattern, CultureInfo.CurrentCulture);
            set => Headers.Set("Date", value.ToUniversalTime().ToString("r"));
        }
        public string Host {
            get => Headers.Get("Host");
            set => Headers.Set("Host", value);
        }
        public Uri RequestUri {
            get => mRequest.uri;
        }
        public long ContentLength {
            get => 0;
            set
            {

            }
        }
        public bool KeepAlive {
            get => false;
            set {
            }
        }
        public string Expect {
            get => Headers.Get("Expect");
            set => Headers.Set("Expect", value);
        }
        public DateTime IfModifiedSince {
            get => DateTime.ParseExact(Headers.Get("If-Modified-Since"), CultureInfo.CurrentCulture.DateTimeFormat.RFC1123Pattern, CultureInfo.CurrentCulture);
            set => Headers.Set("If-Modified-Since", value.ToUniversalTime().ToString("r"));
        }
        public string Referer {
            get => Headers.Get("Referer");
            set => Headers.Set("Referer", value);
        }
        public string TransferEncoding {
            get => Headers.Get("Transfer-Encoding");
            set => Headers.Set("Transfer-Encoding", value);
        }
        public bool SendChunked {
            get => mRequest.chunkedTransfer;
            set => mRequest.chunkedTransfer = value;
        }
        public bool UseDefaultCredentials {
            get => false;
            set {
            }
        }
        public bool PreAuthenticate {
            get => false;
            set {
            }
        }
        public bool Pipelined {
            get => false;
            set {

            }
        }
        public bool UnsafeAuthenticatedConnectionSharing {
            get => false;
            set {

            }
        }
        public string Method {
            get => mRequest.method;
            set => mRequest.method = value;
        }
        public string UserAgent {
            get => Headers.Get("User-Agent");
            set => Headers.Set("User-Agent", value);
        }
        public int Timeout { get => mRequest.timeout; set => mRequest.timeout = value; }
        public int ReadWriteTimeout {
            get => 0;
            set {
            }
        }
        public bool AllowAutoRedirect {
            get => true;
            set {
            }
        }
        public int MaximumAutomaticRedirections {
            get => mRequest.redirectLimit;
            set => mRequest.redirectLimit = value;
        }
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback {
            get => throw new NotImplementedException();
            set {
            }
        }
        public string ConnectionGroupName {
            get => throw new NotImplementedException();
            set {
            }
        }
        public RequestCachePolicy CachePolicy {
            get => throw new NotImplementedException();
            set {
            }
        }
        public DecompressionMethods AutomaticDecompression {
            get => DecompressionMethods.None;
            set {
            }
        }
        public WebProxy Proxy { get => throw new NotImplementedException(); set {

            }
        }
        public ICredentials Credentials {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public string Connection {
            get => Headers.Get("Connection");
            set => Headers.Set("Connection", value);
        }

        public ServicePoint ServicePoint {get; private set;}

        IWebProxy IHttpWebRequest.Proxy { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public CookieContainer CookieContainer { get; set; }

        public X509CertificateCollection ClientCertificates => throw new NotImplementedException();

        public void Abort()
        {
            mRequest.Abort();
        }

        public void AddRange(string rangeSpecifier, long from, long to)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginGetRequestStream(AsyncCallback callback, IHttpWebRequest webRequest)
        {
            var result = new UnityWebRequestResult(this);
            result.Complete(true);
            callback(result);
            return result;
        }

        public IAsyncResult BeginGetResponse(AsyncCallback callback, IHttpWebRequest webRequest)
        {
            mOperation = SendRequest();

            var result = new UnityWebRequestResult(this);

            mOperation.completed += (UnityEngine.AsyncOperation obj) =>
            {
                result.Complete(false);
                callback(result);
            };

            return result;
        }

        private UnityWebRequestAsyncOperation SendRequest()
        {
            mRequest.useHttpContinue = ServicePoint.Expect100Continue;
            mRequest.downloadHandler = new DownloadHandlerBuffer();
            Headers.Set("Cookie", CookieContainer.GetCookieHeader(mRequest.uri));

            foreach ( var key in Headers.AllKeys) 
            {
                mRequest.SetRequestHeader(key, Headers[key]);
            }

            if (mRequestStream != null)
            {
                MemoryStream readStream = new MemoryStream(mRequestStream.ToArray());
                byte[] buffer = new byte[readStream.Length];
                readStream.Position = 0;
                readStream.Read(buffer, 0, (int)readStream.Length);
                var uploadHandler = new UploadHandlerRaw(buffer);
                mRequestStream.Close();
                mRequestStream = null;
                mRequest.uploadHandler = uploadHandler;
            }

            return mRequest.SendWebRequest();
        }

        public IHttpWebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            while (!mOperation.isDone);

            if (mRequest.isNetworkError) {
                throw new WebException(mRequest.error);
            }

            return new UnityWebResponse(mRequest);
        }

        public Stream GetRequestStream()
        {
            mRequestStream = new MemoryStream();
            return mRequestStream;
        }

        public IHttpWebResponse GetResponse()
        {
            mOperation = SendRequest();
            while (!mOperation.isDone);

            if (mRequest.isNetworkError) {
                throw new WebException(mRequest.error);
            }

            return new UnityWebResponse(mRequest);
        }

        public void GetResponseAsync()
        {
            
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    mRequest.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UnityWebRequestWrapper()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public void AddHeader(string name, string value)
        {
            Headers.Set(name, value);
        }

        public void AddCookie(Cookie cookie)
        {
            CookieContainer.Add(mRequest.uri, cookie);
        }

        public Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            mRequestStream = new MemoryStream();
            return mRequestStream;
        }
        #endregion

    }

    class UnityWebRequestWaitHandle: WaitHandle
    {

    }

    class UnityWebRequestResult : IAsyncResult
    {
        private UnityWebRequestWrapper mAsyncState;

        public object AsyncState => mAsyncState;

        public WaitHandle AsyncWaitHandle => new UnityWebRequestWaitHandle();

        public bool CompletedSynchronously { get; private set; } = false;

        public bool IsCompleted { get; private set; } = false;

        public UnityWebRequestResult(UnityWebRequestWrapper request)
        {
            mAsyncState = request;
        }

        public void Complete(bool synchronous)
        {
            IsCompleted = true;
            CompletedSynchronously = synchronous;
        }
    }
}
