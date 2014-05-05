using System.Net.Http;

namespace OfflineSender
{
    public class CachedRequest
    {
        public string Uri { get; set; }
        public HttpMethod Method { get; set; }
        public string Content { get; set; }
    }
}