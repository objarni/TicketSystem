using System;
using System.ComponentModel.Design.Serialization;
using System.Data.SqlClient;

namespace TicketManagementSystem
{
    public class UserRepository : IDisposable
    {
        private SqlConnection connection;

        public static bool TestMode = false;
        
        public UserRepository()
        {
            if (TestMode)
                return;
            var connectionString = "a fake db conn string"; 
            connection = new SqlConnection(connectionString);
        }
        
        public User GetUser(string username)
        {
            if (TestMode)
            {
                return new User()
                {
                    FirstName = username[0].ToString(), 
                    LastName = username.Substring(1), 
                    Username = username
                };
            }
            // Assume this method does not need to change and is connected to a database with users populated.
            try
            {
                string sql = "SELECT TOP 1 FROM Users u WHERE u.Username == @p1";
                connection.Open();

                var command = new SqlCommand(sql, connection)
                {
                    CommandType = System.Data.CommandType.Text,
                };

                command.Parameters.Add("@p1", System.Data.SqlDbType.NVarChar).Value = username;

                return (User)command.ExecuteScalar();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        public User GetAccountManager()
        {
            // Assume this method does not need to change.
            return GetUser("Sarah");
        }

        public void Dispose()
        {
            if (TestMode)
                return;
            // Assume this method does not need to change.
            connection.Dispose();
        }
    }
}
