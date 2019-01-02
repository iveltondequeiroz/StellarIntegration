using System;
using System.Collections.Generic;
using System.Data.SqlClient;


namespace IntegrationConsole
{
    public class DB
    {
        public static string ConnectionString { get; set; }

       
        public static string GetLatestCursor()
        {
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT TOP 1 CursorNo FROM dbo.StellarCursor ORDER BY Id DESC", conn);
                var result = (string) cmd.ExecuteScalar();
                conn.Close();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("error:" + e);
                return "error";
            }
        }

        public static bool CustomerExists(string account)
        {
            Console.WriteLine("CustomerExists:" + account);

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.ExchangeUsers WHERE Account=@Account", conn);
                cmd.Parameters.AddWithValue("@Account", account);
                conn.Open();
                var result = cmd.ExecuteScalar();
                Console.WriteLine("SELECT result:" + result);
                conn.Close();
                return result.Equals(1);
            }
            catch (Exception e)
            {
                Console.WriteLine("error:" + e);
                return false;
            }
        }

        public static bool StoreDeposit(int amount, int customerId)
        {
            Console.WriteLine("StoreDeposit:" + amount + " -> " + customerId);
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                string strcmd = "INSERT INTO dbo.StellarDeposits(CustomerId, Amount, Date) VALUES(@CustomerId, @Amount, @Date)";
                SqlCommand cmd = new SqlCommand(strcmd, conn);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                conn.Open();
                var result = cmd.ExecuteNonQuery();
                // get current balance
                cmd.CommandText = "SELECT Balance from dbo.ExchangeUsers WHERE CustomerId=@CustomerId";
                int balance = (int)cmd.ExecuteScalar();

                // update balance
                cmd.CommandText = "UPDATE dbo.ExchangeUsers SET Balance=@Balance WHERE CustomerId=@CustomerId";
                cmd.Parameters.AddWithValue("@Balance", balance + amount);
                cmd.ExecuteNonQuery(); 
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL error:" + e);
                return false;
            }
        }

        public static bool StoreCursor(string cursor)
        {
            Console.WriteLine("StoreCursor:" + cursor);
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                string strcmd = "INSERT INTO dbo.StellarCursor(CursorNo) VALUES(@Cursor)";
                SqlCommand cmd = new SqlCommand(strcmd, conn);
                cmd.Parameters.AddWithValue("@Cursor", cursor);
                conn.Open();
                cmd.ExecuteNonQuery(); 
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL error:" + e);
                return false;
            }
        }

        public static int UserBalance(int customerId)
        {
            Console.WriteLine("UserBalance>customerId :" + customerId);
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                string strcmd = "SELECT Balance FROM dbo.ExchangeUsers WHERE CustomerId=@CustomerId";
                SqlCommand cmd = new SqlCommand(strcmd, conn);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                conn.Open();
                var result = (int)cmd.ExecuteScalar();
                conn.Close();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL error:" + e);
                return 0;
            }
        }

        public static bool AddUser(string account)
        {
            Console.WriteLine("Add user:" + account);
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                string strcmd = "SELECT COUNT(*) FROM dbo.ExchangeUsers WHERE Account=@Account";
                SqlCommand cmd = new SqlCommand(strcmd, conn);
                cmd.Parameters.AddWithValue("@Account", account);
                conn.Open();

                var result = (int) cmd.ExecuteScalar();
                if(result==0)
                {
                    strcmd = "INSERT INTO dbo.ExchangeUsers(Account, Balance) VALUES(@Account, @Balance)";
                    cmd = new SqlCommand(strcmd, conn);
                    cmd.Parameters.AddWithValue("@Account", account);
                    cmd.Parameters.AddWithValue("@Balance", 0);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL error:" + e);
                return false;
            }
        }

        public static int CustomerId(string account)
        {
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                string strcmd = "SELECT CustomerId FROM dbo.ExchangeUsers WHERE Account=@Account";
                SqlCommand cmd = new SqlCommand(strcmd, conn);
                cmd.Parameters.AddWithValue("@Account", account);
                conn.Open();
                var result = (int)cmd.ExecuteScalar();
                conn.Close();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL error:" + e);
                return -1;
            }
        }

        public static bool UpdateBalance(int customerId, int newBalance)
        {
            Console.WriteLine("Update Balance:" + customerId + ">" + newBalance);
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                string strcmd = "UPDATE dbo.ExchangeUsers SET Balance=@Balance WHERE CustomerId=@CustomerId";
                SqlCommand cmd = new SqlCommand(strcmd, conn);
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                cmd.Parameters.AddWithValue("@Balance", newBalance);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL error:" + e);
                return false;
            }
        }

        public static bool StoreTransaction(int customerId, string destination, int amount, string state)
        {
            Console.WriteLine("Add Transaction: customerId:" + customerId + "> destination:" + destination + "> amount:" + amount + "> state:" + state);
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                string strcmd = "INSERT INTO dbo.StellarTransactions(UserId, Destination, XLMAmount, State) VALUES(@CustomerId, @Destination, @Amount, @State)";
                SqlCommand cmd = new SqlCommand(strcmd, conn);
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                cmd.Parameters.AddWithValue("@Destination", destination);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@State", state);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL error:" + e);
                return false;
            }
        }

        public static bool UpdateTransactionState(int customerId, string destination, int amount, string state)
        {
            Console.WriteLine("Update Transaction State: Id:" + customerId + "> destination:" + destination + "> state:"+state);
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                string strcmd = "UPDATE dbo.StellarTransactions SET State=@State WHERE UserId=@UserId AND Destination=@Destination AND XLMAmount=@Amount";
                SqlCommand cmd = new SqlCommand(strcmd, conn);
                cmd.Parameters.AddWithValue("@UserID", customerId);
                cmd.Parameters.AddWithValue("@Destination", destination);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@State", state);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL error:" + e);
                return false;
            }
        }

        public static List<PendingTransaction> PendingTransactions(string exchangeAccount)
        {
            Console.WriteLine("Pending Transactions for: " + exchangeAccount);
            try
            {
                List<PendingTransaction> Pendings = new List<PendingTransaction>();

                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                string strcmd = "SELECT * FROM StellarTransactions WHERE state ='pending'";
                SqlCommand cmd = new SqlCommand(strcmd, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var pending = new PendingTransaction
                    {
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Destination = reader["Destination"].ToString(),
                        Amount = Convert.ToInt32(reader["XLMAmount"]),
                        State = reader["State"].ToString()
                    };

                    Pendings.Add(pending);
                }

                conn.Close();
                return Pendings;
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL error:" + e);
                return new List<PendingTransaction>();
            }
        }

        public static void Connect()
        {
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.ExchangeUsers", conn);

                var result = cmd.ExecuteScalar();
                Console.WriteLine("result:" + result);
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("error:" + e);
            }
            finally
            {
                Console.ReadLine();
            }

        }
    }

    public class PendingTransaction
    {
        public int UserId { get; set; }
        public string Destination { get; set; }
        public int Amount { get; set; }
        public string State { get; set; }
    }
}
