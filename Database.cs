using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace VK_Unicorn
{
    class Database
    {
        public Database()
        {
            CreateDatabaseIfDontExists();
        }

        void CreateDatabaseIfDontExists()
        {
            if (!File.Exists(Constants.DATABASE_FILENAME))
            {
                SQLiteConnection.CreateFile(Constants.DATABASE_FILENAME);
            }
        }
    }
}
