#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AuthPractice.Models;

namespace AuthPractice.Data
{
    public class AuthPracticeContext : DbContext
    {
        public AuthPracticeContext (DbContextOptions<AuthPracticeContext> options)
            : base(options)
        {
        }

        public DbSet<AuthPractice.Models.Phone> Phone { get; set; }
    }
}
