using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TollFreeMonthConfiguration : IEntityTypeConfiguration<TollFreeMonth>
{
    public void Configure(EntityTypeBuilder<TollFreeMonth> builder) { }
}
