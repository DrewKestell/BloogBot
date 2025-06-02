// Configuration constants
public static class WowServerConfig
{
    public const string DbUser = "app";
    public const string DbPassword = "app";
    //public const string DbContainerImage = "ragedunicorn/mysql:1.1.0-stable";
    //public const string ServerContainerImage = "ragedunicorn/wow-vanilla:1.0.7-stable";
    public const string DbContainerImage = "ragedunicorn/mysql";
    public const string ServerContainerImage = "ragedunicorn/wow-vanilla";

    public static class Ports
    {
        public const int MySql = 3306;
        public const int MangosWorld = 8085;
        public const int MangosRealm = 3724;
    }

    public static class Volumes
    {
        public const string MySqlData = "wow_vanilla_mysql_data";
        public const string MySqlPath = "/var/lib/mysql";
        public const string LogData = "wow_vanilla_log_data";
        public const string LogPath = "/var/log/wow";
    }

    public static class Paths
    {
        public const string ConfigDir = "./config";
        public const string DataDir = "./data";
        public const string ServerConfigPath = "/opt/vanilla/etc";
        public const string ServerDataPath = "/opt/vanilla/data";
    }
}
