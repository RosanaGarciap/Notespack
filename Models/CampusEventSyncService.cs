using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NOTESPACK.Data;
using NOTESPACK.Models;

namespace NOTESPACK.Models
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
            // Sincroniza cada 30 minutos (ajusta el tiempo si lo deseas)
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                await SyncEventsAsync();
            }
            catch (Exception)
            {
                // Ignorar o registrar errores en background de forma silenciosa
            }
        }

        private async Task SyncEventsAsync()
        {
            using var client = _httpClientFactory.CreateClient();
            // Cambia la URL por el endpoint real de la API de la universidad cuando lo tengas
            var response = await client.GetAsync("https://api.university.edu/events"); 
            
            if (!response.IsSuccessStatusCode) return;

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);

            if (doc.RootElement.ValueKind != JsonValueKind.Array) return;

            using var context = await _contextFactory.CreateDbContextAsync();

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                // Extraer strings de forma segura controlando nulos
                string title = element.TryGetProperty("title", out var t) ? t.GetString() ?? "No Title" : "No Title";
                string description = element.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
                string address = element.TryGetProperty("address", out var a) ? a.GetString() ?? "Unknown" : "Unknown";
                
                DateTime eventDate = DateTime.Today;
                if (element.TryGetProperty("startDate", out var sd) && sd.GetString() != null)
                {
                    DateTime.TryParse(sd.GetString(), out eventDate);
                }

                // Verificar si ya existe un evento con el mismo título y fecha para no duplicarlo
                var exists = await context.Events.AnyAsync(e => e.Title == title && e.Date == eventDate.Date);

                if (!exists)
                {
                    var newEvent = new Event
                    {
                        Title = title,
                        Description = description,
                        Date = eventDate.Date,
                        Location = address,
                        UserId = null
                    };
                    context.Events.Add(newEvent);
                }
            }

            await context.SaveChangesAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}