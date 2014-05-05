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
    public class When_Processing_And_Server_Is_Offline : BaseTest
    {
        public When_Processing_And_Server_Is_Offline()
        {
            Sender.Servers.TryAdd("fakeurl.com", new Server() { Host = "fakeurl.com", LastOffline = DateTimeOffset.UtcNow });
            Sender.SendWhenPossible(HttpMethod.Post, "http://fakeurl.com", new { test = "data" }, runImediately: false);
            Sender.ProcessQueue();
        }

        [Fact]
        public void It_Should_Not_Make_Request()
        {
            A.CallTo(() => FakeClient.SendAsync(A<HttpRequestMessage>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public void It_Should_Not_Delete_File()
        {
            FileSystem.Directory.GetFiles(TestPath).Length.ShouldBe(1);
        }
    }
}