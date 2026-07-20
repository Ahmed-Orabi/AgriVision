using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgriVision.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace AgriVision.Infrastructure.Data.Configurations
{
    public class IrrigationRecommendaionHistoryConfiguration : IEntityTypeConfiguration<IrrigationRecommendationHistory>
    {
        public void Configure(EntityTypeBuilder<IrrigationRecommendationHistory> builder)
        {
            builder.HasKey(i => i.Id);
        }
    }
}
