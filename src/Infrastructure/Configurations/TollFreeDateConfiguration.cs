using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TollFreeDateConfiguration : IEntityTypeConfiguration<TollFreeDate>
{
    public void Configure(EntityTypeBuilder<TollFreeDate> builder) { }
}
