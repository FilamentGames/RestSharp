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
using System.Linq;
using System.Net;
using System.Threading;
using RestSharp.Extensions;

namespace RestSharp
{
    /// <summary>
    ///     HttpWebRequest wrapper (async methods)
    /// </summary>
    public partial class Http
    {
        private TimeOutState timeoutState;

        public IHttpWebRequest DeleteAsync(Action<HttpResponse> action)
        {
            return GetStyleMethodInternalAsync("DELETE", action);
        }

        public IHttpWebRequest GetAsync(Action<HttpResponse> action)
        {
            return GetStyleMethodInternalAsync("GET", action);
        }

        public IHttpWebRequest HeadAsync(Action<HttpResponse> action)
        {
            return GetStyleMethodInternalAsync("HEAD", action);
        }

        public IHttpWebRequest OptionsAsync(Action<HttpResponse> action)
        {
            return GetStyleMethodInternalAsync("OPTIONS", action);
        }

        public IHttpWebRequest PostAsync(Action<HttpResponse> action)
        {
            return PutPostInternalAsync("POST", action);
        }

        public IHttpWebRequest PutAsync(Action<HttpResponse> action)
        {
            return PutPostInternalAsync("PUT", action);
        }

        public IHttpWebRequest PatchAsync(Action<HttpResponse> action)
        {
            return PutPostInternalAsync("PATCH", action);
        }

        public IHttpWebRequest MergeAsync(Action<HttpResponse> action)
        {
            return PutPostInternalAsync("MERGE", action);
        }

        /// <summary>
        ///     Execute an async POST-style request with the specified HTTP Method.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public IHttpWebRequest AsPostAsync(Action<HttpResponse> action, string httpMethod)
        {
            return PutPostInternalAsync(httpMethod.ToUpperInvariant(), action);
        }

        /// <summary>
        ///     Execute an async GET-style request with the specified HTTP Method.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public IHttpWebRequest AsGetAsync(Action<HttpResponse> action, string httpMethod)
        {
            return GetStyleMethodInternalAsync(httpMethod.ToUpperInvariant(), action);
        }

        private IHttpWebRequest GetStyleMethodInternalAsync(string method, Action<HttpResponse> callback)
        {
            IHttpWebRequest webRequest = null;

            try
            {
                var url = Url;

                webRequest = ConfigureAsyncWebRequest(method, url);

                if (HasBody && (method == "DELETE" || method == "OPTIONS"))
                {
                    webRequest.ContentType = RequestContentType;
                    WriteRequestBodyAsync(webRequest, callback);
                }
                else
                {
                    timeoutState = new TimeOutState {Request = webRequest};

                    var asyncResult = webRequest.BeginGetResponse(
                        result => ResponseCallback(result, callback), webRequest);

                    SetTimeout(asyncResult, timeoutState);
                }
            }
            catch (Exception ex)
            {
                ExecuteCallback(CreateErrorResponse(ex), callback);
            }

            return webRequest;
        }

        private HttpResponse CreateErrorResponse(Exception ex)
        {
            var response = new HttpResponse();

            if (ex is WebException webException && webException.Status == WebExceptionStatus.RequestCanceled)
            {
                response.ResponseStatus = timeoutState.TimedOut
                    ? ResponseStatus.TimedOut
                    : ResponseStatus.Aborted;

                return response;
            }

            response.ErrorMessage = ex.Message;
            response.ErrorException = ex;
            response.ResponseStatus = ResponseStatus.Error;

            return response;
        }

        private IHttpWebRequest PutPostInternalAsync(string method, Action<HttpResponse> callback)
        {
            IHttpWebRequest webRequest = null;

            try
            {
                webRequest = ConfigureAsyncWebRequest(method, Url);
                PreparePostBody(webRequest);
                WriteRequestBodyAsync(webRequest, callback);
            }
            catch (Exception ex)
            {
                ExecuteCallback(CreateErrorResponse(ex), callback);
            }

            return webRequest;
        }

        private void WriteRequestBodyAsync(IHttpWebRequest webRequest, Action<HttpResponse> callback)
        {
            IAsyncResult asyncResult;
            timeoutState = new TimeOutState {Request = webRequest};

            if (HasBody || HasFiles || AlwaysMultipartFormData)
            {
                webRequest.ContentLength = CalculateContentLength();
                asyncResult = webRequest.BeginGetRequestStream(
                    result => RequestStreamCallback(result, callback), webRequest);
            }
            else
            {
                asyncResult = webRequest.BeginGetResponse(r => ResponseCallback(r, callback), webRequest);
            }

            SetTimeout(asyncResult, timeoutState);
        }

        private long CalculateContentLength()
        {
            if (RequestBodyBytes != null)
                return RequestBodyBytes.Length;

            if (!HasFiles && !AlwaysMultipartFormData)
                return Encoding.GetByteCount(RequestBody);

            // calculate length for multipart form
            long length = 0;

            foreach (var file in Files)
            {
                length += Encoding.GetByteCount(GetMultipartFileHeader(file));
                length += file.ContentLength;
                length += Encoding.GetByteCount(LineBreak);
            }

            length = Parameters.Aggregate(length,
                (current, param) => current + Encoding.GetByteCount(GetMultipartFormData(param)));

            length += Encoding.GetByteCount(GetMultipartFooter());

            return length;
        }

        private void RequestStreamCallback(IAsyncResult result, Action<HttpResponse> callback)
        {
            var webRequest = (IHttpWebRequest) result.AsyncState;

            if (timeoutState.TimedOut)
            {
                var response = new HttpResponse {ResponseStatus = ResponseStatus.TimedOut};

                ExecuteCallback(response, callback);

                return;
            }

            // write body to request stream
            try
            {
                using (var requestStream = webRequest.EndGetRequestStream(result))
                {
                    if (HasFiles || AlwaysMultipartFormData)
                        WriteMultipartFormData(requestStream);
                    else if (RequestBodyBytes != null)
                        requestStream.Write(RequestBodyBytes, 0, RequestBodyBytes.Length);
                    else if (RequestBody != null)
                        WriteStringTo(requestStream, RequestBody);
                }

                var asyncResult = webRequest.BeginGetResponse(r => ResponseCallback(r, callback), webRequest);

                SetTimeout(asyncResult, timeoutState);
            }
            catch (Exception ex)
            {
                ExecuteCallback(CreateErrorResponse(ex), callback);
            }
        }

        private void SetTimeout(IAsyncResult asyncResult, TimeOutState timeOutState)
        {
            if (Timeout != 0)
                ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle,
                    TimeoutCallback, timeOutState, Timeout, true);
        }

        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (!timedOut)
                return;

            if (!(state is TimeOutState timeoutState))
                return;

            lock (timeoutState)
            {
                timeoutState.TimedOut = true;
            }

            timeoutState.Request?.Abort();
        }

        private static void GetRawResponseAsync(IAsyncResult result, Action<IHttpWebResponse> callback)
        {
            IHttpWebResponse raw;

            try
            {
                var webRequest = (IHttpWebRequest) result.AsyncState;

                raw = webRequest.EndGetResponse(result) as IHttpWebResponse;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.RequestCanceled)
                    throw;

                // Check to see if this is an HTTP error or a transport error.
                // In cases where an HTTP error occurs ( status code >= 400 )
                // return the underlying HTTP response, otherwise assume a
                // transport exception (ex: connection timeout) and
                // rethrow the exception

                if (ex.Response is IHttpWebResponse response)
                    raw = response;
                else
                    throw;
            }

            callback(raw);

            raw?.Close();
        }

        private void ResponseCallback(IAsyncResult result, Action<HttpResponse> callback)
        {
            var response = new HttpResponse {ResponseStatus = ResponseStatus.None};

            try
            {
                if (timeoutState.TimedOut)
                {
                    response.ResponseStatus = ResponseStatus.TimedOut;
                    ExecuteCallback(response, callback);

                    return;
                }

                GetRawResponseAsync(result, webResponse =>
                {
                    ExtractResponseData(response, webResponse);
                    ExecuteCallback(response, callback);
                });
            }
            catch (Exception ex)
            {
                ExecuteCallback(CreateErrorResponse(ex), callback);
            }
        }

        private static void ExecuteCallback(HttpResponse response, Action<HttpResponse> callback)
        {
            PopulateErrorForIncompleteResponse(response);
            callback(response);
        }

        private static void PopulateErrorForIncompleteResponse(HttpResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed && response.ErrorException == null)
            {
                response.ErrorException = response.ResponseStatus.ToWebException();
                response.ErrorMessage = response.ErrorException.Message;
            }
        }

        protected virtual IHttpWebRequest ConfigureAsyncWebRequest(string method, Uri url)
        {
            return ConfigureWebRequest(method, url);
        }

        private class TimeOutState
        {
            public bool TimedOut { get; set; }

            public IHttpWebRequest Request { get; set; }
        }
    }
}