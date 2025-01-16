using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class HomeVM
    {        
        public string name { get; set; }
        public string staffCode { get; set; }
        public bool isLive { get; set; }
        public string notificationMessage { get; set; }
        public string dllVersion { get; set; }
        public string appVersion { get; set; }
        public int contactOutcomes { get; set; }
        public int triageOutcomes { get; set; }
        public int reviewOutcomes { get; set; }
        public int dictatedLetters { get; set; }
    }
}
