using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<User> Users => Set<User>();
}
