using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TollFreeVehicleConfiguration : IEntityTypeConfiguration<TollFreeVehicle>
{
    public void Configure(EntityTypeBuilder<TollFreeVehicle> builder) { }
}
