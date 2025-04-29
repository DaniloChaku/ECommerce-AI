using ECommerce.DAL.Constants;
using ECommerce.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.DAL.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.Name)
            .HasMaxLength(EntityConstants.User.NameMaxLength);

        builder.Property(u => u.Email)
            .HasMaxLength(EntityConstants.User.EmailMaxLength);
    }
}
