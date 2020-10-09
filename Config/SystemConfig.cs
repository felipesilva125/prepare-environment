using System;
using System.IO;
using System.Xml;

namespace prepare_environment
{
    internal static class SystemConfig
    {
        private static string CONNECTIONSTRING_NAME = "ConnectionStrings.config";
        private static char[] CONECTIONSTRING_SEPARATOR = { '"' };
        private static string CONECTIONSTRING_NAVIGATOR = "configuration/connectionStrings/add";

        private static string sqlConnectionString;
        public static string SqlConnectionString { get { loadConnectionString(); return sqlConnectionString; } }

        private static void loadConnectionString()
        {
            if (string.IsNullOrEmpty(sqlConnectionString))
            {
                var sqlConnectionStringWithMetadata = getFirstConnctionStringOfConfigFile(Path.Combine(AppDataPath.GetRootPath(), CONNECTIONSTRING_NAME));
                var sqlConnectionStringSplited = sqlConnectionStringWithMetadata.Split(CONECTIONSTRING_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

                sqlConnectionString = sqlConnectionStringSplited[sqlConnectionStringSplited.Length - 1];
            }
        }

        private static string getFirstConnctionStringOfConfigFile(string configFilePath)
        {
            var stream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(stream);
            var xmlNavigator = xmlDocument.CreateNavigator();
            var selectExpression = xmlNavigator.Compile(CONECTIONSTRING_NAVIGATOR);
            var selectedNodes = xmlNavigator.Select(selectExpression);

            if (selectedNodes.MoveNext())
                return selectedNodes.Current.GetAttribute("connectionString", string.Empty);

            return null;
        }
    }
}
