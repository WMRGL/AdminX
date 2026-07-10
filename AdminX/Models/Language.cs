using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminX.Models
{
    [Table("ListLanguages", Schema = "dbo")]
    public class Language
    {
        [Key]
        public int ID { get; set; }
        public string LanguageName { get; set; }
        public bool InUse { get; set; }
        public string ExternalID { get; set; }

    }
}
