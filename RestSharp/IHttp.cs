#region License
//   Copyright 2010 John Sheehan
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RestSharp
{
    public interface IHttp
    {
        Action<Stream> ResponseWriter { get; set; }

#if !PocketPC
        CookieContainer CookieContainer { get; set; }
#endif

        ICredentials Credentials { get; set; }

        /// <summary>
        /// Always send a multipart/form-data request - even when no Files are present.
        /// </summary>
        bool AlwaysMultipartFormData { get; set; }

        string UserAgent { get; set; }

        int Timeout { get; set; }

        int ReadWriteTimeout { get; set; }

#if !SILVERLIGHT
        bool FollowRedirects { get; set; }
#endif

#if FRAMEWORK
        X509CertificateCollection ClientCertificates { get; set; }

        int? MaxRedirects { get; set; }
#endif

#if !PocketPC
        bool UseDefaultCredentials { get; set; }
#endif

        Encoding Encoding { get; set; }

        IList<HttpHeader> Headers { get; }

        IList<HttpParameter> Parameters { get; }

        IList<HttpFile> Files { get; }

        IList<HttpCookie> Cookies { get; }

        string RequestBody { get; set; }

        string RequestContentType { get; set; }

        bool PreAuthenticate { get; set; }

        /// <summary>
        /// An alternative to RequestBody, for when the caller already has the byte array.
        /// </summary>
        byte[] RequestBodyBytes { get; set; }

        Uri Url { get; set; }

        IHttpWebRequest DeleteAsync(Action<HttpResponse> action);

        IHttpWebRequest GetAsync(Action<HttpResponse> action);

        IHttpWebRequest HeadAsync(Action<HttpResponse> action);

        IHttpWebRequest OptionsAsync(Action<HttpResponse> action);

        IHttpWebRequest PostAsync(Action<HttpResponse> action);

        IHttpWebRequest PutAsync(Action<HttpResponse> action);

        IHttpWebRequest PatchAsync(Action<HttpResponse> action);

        IHttpWebRequest MergeAsync(Action<HttpResponse> action);

        IHttpWebRequest AsPostAsync(Action<HttpResponse> action, string httpMethod);

        IHttpWebRequest AsGetAsync(Action<HttpResponse> action, string httpMethod);

#if FRAMEWORK || PocketPC
        HttpResponse Delete();

        HttpResponse Get();

        HttpResponse Head();

        HttpResponse Options();

        HttpResponse Post();

        HttpResponse Put();

        HttpResponse Patch();

        HttpResponse Merge();

        HttpResponse AsPost(string httpMethod);

        HttpResponse AsGet(string httpMethod);

        IWebProxy Proxy { get; set; }
#endif
    }
}
