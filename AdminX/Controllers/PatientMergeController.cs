using AdminX.Meta;
using AdminX.ViewModels;
//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class PatientMergeController : Controller
    {
        //private readonly ClinicalContext _context;
        private readonly IConfiguration _config;
        private readonly IPatientDataAsync _patientData;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IAuditServiceAsync _audit;
        private readonly ICRUD _crud;
        private readonly PatientMergeVM _pvm;
        //private readonly IAlertData _alert;

        public PatientMergeController(IConfiguration config, IPatientDataAsync patient, IStaffUserDataAsync staffUser, IAuditServiceAsync audit, ICRUD crud)
        {
            //_context = context;
            _config = config;
            _pvm = new PatientMergeVM();
            _patientData = patient;
            _staffUser = staffUser;
            _audit = audit;
            //_alert = new AlertData(_context);
            _crud = crud;
        }

        [HttpGet]
        public async Task<IActionResult> MergePatient(int mpiFrom, int? mpiTo)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Merge");

                _pvm.patientFrom = await _patientData.GetPatientDetails(mpiFrom);
                if (mpiTo != null)
                {
                    _pvm.patientTo = await _patientData.GetPatientDetails(mpiTo.GetValueOrDefault());
                }

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "MergePatient" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> MergePatient(int mpiFrom, int? mpiTo, string? cguNumberToMerge)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Merge");

                _pvm.patientFrom = await _patientData.GetPatientDetails(mpiFrom);

                if (mpiTo != null)
                {
                    _pvm.patientTo = await _patientData.GetPatientDetails(mpiTo.GetValueOrDefault());

                    bool isSuccess = false;
                    bool isMatch = false;
                    string sMessage = "";

                    if (_pvm.patientFrom.FIRSTNAME == _pvm.patientTo.FIRSTNAME && _pvm.patientFrom.LASTNAME == _pvm.patientTo.LASTNAME && _pvm.patientFrom.DOB == _pvm.patientTo.DOB
                        && _pvm.patientFrom.POSTCODE == _pvm.patientTo.POSTCODE && _pvm.patientFrom.SOCIAL_SECURITY == _pvm.patientTo.SOCIAL_SECURITY)
                    {
                        /* //temporarily disabled - do we want to test for matching Alerts?
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


                    return RedirectToAction("PatientDetails", "Patient", new { id = mpiTo, message = sMessage, success = isSuccess });
                }
                else
                {
                    _pvm.patientTo = await _patientData.GetPatientDetailsByCGUNo(cguNumberToMerge);
                    return RedirectToAction("MergePatient", "PatientMerge", new { mpiFrom = mpiFrom, mpiTo = _pvm.patientTo.MPI });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "MergePatient" });
            }
        }
    }
}
