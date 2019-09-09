using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine.Networking;

namespace RestSharp
{
    public class UnityWebResponse : IHttpWebResponse
    {
        private UnityWebRequest mRequest;
        private MemoryStream mResponseStream;

        public string ContentEncoding => throw new NotImplementedException();

        public string Server => throw new NotImplementedException();

        public Version ProtocolVersion => throw new NotImplementedException();

        public ICollection<Cookie> Cookies => throw new NotImplementedException();

        public string ContentType => throw new NotImplementedException();

        public long ContentLength => throw new NotImplementedException();

        public HttpStatusCode StatusCode => throw new NotImplementedException();

        public string StatusDescription => throw new NotImplementedException();

        public Uri ResponseUri => throw new NotImplementedException();

        public NameValueCollection Headers => throw new NotImplementedException();

        public UnityWebResponse(UnityWebRequest request)
        {
            mRequest = request;
            mResponseStream = new MemoryStream(mRequest.downloadHandler.data);
        }

        public void Close()
        {
            mRequest.Dispose();
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
