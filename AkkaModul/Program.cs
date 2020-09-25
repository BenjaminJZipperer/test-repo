using System;
/**
Since actors can be running locally or on a remote server, a common message exchange format 
is needed. Akka.Net messages are immutable. 
They can be instances of a string, an integer, or even a custom class.
**/
using Akka.Actor;
using Akka.Configuration;
using System.Threading.Tasks;
namespace AkkaModule
{
    class Program
    {
		static String pseudoName;
        static void Main(string[] args)
        {
            Console.WriteLine("Starte AKKA Modul um Uhrzeit []");
			var actorSystem = ActorSystem.Create("test-actor");
			
			var config = ConfigurationFactory.ParseString(@"
			akka {
				actor {
				akka.suppress-json-serializer-warning = on
				}
				}");
				 var actor = actorSystem.ActorOf<FreeBusyActor>();
			//TlogSystem = ActorSystem.Create("TlogSystem",config);
			 Task.Run(async () =>
            {

                for (var i = 0; i < 2300; i++)
                {
                    actor.Tell("get busy");
                    await Task.Delay(400);
                 }
			
			// alow some time for .NETF to flush
			    await Task.Delay(500);
            }).Wait();
            Console.ReadLine();
            actorSystem.Terminate();
        }
    }
	//public abstract class StateLoggerAgent
	//extends akka.actor.UntypedActor
	class FreeBusyActor : ReceiveActor
    { 
        static int counter = 1;
        public FreeBusyActor()
        {
            // the actor starts as "Free"
            Free();
        }

        void Free()
        {
            // when free, only "get busy" commands are handled
            Receive<string>(s => {

                if (s == "get busy")
                {
                    Console.WriteLine("Getting busy..."+counter);

                    // the actor becomes busy, so the next messages are handled differently
                    Become(Busy);
                    counter++;
                    // the actor starts some work in the background
                    // when it's done tell itself it's free
                    Task.Delay(80).ContinueWith(_ => "you're free").PipeTo(Self, Self);
                }
            });
        }

        void Busy()
        {
            Receive<string>(s =>
            {
                // when busy, only accept messages from itself to get free
                if (s == "you're free" && Sender.Equals(Self))
                {
                    Console.WriteLine("Getting free...");
                    Become(Free);
                }
                // otherwise we won't do anything
                else
                {
                    Console.WriteLine("Not doing anything, I'm busy...");
                }
            });
        }
    }
}
