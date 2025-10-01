using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMARTMOB_PANTAREI_BACK.Data;
using SMARTMOB_PANTAREI_BACK.Hubs;
using SMARTMOB_PANTAREI_BACK.Models;

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
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AcquisizioniRealtimeService> _logger;
        private bool _isMonitoring = false;
        private int _lastKnownId = 0;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(3);

        public AcquisizioniRealtimeService(
            IHubContext<AcquisizioniHub> hubContext,
            IServiceProvider serviceProvider,
            ILogger<AcquisizioniRealtimeService> logger)
        {
            _hubContext = hubContext;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await StartMonitoringAsync();
            await InitializeLastKnownIdAsync();

            while (!stoppingToken.IsCancellationRequested && _isMonitoring)
            {
                try
                {
                    await CheckForNewRecordsAsync();
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during polling loop");
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
            }
        }

        public Task StartMonitoringAsync()
        {
            _isMonitoring = true;
            _logger.LogInformation("Acquisizioni monitoring started");
            return Task.CompletedTask;
        }

        public Task StopMonitoringAsync()
        {
            _isMonitoring = false;
            _logger.LogInformation("Acquisizioni monitoring stopped");
            return Task.CompletedTask;
        }

        private async Task InitializeLastKnownIdAsync()
        {
            try
            {
                var latest = await GetLatestSingleRecordAsync();
                if (latest != null)
                {
                    _lastKnownId = latest.Id;
                    _logger.LogInformation($"Initialized lastKnownId={_lastKnownId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing last known id");
            }
        }

        private async Task CheckForNewRecordsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var newRecords = await context.Acquisizioni
                    .Where(a => a.Id > _lastKnownId)
                    .OrderBy(a => a.Id)
                    .ToListAsync();

                if (!newRecords.Any())
                    return;

                foreach (var rec in newRecords)
                {
                    _logger.LogInformation($"New record detected Id={rec.Id}");
                    await _hubContext.Clients.All.SendAsync("NewAcquisizioniAdded", rec);
                    _lastKnownId = rec.Id;
                }

                var latest = newRecords.Last();
                await _hubContext.Clients.All.SendAsync("AcquisizioniUpdated", new[] { latest });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for new records");
            }
        }

        public async Task<IEnumerable<Acquisizioni>> GetLatestAcquisizioniAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await context.Acquisizioni.OrderByDescending(a => a.Id).Take(10).ToListAsync();
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
                return await context.Acquisizioni.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest single record");
                return null;
            }
        }

        public async Task<IEnumerable<Acquisizioni>> GetLatestByLineAsync(string codLinea)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await context.Acquisizioni.Where(a => a.CodLinea == codLinea).OrderByDescending(a => a.Id).Take(10).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest by line");
                return Enumerable.Empty<Acquisizioni>();
            }
        }

        public async Task<IEnumerable<Acquisizioni>> GetLatestByStationAsync(string codPostazione)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await context.Acquisizioni.Where(a => a.CodPostazione == codPostazione).OrderByDescending(a => a.Id).Take(10).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest by station");
                return Enumerable.Empty<Acquisizioni>();
            }
        }

        public async Task<IEnumerable<Acquisizioni>> GetLatestByLineAndStationAsync(string codLinea, string codPostazione)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await context.Acquisizioni.Where(a => a.CodLinea == codLinea && a.CodPostazione == codPostazione).OrderByDescending(a => a.Id).Take(10).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest by line+station");
                return Enumerable.Empty<Acquisizioni>();
            }
        }

        public async Task<Acquisizioni?> GetLatestSingleByLineAsync(string codLinea)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await context.Acquisizioni.Where(a => a.CodLinea == codLinea).OrderByDescending(a => a.Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest single by line");
                return null;
            }
        }

        public async Task<Acquisizioni?> GetLatestSingleByStationAsync(string codPostazione)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await context.Acquisizioni.Where(a => a.CodPostazione == codPostazione).OrderByDescending(a => a.Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest single by station");
                return null;
            }
        }

        public async Task<Acquisizioni?> GetLatestSingleByLineAndStationAsync(string codLinea, string codPostazione)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await context.Acquisizioni.Where(a => a.CodLinea == codLinea && a.CodPostazione == codPostazione).OrderByDescending(a => a.Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest single by line+station");
                return null;
            }
        }
    }
}
