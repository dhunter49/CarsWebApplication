using Cars.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cars.Infrastructure;

public class DataContext : IdentityDbContext<AppUser, IdentityRole, string>
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options) {}
    
    public DbSet<Car> Cars => Set<Car>();
}