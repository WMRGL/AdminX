using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Models
{
    [Keyless]
    [Table("ListRefDiagReasonOther", Schema = "dbo")]
    public class ListDisease
    {
        public string Reason { get; set; }
    }
}
