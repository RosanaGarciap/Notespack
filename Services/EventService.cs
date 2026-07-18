using Microsoft.EntityFrameworkCore;
using NOTESPACK.Data;
using NOTESPACK.Models;

namespace NOTESPACK.Services
{
    public class EventService
    {
        private readonly IDbContextFactory<EventContext> _contextFactory;

        public EventService(IDbContextFactory<EventContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Event>> GetEventsByUserIdAsync(int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }

        public async Task AddEventAsync(Event newEvent, int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            newEvent.UserId = userId;
            context.Events.Add(newEvent);
            await context.SaveChangesAsync();
        }

        public async Task UpdateEventAsync(Event updatedEvent, int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            var existingEvent = await context.Events
                .FirstOrDefaultAsync(e => e.Id == updatedEvent.Id && e.UserId == userId);

            if (existingEvent != null)
            {
                existingEvent.Title = updatedEvent.Title;
                existingEvent.Description = updatedEvent.Description;
                existingEvent.Date = updatedEvent.Date;
                existingEvent.EndDate = updatedEvent.EndDate;
                existingEvent.Location = updatedEvent.Location;
                existingEvent.Duration = updatedEvent.Duration;
                existingEvent.Organizer = updatedEvent.Organizer;

                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteEventAsync(int id, int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            var eventToDelete = await context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            
            if (eventToDelete != null)
            {
                context.Events.Remove(eventToDelete);
                await context.SaveChangesAsync();
            }
        }
         public async Task<List<Event>> GetCampusEventsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            // IMPORTANTE: Cambia el 0 por null para que coincida con tu base de datos
            return await context.Events
                .Where(e => e.UserId == null) 
                .ToListAsync<Event>();
        }
        public async Task<List<Event>> GetEventsByMonthAsync(int year, int month)
        {
            using var context = _contextFactory.CreateDbContext();
            
            var query = context.Events
                .Where(e => e.UserId == null && e.Date.Year == year && e.Date.Month == month)
                .OrderBy(e => e.Date);

            var allEvents = await query.ToListAsync(); 

            return allEvents.DistinctBy(e => e.Title).ToList();
        }
        public async Task<List<Event>> GetEventsByYearAsync(int year)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(e => e.UserId == null && e.Date.Year == year)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }
        public async Task AddCampusEventToUserAsync(int eventId, int userId)
        {
            using var context = _contextFactory.CreateDbContext();
          
            var originalEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == eventId && e.UserId == null);
            
            if (originalEvent != null)
            {
                var userEvent = new Event
                {
                    Title = originalEvent.Title,
                    Description = originalEvent.Description,
                    Date = originalEvent.Date,
                    EndDate = originalEvent.EndDate,
                    Location = originalEvent.Location,
                    Duration = originalEvent.Duration,
                    Organizer = originalEvent.Organizer,
                    UserId = userId
                };
                context.Events.Add(userEvent);
                await context.SaveChangesAsync();
            }
        }
    }
}