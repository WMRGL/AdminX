using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AdminX.Controllers
{
    public class LabReportController : Controller
    {
     //   private readonly LabContext _context;
       // private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly ILabDataAsync _labData;
        private readonly LabReportVM _lvm;
        private readonly IStaffUserDataAsync _staff;
        private readonly IAuditServiceAsync _audit;
        private readonly IPAddressFinder _ip;

        public LabReportController(IConfiguration config, ILabDataAsync lab, IStaffUserDataAsync staffUser, IAuditServiceAsync audit)
        {
         //   _context = context;
           // _clinContext = clinContext;
            _config = config;
            _labData = lab;
            _lvm = new LabReportVM();
            _staff = staffUser;
            _audit = audit;
            _ip = new IPAddressFinder(HttpContext);
        }

        [Authorize]
        public async Task<IActionResult> LabPatientSearch(string? firstname, string? lastname, string? nhsno, string? postcode, DateTime? dob)
        {
            try
            {
                string staffCode = await _staff.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - LabReports", "", _ip.GetIPAddress());

                _lvm.patientsList = new List<LabPatient>();

                if (firstname != null || lastname != null || nhsno != null || postcode != null || dob != null)
                {
                    _lvm.patientsList = await _labData.GetPatients(firstname, lastname, nhsno, postcode, dob);
                    _lvm.searchTerms = "Firstname:" + firstname + ",Lastname:" + lastname + ",NHSNo:" + nhsno + ",Postcode:" + postcode;
                    if (dob != null)
                    {
                        _lvm.searchTerms = _lvm.searchTerms + dob.Value.ToString("yyyy-MM-dd");
                    }
                }

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "LabPatientSearch" });
            }
        }

        public async Task<IActionResult> LabReports(int intID)
        {
            try
            {
                string staffCode = await _staff.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - LabReports", "PatientID=" + intID.ToString(), _ip.GetIPAddress());

                _lvm.patient = await _labData.GetPatientDetails(intID);
                _lvm.cytoReportsList = await _labData.GetCytoReportsList(intID);
                //_lvm.dnaReportsList = _labData.GetDNAReportsList(intID);
                _lvm.labDNALabDataList = await _labData.GetDNALabDataList(intID);

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "LabReports" });
            }
        }

        public async Task<IActionResult> SampleDetails(string labno)
        {
            try
            {
                string staffCode = await _staff.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - LabReports", "LabNo=" + labno, _ip.GetIPAddress());

                _lvm.cytoReport = await _labData.GetCytoReport(labno);
                _lvm.dnaReport = await _labData.GetDNAReport(labno);

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "SampleDetails" });
            }
        }

        public async Task<IActionResult> DNALabReport(string labno, string indication, string reason)
        {
            try
            {
                string staffCode = await _staff.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - LabReports", "LabNo=" + labno, _ip.GetIPAddress());

                _lvm.dnaReportDetails = await _labData.GetDNAReportDetails(labno, indication, reason);
                _lvm.dnaReport = await _labData.GetDNAReport(labno);

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DNAReport" });
            }
        }

        public async Task<IActionResult> CytoLabReport(string labno)
        {
            try
            {
                string staffCode = await _staff.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - LabReports", "LabNo=" + labno, _ip.GetIPAddress());

                _lvm.cytoReport = await _labData.GetCytoReport(labno);

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "CytoReport" });
            }
        }
    }
}
