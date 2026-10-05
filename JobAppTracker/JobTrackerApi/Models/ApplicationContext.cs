using Microsoft.EntityFrameworkCore;

namespace JobTrackerApi.Models;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    public DbSet<Application> Applications => Set<Application>();
}