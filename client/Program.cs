using Greet;
using Grpc.Core;
using System;
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

            var client = new GreetingService.GreetingServiceClient(channel);

            var greeting = new Greeting()
            {
                FirstName = "Milad",
                LastName = "Kardgar"
            };

            //var request = new GreetingRequest() { Greeting = greeting };
            //var response = client.Greet(request);
            // Console.WriteLine(response.Result);


            var request = new GreetManyTimesRequest() { Greeting = greeting };
            var response = client.GreetManyTimes(request);

            while (await response.ResponseStream.MoveNext())
            {
                Console.WriteLine(response.ResponseStream.Current.Result);
                await Task.Delay(200);
            }


            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }
    }
}
