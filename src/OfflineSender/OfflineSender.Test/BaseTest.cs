using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Newtonsoft.Json;
using OfflineSender.Http;

namespace OfflineSender.Test
{
    public abstract class BaseTest
    {
        protected readonly string TestPath;
        protected readonly Sender Sender;
        protected readonly MockFileSystem FileSystem;
        protected readonly IHttpClient FakeClient;

        protected BaseTest()
        {
            TestPath = @"C:\Temp\Queue";
            FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), TestPath);
            FakeClient = A.Fake<IHttpClient>();
            Sender = new Sender(TestPath, FakeClient, FileSystem);
        }

        protected CachedRequest GetLastRequest()
        {
            var files = FileSystem.Directory.GetFiles(TestPath);
            if (files.Length > 1)
            {
                throw new InvalidOperationException("More than one request is made");
            }
            var file = files[0];

            return JsonConvert.DeserializeObject<CachedRequest>(FileSystem.GetFile(file).TextContents);
        }
    }
}
