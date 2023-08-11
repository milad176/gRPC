using Blog;
using Greet;
using Grpc.Core;
using Sqrt;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace client
{
    class Program
    {
        const string target = "127.0.0.1:50051";

        static async Task Main(string[] args)
        {
            //var clientCert = File.ReadAllText("ssl/client.crt");
            //var clientKey = File.ReadAllText("ssl/client.key");
            //var caCrt = File.ReadAllText("ssl/ca.crt");

            //var channelCredentials = new SslCredentials(caCrt, new KeyCertificatePair(clientCert, clientKey));

            Channel channel = new Channel("localhost", 50051, ChannelCredentials.Insecure);

            await channel.ConnectAsync().ContinueWith((task) =>
             {
                 if (task.Status == TaskStatus.RanToCompletion)
                     Console.WriteLine("The client connected successfully");
             });

            var client = new BlogService.BlogServiceClient(channel);
            //var client = new GreetingService.GreetingServiceClient(channel);
            //var client = new SqrtService.SqrtServiceClient(channel);

            var newBlog = CreateBlog(client);
            //DoSimpleGreet(client);
            //await DoManyGreetings(client);
            //await DoLongGreet(client);
            //await DoGreetEveryone(client);
            //GetNumberSquareRoot(client);
            //DoSimpleGreetWithDeadline(client);

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }

        public static void DoSimpleGreet(GreetingService.GreetingServiceClient client)
        {
            var greeting = new Greeting()
            {
                FirstName = "Milad",
                LastName = "Kardgar"
            };

            var request = new GreetingRequest() { Greeting = greeting };
            var response = client.Greet(request);
            Console.WriteLine(response.Result);
        }
        public static async Task DoManyGreetings(GreetingService.GreetingServiceClient client)
        {
            var greeting = new Greeting()
            {
                FirstName = "Milad",
                LastName = "Kardgar"
            };

            var request = new GreetManyTimesRequest() { Greeting = greeting };
            var response = client.GreetManyTimes(request);

            while (await response.ResponseStream.MoveNext())
            {
                Console.WriteLine(response.ResponseStream.Current.Result);
                await Task.Delay(200);
            }
        }
        public static async Task DoLongGreet(GreetingService.GreetingServiceClient client)
        {
            var greeting = new Greeting()
            {
                FirstName = "Milad",
                LastName = "Kardgar"
            };

            var request = new LongGreetRequest() { Greeting = greeting };
            var stream = client.LongGreet();

            foreach (var i in Enumerable.Range(1, 10))
            {
                await stream.RequestStream.WriteAsync(request);
            }

            await stream.RequestStream.CompleteAsync();
            var response = await stream.ResponseAsync;

            Console.WriteLine(response.Result);
        }
        public static async Task DoGreetEveryone(GreetingService.GreetingServiceClient client)
        {
            var stream = client.GreetEveryone();

            var responseReaderTask = Task.Run(async () =>
            {
                while (await stream.ResponseStream.MoveNext())
                {
                    Console.WriteLine("Received: " + stream.ResponseStream.Current.Result);
                }
            });

            var greetings = new Greeting[]
            {
                new Greeting(){FirstName = "Milad",LastName = "Kardgar"},
                new Greeting(){FirstName = "Lionel",LastName = "Messi"},
                new Greeting(){FirstName = "Cris",LastName = "Ronalso"},
            };

            foreach (var greeting in greetings)
            {
                Console.WriteLine("Sending: " + greeting.ToString());
                await stream.RequestStream.WriteAsync(new GreetEveryoneRequest()
                {
                    Greeting = greeting
                });
            }

            await stream.RequestStream.CompleteAsync();

            await responseReaderTask;
        }
        public static void GetNumberSquareRoot(SqrtService.SqrtServiceClient client)
        {
            var number = -1;

            try
            {
                var request = new SqrtRequest() { Number = number };
                var response = client.Sqrt(request);
                Console.WriteLine(response.SquareRoot);
            }
            catch (RpcException e)
            {
                Console.WriteLine("Error : " + e.Status.Detail);
            }
        }
        public static void DoSimpleGreetWithDeadline(GreetingService.GreetingServiceClient client)
        {
            var greeting = new Greeting()
            {
                FirstName = "Milad",
                LastName = "Kardgar"
            };

            try
            {
                var request = new GreetingRequest() { Greeting = greeting };
                var response = client.greet_with_deadline(request, deadline: DateTime.UtcNow.AddMilliseconds(200));
                Console.WriteLine(response.Result);
            }
            catch (RpcException e) when (e.StatusCode == StatusCode.DeadlineExceeded)
            {
                Console.WriteLine("Error : " + e.Status.Detail);
            }
        }
        private static Blog.Blog CreateBlog(BlogService.BlogServiceClient client)
        {
            var response = client.CreateBlog(new CreateBlogRequest()
            {
                Blog = new Blog.Blog()
                {
                    AuthorId = "Milad",
                    Title = "New blog!",
                    Content = "Hello world, this is a new blog"
                }
            });

            Console.WriteLine("The blog " + response.Blog.Id + " was created!");

            return response.Blog;
        }
    }
}
