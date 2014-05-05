using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using FakeItEasy;
using Shouldly;
using Xunit;

namespace OfflineSender.Test
{
    public class When_Processing_And_Request_Is_Completed : BaseTest
    {
        public When_Processing_And_Request_Is_Completed()
        {
            A.CallTo(() => FakeClient.SendAsync(A<HttpRequestMessage>.Ignored, A<CancellationToken>.Ignored))
                .Returns(new HttpResponseMessage(HttpStatusCode.OK));

            Sender.SendWhenPossible(HttpMethod.Post, "http://fakeurl.com", new { test = "data" }, runImediately: false);
            Sender.ProcessQueue();
        }

        [Fact]
        public void It_Sould_Delete_File()
        {
            FileSystem.Directory.GetFiles(TestPath).Length.ShouldBe(0);
        }
    }
}