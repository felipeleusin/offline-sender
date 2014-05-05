using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace OfflineSender.Test
{
    public class When_Sending_A_Request : BaseTest
    {
        public When_Sending_A_Request()
        {
            Sender.SendWhenPossible(HttpMethod.Get, "http://fakeurl.com", new { test = "data" });    
        }

        [Fact]
        public void It_Should_Create_File_On_Queue_Directory()
        {
            FileSystem.Directory.GetFiles(TestPath).Length.ShouldBe(1);
        }

        [Fact]
        public void It_Should_ProcessQueue()
        {
            Sender.IsRunning.ShouldBe(true);
        }

        [Fact]
        public void Created_Request_Should_Be_Valid()
        {
            var request = GetLastRequest();

            request.ShouldNotBe(null);
        }
    }
}