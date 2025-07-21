using AdminX.Meta;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class PatientMergeController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;
        private readonly IPatientData _patientData;
        private readonly IStaffUserData _staffUser;
        private readonly IAuditService _audit;
        private readonly ICRUD _crud;
        private readonly PatientMergeVM _pvm;
        //private readonly IAlertData _alert;

        public PatientMergeController(ClinicalContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _pvm = new PatientMergeVM();
            _patientData = new PatientData(_context);
            _staffUser = new StaffUserData(_context);
            _audit = new AuditService(_config);
            //_alert = new AlertData(_context);
            _crud = new CRUD(_config);
        }

        [HttpGet]
        public async Task<IActionResult> MergePatient(int mpiFrom, int? mpiTo)
        {
            _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _pvm.staffMember.STAFF_CODE;
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Merge");

            _pvm.patientFrom = _patientData.GetPatientDetails(mpiFrom);
            if (mpiTo != null)
            {
                _pvm.patientTo = _patientData.GetPatientDetails(mpiTo.GetValueOrDefault());
            }

            return View(_pvm);
        }


        [HttpPost]
        public async Task<IActionResult> MergePatient(int mpiFrom, int? mpiTo, string? cguNumberToMerge)
        {
            _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _pvm.staffMember.STAFF_CODE;
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Merge");

            _pvm.patientFrom = _patientData.GetPatientDetails(mpiFrom);

            if (mpiTo != null)
            {
                _pvm.patientTo = _patientData.GetPatientDetails(mpiTo.GetValueOrDefault());

                bool isSuccess = false;
                bool isMatch = false;
                string sMessage = "";

                if (_pvm.patientFrom.FIRSTNAME == _pvm.patientTo.FIRSTNAME && _pvm.patientFrom.LASTNAME == _pvm.patientTo.LASTNAME && _pvm.patientFrom.DOB == _pvm.patientTo.DOB
                    && _pvm.patientFrom.POSTCODE == _pvm.patientTo.POSTCODE && _pvm.patientFrom.SOCIAL_SECURITY == _pvm.patientTo.SOCIAL_SECURITY)
                {
                    /*
                    List<Alert> alertsForPatientFrom = _alert.GetAlertsList(mpiFrom);
                    List<Alert> alertsForPatientTo = _alert.GetAlertsList(mpiTo);

                    int matchingAlertCount = 0;

                    foreach (var alertFrom in alertsForPatientFrom)
                    {
                        foreach(var alertTo in alertsForPatientTo)
                        {
                            if(alertFrom.MPI == alertTo.MPI && alertFrom.Comments == alertTo.Comments && alertFrom.EffectiveFromDate == alertTo.EffectiveFromDate)
                            {
                                matchingAlertCount++;
                            }
                        }
                    }

                    if (matchingAlertCount == alertsForPatientFrom.Count && matchingAlertCount == alertsForPatientTo.Count) 
                    {
                        isMatch = true;
                    }
                    else
                    {
                        sMessage = "Patients' alerts don't match.";
                    }*/
                    if (_pvm.patientFrom.INFECTION_RISK == _pvm.patientTo.INFECTION_RISK && _pvm.patientFrom.ADDITIONAL_NOTES == _pvm.patientTo.ADDITIONAL_NOTES)
                    {
                        isMatch = true;
                    }
                    else
                    {
                        sMessage = "Merge failed: Patients' alert details don't match, please check both and try the merge again.";
                    }

                }
                else
                {
                    sMessage = "Merge failed: Patients' details don't match, please check both and try the merge again.";
                }

                if (isMatch)
                {
                    int success = _crud.MergePatient(mpiFrom, mpiTo.GetValueOrDefault(), staffCode);

                    if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "PatientDetails-PatientMerge(SQL)" }); }

                    sMessage = "Patients have been merged.";
                    isSuccess = true;
                }


                return RedirectToAction("PatientDetails", "Patient", new { id = mpiFrom, message = sMessage, success = isSuccess });
            }
            else
            {
                _pvm.patientTo = _patientData.GetPatientDetailsByCGUNo(cguNumberToMerge);
                return RedirectToAction("MergePatient", "PatientMerge", new { mpiFrom = mpiFrom, mpiTo = _pvm.patientTo.MPI });
            }
        }
    }
}
