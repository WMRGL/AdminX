using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminX.Models
{
    [Table("AuditLogs", Schema = "dbo")]
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? EventType { get; set; }
        public string? TableName { get; set; }
        public DateTime DateTime { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Action { get; set; } 
        public string? Duration { get; set; }
        public string? IpAddress { get; set; }
    }
}
