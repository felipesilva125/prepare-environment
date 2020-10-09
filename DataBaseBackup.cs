using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace prepare_environment
{
    public class DataBaseBackup
    {
        /// <summary>
        /// Backup a whole database to the specified file.
        /// </summary>
        /// <remarks>
        /// The database must not be in use when backing up
        /// The folder holding the file must have appropriate permissions given
        /// </remarks>
        /// <param name="backUpFile">Full path to file to hold the backup</param>
        public static void BackupDatabase(PercentCompleteEventHandler eventHandler)
        {            
            var sqlConn = new SqlConnection(SystemConfig.SqlConnectionString);            
            var con = new ServerConnection(sqlConn);            
            var server = new Server(con);
            var source = new Backup();
            source.Action = BackupActionType.Database;
            source.Database = sqlConn.Database;
            var fullPath = buildPath(source.Database);
            var destination = new BackupDeviceItem(fullPath, DeviceType.File);
            source.Devices.Add(destination);
            source.PercentCompleteNotification = 1;
            source.PercentComplete += eventHandler;
            source.SqlBackupAsync(server);            

            con.Disconnect();
        }

        private static string buildPath(string database)
        {
            var fileName = new StringBuilder().Append(database)
                                              .Append("_")
                                              .Append(DateTime.Now.ToString("yyyyMMddHHmmss"))
                                              .ToString();

            var path = Path.Combine(@"C:\Temp", fileName);
            return Path.ChangeExtension(path, "bak");            
        }

        /// <summary>
        /// Restore a whole database from a backup file.
        /// </summary>
        /// <remarks>
        /// The database must not be in use when backing up
        /// The folder holding the file must have appropriate permissions given
        /// </remarks>
        /// <param name="backUpFile">Full path to file to holding the backup</param>
        public static void RestoreDatabase(string backUpFile)
        {
            var sqlConn = new SqlConnection(SystemConfig.SqlConnectionString);
            var con = new ServerConnection(sqlConn);
            Server server = new Server(con);
            Restore destination = new Restore();
            destination.Action = RestoreActionType.Database;
            destination.Database = sqlConn.Database;
            BackupDeviceItem source = new BackupDeviceItem(backUpFile, DeviceType.File);
            destination.Devices.Add(source);
            destination.ReplaceDatabase = true;
            destination.SqlRestore(server);
        }
    }
}
