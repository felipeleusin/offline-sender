using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace OfflineSender.Test
{
    public class When_Sending_A_Post_Request : BaseTest
    {
        private object TestData = new {test = "data"};

        public When_Sending_A_Post_Request()
        {
            Sender.SendWhenPossible(HttpMethod.Post, "http://fakeurl.com", TestData);    
        }

        [Fact]
        public void It_Should_Serialize_Data_As_Body()
        {
            var request = GetLastRequest();

            request.Content.ShouldBe(JsonConvert.SerializeObject(TestData));
        }
    }
}