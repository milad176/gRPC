using Greet;
using Grpc.Core;
using Sqrt;
using System;
using System.IO;

namespace server
{
    class Program
    {
        const int port = 50051;
        static void Main(string[] args)
        {
            Server server = null;

            try
            {
                server = new Server()
                {
                    //Services = { SqrtService.BindService(new SqrtServiceImpl()) },

                    Services = { GreetingService.BindService(new GreetingServiceImpl()) },
                    Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
                };

                server.Start();
                Console.WriteLine("The server is listening to the port :" + port);
                Console.ReadKey();
            }
            catch (IOException e)
            {
                Console.WriteLine("The server failed to start :" + e.Message);
                throw;
            }
            finally
            {
                if (server != null)
                    server.ShutdownAsync().Wait();
            }
        }
    }
}
