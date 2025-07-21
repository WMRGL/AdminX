using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class LabReportController : Controller
    {
        private readonly LabContext _context;
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly ILabData _labData;
        private readonly LabReportVM _lvm;
        private readonly StaffUserData _staff;
        private readonly AuditService _audit;

        public LabReportController(LabContext context, ClinicalContext clinContext, IConfiguration config)
        {
            _context = context;
            _clinContext = clinContext;
            _config = config;
            _labData = new LabReportData(_context);
            _lvm = new LabReportVM();
            _staff = new StaffUserData(_clinContext);
            _audit = new AuditService(_config);
        }

        

        public IActionResult LabPatientSearch(string? firstname, string? lastname, string? nhsno, string? postcode, DateTime? dob)
        {
            string staffCode = _staff.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "ClinicX - LabReports", "", _ip.GetIPAddress());

            _lvm.patientsList = new List<LabPatient>();

            if (firstname != null || lastname != null || nhsno != null || postcode != null || dob != null)
            {
                _lvm.patientsList = _labData.GetPatients(firstname, lastname, nhsno, postcode, dob);
                _lvm.searchTerms = "Firstname:" + firstname + ",Lastname:" + lastname + ",NHSNo:" + nhsno + ",Postcode:" + postcode;
                if(dob != null)
                {
                    _lvm.searchTerms = _lvm.searchTerms + dob.Value.ToString("yyyy-MM-dd");
                }
            }

            return View(_lvm);
        }

        public IActionResult LabReports(int intID)
        {
            _lvm.patient = _labData.GetPatientDetails(intID);
            _lvm.cytoReportsList = _labData.GetCytoReportsList(intID);
            //_lvm.dnaReportsList = _labData.GetDNAReportsList(intID);
            _lvm.labDNALabDataList = _labData.GetDNALabDataList(intID);

            return View(_lvm);
        }

        public IActionResult SampleDetails(string labno)
        {
            _lvm.cytoReport = _labData.GetCytoReport(labno);
            _lvm.dnaReport = _labData.GetDNAReport(labno);            

            return View(_lvm);
        }

        public IActionResult DNALabReport(string labno, string indication, string reason)
        {            
            _lvm.dnaReportDetails = _labData.GetDNAReportDetails(labno, indication, reason);
            _lvm.dnaReport = _labData.GetDNAReport(labno);            
    
            return View(_lvm);
        }

        public IActionResult CytoLabReport(string labno)
        {
            _lvm.cytoReport = _labData.GetCytoReport(labno);            

            return View(_lvm);
        }
    }
}
