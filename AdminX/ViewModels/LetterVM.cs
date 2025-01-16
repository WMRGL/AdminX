using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class LetterVM
    {
        public Patient patient { get; set; }
        public Relative relative { get; set; }
        public DocumentsContent documentsContent { get; set; }
        public StaffMember staffMember { get; set; }
        public StaffMember letterOwner { get; set; }
        public ExternalClinician referrer { get; set; }
        public ExternalClinician gp { get; set; }
        public ExternalClinician other { get; set; }
        public ExternalFacility facility { get; set; }
        public DictatedLetter dictatedLetter { get; set; }
    }
}
