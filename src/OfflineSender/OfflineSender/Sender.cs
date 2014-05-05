using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OfflineSender.Http;

namespace OfflineSender
{
    public class Sender : IDisposable
    {
        private readonly string path;
        private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private readonly IHttpClient client;
        private readonly IFileSystem fs;

        public readonly ConcurrentDictionary<string, Server> Servers = new ConcurrentDictionary<string, Server>();

        public bool IsRunning { get; private set; }

        public Sender(string path) : this(path, new HttpClientWrapper(new HttpClient()), new FileSystem()) { }

        public Sender(string path, IHttpClient client, IFileSystem fs)
        {
            this.path = path;
            this.client = client;
            this.fs = fs;

            if (!fs.Directory.Exists(path))
            {
                fs.Directory.CreateDirectory(path);
            }
        }

        public void SetAuthorization(string host, AuthenticationHeaderValue header)
        {
            Servers.AddOrUpdate(host, new Server()
            {
                Authorization = header,
                Host = host
            }, (h, server) => {
                server.Authorization = header;
                return server;
            });
        }

        public HttpRequestMessage SendWhenPossible(HttpMethod method, string url, object data, bool runImediately = true)
        {
            var request = new HttpRequestMessage(method, url);
            if (data != null)
            {
                if (method == HttpMethod.Get || method == HttpMethod.Head || method == HttpMethod.Options)
                {
                    request.RequestUri = new Uri(string.Format("{0}?{1}", url, BuildQueryString(data)));
                }
                else
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(data));    
                }    
            }


            using (var sw = fs.File.CreateText(fs.Path.Combine(path, DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssFFFFFFF"))))
            {
                var file = new CachedRequest()
                {
                    Uri = request.RequestUri.ToString(),
                    Method = request.Method,
                    Content = request.Content != null ? request.Content.ReadAsStringAsync().Result : String.Empty,
                };

                sw.Write(JsonConvert.SerializeObject(file));
            }

            if (!IsRunning && runImediately)
            {
                Task.Factory.StartNew(ProcessQueue, cancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

            return request;
        }

        public void ProcessQueue()
        {
            if (IsRunning)
            {
                return;
            }

            IsRunning = true;
            var requests = fs.Directory.GetFiles(path);
            foreach (var file in requests)
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<CachedRequest>(fs.File.ReadAllText(file));
                    var request = new HttpRequestMessage(data.Method, data.Uri);
                    
                    var server = Servers.GetOrAdd(request.RequestUri.Host, s => new Server()
                    {
                        Host = request.RequestUri.Host,
                        Authorization = request.Headers.Authorization,
                        LastOffline = DateTimeOffset.MinValue
                    });

                    var now = DateTimeOffset.UtcNow;
                    if (now.Subtract(server.LastOffline) < TimeSpan.FromMinutes(10))
                    {
                        continue;
                    }

                    if (server.Authorization != null)
                    {
                        request.Headers.Authorization = server.Authorization;
                    }

                    var requestFile = file;
                    client.SendAsync(request, cancellationToken.Token)
                        .ContinueWith(x =>
                        {
                            
                            if (x.IsFaulted)
                            {
                                Servers.AddOrUpdate(request.RequestUri.Host, server, (host, s) =>
                                {
                                    s.LastOffline = DateTimeOffset.UtcNow;
                                    return s;
                                });
                            }
                            else
                            {
                                server.LastOffline = DateTimeOffset.MinValue;
                                fs.File.Delete(requestFile);
                            }

                        });

                    Thread.Sleep(100);

                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                }
            }
            IsRunning = false;
        }

        private string BuildQueryString(object data)
        {
            var fields = new List<string>();
            foreach (var propertyInfo in data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                fields.Add(string.Format("{0}={1}",propertyInfo.Name,propertyInfo.GetValue(data, null)));
            }

            return string.Join("&", fields);
        }

        public void Dispose()
        {
            cancellationToken.Cancel();
        }
    }

    public class Server
    {
        public string Host { get; set; }

        public AuthenticationHeaderValue Authorization { get; set; }

        public DateTimeOffset LastOffline { get; set; }
    }
}
