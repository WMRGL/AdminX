using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminX.Models
{
    [Table("ListLanguages", Schema = "dbo")]
    public class Language
    {
        [Key]
        public int Id { get; set; }
        public string LanguageName { get; set; }
    }
}
