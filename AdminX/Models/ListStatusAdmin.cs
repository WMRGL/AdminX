using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
namespace AdminX.Models
{
    [Keyless]
    [Table("ListStatusAdmin", Schema = "dbo")]
    public class ListStatusAdmin
    {
        public string Status_Admin {  get; set; }
    }
}
