using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Shouldly;
using Xunit;

namespace OfflineSender.Test
{
    public class When_Processing_And_Request_Throws : BaseTest
    {
        public When_Processing_And_Request_Throws()
        {
            A.CallTo(() => FakeClient.SendAsync(A<HttpRequestMessage>.Ignored, A<CancellationToken>.Ignored))
                .Returns(Task.Factory.StartNew<HttpResponseMessage>(() =>
                {
                    throw new HttpRequestException("Unable to connect to remote host.");
                }));

            Sender.SendWhenPossible(HttpMethod.Post, "http://fakeurl.com", new { test = "data" }, runImediately: false);
            Sender.ProcessQueue();
        }

        [Fact]
        public void It_Sould_Not_Delete_File()
        {
            FileSystem.Directory.GetFiles(TestPath).Length.ShouldBe(1);
        }

        [Fact]
        public void It_Should_Add_Host_To_Offline_Hosts()
        {
            Server server;

            Sender.Servers.TryGetValue("fakeurl.com", out server).ShouldBe(true);
            server.LastOffline.ShouldBeGreaterThan(DateTimeOffset.MinValue);
        }
    }
}