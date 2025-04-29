using ECommerce.DAL.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ECommerce.DAL.Entities;

namespace ECommerce.DAL.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(EntityConstants.Category.NameMaxLength);

        builder.Property(e => e.Description)
            .HasMaxLength(EntityConstants.Category.DescriptionMaxLength);
    }
}
