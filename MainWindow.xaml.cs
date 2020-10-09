using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace prepare_environment
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker worker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            initializeWorker();
        }

        private void initializeWorker()
        {
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += worker_Completed;
            worker.ProgressChanged += worker_ProgressChanged;
        }

        private void worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show($"Erro: {e.Error.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                MessageBox.Show("Operação cancelada!", "Cancelado", MessageBoxButton.OK, MessageBoxImage.Warning);
                BackupProgressBar.Value = 0;
            }
            else
            {
                MessageBox.Show("Backup efetuado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackupProgressBar.Value = e.ProgressPercentage;
        }

        private void ExecuteBackup(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!worker.IsBusy)
                    worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
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
            source.PercentComplete += Source_PercentComplete;
            source.SqlBackupAsync(server);

            while(source.AsyncStatus.ExecutionStatus == ExecutionStatus.InProgress)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    source.Abort();
                    return;
                }
            }

            con.Disconnect();
        }

        private void Source_PercentComplete(object sender, PercentCompleteEventArgs e)
        {
            worker.ReportProgress(e.Percent);
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

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            var button = (Button)sender;
            button.Background = Brushes.Aqua;
            button.Foreground = Brushes.Black;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            var button = (Button)sender;

            var background = (Color)ColorConverter.ConvertFromString("#135E8E");
            button.Background = new SolidColorBrush(background);
            button.Foreground = Brushes.White;
        }

        private void CancelBackup(object sender, RoutedEventArgs e)
        {
            if (worker.WorkerSupportsCancellation)
                worker.CancelAsync();
        }
    }
}

