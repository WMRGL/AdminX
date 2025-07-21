using AdminX.Models;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.ViewModels
{
    public class RelativeDiaryVM
    {
        public RelativeDiary relativeDiary {  get; set; }
        public List<RelativeDiary> relativeDiaryList { get; set; }
        public Relative relative { get; set; }
        public Patient patient { get; set; }
        public List<DiaryAction> diaryActionsList { get; set; }
        public List<Document> documentsList { get; set; }

        public StaffMember staffMember { get; set; }
    }
}
