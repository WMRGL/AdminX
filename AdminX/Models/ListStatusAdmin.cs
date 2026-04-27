using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace AdminX.Models
{
    [Keyless]
    [Table("ListStatusAdmin", Schema = "dbo")]
    public class ListStatusAdmin
    {
        [Key]
        public Int16 Sequence { get; set; }
        public string Status_Admin {  get; set; }
        
    }
}
