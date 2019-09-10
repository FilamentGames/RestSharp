using System;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Cache;
using System.Collections.Specialized;

namespace RestSharp
{
    public interface IHttpWebRequest : IDisposable
    {
        string ContentType { get; set; }
        string Accept { get; set; }
        DateTime Date { get; set; }
        string Host { get; set; }
        Uri RequestUri { get; }
        long ContentLength { get; set;  }
        bool KeepAlive { get; set; }
        string Expect { get; set; }
        DateTime IfModifiedSince { get; set; }
        string Referer { get; set; }
        string TransferEncoding { get; set; }
        bool SendChunked { get; set; }
        bool UseDefaultCredentials { get; set; }
        bool PreAuthenticate { get; set; }
        bool Pipelined { get; set; }
        bool UnsafeAuthenticatedConnectionSharing { get; set; }
        string Method { get; set; }
        string UserAgent { get; set; }
        DecompressionMethods AutomaticDecompression { get; set; }
        int Timeout { get; set; }
        int ReadWriteTimeout { get; set; }
        bool AllowAutoRedirect { get; set; }
        bool Expect100Continue { get; set; }
        int MaximumAutomaticRedirections { get; set; }
        RemoteCertificateValidationCallback ServerCertificateValidationCallback { get; set; }
        string ConnectionGroupName { get; set; }
        RequestCachePolicy CachePolicy { get; set; }
        WebProxy Proxy { get; set; }

        ICredentials Credentials { get; set; }

        void GetResponseAsync();
        IAsyncResult BeginGetResponse(AsyncCallback callback, IHttpWebRequest webRequest);
        IAsyncResult BeginGetRequestStream(AsyncCallback callback, IHttpWebRequest webRequest);
        Stream EndGetRequestStream(IAsyncResult asyncResult);
        IHttpWebResponse GetResponse();
        IHttpWebResponse EndGetResponse(IAsyncResult asyncResult);
        void Abort();
        void AddRange(string rangeSpecifier, long from, long to);
        Stream GetRequestStream();
        void AddHeader(string name, string value);
        void AddCookie(Cookie cookie);
    }
}
