using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.models
{
    [Table("viruses")]
    public class Viruses
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }


        [Required]
        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        [Column("virus_name")]
        public string VirusName { get; set; } = string.Empty;

        [Column("virus_type")]
        public string virusType { get; set; } = string.Empty;

        [Column("user_id")]
        public int userId { get; set; }

    }
}
