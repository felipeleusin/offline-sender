using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OfflineSender.Http
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token);
    }

    public class HttpClientWrapper : IHttpClient
    {
        private readonly HttpClient client;

        public HttpClientWrapper(HttpClient client)
        {
            this.client = client;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            return client.SendAsync(request, token);
        }
    }
}
