using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore_JWT.Models
{
    internal class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasData(new User
            {
                UserName = "admin@exemplo.org",
                Email = "admin@exemplo.org",
                PasswordHash = "AQAAAAEAACcQAAAAEHx1/xJOakHAP1VdXINgRCkJFKrJN/J7EDNrgjP5eHPWivVSUGQQNoP7MuY7ulklFQ==",
                EmailConfirmed = true,
            });
        }
    }
}