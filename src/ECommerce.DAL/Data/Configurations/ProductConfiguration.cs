using ECommerce.DAL.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ECommerce.DAL.Entities;

namespace ECommerce.DAL.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(EntityConstants.Product.NameMaxLength);

        builder.Property(e => e.Description)
            .HasMaxLength(EntityConstants.Product.DescriptionMaxLength);

        builder.Property(e => e.Price)
            .HasColumnType(EntityConstants.Product.PriceColumnType);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(EntityConstants.Product.ImageUrlMaxLength);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}