using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BloogBot
{
    public abstract class SqlRepository
    {

        string connectionString;

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

    }
}
