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
    public class UnityWebRequestWrapper : IHttpWebRequest 
    {

        private UnityWebRequest mRequest;
        private UnityWebRequestAsyncOperation mOperation;
        private MemoryStream mRequestStream;
        private CookieContainer mCookies;

        public UnityWebRequestWrapper(Uri url) 
        {
            mRequest = new UnityWebRequest(url);
            mRequest.useHttpContinue = false;
            mCookies = new CookieContainer();
        }

        public string ContentType
        {
            get => mRequest.GetRequestHeader("Content-Type");
            set => mRequest.SetRequestHeader("Content-Type", value);
        }
        public string Accept
        {
            get => mRequest.GetRequestHeader("Accept");
            set => mRequest.SetRequestHeader("Accept", value);
        }
        public DateTime Date
        {
            get => DateTime.ParseExact(mRequest.GetRequestHeader("Date"), CultureInfo.CurrentCulture.DateTimeFormat.RFC1123Pattern, CultureInfo.CurrentCulture);
            set => mRequest.SetRequestHeader("Date", value.ToUniversalTime().ToString("r"));
        }
        public string Host {
            get => mRequest.GetRequestHeader("Host");
            set => mRequest.SetRequestHeader("Host", value);
        }
        public Uri RequestUri
        {
            get => mRequest.uri;
        }
        public long ContentLength
        {
            get => long.Parse(mRequest.GetRequestHeader("Content-Length"));
            set => mRequest.SetRequestHeader("Content-Length", value.ToString());
        }
        public bool KeepAlive
        {
            get => false;
            set
            {
            }
        }
        public string Expect
        {
            get => mRequest.GetRequestHeader("Expect");
            set => mRequest.SetRequestHeader("Expect", value);
        }
        public DateTime IfModifiedSince
        {
            get => DateTime.ParseExact(mRequest.GetRequestHeader("If-Modified-Since"), CultureInfo.CurrentCulture.DateTimeFormat.RFC1123Pattern, CultureInfo.CurrentCulture);
            set => mRequest.SetRequestHeader("If-Modified-Since", value.ToUniversalTime().ToString("r"));
        }
        public string Referer
        {
            get => mRequest.GetRequestHeader("Referer");
            set => mRequest.SetRequestHeader("Referer", value);
        }
        public string TransferEncoding
        {
            get => mRequest.GetRequestHeader("Transfer-Encoding");
            set => mRequest.SetRequestHeader("Transfer-Encoding", value);
        }
        public bool SendChunked
        {
            get => mRequest.chunkedTransfer;
            set => mRequest.chunkedTransfer = value;
        }
        public bool UseDefaultCredentials
        {
            get => false;
            set
            {
            }
        }
        public bool PreAuthenticate
        {
            get => false;
            set
            {
            }
        }
        public bool Pipelined
        {
            get => false;
            set
            {

            }
        }
        public bool UnsafeAuthenticatedConnectionSharing
        {
            get => false;
            set
            {

            }
        }
        public string Method
        {
            get => mRequest.method;
            set => mRequest.method = value;
        }
        public string UserAgent
        {
            get => mRequest.GetRequestHeader("User-Agent");
            set => mRequest.SetRequestHeader("User-Agent", value);
        }
        public int Timeout { get => mRequest.timeout; set => mRequest.timeout = value; }
        public int ReadWriteTimeout {
            get => 0;
            set
            {
            }
        }
        public bool AllowAutoRedirect
        {
            get => true;
            set {
            }
        }
        public int MaximumAutomaticRedirections
        {
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
        public RequestCachePolicy CachePolicy
        {
            get => throw new NotImplementedException();
            set
            {
            }
        }
        public DecompressionMethods AutomaticDecompression {
            get => DecompressionMethods.None;
            set
            {
            }
        }
        public bool Expect100Continue
        {
            get => mRequest.useHttpContinue;
            set => mRequest.useHttpContinue = true;
        }
        public WebProxy Proxy { get => throw new NotImplementedException(); set
            {

            }
        }
        public ICredentials Credentials
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

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
            mRequestStream = new MemoryStream();
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
            mRequest.downloadHandler = new DownloadHandlerBuffer();
            mRequest.SetRequestHeader("Cookie", mCookies.GetCookieHeader(mRequest.uri));

            if (mRequestStream != null)
            {
                var uploadHandler = new UploadHandlerRaw(mRequestStream.GetBuffer());
                mRequestStream.Close();
                mRequestStream = null;
                mRequest.uploadHandler = uploadHandler;
            }

            return mRequest.SendWebRequest();
        }

        private void handleOperationCompleted(UnityEngine.AsyncOperation obj)
        {
            throw new NotImplementedException();
        }

        public IHttpWebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            while (!mOperation.isDone);
            return new UnityWebResponse(mRequest);
        }

        public Stream GetRequestStream()
        {
            return mRequestStream;
        }

        public IHttpWebResponse GetResponse()
        {
            mOperation = SendRequest();
            while (!mOperation.isDone);
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
            mRequest.SetRequestHeader(name, value);
        }

        public void AddCookie(Cookie cookie)
        {
            mCookies.Add(mRequest.uri, cookie);
        }

        public Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return mRequestStream;
        }
        #endregion

    }

    class UnityWebRequestWaitHandle: WaitHandle
    {

    }

    class UnityWebRequestResult : IAsyncResult
    {
        private bool mCompleted = false;
        private bool mCompletedSynchronously = false;
        private UnityWebRequestWrapper mAsyncState;

        public object AsyncState => mAsyncState;

        public WaitHandle AsyncWaitHandle => new UnityWebRequestWaitHandle();

        public bool CompletedSynchronously => mCompletedSynchronously;

        public bool IsCompleted => mCompleted;

        public UnityWebRequestResult(UnityWebRequestWrapper request)
        {
            mAsyncState = request;
        }

        public void Complete(bool synchronous)
        {
            mCompleted = true;
            mCompletedSynchronously = synchronous;
        }
    }
}
