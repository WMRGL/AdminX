﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class ICPVM
    {
        public List<StaffMember> staffMembers { get; set; }
        public List<StaffMember> consultants { get; set; }
        public List<StaffMember> GCs { get; set; }
        public List<Triage> triages { get; set; }
        public List<ICPCancer> icpCancerListOwn { get; set; }
        public List<ICPCancer> icpCancerListOther { get; set; }
        public ICPCancerReviewAction cancerAction { get; set; }
        public Triage triage { get; set; }
        public ICPGeneral? icpGeneral { get; set; }
        public ICPCancer? icpCancer { get; set; }
        public Risk riskDetails { get; set; }
        public List<Document> documentList { get; set; }
        public Referral referralDetails { get; set; }
        public List<Pathway> pathways { get; set; }
        public List<Priority> priorityList { get; set; }
        public List<Relative> relatives { get; set; }        
        public List<ExternalCliniciansAndFacilities> clinicians { get; set; }
        public List<ExternalCliniciansAndFacilities> screeningCoordinators { get; set; }
        public List<string> specialities { get; set; }
        public string staffCode { get; set; }
        public string loggedOnUserType { get; set; }
        public bool isICPTriageStarted { get; set; }
    }
}
