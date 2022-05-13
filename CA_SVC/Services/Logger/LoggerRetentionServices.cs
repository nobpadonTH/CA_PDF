using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CA_SVC.Services.Logger
{
    public class LoggerRetentionServices : IHostedService, IDisposable
    {
        private string _connectionString;
        private int _retentionTime;

        private Timer _timer;

        public LoggerRetentionServices(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _retentionTime = configuration.GetValue<int>("Serilog:WriteTo:2:Args:sinkOptionsSection:retainedPeriod", 90);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Set timer and start services
            _timer = new Timer(Services, null, TimeSpan.Zero, TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        public async void Services(object state)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var retentionDatetime = DateTime.Now.AddDays(-1 * _retentionTime);
                    var sqlCommand = new SqlCommand($"DELETE FROM [EventLogging].[Logs] WHERE [TimeStamp] < @RetentionTime;", connection);
                    sqlCommand.Parameters.Add("@RetentionTime", SqlDbType.DateTime);
                    sqlCommand.Parameters["@RetentionTime"].Value = retentionDatetime;

                    var returnRows = await sqlCommand.ExecuteNonQueryAsync();

                    Log.Verbose($"RetentionTime : {retentionDatetime}");
                    Log.Information("{Services} Delete log out of retention completed [From={RetentionTime},Deleted={NumberOfRows}]", nameof(LoggerRetentionServices), retentionDatetime, returnRows);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "{Services} Delete log out of retention failed.", nameof(LoggerRetentionServices));
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}