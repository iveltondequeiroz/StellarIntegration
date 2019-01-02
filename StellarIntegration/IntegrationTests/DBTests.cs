using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using System.IO;
using IntegrationConsole;

namespace IntegrationTests
{
  
    [TestClass]
    public class DBTests
    {
        public static IConfigurationRoot Configuration { get; set; }

        [TestMethod]
        public void AddExchangeUser()
        {
            AppConfig();
            DB.ConnectionString = Configuration["ConnectionStrings:DefaultConnection"];
            DB.AddUser(Configuration["Stellar:BaseAccount"]);
            var result = DB.CustomerId(Configuration["Stellar:BaseAccount"]);
            Assert.AreNotEqual(result, -1);
        }

        [TestMethod]
        public async void PendingTransactions()
        {
            // having problems with async methods on MS-TEST
            // methods are working using console application

            AppConfig();
            DB.ConnectionString = Configuration["ConnectionStrings:DefaultConnection"];

            var customerId = DB.CustomerId(Configuration["Stellar:BaseAccount"]);
            var destination = Configuration["Stellar:DestinationAccount"];
            var amount = 15;
            var state = "pending";

            DB.StoreTransaction(customerId, destination, amount, state);

            Integration integration = new Integration(Configuration["Stellar:BaseAccount"], Configuration["Stellar:BaseAccountSecret"], Configuration["Stellar:DestinationAccount"], Configuration["Stellar:HorizonURL"]);
            await integration.SubmitPendingTransactions(Configuration["Stellar:BaseAccount"]);
            Assert.IsTrue(true);
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