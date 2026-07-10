using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;
namespace AdminX.ViewModels
{
    [Keyless]
    public class VhrAuditVM
    {
        public List<VhrAudit> vhrAuditList { get; set; }
    }
}
