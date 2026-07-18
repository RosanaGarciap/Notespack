using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NOTESPACK.Data;
using NOTESPACK.Models;
using System.Text.RegularExpressions;

namespace NOTESPACK.Services
{
    public class CampusEventSyncService : IHostedService, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDbContextFactory<EventContext> _contextFactory;
        private Timer? _timer;

        public CampusEventSyncService(IHttpClientFactory httpClientFactory, IDbContextFactory<EventContext> contextFactory)
        {
            _httpClientFactory = httpClientFactory;
            _contextFactory = contextFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                await SyncEventsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CampusEventSyncService: {ex.Message}");
            }
        }
        public async Task<List<Event>> GetEventsByMonthAsync(int year, int month)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(e => e.UserId == null && e.Date.Year == year && e.Date.Month == month)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }

        public async Task<List<Event>> GetEventsByYearAsync(int year)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(e => e.UserId == null && e.Date.Year == year)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }

        private async Task SyncEventsAsync()
        {
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://calendar.byu.edu/api/Events.json?categories=all"); 
            
            if (!response.IsSuccessStatusCode) return;

            var jsonString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(jsonString);

            using var context = _contextFactory.CreateDbContext();

            // 1. Fetch existing events into a HashSet for O(1) lookup
            // Using a tuple of (Title, Date) as the key
            var existingEvents = await context.Events
                .Select(e => new { e.Title, Date = e.Date.Date })
                .ToListAsync();
            
            var existingLookup = new HashSet<(string Title, DateTime Date)>(
                existingEvents.Select(e => (e.Title, e.Date))
            );

            var newEvents = new List<Event>();

            foreach (var element in jsonDoc.RootElement.EnumerateArray())
            {
                string title = element.TryGetProperty("Title", out var t) ? t.GetString() ?? "No Title" : "No Title";
                DateTime eventDate = DateTime.Today;
                if (element.TryGetProperty("StartDateTime", out var sd))
                {
                    DateTime.TryParse(sd.GetString(), out eventDate);
                }

                // 2. Check in memory instead of querying the database
                if (!existingLookup.Contains((title, eventDate.Date)))
                {
                    newEvents.Add(new Event
                    {
                        Title = title,
                        Description = element.TryGetProperty("Description", out var d) ? d.GetString() ?? "" : "",
                        Date = eventDate.Date,
                        Location = element.TryGetProperty("LocationName", out var a) ? a.GetString() ?? "Unknown" : "Unknown",
                        UserId = null,
                        EndDate = eventDate.Date, 
                        Duration = "TBD",
                        Organizer = "BYU Calendar"
                    });
                    
                    // Mark as added so we don't try to add it again if the JSON contains duplicates
                    existingLookup.Add((title, eventDate.Date));
                }
            }

            // 3. Batch insert new records
            if (newEvents.Any())
            {
                await context.Events.AddRangeAsync(newEvents);
                await context.SaveChangesAsync();
                Console.WriteLine($"Guardados {newEvents.Count} nuevos eventos.");
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        public void Dispose() => _timer?.Dispose();
    }
}