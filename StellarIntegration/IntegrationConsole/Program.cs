using System;
using System.IO;
using Microsoft.Extensions.Configuration;

using System.Threading.Tasks;

namespace IntegrationConsole
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static async Task Main(string[] args)
        {
            AppConfig();
            DB.ConnectionString = Configuration["ConnectionStrings:DefaultConnection"];
            Integration integration = new Integration(Configuration["Stellar:BaseAccount"], Configuration["Stellar:BaseAccountSecret"], Configuration["Stellar:DestinationAccount"], Configuration["Stellar:HorizonURL"]);
          
            Console.WriteLine("Handling pending transactions...");

            // add base user to db
            DB.AddUser(Configuration["Stellar:BaseAccount"]);

            // add pending transaction
            var customerId = DB.CustomerId(Configuration["Stellar:BaseAccount"]);
            var destination = Configuration["Stellar:DestinationAccount"];
            var amount = 15;
            var state = "pending";
            DB.StoreTransaction(customerId, destination, amount, state);

            // handle pending transactions
            await integration.SubmitPendingTransactions(Configuration["Stellar:BaseAccount"]);
            
            Console.WriteLine("stellar end");
            Console.ReadLine();
           
        }

        static void AppConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
        }

    }
}
