using Shouldly;
using Xunit;

namespace OfflineSender.Test
{
    public class When_Initializing_Sender : BaseTest
    {
        [Fact]
        public void It_Should_Create_Directory()
        {
            FileSystem.Directory.Exists(TestPath).ShouldBe(true);
        }
    }
}