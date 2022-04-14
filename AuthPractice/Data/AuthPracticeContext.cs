#nullable disable
using Microsoft.EntityFrameworkCore;
using AuthPractice.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AuthPractice.Data
{
    public class AuthPracticeContext : IdentityDbContext<User>
    {
        public AuthPracticeContext (DbContextOptions<AuthPracticeContext> options)
            : base(options)
        {
        }

        public DbSet<Phone> Phone { get; set; }

    }
}
