using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine.Networking;

namespace RestSharp
{
    public class UnityWebResponse : IHttpWebResponse
    {
        private UnityWebRequest mRequest;
        private MemoryStream mResponseStream;
        private CookieContainer mResponseCookies;

        public string ContentEncoding => mRequest.GetResponseHeader("Content-Encoding");

        public string Server => mRequest.GetResponseHeader("Server");

        public Version ProtocolVersion => HttpVersion.Version10;

        public CookieCollection Cookies => mResponseCookies.GetCookies(mRequest.uri);

        public string ContentType => mRequest.GetResponseHeader("Content-Type");

        public long ContentLength => long.Parse(mRequest.GetResponseHeader("Content-Length"));

        public HttpStatusCode StatusCode => (HttpStatusCode) mRequest.responseCode;

        public string StatusDescription => mRequest.error;

        public Uri ResponseUri => mRequest.uri;

        public NameValueCollection Headers { get; private set; }

        public UnityWebResponse(UnityWebRequest request)
        {
            mRequest = request;
            mResponseStream = new MemoryStream(mRequest.downloadHandler.data);
            Headers = mRequest.GetResponseHeaders().Aggregate(new NameValueCollection(),
                (seed, current) => {
                    seed.Add(current.Key, current.Value);
                    return seed;
                });

            mResponseCookies = new CookieContainer();
            mResponseCookies.SetCookies(mRequest.uri, mRequest.GetResponseHeader("Cookie"));
        }

        public void Close()
        {
            mResponseStream.Close();
            mResponseStream = null;
            mRequest = null;
            Headers = null;
        }

        public Stream GetResponseStream()
        {
            return mResponseStream;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UnityWebResponse()
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
        #endregion
    }
}
