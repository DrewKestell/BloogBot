using MySql.Data.MySqlClient;

namespace StateManager.Repository
{
    public class ReamldRepository
    {
        private static readonly string ConnectionString = "server=localhost;user=app;database=realmd;port=3306;password=app";

        public static bool CheckIfAccountExists(string accountName)
        {
            using MySqlConnection connection = new(ConnectionString);
            try
            {
                connection.Open();

                MySqlCommand command = connection.CreateCommand();
                command.CommandText = @$"SELECT * FROM account where username = '{accountName}'";

                using MySqlDataReader reader = command.ExecuteReader();
                return reader.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
            }

            return false;
        }
    }
}
