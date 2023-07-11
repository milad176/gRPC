using Greet;
using Grpc.Core;
using Sqrt;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace client
{
    class Program
    {
        const string target = "127.0.0.1:50051";

        static async Task Main(string[] args)
        {
            Channel channel = new Channel(target, ChannelCredentials.Insecure);

            await channel.ConnectAsync().ContinueWith((task) =>
             {
                 if (task.Status == TaskStatus.RanToCompletion)
                     Console.WriteLine("The client connected successfully");
             });

            //var client = new GreetingService.GreetingServiceClient(channel);
            var client = new SqrtService.SqrtServiceClient(channel);


            //DoSimpleGreet(client);
            //await DoManyGreetings(client);
            //await DoLongGreet(client);
            //await DoGreetEveryone(client);

            GetNumberSquareRoot(client);

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
    }
}
