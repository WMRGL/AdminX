using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdminX.Models
{
    [Table("ListCity", Schema="dbo")]
    public class ListCity
    {
        [Key]
        public string TownCity { get; set; }
        public string? County { get; set; }
        public string? AreaCode { get; set; }
    }
}
