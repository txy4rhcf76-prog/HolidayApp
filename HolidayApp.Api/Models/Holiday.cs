using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HolidayApp.Api.Models
{
    public class Holiday
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CountryCode { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string LocalName { get; set; } = null!;

        public string? Name { get; set; }

        public bool Global { get; set; }

        public string? Counties { get; set; }

        public int? LaunchYear { get; set; }

        public string? Types { get; set; }
    }
}
