using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Data;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using ClinicalXPDataConnections.Meta;
using AdminX.Meta;
using ClinicalXPDataConnections.Models;
using AdminX.Models;
using AdminX.Data;

namespace AdminX.Controllers
{
    public class ClinicController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly AdminContext _adminContext;
        private readonly ClinicVM _cvm;
        private readonly IConfiguration _config;
        private readonly IPatientData _patientData;
        private readonly IReferralData _referralData;
        private readonly IActivityData _activityData;
        private readonly IStaffUserData _staffUser;
        private readonly IClinicData _clinicData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        private readonly IOutcomeData _outcomeData;
        private readonly IClinicVenueData _venueData;
        private readonly IActivityTypeData _activityTypeData;

        public ClinicController(ClinicalContext context, AdminContext adminContext, IConfiguration config)
        {
            _clinContext = context;
            _adminContext = adminContext;
            _config = config;
            _cvm = new ClinicVM();
            _patientData = new PatientData(_clinContext);
            _referralData = new ReferralData(_clinContext);
            _activityData = new ActivityData(_clinContext);
            _staffUser = new StaffUserData(_clinContext);
            _clinicData = new ClinicData(_clinContext);
            _crud = new CRUD(_config);
            _audit = new AuditService(_config);
            _outcomeData = new OutcomeData(_clinContext);
            _activityTypeData = new ActivityTypeData(_clinContext, _adminContext);
            _venueData = new ClinicVenueData(_clinContext);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string? filterClinician)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                else
                {
                    string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                    IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                    _audit.CreateUsageAuditEntry(staffCode, "AdminX - Clinics", "", _ip.GetIPAddress());

                    _cvm.staffMembers = _staffUser.GetClinicalStaffList();

                    if (filterClinician != "" && filterClinician != null)
                    {
                        _cvm.outstandingClinicsList = _clinicData.GetClinicList(filterClinician);
                    }
                    else
                    {
                        _cvm.outstandingClinicsList = _clinicData.GetAllOutstandingClinics();
                    }

                    _cvm.outstandingClinicsList = _cvm.outstandingClinicsList.Where(c => c.BOOKED_DATE <= DateTime.Today).OrderByDescending(c => c.BOOKED_DATE).ThenBy(c => c.BOOKED_TIME).ToList();
                    _cvm.filterClinician = filterClinician; //to allow the HTML to keep selected parameters

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
        public IActionResult GetFilteredClinics(string filterClinician)
        {
            try
            {
                List<Appointment> filteredClinics;

                if (string.IsNullOrEmpty(filterClinician))
                {
                    filteredClinics = _clinicData.GetAllOutstandingClinics();
                }
                else
                {
                    filteredClinics = _clinicData.GetClinicList(filterClinician);
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

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Clinic Details", "RefID=" + id.ToString(), _ip.GetIPAddress());

                _cvm.Clinic = _clinicData.GetClinicDetails(id);
                _cvm.linkedReferral = _referralData.GetReferralDetails(_cvm.Clinic.ReferralRefID);

                if (_cvm.Clinic == null)
                {
                    return RedirectToAction("NotFound", "WIP");
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

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit Clinic", "RefID=" + id.ToString());

                _cvm.staffMembers = _staffUser.GetClinicalStaffList();
                _cvm.activityItems = _activityData.GetClinicDetailsList(id);
                _cvm.activityItem = _activityData.GetActivityDetails(id);
                _cvm.outcomes = _clinicData.GetOutcomesList();
                int mpi = _cvm.activityItem.MPI;
                _cvm.patient = _patientData.GetPatientDetails(mpi);

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
        public async Task<IActionResult> Edit(int refID, string counseled, string seenBy, DateTime arrivalTime, int noSeen, string letterRequired, bool isClockStop, bool? isComplete = false)
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

                //if (ethnicity == null)
                //{
                //    ethnicity = "";
                // }

                int success = _crud.CallStoredProcedure("Appointment", "Update", refID, noSeen, 0, counseled, seenBy,
                    letterRequired, "", User.Identity.Name, arrivalTime, null, isClockStop, isComplete);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }

                if (letterRequired != "No")
                {
                    int success2 = _crud.CallStoredProcedure("Letter", "Create", 0, refID, 0, "", "",
                    "", "", User.Identity.Name);

                    if (success2 == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }
                }

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
            _cvm.patient = _patientData.GetPatientDetails(mpi);
            _cvm.outcomes = _outcomeData.GetOutcomeList();
            _cvm.staffMembers = _staffUser.GetClinicalStaffList();
            _cvm.referralsList = _referralData.GetActiveReferralsListForPatient(mpi);
            _cvm.venueList = _venueData.GetVenueList();
            _cvm.appTypeList = _activityTypeData.GetApptTypes();

            return View(_cvm);
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int mpi, int linkedRefID, DateTime bookedDate, DateTime bookedTime, string appType, string? outcome, string? venue, string? clinician1, 
            string? clinician2, string? clinician3, int? timeSpent, int? noPatientsSeen, string? letterReq, string? counseled, string? callersName, string? callersOrg, string? callersTelNo, 
            string? message, string? urgency, bool? isAddAsNote = false, bool? isClockStop = false)
        {
            int refID = 0;

            if (venue == null) { venue = ""; }
            if (clinician1 == null) { clinician1 = ""; }
            if (noPatientsSeen == null) { noPatientsSeen = 1; }

            DateTime bookedTimeEdited = DateTime.Parse("1900-01-01 " + bookedTime.Hour + ":" + bookedTime.Minute + ":" + bookedTime.Second);

            int success = _crud.CallStoredProcedure("Contact", "Create", mpi, linkedRefID, timeSpent.GetValueOrDefault(), appType, venue, clinician1, "", User.Identity.Name, 
                bookedDate, bookedTimeEdited, isClockStop, false, noPatientsSeen, 0, 0, clinician2, clinician3, letterReq, 0, 0, 0, 0, 0,counseled);
            
            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-create(SQL)" }); }

            List<ActivityItem> appts = _activityData.GetActivityList(mpi).Where(a => a.BOOKED_DATE == bookedDate).OrderByDescending(a => a.RefID).ToList();


            refID = appts.First().RefID;

            if (appType == "Tel. Admin")
            {
                Patient patient = _patientData.GetPatientDetails(mpi);
                string emailSubject = $"{patient.CGU_No} - {patient.FIRSTNAME} {patient.LASTNAME} - {urgency} Telephone Message";

                string emailMessage = $"Caller - {callersName}%0D%0A%0D%0A"  + 
                    $"Organisation - {callersOrg}%0D%0A%0D%0A" + 
                $"Contact Tel No - {callersTelNo}%0D%0A%0D%0A" + 
                message;

                string emailBodyText = "";

                if (isAddAsNote.GetValueOrDefault())
                {
                    emailBodyText = "A copy of this message has already been queued for creation in EDMS%0D%0A%0D%0A";

                    _crud.CallStoredProcedure("ClinicalNote", "Create", refID, 0, 0, "", "", "", emailBodyText, User.Identity.Name);
                }

                emailBodyText = emailBodyText + emailMessage;
                
                return Redirect($"mailto:?subject={emailSubject}&body={emailBodyText}");
            }            
            
            return RedirectToAction("ApptDetails", new { id = refID });
        }
    }
}
