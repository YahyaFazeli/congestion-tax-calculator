using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TollFreeWeekdayConfiguration : IEntityTypeConfiguration<TollFreeWeekday>
{
    public void Configure(EntityTypeBuilder<TollFreeWeekday> builder) { }
}
