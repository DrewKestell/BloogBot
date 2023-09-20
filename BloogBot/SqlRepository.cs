namespace BloogBot
{
    public abstract class SqlRepository
    {
        public abstract dynamic NewConnection();

        public abstract dynamic NewCommand(string sql, dynamic db);

        public abstract void Initialize(string connectionString);

        public void RunSqlQuery(string sql)
        {
            using (var db = NewConnection())
            {
                db.Open();

                var command = NewCommand(sql, db);
                command.Prepare();
                command.ExecuteNonQuery();

                db.Close();
            }
        }

        public bool RowExistsSql(string sql)
        {
            using (var db = this.NewConnection())
            {
                db.Open();

                var command = this.NewCommand(sql, db);
                var exists = command.ExecuteReader().HasRows;

                db.Close();

                return exists;
            }
        }
    }
}
