using Microsoft.EntityFrameworkCore;
using Sonderful.API.Models;

namespace Sonderful.API.Data;

/// <summary>
/// Seeds the database with sample users, plans, and RSVPs so the app
/// is usable straight away after a fresh deployment.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Populates users, plans, and RSVPs if the database is empty.
    /// All seed users share the same password for convenience during development.
    /// </summary>
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync())
            return;

        // Users
        const string password = "Password1!";
        string hash = BCrypt.Net.BCrypt.HashPassword(password);

        var users = new List<User>
        {
            new() { Username = "micheal_m",   Email = "micheal.m@atu.ie",      PasswordHash = hash, Bio = "Love hiking and coffee chats around Dublin." },
            new() { Username = "shane_k",     Email = "shane.k@gmail.com",    PasswordHash = hash, Bio = "Weekend footballer, always up for a game." },
            new() { Username = "shannon_b",   Email = "shannon.b@atu.ie",     PasswordHash = hash, Bio = "Board game enthusiast and foodie." },
            new() { Username = "pamela_r",    Email = "pamela.r@gmail.com",   PasswordHash = hash, Bio = "Cyclist and outdoor cooking fan." },
            new() { Username = "sharry_d",    Email = "sharry.d@atu.ie",      PasswordHash = hash, Bio = "Running club regular and dog walker." },
        };

        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        // Plans
        var now = DateTime.UtcNow;

        var plans = new List<Plan>
        {
            new()
            {
                Title = "Morning walk in Phoenix Park",
                Description = "Easy 5km loop starting at the Papal Cross. All paces welcome — we'll grab coffee after at the Visitors Centre.",
                Category = PlanCategory.Walking,
                Capacity = 12, RsvpCount = 0,
                Latitude = 53.3567, Longitude = -6.3239, County = "Dublin",
                ScheduledAt = now.AddDays(2).Date.AddHours(9),
                Creator = users[0]
            },
            new()
            {
                Title = "5-a-side football – Irishtown",
                Description = "Casual 5-a-side at Irishtown Stadium. Bring boots and water. Bibs provided.",
                Category = PlanCategory.Sports,
                Capacity = 10, RsvpCount = 0,
                Latitude = 53.3345, Longitude = -6.2213, County = "Dublin",
                ScheduledAt = now.AddDays(3).Date.AddHours(18),
                Creator = users[1]
            },
            new()
            {
                Title = "Board game night – Ranelagh",
                Description = "Hosting a board game night at my place in Ranelagh. Catan, Ticket to Ride, and more. BYOB.",
                Category = PlanCategory.Gaming,
                Capacity = 8, RsvpCount = 0,
                Latitude = 53.3240, Longitude = -6.2595, County = "Dublin",
                ScheduledAt = now.AddDays(4).Date.AddHours(19).AddMinutes(30),
                Creator = users[2]
            },
            new()
            {
                Title = "Coffee & chat – Grafton Street",
                Description = "Meetup for a coffee and a chat at Bewley's. Great spot to meet new people.",
                Category = PlanCategory.Coffee,
                Capacity = 6, RsvpCount = 0,
                Latitude = 53.3410, Longitude = -6.2602, County = "Dublin",
                ScheduledAt = now.AddDays(1).Date.AddHours(11),
                Creator = users[0]
            },
            new()
            {
                Title = "Sunday brunch – The Woollen Mills",
                Description = "Group brunch at The Woollen Mills on the quays. Booking under 'Sonderful'.",
                Category = PlanCategory.Dining,
                Capacity = 10, RsvpCount = 0,
                Latitude = 53.3454, Longitude = -6.2537, County = "Dublin",
                ScheduledAt = now.AddDays(5).Date.AddHours(12),
                Creator = users[3]
            },
            new()
            {
                Title = "Coastal run – Sandymount Strand",
                Description = "6km out-and-back along the strand. Meet at the Martello tower. All paces welcome.",
                Category = PlanCategory.Sports,
                Capacity = 15, RsvpCount = 0,
                Latitude = 53.3225, Longitude = -6.2019, County = "Dublin",
                ScheduledAt = now.AddDays(2).Date.AddHours(7).AddMinutes(30),
                Creator = users[4]
            },
            new()
            {
                Title = "Retro gaming session – Drumcondra",
                Description = "N64, SNES and PS1 classics. Winner stays on. Pizza ordered for 8pm.",
                Category = PlanCategory.Gaming,
                Capacity = 6, RsvpCount = 0,
                Latitude = 53.3660, Longitude = -6.2574, County = "Dublin",
                ScheduledAt = now.AddDays(6).Date.AddHours(17),
                Creator = users[1]
            },
            new()
            {
                Title = "Cycle to Howth Summit",
                Description = "35km round trip from the Docklands out to Howth Summit. Moderate pace. Café stop at the top.",
                Category = PlanCategory.Walking,
                Capacity = 8, RsvpCount = 0,
                Latitude = 53.3785, Longitude = -6.0639, County = "Dublin",
                ScheduledAt = now.AddDays(7).Date.AddHours(10),
                Creator = users[3]
            },
            new()
            {
                Title = "Pub quiz – The Long Hall",
                Description = "Team up for the weekly pub quiz at The Long Hall on South Great George's St. Teams of 4.",
                Category = PlanCategory.Other,
                Capacity = 8, RsvpCount = 0,
                Latitude = 53.3431, Longitude = -6.2665, County = "Dublin",
                ScheduledAt = now.AddDays(3).Date.AddHours(20),
                Creator = users[2]
            },
            new()
            {
                Title = "Lunchtime coffee – IFSC",
                Description = "Quick lunchtime coffee at 3FE in the IFSC. Good craic, good coffee.",
                Category = PlanCategory.Coffee,
                Capacity = 4, RsvpCount = 0,
                Latitude = 53.3492, Longitude = -6.2379, County = "Dublin",
                ScheduledAt = now.AddDays(1).Date.AddHours(13),
                Creator = users[4]
            },
        };

        db.Plans.AddRange(plans);
        await db.SaveChangesAsync();

        // RSVPs
        var rsvps = new List<Rsvp>
        {
            // Phoenix Park walk — shane, shannon join
            new() { Plan = plans[0], User = users[1], CreatedAt = now },
            new() { Plan = plans[0], User = users[2], CreatedAt = now },
            // Football — micheal, sharry join
            new() { Plan = plans[1], User = users[0], CreatedAt = now },
            new() { Plan = plans[1], User = users[4], CreatedAt = now },
            // Board game night — pamela joins
            new() { Plan = plans[2], User = users[3], CreatedAt = now },
            // Coffee Grafton — shane joins
            new() { Plan = plans[3], User = users[1], CreatedAt = now },
            // Brunch — micheal, shannon join
            new() { Plan = plans[4], User = users[0], CreatedAt = now },
            new() { Plan = plans[4], User = users[2], CreatedAt = now },
            // Coastal run — shane, pamela join
            new() { Plan = plans[5], User = users[1], CreatedAt = now },
            new() { Plan = plans[5], User = users[3], CreatedAt = now },
            // Pub quiz — micheal, pamela, sharry join
            new() { Plan = plans[8], User = users[0], CreatedAt = now },
            new() { Plan = plans[8], User = users[3], CreatedAt = now },
            new() { Plan = plans[8], User = users[4], CreatedAt = now },
        };

        plans[0].RsvpCount = 2;
        plans[1].RsvpCount = 2;
        plans[2].RsvpCount = 1;
        plans[3].RsvpCount = 1;
        plans[4].RsvpCount = 2;
        plans[5].RsvpCount = 2;
        plans[8].RsvpCount = 3;

        db.Rsvps.AddRange(rsvps);
        await db.SaveChangesAsync();
    }
}
