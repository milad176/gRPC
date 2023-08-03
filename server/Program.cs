using Greet;
using Grpc.Core;
using System;
using System.Collections.Generic;
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
                var serverCert = File.ReadAllText("ssl/server.crt");
                var serverKey = File.ReadAllText("ssl/server.key");
                var keypair = new KeyCertificatePair(serverCert, serverKey);
                var cacert = File.ReadAllText("ssl/ca.crt");

                var Credentials = new SslServerCredentials(new List<KeyCertificatePair>() { keypair }, cacert, true);

                server = new Server()
                {
                    //Services = { SqrtService.BindService(new SqrtServiceImpl()) },

                    Services = { GreetingService.BindService(new GreetingServiceImpl()) },
                    Ports = { new ServerPort("localhost", port, Credentials) }
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
