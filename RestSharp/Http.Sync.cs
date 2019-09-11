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

#if FRAMEWORK || PocketPC
using System;
using System.Net;

#if !MONOTOUCH && !MONODROID && !PocketPC
using System.Web;
#endif

using RestSharp.Extensions;

namespace RestSharp
{
    /// <summary>
    /// HttpWebRequest wrapper (sync methods)
    /// </summary>
    public partial class Http
    {
        /// <summary>
        /// Execute a POST request
        /// </summary>
        public HttpResponse Post()
        {
            return PostPutInternal("POST");
        }

        /// <summary>
        /// Execute a PUT request
        /// </summary>
        public HttpResponse Put()
        {
            return PostPutInternal("PUT");
        }

        /// <summary>
        /// Execute a GET request
        /// </summary>
        public HttpResponse Get()
        {
            return GetStyleMethodInternal("GET");
        }

        /// <summary>
        /// Execute a HEAD request
        /// </summary>
        public HttpResponse Head()
        {
            return GetStyleMethodInternal("HEAD");
        }

        /// <summary>
        /// Execute an OPTIONS request
        /// </summary>
        public HttpResponse Options()
        {
            return GetStyleMethodInternal("OPTIONS");
        }

        /// <summary>
        /// Execute a DELETE request
        /// </summary>
        public HttpResponse Delete()
        {
            return GetStyleMethodInternal("DELETE");
        }

        /// <summary>
        /// Execute a PATCH request
        /// </summary>
        public HttpResponse Patch()
        {
            return PostPutInternal("PATCH");
        }

        /// <summary>
        /// Execute a MERGE request
        /// </summary>
        public HttpResponse Merge()
        {
            return PostPutInternal("MERGE");
        }

        /// <summary>
        /// Execute a GET-style request with the specified HTTP Method.  
        /// </summary>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public HttpResponse AsGet(string httpMethod)
        {
#if PocketPC
            return GetStyleMethodInternal(httpMethod.ToUpper());
#else
            return GetStyleMethodInternal(httpMethod.ToUpperInvariant());
#endif
        }

        /// <summary>
        /// Execute a POST-style request with the specified HTTP Method.  
        /// </summary>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public HttpResponse AsPost(string httpMethod)
        {
#if PocketPC
            return PostPutInternal(httpMethod.ToUpper());
#else
            return PostPutInternal(httpMethod.ToUpperInvariant());
#endif
        }

        private HttpResponse GetStyleMethodInternal(string method)
        {
            var webRequest = ConfigureWebRequest(method, Url);

            if (HasBody && (method == "DELETE" || method == "OPTIONS"))
            {
                webRequest.ContentType = RequestContentType;
                WriteRequestBody(webRequest);
            }

            return GetResponse(webRequest);
        }

        private HttpResponse PostPutInternal(string method)
        {
            var webRequest = ConfigureWebRequest(method, Url);

            PreparePostData(webRequest);

            WriteRequestBody(webRequest);
            return GetResponse(webRequest);
        }

        partial void AddSyncHeaderActions()
        {
            restrictedHeaderActions.Add("Connection", (r, v) => r.Connection = v);
            restrictedHeaderActions.Add("Content-Length", (r, v) => r.ContentLength = Convert.ToInt64(v));
            restrictedHeaderActions.Add("Expect", (r, v) => r.Expect = v);
            restrictedHeaderActions.Add("If-Modified-Since", (r, v) => r.IfModifiedSince = Convert.ToDateTime(v));
            restrictedHeaderActions.Add("Referer", (r, v) => r.Referer = v);
            restrictedHeaderActions.Add("Transfer-Encoding", (r, v) => { r.TransferEncoding = v; r.SendChunked = true; });
            restrictedHeaderActions.Add("User-Agent", (r, v) => r.UserAgent = v);
        }

        private void ExtractErrorResponse(HttpResponse httpResponse, Exception ex)
        {
            var webException = ex as WebException;

            if (webException != null && webException.Status == WebExceptionStatus.Timeout)
            {
                httpResponse.ResponseStatus = ResponseStatus.TimedOut;
                httpResponse.ErrorMessage = ex.Message;
                httpResponse.ErrorException = webException;
                return;
            }

            httpResponse.ErrorMessage = ex.Message;
            httpResponse.ErrorException = ex;
            httpResponse.ResponseStatus = ResponseStatus.Error;
        }

        private HttpResponse GetResponse(IHttpWebRequest request)
        {
            var response = new HttpResponse { ResponseStatus = ResponseStatus.None };

            try
            {
                var webResponse = GetRawResponse(request);
                ExtractResponseData(response, webResponse);
            }
            catch (Exception ex)
            {
                ExtractErrorResponse(response, ex);
            }

            return response;
        }

        private static IHttpWebResponse GetRawResponse(IHttpWebRequest request)
        {
            return request.GetResponse();
        }

        private void PreparePostData(IHttpWebRequest webRequest)
        {
            if (HasFiles || AlwaysMultipartFormData)
            {
                webRequest.ContentType = GetMultipartFormContentType();

                using (var requestStream = webRequest.GetRequestStream())
                {
                    WriteMultipartFormData(requestStream);
                }
            }

            PreparePostBody(webRequest);
        }

        private void WriteRequestBody(IHttpWebRequest webRequest)
        {
            if (!HasBody)
                return;

            var bytes = this.RequestBodyBytes ?? this.Encoding.GetBytes(this.RequestBody);

            webRequest.ContentLength = bytes.Length;

            using (var requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
        }

        // TODO: Try to merge the shared parts between ConfigureWebRequest and ConfigureAsyncWebRequest (quite a bit of code
        // TODO: duplication at the moment).
        private IHttpWebRequest ConfigureWebRequest(string method, Uri url)
        {
            IHttpWebRequest webRequest = new UnityWebRequestWrapper(url);
#if !PocketPC
            webRequest.UseDefaultCredentials = UseDefaultCredentials;
#endif
            webRequest.PreAuthenticate = PreAuthenticate;

            webRequest.ServicePoint.Expect100Continue = false;

            AppendHeaders(webRequest);

            webRequest.Method = method;

            // make sure Content-Length header is always sent since default is -1
            if (!HasFiles && !AlwaysMultipartFormData)
            {
                webRequest.ContentLength = 0;
            }

            webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None;

#if FRAMEWORK
            if (ClientCertificates != null)
            {
                webRequest.ClientCertificates.AddRange(ClientCertificates);
            }
#endif

            if (UserAgent.HasValue())
            {
                webRequest.UserAgent = UserAgent;
            }

            if (Timeout != 0)
            {
                webRequest.Timeout = Timeout;
            }

            if (ReadWriteTimeout != 0)
            {
                webRequest.ReadWriteTimeout = ReadWriteTimeout;
            }

            if (Credentials != null)
            {
                webRequest.Credentials = Credentials;
            }

            if (Proxy != null)
            {
                webRequest.Proxy = Proxy;
            }

            webRequest.AllowAutoRedirect = FollowRedirects;
            if (FollowRedirects && MaxRedirects.HasValue)
            {
                webRequest.MaximumAutomaticRedirections = MaxRedirects.Value;
            }

            return webRequest;
        }
    }
}

#endif
