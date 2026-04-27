using ClinicalXPDataConnections.Models;
using AdminX.Models;

namespace AdminX.ViewModels
{
    public class AlertVM
    {
        public Patient patient {  get; set; }
        public Alert alert { get; set; }
        public List<Alert> alertList {  get; set; }
        public List<AlertTypes> alertTypes { get; set; }
        public bool success { get; set; }
        public string? message { get; set; }
    }
}
