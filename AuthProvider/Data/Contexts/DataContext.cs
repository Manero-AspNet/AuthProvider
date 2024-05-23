using AuthProvider.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthProvider.Data.Contexts;

public class DataContext : IdentityDbContext<UserEntity>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DataContext()
    {
    }

    public DbSet<VerificationRequestEntity> VerificationRequests { get; set; }
    public DbSet<ForgotPasswordRequestEntity> ForgotPasswordRequests { get; set;}
}
