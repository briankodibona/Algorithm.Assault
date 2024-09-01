using AttemptAtUserIdPush.Models;
using Microsoft.EntityFrameworkCore;

namespace AttemptAtUserIdPush.InMemoryDB;

public class ApplicationDbContext : DbContext
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
    public DbSet<UserModel> Users { get; set; }
    
    public DbSet<customUserIdModel> CustomUsersIdModels { get; set; }
    
    public DbSet<RolesModel> RolesModels { get; set; }
    
    public DbSet<TimeMangementModel> TimeMangementModels { get; set; }
    
   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}