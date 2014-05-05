using System.Net.Http;
using System.Net.Http.Headers;
using Shouldly;
using Xunit;

namespace OfflineSender.Test
{
    public class When_Sending_A_Get_Request : BaseTest
    {
        public When_Sending_A_Get_Request()
        {
            Sender.SendWhenPossible(HttpMethod.Get, "http://fakeurl.com", new { test = "data" });    
        }

        [Fact]
        public void It_Should_Attach_Query_String_To_Url()
        {
            var request = GetLastRequest();

            request.Uri.ShouldContain("?test=data");
        }
    }
}