using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using FakeItEasy;
using Xunit;

namespace OfflineSender.Test
{
    public class When_Processing_Queue : BaseTest
    {
        public When_Processing_Queue()
        {
            Sender.SendWhenPossible(HttpMethod.Post, "http://fakeurl.com", new { test = "data" }, runImediately: false);
            Sender.ProcessQueue();
        }

        [Fact]
        public void It_Should_Call_Http_Client()
        {
            A.CallTo(() => FakeClient.SendAsync(A<HttpRequestMessage>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappened();
        }
    }
}