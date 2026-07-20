using System;
using System.ComponentModel.DataAnnotations;

namespace AgriVision.Domain.Entities
{
    public class InsectDetection
    {
        [Key]
        [Required]
        public int InsectDetectionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? DetectionResultURL { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ApplicationUser User { get; set; } = default!;
    }
}
