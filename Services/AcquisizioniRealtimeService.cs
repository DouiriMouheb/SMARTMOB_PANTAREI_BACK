using SMARTMOB_PANTAREI_BACK.Hubs;
using SMARTMOB_PANTAREI_BACK.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SMARTMOB_PANTAREI_BACK.Data;
using Microsoft.EntityFrameworkCore;

namespace SMARTMOB_PANTAREI_BACK.Services
{
    public interface IAcquisizioniRealtimeService
    {
        Task StartMonitoringAsync();
        Task StopMonitoringAsync();
        Task<IEnumerable<Acquisizioni>> GetLatestAcquisizioniAsync();
        Task<Acquisizioni?> GetLatestSingleRecordAsync();
        Task<IEnumerable<Acquisizioni>> GetLatestByLineAsync(string codLinea);
        Task<IEnumerable<Acquisizioni>> GetLatestByStationAsync(string codPostazione);
        Task<IEnumerable<Acquisizioni>> GetLatestByLineAndStationAsync(string codLinea, string codPostazione);
        Task<Acquisizioni?> GetLatestSingleByLineAsync(string codLinea);
        Task<Acquisizioni?> GetLatestSingleByStationAsync(string codPostazione);
        Task<Acquisizioni?> GetLatestSingleByLineAndStationAsync(string codLinea, string codPostazione);
    }

    public class AcquisizioniRealtimeService : BackgroundService, IAcquisizioniRealtimeService
    {
        private readonly IHubContext<AcquisizioniHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AcquisizioniRealtimeService> _logger;
        private readonly string _connectionString;
        private bool _isMonitoring = false;
        private int _lastKnownId = 0;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(3);

        public AcquisizioniRealtimeService(
            IHubContext<AcquisizioniHub> hubContext,
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<AcquisizioniRealtimeService> logger)
        {
            _hubContext = hubContext;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await StartMonitoringAsync();
            
            // Initialize the last known ID
            await InitializeLastKnownIdAsync();
            
            _logger.LogInformation("🚀 Starting polling-based ACQUISIZIONI monitoring every 3 seconds...");
            
            while (!stoppingToken.IsCancellationRequested && _isMonitoring)
            {
                try
                {
                    await CheckForNewRecordsAsync();
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during polling check");
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
            }
            
            await StopMonitoringAsync();
        }

        public Task StartMonitoringAsync()
        {
            _isMonitoring = true;
            _logger.LogInformation("✅ Started monitoring ACQUISIZIONI table with 3-second polling.");
            return Task.CompletedTask;
        }

        public Task StopMonitoringAsync()
        {
            _isMonitoring = false;
            _logger.LogInformation("⏹️ Stopped monitoring ACQUISIZIONI table.");
            return Task.CompletedTask;
        }

        private async Task InitializeLastKnownIdAsync()
        {
            try
            {
                var latestRecord = await GetLatestSingleRecordAsync();
                if (latestRecord != null)
                {
                    _lastKnownId = latestRecord.Id;
                    _logger.LogInformation($"📊 Initialized last known ID: {_lastKnownId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing last known ID");
            }
        }

        private async Task CheckForNewRecordsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Get records with ID greater than last known ID
                var newRecords = await context.Acquisizioni
                    .Where(a => a.Id > _lastKnownId)
                    .OrderBy(a => a.Id)
                    .ToListAsync();

                if (newRecords.Any())
                {
                    foreach (var newRecord in newRecords)
                    {
                        _logger.LogInformation($"🆕 New record detected: ID {newRecord.Id}, Line: {newRecord.CodLinea}, Station: {newRecord.CodPostazione}");
                        
                        // Send to ALL connected clients (original functionality)
                        await _hubContext.Clients.All.SendAsync("NewAcquisizioniAdded", newRecord);
                        
                        // Send to specific groups based on filters
                        await BroadcastToFilteredGroups(newRecord);
                        
                        // Update last known ID
                        _lastKnownId = newRecord.Id;
                    }
                    
                    // Also send the latest record in the old format for backward compatibility
                    var latestRecord = newRecords.Last();
                    await _hubContext.Clients.All.SendAsync("AcquisizioniUpdated", new[] { latestRecord });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for new records");
            }
        }

        /// <summary>
        /// Broadcast new record to filtered groups based on COD_LINEA and COD_POSTAZIONE
        /// </summary>
        /// <param name="newRecord">The new acquisition record</param>
        private async Task BroadcastToFilteredGroups(Acquisizioni newRecord)
        {
            try
            {
                // Send to line-specific group
                if (!string.IsNullOrEmpty(newRecord.CodLinea))
                {
                    string lineGroup = $"line_{newRecord.CodLinea}";
                    await _hubContext.Clients.Group(lineGroup).SendAsync("NewAcquisizioniForLine", newRecord);
                    _logger.LogInformation($"📡 Sent to line group: {lineGroup}");
                }

                // Send to station-specific group
                if (!string.IsNullOrEmpty(newRecord.CodPostazione))
                {
                    string stationGroup = $"station_{newRecord.CodPostazione}";
                    await _hubContext.Clients.Group(stationGroup).SendAsync("NewAcquisizioniForStation", newRecord);
                    _logger.LogInformation($"📡 Sent to station group: {stationGroup}");
                }

                // Send to line+station combination group
                if (!string.IsNullOrEmpty(newRecord.CodLinea) && !string.IsNullOrEmpty(newRecord.CodPostazione))
                {
                    string combinedGroup = $"line_{newRecord.CodLinea}_station_{newRecord.CodPostazione}";
                    await _hubContext.Clients.Group(combinedGroup).SendAsync("NewAcquisizioniForLineStation", newRecord);
                    _logger.LogInformation($"📡 Sent to combined group: {combinedGroup}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting to filtered groups");
            }
        }

        public async Task<IEnumerable<Acquisizioni>> GetLatestAcquisizioniAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Get the latest single record ordered by ID descending (most recent)
                var latestRecord = await context.Acquisizioni
                    .OrderByDescending(a => a.Id)
                    .Take(1)
                    .ToListAsync();

                return latestRecord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest acquisizioni");
                return Enumerable.Empty<Acquisizioni>();
            }
        }

        public async Task<Acquisizioni?> GetLatestSingleRecordAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Get the single latest record by ID
                var latestRecord = await context.Acquisizioni
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefaultAsync();

                return latestRecord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest single Acquisizioni record");
                return null;
            }
        }

        // Filtered methods implementation
        public async Task<IEnumerable<Acquisizioni>> GetLatestByLineAsync(string codLinea)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var records = await context.Acquisizioni
                    .Where(a => a.CodLinea == codLinea)
                    .OrderByDescending(a => a.Id)
                    .Take(10) // Get latest 10 records for this line
                    .ToListAsync();

                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting latest acquisizioni for line {codLinea}");
                return Enumerable.Empty<Acquisizioni>();
            }
        }

        public async Task<IEnumerable<Acquisizioni>> GetLatestByStationAsync(string codPostazione)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var records = await context.Acquisizioni
                    .Where(a => a.CodPostazione == codPostazione)
                    .OrderByDescending(a => a.Id)
                    .Take(10) // Get latest 10 records for this station
                    .ToListAsync();

                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting latest acquisizioni for station {codPostazione}");
                return Enumerable.Empty<Acquisizioni>();
            }
        }

        public async Task<IEnumerable<Acquisizioni>> GetLatestByLineAndStationAsync(string codLinea, string codPostazione)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var records = await context.Acquisizioni
                    .Where(a => a.CodLinea == codLinea && a.CodPostazione == codPostazione)
                    .OrderByDescending(a => a.Id)
                    .Take(10) // Get latest 10 records for this line+station combination
                    .ToListAsync();

                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting latest acquisizioni for line {codLinea} and station {codPostazione}");
                return Enumerable.Empty<Acquisizioni>();
            }
        }

        public async Task<Acquisizioni?> GetLatestSingleByLineAsync(string codLinea)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var record = await context.Acquisizioni
                    .Where(a => a.CodLinea == codLinea)
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefaultAsync();

                return record;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting latest single Acquisizioni for line {codLinea}");
                return null;
            }
        }

        public async Task<Acquisizioni?> GetLatestSingleByStationAsync(string codPostazione)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var record = await context.Acquisizioni
                    .Where(a => a.CodPostazione == codPostazione)
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefaultAsync();

                return record;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting latest single Acquisizioni for station {codPostazione}");
                return null;
            }
        }

        public async Task<Acquisizioni?> GetLatestSingleByLineAndStationAsync(string codLinea, string codPostazione)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var record = await context.Acquisizioni
                    .Where(a => a.CodLinea == codLinea && a.CodPostazione == codPostazione)
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefaultAsync();

                return record;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting latest single Acquisizioni for line {codLinea} and station {codPostazione}");
                return null;
            }
        }
    }
}
