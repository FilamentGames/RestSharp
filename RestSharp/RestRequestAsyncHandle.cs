using System.Net;

namespace RestSharp
{
    public class RestRequestAsyncHandle
    {
        public IHttpWebRequest WebRequest;

        public RestRequestAsyncHandle()
        {
        }

        public RestRequestAsyncHandle(IHttpWebRequest webRequest)
        {
            WebRequest = webRequest;
        }

        public void Abort()
        {
            if (WebRequest != null)
                WebRequest.Abort();
        }
    }
}
