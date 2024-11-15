using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdminX.Models
{
    [Table("APPTYPE", Schema = "dbo")]
    public class ActivityType
    {
        [Key]
        public string APP_TYPE { get; set; }
        public Int16 NON_ACTIVE { get; set; }
        public bool ISREFERRAL { get; set; }
        public bool ISAPPT { get; set; }
        public bool ISSTUDY { get; set; }
    }
}
