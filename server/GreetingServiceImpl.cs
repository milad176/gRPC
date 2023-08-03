using Greet;
using Grpc.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Greet.GreetingService;

namespace server
{
    public class GreetingServiceImpl : GreetingServiceBase
    {
        public override Task<GreetingResponse> Greet(GreetingRequest request, ServerCallContext context)
        {
            string result = string.Format("hello {0} {1}", request.Greeting.FirstName, request.Greeting.LastName);

            return Task.FromResult(new GreetingResponse() { Result = result });
        }

        public override async Task GreetManyTimes(GreetManyTimesRequest request, IServerStreamWriter<GreetManyTimesResponse> responseStream, ServerCallContext context)
        {
            Console.WriteLine("The server recived the request : ");
            Console.WriteLine(request.ToString());

            string result = string.Format("hello {0} {1}", request.Greeting.FirstName, request.Greeting.LastName);

            foreach (var i in Enumerable.Range(1, 10))
            {
                await responseStream.WriteAsync(new GreetManyTimesResponse() { Result = result });
            }
        }

        public override async Task<LongGreetResponse> LongGreet(IAsyncStreamReader<LongGreetRequest> requestStream, ServerCallContext context)
        {
            var result = "";

            while (await requestStream.MoveNext())
            {
                result += string.Format("hello {0} {1} {2}",
                    requestStream.Current.Greeting.FirstName,
                    requestStream.Current.Greeting.LastName,
                    Environment.NewLine);
            }

            return new LongGreetResponse() { Result = result };
        }

        public override async Task GreetEveryone(IAsyncStreamReader<GreetEveryoneRequest> requestStream, IServerStreamWriter<GreetEveryoneResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var result = string.Format("hello {0} {1}",
                      requestStream.Current.Greeting.FirstName,
                      requestStream.Current.Greeting.LastName);

                Console.WriteLine("Received: " + result);

                await responseStream.WriteAsync(new GreetEveryoneResponse() { Result = result });
            }
        }

        public override async Task<GreetingResponse> greet_with_deadline(GreetingRequest request, ServerCallContext context)
        {
            await Task.Delay(300);

            string result = string.Format("hello {0} {1}", request.Greeting.FirstName, request.Greeting.LastName);
            return new GreetingResponse() { Result = result };
        }
    }
}
