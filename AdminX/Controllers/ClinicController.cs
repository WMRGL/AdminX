using Microsoft.AspNetCore.Mvc;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using ClinicalXPDataConnections.Meta;
using AdminX.Meta;
using ClinicalXPDataConnections.Models;
using AdminX.Models;

namespace AdminX.Controllers
{
    public class ClinicController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly AdminContext _adminContext;
        private readonly ClinicVM _cvm;
        private readonly IConfiguration _config;
        private readonly IPatientDataAsync _patientData;
        private readonly IReferralDataAsync _referralData;
        private readonly IActivityDataAsync _activityData;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IClinicDataAsync _clinicData;
        private readonly ICRUD _crud;
        private readonly IAuditServiceAsync _audit;
        private readonly IOutcomeDataAsync _outcomeData;
        private readonly IClinicVenueDataAsync _venueData;
        private readonly IActivityTypeDataAsync _activityTypeData;        
        private readonly IPAddressFinder _ip;

        public ClinicController(IConfiguration config, IPatientDataAsync patient, IReferralDataAsync referral, IActivityDataAsync activity, IStaffUserDataAsync staffUser, IClinicDataAsync clinic, 
            ICRUD crud, IAuditServiceAsync audit, IOutcomeDataAsync outcome, IActivityTypeDataAsync activityType, IClinicVenueDataAsync clinicVenue)
        {
            //_clinContext = context;
            //_adminContext = adminContext;
            _config = config;
            _cvm = new ClinicVM();
            _patientData = patient;
            _referralData = referral;
            _activityData = activity;
            _staffUser = staffUser;
            _clinicData = clinic;
            _crud = crud;
            _audit = audit;
            _outcomeData = outcome;
            _activityTypeData = activityType;
            _venueData = clinicVenue;
            _ip = new IPAddressFinder(HttpContext);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string? filterClinician, string? message, bool? success)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                else
                {
                    string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                    //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                    _audit.CreateUsageAuditEntry(staffCode, "AdminX - Clinics", "", _ip.GetIPAddress());

                    _cvm.staffMembers = await _staffUser.GetClinicalStaffList();

                    if (filterClinician != "" && filterClinician != null)
                    {
                        var clinicList = await _clinicData.GetClinicList(filterClinician);
                        _cvm.outstandingClinicsList = clinicList.Distinct().ToList();
                    }
                    else
                    {
                        var clinicList = await _clinicData.GetAllOutstandingClinics();
                        _cvm.outstandingClinicsList = clinicList.Distinct().ToList();
                    }

                    _cvm.outstandingClinicsList = _cvm.outstandingClinicsList.Where(c => c.BOOKED_DATE <= DateTime.Today).OrderByDescending(c => c.BOOKED_DATE).ThenBy(c => c.BOOKED_TIME).ToList();
                    _cvm.filterClinician = filterClinician; //to allow the HTML to keep selected parameters

                    if (message != null && message != "")
                    {
                        _cvm.message = message;
                        _cvm.success = success.GetValueOrDefault();
                    }

                    ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                    {
                        new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },

                        new BreadcrumbItem { Text = "Clinic" }
                    };

                    return View(_cvm);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Clinic" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetFilteredClinics(string filterClinician)
        {
            try
            {
                List<Appointment> filteredClinics;

                if (string.IsNullOrEmpty(filterClinician))
                {
                    filteredClinics = await _clinicData.GetAllOutstandingClinics();
                }
                else
                {
                    filteredClinics = await _clinicData.GetClinicList(filterClinician);
                }

                filteredClinics = filteredClinics.Where(c => c.BOOKED_DATE <= DateTime.Today)
                                                 .OrderByDescending(c => c.BOOKED_DATE)
                                                 .ThenBy(c => c.BOOKED_TIME)
                                                 .ToList();

                return PartialView("_ClinicsTablePartial", filteredClinics);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ApptDetails(int id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Clinic Details", "RefID=" + id.ToString(), _ip.GetIPAddress());

                _cvm.Clinic = await _clinicData.GetClinicDetails(id);
                _cvm.linkedReferral = await _referralData.GetReferralDetails(_cvm.Clinic.ReferralRefID);
                _cvm.patient = await _patientData.GetPatientDetails(_cvm.Clinic.MPI);

                if (_cvm.Clinic == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                if(_cvm.linkedReferral == null)
                {
                    return RedirectToAction("Index", "Clinic", new { message = "Appointment ID: " + _cvm.Clinic.RefID + " is not linked to a referral, or the linked referral was deleted", success = false });
                }

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                    new BreadcrumbItem { Text = "Contacts", Controller = "Clinic", Action = "Index"},
                    new BreadcrumbItem { Text = "Details"}
                };
               

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Clinic-AppDetails" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit Clinic", "RefID=" + id.ToString(), _ip.GetIPAddress());

                _cvm.staffMembers = await _staffUser.GetClinicalStaffList();
                _cvm.activityItems = await _activityData.GetClinicDetailsList(id);
                _cvm.activityItem = await _activityData.GetActivityDetails(id);
                _cvm.outcomes = await _clinicData.GetOutcomesList();
                int mpi = _cvm.activityItem.MPI;
                _cvm.patient = await _patientData.GetPatientDetails(mpi);

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                    new BreadcrumbItem { Text = "Contacts", Controller = "Clinic", Action = "Index"},
                    new BreadcrumbItem { Text = "Details", Controller = "Clinic", Action = "ApptDetails", RouteValues = new Dictionary<string, string>
                    {
                        { "id", id.ToString() }
                    }},

                    new BreadcrumbItem { Text = "Clinic" }
                };

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formname = "Clinic-edit" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int refID, string counseled, string seenBy, DateTime arrivalTime, int noSeen, string letterRequired, bool isClockStop,
            string? seenBy2, string? seenBy3, bool? isComplete = false)
        {
            try
            {
                if (refID == null)
                {
                    return NotFound();
                }

                //because it doesn't like passing nulls to SQL
                if (isClockStop == null) { isClockStop = false; }

                if (seenBy == null)
                {
                    seenBy = "";
                }

                if (letterRequired == null)
                {
                    letterRequired = "No";
                }

                int success = _crud.CallStoredProcedure("Appointment", "Update", refID, noSeen, 0, counseled, seenBy,
                    letterRequired, "", User.Identity.Name, arrivalTime, null, isClockStop, isComplete, 0,0,0,seenBy2,seenBy3);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }

                if (letterRequired != "No")
                {
                    int success2 = _crud.CallStoredProcedure("Letter", "Create", 0, refID, 0, "", "",
                    "", "", User.Identity.Name);

                    if (success2 == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }
                }

                TempData["SuccessMessage"] = "Clinic details updated successfully.";

                return RedirectToAction("ApptDetails", new { id = refID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Clinic-edit" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int mpi)
        {
            _cvm.patient = await _patientData.GetPatientDetails(mpi);
            _cvm.outcomes = await _outcomeData.GetOutcomeList();
            _cvm.staffMembers = await _staffUser.GetClinicalStaffList();
            _cvm.referralsList = await _referralData.GetActiveReferralsListForPatient(mpi);
            _cvm.venueList = await _venueData.GetVenueList();
            _cvm.appTypeList = await _activityTypeData.GetApptTypes();

            return View(_cvm);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddNew(int mpi, int linkedRefID, DateTime bookedDate, DateTime bookedTime, string appType, string? outcome, string? venue, string? clinician1, 
            string? clinician2, string? clinician3, int? timeSpent, int? noPatientsSeen, string? letterReq, string? counseled, string? callersName, string? callersOrg, string? callersTelNo, 
            string? message, string? urgency, bool? isAddAsNote = false, bool? isClockStop = false)
        {
            int refID = 0;

            if (venue == null) { venue = ""; }
            if (clinician1 == null) { clinician1 = await _staffUser.GetStaffCode(User.Identity.Name); }
            if (noPatientsSeen == null) { noPatientsSeen = 1; }

            DateTime bookedTimeEdited = DateTime.Parse("1900-01-01 " + bookedTime.Hour + ":" + bookedTime.Minute + ":" + bookedTime.Second);

            int success = _crud.CallStoredProcedure("Contact", "Create", mpi, linkedRefID, timeSpent.GetValueOrDefault(), appType, venue, clinician1, "", User.Identity.Name, 
                bookedDate, bookedTimeEdited, isClockStop, false, noPatientsSeen, 0, 0, clinician2, clinician3, letterReq, 0, 0, 0, 0, 0,counseled);
            
            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-create(SQL)" }); }

            List<ActivityItem> appts = await _activityData.GetActivityList(mpi);
            appts = appts.Where(a => a.BOOKED_DATE == bookedDate).OrderByDescending(a => a.RefID).ToList();

            refID = appts.First().RefID;

            if (appType == "Tel. Admin")
            {
                Patient patient = await _patientData.GetPatientDetails(mpi);
                string emailSubject = $"{patient.CGU_No} - {patient.FIRSTNAME} {patient.LASTNAME} - {urgency} Telephone Message";

                string emailMessage = $"Caller - {callersName}%0D%0A%0D%0A"  + 
                    $"Organisation - {callersOrg}%0D%0A%0D%0A" + 
                $"Contact Tel No - {callersTelNo}%0D%0A%0D%0A" + 
                message;

                string emailBodyText = "";
                bool isHidden = true;

                if (isAddAsNote.GetValueOrDefault())
                {
                    emailBodyText = "A copy of this message has already been queued for creation in EDMS%0D%0A%0D%0A";
                    isHidden = false;
                }

                _crud.CallStoredProcedure("ClinicalNote", "Create", refID, 0, 0, "", "", "", emailBodyText, User.Identity.Name, null, null, isHidden);

                emailBodyText = emailBodyText + emailMessage;

                TempData["SuccessMessage"] = "Created successfully.";

                return Redirect($"mailto:?subject={emailSubject}&body={emailBodyText}");                
            }

            return RedirectToAction("ApptDetails", new { id = refID });
        }
    }
}
