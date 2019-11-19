using Quaver.Shared.Config;
using SQLite;

namespace Quaver.Shared.Database
{
    public class DatabaseManager
    {
        /// <summary>
        ///     The path of the local database
        /// </summary>
        public static readonly string DatabasePath = ConfigManager.GameDirectory + "/quaver.db";

        /// <summary>
        /// </summary>
        public static SQLiteConnection Connection { get; private set; }

        /// <summary>
        /// </summary>
        public static void Initialize() => Connection = new SQLiteConnection(DatabasePath);
    }
}