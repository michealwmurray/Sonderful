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
            new() { Username = "micheal_m",   Email = "micheal.m@atu.ie",      PasswordHash = hash, Bio = "Love hiking and coffee chats around Dublin.",  PhotoUrl = "/uploads/profiles/micheal_m.jpg" },
            new() { Username = "shane_k",     Email = "shane.k@gmail.com",    PasswordHash = hash, Bio = "Weekend footballer, always up for a game.",      PhotoUrl = "/uploads/profiles/shane_k.jpg"   },
            new() { Username = "shannon_b",   Email = "shannon.b@atu.ie",     PasswordHash = hash, Bio = "Board game enthusiast and foodie.",               PhotoUrl = "/uploads/profiles/shannon_b.jpg" },
            new() { Username = "pamela_r",    Email = "pamela.r@gmail.com",   PasswordHash = hash, Bio = "Cyclist and outdoor cooking fan.",                PhotoUrl = "/uploads/profiles/pamela_r.jpg"  },
            new() { Username = "sharry_d",    Email = "sharry.d@atu.ie",      PasswordHash = hash, Bio = "Running club regular and dog walker.",            PhotoUrl = "/uploads/profiles/sharry_d.jpg"  },
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

            // Cork
            new()
            {
                Title = "Loop around The Lough",
                Description = "Easy walk around The Lough. Flat, about 1.5km. Good spot for a chat. Might grab a coffee in Douglas after if there's interest.",
                Category = PlanCategory.Walking,
                Capacity = 10, RsvpCount = 0,
                Latitude = 51.8883, Longitude = -8.4879, County = "Cork",
                ScheduledAt = now.AddDays(8).Date.AddHours(10),
                Creator = users[3]
            },
            new()
            {
                Title = "Coffee at Filter – South Mall",
                Description = "Coffee at Filter on South Mall. No agenda, just good coffee and a chance to meet a few people around the city.",
                Category = PlanCategory.Coffee,
                Capacity = 6, RsvpCount = 0,
                Latitude = 51.8985, Longitude = -8.4723, County = "Cork",
                ScheduledAt = now.AddDays(9).Date.AddHours(11),
                Creator = users[2]
            },
            new()
            {
                Title = "6-a-side at the Mardyke",
                Description = "6-a-side at the Mardyke pitches. We have a booking, need the numbers to fill it. Bring boots. All levels.",
                Category = PlanCategory.Sports,
                Capacity = 12, RsvpCount = 0,
                Latitude = 51.8963, Longitude = -8.4939, County = "Cork",
                ScheduledAt = now.AddDays(10).Date.AddHours(18).AddMinutes(30),
                Creator = users[1]
            },
            new()
            {
                Title = "Brunch at Nash 19",
                Description = "Group brunch at Nash 19 on Princes Street. Good food. Booking under Sonderful, arrive by half 11.",
                Category = PlanCategory.Dining,
                Capacity = 8, RsvpCount = 0,
                Latitude = 51.8983, Longitude = -8.4750, County = "Cork",
                ScheduledAt = now.AddDays(12).Date.AddHours(11).AddMinutes(30),
                Creator = users[0]
            },
            new()
            {
                Title = "Quiz night – Franciscan Well",
                Description = "Weekly quiz at the Franciscan Well. Teams of four max. Gets packed so arrive a bit early to get a table.",
                Category = PlanCategory.Other,
                Capacity = 8, RsvpCount = 0,
                Latitude = 51.9003, Longitude = -8.4853, County = "Cork",
                ScheduledAt = now.AddDays(11).Date.AddHours(19).AddMinutes(30),
                Creator = users[4]
            },

            // Waterford
            new()
            {
                Title = "Greenway cycle – city to Kilmacthomas",
                Description = "Cycling the Waterford Greenway from the city to Kilmacthomas, about 23km. Shuttle back to the city sorted. Bike hire at the start if you need it.",
                Category = PlanCategory.Sports,
                Capacity = 10, RsvpCount = 0,
                Latitude = 52.2569, Longitude = -7.1101, County = "Waterford",
                ScheduledAt = now.AddDays(9).Date.AddHours(9),
                Creator = users[3]
            },
            new()
            {
                Title = "Coffee on the quays – Waterford",
                Description = "Coffee at the Granary cafe on the quays. Low key, good spot to meet a few people if you're new to Waterford or just looking to get out.",
                Category = PlanCategory.Coffee,
                Capacity = 6, RsvpCount = 0,
                Latitude = 52.2593, Longitude = -7.1130, County = "Waterford",
                ScheduledAt = now.AddDays(8).Date.AddHours(10).AddMinutes(30),
                Creator = users[2]
            },
            new()
            {
                Title = "Viking Triangle wander",
                Description = "Wander through the Viking Triangle and along the quays, stopping at Reginald's Tower. About 3km, no real pace to it.",
                Category = PlanCategory.Walking,
                Capacity = 12, RsvpCount = 0,
                Latitude = 52.2580, Longitude = -7.1113, County = "Waterford",
                ScheduledAt = now.AddDays(13).Date.AddHours(11),
                Creator = users[0]
            },
            new()
            {
                Title = "5-a-side – Regional Sports Centre",
                Description = "5-a-side at the Regional Sports Centre. Got a pitch booked. Bring boots and a fiver for the cost. Any skill level.",
                Category = PlanCategory.Sports,
                Capacity = 10, RsvpCount = 0,
                Latitude = 52.2519, Longitude = -7.1204, County = "Waterford",
                ScheduledAt = now.AddDays(10).Date.AddHours(19),
                Creator = users[1]
            },
            new()
            {
                Title = "Sunday lunch at Bodega",
                Description = "Sunday lunch at Bodega on John Street. Booking under Sonderful for 1pm. Good food and usually a nice relaxed vibe.",
                Category = PlanCategory.Dining,
                Capacity = 8, RsvpCount = 0,
                Latitude = 52.2588, Longitude = -7.1152, County = "Waterford",
                ScheduledAt = now.AddDays(12).Date.AddHours(13),
                Creator = users[4]
            },

            // Donegal
            new()
            {
                Title = "Slieve League hike",
                Description = "Hike up Slieve League from the lower car park. About 4 hours there and back. Bring layers, it can change fast on the way up. Moderate fitness required.",
                Category = PlanCategory.Walking,
                Capacity = 10, RsvpCount = 0,
                Latitude = 54.6339, Longitude = -8.6923, County = "Donegal",
                ScheduledAt = now.AddDays(14).Date.AddHours(9).AddMinutes(30),
                Creator = users[1]
            },
            new()
            {
                Title = "Surf session at Rossnowlagh",
                Description = "Surf session at Rossnowlagh. Mix of beginners and intermediates. The surf school on the beach does lessons if you're new. Meet at the main car park.",
                Category = PlanCategory.Sports,
                Capacity = 12, RsvpCount = 0,
                Latitude = 54.5496, Longitude = -8.2011, County = "Donegal",
                ScheduledAt = now.AddDays(11).Date.AddHours(10),
                Creator = users[0]
            },
            new()
            {
                Title = "Coffee at The Blueberry Tearoom",
                Description = "Coffee at The Blueberry Tearoom in Donegal town. Good spot, drop in anytime between 10 and 12. No need to RSVP if you're passing through.",
                Category = PlanCategory.Coffee,
                Capacity = 8, RsvpCount = 0,
                Latitude = 54.6541, Longitude = -8.1100, County = "Donegal",
                ScheduledAt = now.AddDays(8).Date.AddHours(10),
                Creator = users[2]
            },
            new()
            {
                Title = "Glenveagh lough loop",
                Description = "Walking the lough loop in Glenveagh National Park, about 7km. Park opens at 9. Happy to organise a carpool from Letterkenny if people need it.",
                Category = PlanCategory.Walking,
                Capacity = 12, RsvpCount = 0,
                Latitude = 55.0238, Longitude = -7.9225, County = "Donegal",
                ScheduledAt = now.AddDays(15).Date.AddHours(9),
                Creator = users[3]
            },
            new()
            {
                Title = "Pints in McGettigan's – Donegal town",
                Description = "Few pints in McGettigan's in Donegal town from about 8. Nothing organised, just showing up. Come and go as you like.",
                Category = PlanCategory.Other,
                Capacity = 15, RsvpCount = 0,
                Latitude = 54.6541, Longitude = -8.1095, County = "Donegal",
                ScheduledAt = now.AddDays(10).Date.AddHours(20),
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
            // Cork: The Lough walk — shane, shannon join
            new() { Plan = plans[10], User = users[1], CreatedAt = now },
            new() { Plan = plans[10], User = users[2], CreatedAt = now },
            // Cork: Nash 19 brunch — sharry, pamela join
            new() { Plan = plans[13], User = users[4], CreatedAt = now },
            new() { Plan = plans[13], User = users[3], CreatedAt = now },
            // Cork: Franciscan Well quiz — micheal, shane join
            new() { Plan = plans[14], User = users[0], CreatedAt = now },
            new() { Plan = plans[14], User = users[1], CreatedAt = now },
            // Waterford: Greenway cycle — shane, pamela join
            new() { Plan = plans[15], User = users[1], CreatedAt = now },
            new() { Plan = plans[15], User = users[3], CreatedAt = now },
            // Waterford: Viking Triangle — micheal joins
            new() { Plan = plans[17], User = users[0], CreatedAt = now },
            // Donegal: Slieve League — shane, sharry join
            new() { Plan = plans[20], User = users[1], CreatedAt = now },
            new() { Plan = plans[20], User = users[4], CreatedAt = now },
            // Donegal: Rossnowlagh surf — micheal, shannon join
            new() { Plan = plans[21], User = users[0], CreatedAt = now },
            new() { Plan = plans[21], User = users[2], CreatedAt = now },
        };

        plans[0].RsvpCount = 2;
        plans[1].RsvpCount = 2;
        plans[2].RsvpCount = 1;
        plans[3].RsvpCount = 1;
        plans[4].RsvpCount = 2;
        plans[5].RsvpCount = 2;
        plans[8].RsvpCount = 3;
        plans[10].RsvpCount = 2;
        plans[13].RsvpCount = 2;
        plans[14].RsvpCount = 2;
        plans[15].RsvpCount = 2;
        plans[17].RsvpCount = 1;
        plans[20].RsvpCount = 2;
        plans[21].RsvpCount = 2;

        db.Rsvps.AddRange(rsvps);
        await db.SaveChangesAsync();
    }
}
