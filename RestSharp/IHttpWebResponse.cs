using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace RestSharp
{
    public interface IHttpWebResponse : IDisposable
    {
        string ContentEncoding { get; }
        string Server { get; }
        Version ProtocolVersion { get; }
        ICollection<Cookie> Cookies { get; }
        string ContentType { get; }
        long ContentLength { get; }
        HttpStatusCode StatusCode { get; }
        string StatusDescription { get; }
        Uri ResponseUri { get; }
        NameValueCollection Headers { get; }

        void Close();
        Stream GetResponseStream();
    }
}
