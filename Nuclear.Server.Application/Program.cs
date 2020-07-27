using System;
namespace Nuclear.Server.Application
{
    class Program
    {
        static void Main() //handle config files here.
        {
			Console.WriteLine("//Nuclear Realtime Server\n//Developed by Akash Darshan\n\n\n");
            new Application(27000, 15);
            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}
