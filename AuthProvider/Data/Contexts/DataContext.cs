using AuthProvider.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthProvider.Data.Contexts;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<UserEntity>(options)
{
    public DbSet<VerificationRequestEntity> VerificationRequests { get; set; }
    public DbSet<ForgotPasswordRequestEntity> ForgotPasswordRequests { get; set;}
}
