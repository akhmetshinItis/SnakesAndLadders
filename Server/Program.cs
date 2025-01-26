using System;

namespace TCPServer
{
    internal class Program
    {
        private static async Task Main()
        {
            Console.Title = "XServer";
            Console.ForegroundColor = ConsoleColor.White;

            var server = new Server();
            server.Start();
            Task.Run(server.CheckClients);
            server.AcceptClients();
        }
    }
}
