using Microsoft.EntityFrameworkCore;
using Bible.Models;

namespace Bible.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
    {
        
    }
    public DbSet<Account> Account => Set<Account>();
    public DbSet<Kjv> Kjv => Set<Kjv>();
    public DbSet<User> User => Set<User>();
    public DbSet<Note> Note => Set<Note>();
}