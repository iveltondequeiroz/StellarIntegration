using System;
using System.Globalization;
using System.Threading.Tasks;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.responses;
using stellar_dotnet_sdk.responses.operations;

namespace IntegrationConsole
{
    public class Integration
    {
        private string HorizonUrl { get; set; }
        private string BaseAccount { get; set; }
        private string BaseAccountSecret { get; set; }
        private string DestinationAccount { get; set; }
        public Server server;


        public Integration(string account, string accountSecret, string destination, string url)
        {
            BaseAccount = account;
            BaseAccountSecret = accountSecret;
            DestinationAccount = destination;
            HorizonUrl = url;
        }

       
        public async Task<bool> SubmitTransaction(string exchangeAccount, string destinationAddress, int amountLumens)
        {
            // Update transaction state to 'sending' so it won't be resubmitted in case of the failure.
            var customerId = DB.CustomerId(exchangeAccount);
            DB.UpdateTransactionState(customerId, destinationAddress, amountLumens, "sending");

            try
            {
                // Check if the destination address exists
                var destinationKeyPair = KeyPair.FromAccountId(destinationAddress);

                // If so, continue by submitting a transaction to the destination
                Asset asset = new AssetTypeNative();
                KeyPair sourceKeypair = KeyPair.FromSecretSeed(BaseAccountSecret);
                AccountResponse sourceAccountResponse = await server.Accounts.Account(sourceKeypair);
                Account sourceAccount = new Account(sourceAccountResponse.KeyPair, sourceAccountResponse.SequenceNumber);
                PaymentOperation operation = new PaymentOperation.Builder(destinationKeyPair, asset, amountLumens.ToString()).SetSourceAccount(sourceAccount.KeyPair).Build();
                Transaction transact = new Transaction.Builder(sourceAccount).AddOperation(operation).Build();

                // Sign the transaction
                transact.Sign(sourceKeypair);

                //Try to send the transaction
                try
                {
                    await server.SubmitTransaction(transact);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Send Transaction Failed:" + exception.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Account does not exist:"+e.Message);
            };

            try
            {
                // Submit the transaction created
                DB.UpdateTransactionState(customerId, destinationAddress, amountLumens, "done");
            }
            catch (Exception e)
            {
                // set transaction state to 'error'
                DB.UpdateTransactionState(customerId, destinationAddress, amountLumens, "error");
                Console.WriteLine("error:" + e.Message);              
            }

            return true;
        }

        public async Task<bool> SubmitPendingTransactions(string exchangeAccount)
        {
            Network.UseTestNetwork();
            server = new Server(HorizonUrl);
            var Pendings = DB.PendingTransactions(exchangeAccount);
            if (Pendings.Count > 0)
            {
                foreach (PendingTransaction txn in Pendings)
                {
                    await SubmitTransaction(exchangeAccount, txn.Destination, txn.Amount);
                }
            }
            else
            {
                Console.WriteLine("No pending transactions");
                return false;

            }
            return true;
        }

        public void HandleRequestWithdrawal(int userId, int amountLumens, string destination)
        {
            var userBalance = DB.UserBalance(userId);
            if (amountLumens <= userBalance)
            {
                // Debit the user's internal lumen balance by the amount of lumens they are withdrawing
                DB.UpdateBalance(userId, userBalance - amountLumens);
                // Save the transaction information in the StellarTransactions table
                DB.StoreTransaction(userId, destination, amountLumens, "pending");
            }
            else
            {
                Console.WriteLine("User DOES NOT hold enough XLM!!!");
            }
        }

        public async void ListenForDeposits()
        {
            // Horizon Server settings
            Network.UseTestNetwork();
            server = new Server(HorizonUrl);

            var lastToken = DB.GetLatestCursor();
            Console.WriteLine("latest cursor :" + lastToken);

            // Listen for payments from where you last stopped
            Console.WriteLine("Listen for payments...");
            KeyPair baseAccountKeys = KeyPair.FromSecretSeed(BaseAccountSecret);
            var callBuilder = server.Payments.ForAccount(baseAccountKeys);


            if (!String.IsNullOrEmpty(lastToken))
            {
                callBuilder.Cursor(lastToken);
            }

            var rescal = await callBuilder.Execute();
            //TODO callBuilder.Stream(HandlePaymentResponse);

        }

        public void HandlePaymentResponse(PaymentOperationResponse record)
        {
            var customer = record.To.Address;
            if (record.To.Address != BaseAccount)
            {
                Console.WriteLine("Requesting account not equal to BaseAccount");
                return;
            }

            if (record.AssetType == "native")
            {
                Console.WriteLine("assset type native");
                if (DB.CustomerExists(customer))
                {
                    int amount = (int)float.Parse(record.Amount, CultureInfo.InvariantCulture.NumberFormat);
                    int customerId = DB.CustomerId(customer);
                    DB.StoreDeposit(amount, customerId);
                    DB.StoreCursor(record.PagingToken); ;
                    HandleRequestWithdrawal(customerId, amount, DestinationAccount);
                }
                else
                {
                    Console.WriteLine("Customer Does Not Exist!!");
                }
            }
        }
    }
}
