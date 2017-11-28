using MobaServer.Dao.Model;
using System.Configuration;

namespace MobaServer
{
    public class ServerSettings
    {
        public static int maxClient = 9000;

        public static int gameServerPort = 6630;
        public static string gameServerIP = "127.0.0.1";
        public static string DBName = "moba";
        public static string DBUser = "root";
        public static string DBPwd = "cmasxiao12";
        /// <summary>
        /// 监听Socket挂起的最大长度
        /// </summary>
        public static int backlog = 10;

        public static void SetORM() {
            //IConfigurationSource source = ConfigurationManager.GetSection("activerecord") as IConfigurationSource;
            //ActiveRecordStarter.Initialize(source, typeof(ACCOUNT));
        }
    }
}
