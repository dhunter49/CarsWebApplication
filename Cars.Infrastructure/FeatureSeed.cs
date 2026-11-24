using Cars.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cars.Infrastructure;

public static class FeatureSeed
{
    public static async Task SeedAsync(DataContext context)
    {
        // Jeżeli w tabeli znajduje się już przynajmniej
        // jeden rekord, nie dodajemy danych ponownie.
        if (await context.Features.AnyAsync())
        {
            return;
        }

        var features = new List<Feature>
        {
            new() { Name = "ABS" },
            new() { Name = "ESP" },
            new() { Name = "Klimatyzacja" },
            new() { Name = "Nawigacja" },
            new() { Name = "Tempomat" },
            new() { Name = "Kamera cofania" },
            new() { Name = "Czujniki parkowania" },
            new() { Name = "Podgrzewane fotele" },
        };

        context.Features.AddRange(features);
        await context.SaveChangesAsync();
    }
}