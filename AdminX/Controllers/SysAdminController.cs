//using AdminX.Data;
using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace AdminX.Controllers
{
    public class SysAdminController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly DocumentContext _docContext;
        //private readonly AdminContext _adminContext;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffData;
        private readonly IExternalClinicianDataAsync _clinicianData;
        private readonly IExternalFacilityDataAsync _facilityData;
        private readonly IClinicVenueDataAsync _venueData;
        private readonly ICliniciansClinicDataAsync _clinicDetailsData;
        private readonly ITitleDataAsync _titleData;
        private readonly SysAdminVM _savm;
        private readonly IAuditServiceAsync _audit;
        private readonly IConstantsDataAsync _constants;
        private readonly ICRUD _crud;
        private readonly IPAddressFinder _ip;

        public SysAdminController(IConfiguration config, IStaffUserDataAsync staffUser, IExternalClinicianDataAsync extClinician, IExternalFacilityDataAsync extFacility, 
            IClinicVenueDataAsync clinicVenue, ICliniciansClinicDataAsync cliniciansClinic, ITitleDataAsync title, IAuditServiceAsync audit, IConstantsDataAsync constants, ICRUD crud)
        {
            //_clinContext = context;
            //_docContext = docContext;
            //_adminContext = adminContext;
            _config = config;
            _staffData = staffUser;
            _clinicianData = extClinician;
            _facilityData = extFacility;
            _venueData = clinicVenue;
            _clinicDetailsData = cliniciansClinic;
            _titleData = title;
            _savm = new SysAdminVM();            
            _audit = audit;
            _constants = constants;
            _crud = new CRUD(_config);
            _ip = new IPAddressFinder(HttpContext);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            { 
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                else
                {
                    _savm.staffMember = await _staffData.GetStaffMemberDetails(User.Identity.Name);
                    string userStaffCode = _savm.staffMember.STAFF_CODE;

                    string delICPBtn = await _constants.GetConstant("DeleteICPBtn", 1);

                    if (delICPBtn.Contains(User.Identity.Name.ToUpper()))
                    {
                        _savm.isEditStaff = true;
                    }

                    //IPAddressFinder _ip = new IPAddressFinder(HttpContext); //this is necessary to do here, because there's simply no way to have the DLL find it!
                    _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin", "", _ip.GetIPAddress());

                    return View(_savm);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName="SysAdmin" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> StaffMembers(string? message, bool? success)
        {
            try
            {                
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Staff Members", "", _ip.GetIPAddress());

                _savm.staffMembers = await _staffData.GetStaffMemberListAll();
                _savm.teams = new List<string>();

                foreach (var item in _savm.staffMembers)
                {
                    if (!_savm.teams.Contains(item.BILL_ID))
                    {
                        _savm.teams.Add(item.BILL_ID);
                    }
                }

                _savm.message = message;
                _savm.success = success.GetValueOrDefault();

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "StaffMembers" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> StaffMembers(string? staffCodeSearch, string? nameSearch, string? teamSearch, 
            bool? isOnlyCurrent=false, bool? isNotExternal=false, bool? isOnlyAdmin=false, bool? isOnlyClinical=false)
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Staff Members", "", _ip.GetIPAddress());

                var staffMemberList = await _staffData.GetStaffMemberListAll();                
                staffMemberList = staffMemberList.Where(s => s.BILL_ID != "Exclude").ToList();
                IQueryable<StaffMember> staffMembers = staffMemberList.AsQueryable();
               
                _savm.teams = new List<string>();

                foreach(var item in staffMembers)
                {
                    if(!_savm.teams.Contains(item.BILL_ID))
                    {
                        _savm.teams.Add(item.BILL_ID);
                    }
                }

                if (staffCodeSearch != null)
                {
                    staffMembers = staffMembers.Where(s => s.STAFF_CODE == staffCodeSearch);
                }

                if (nameSearch != null)
                {
                    staffMembers = staffMembers.Where(s => s.NAME != null);
                    staffMembers = staffMembers.Where(s => s.NAME.Contains(nameSearch));                    
                }

                if (teamSearch != null)
                {
                    staffMembers = staffMembers.Where(s => s.BILL_ID != null);
                    staffMembers = staffMembers.Where(s => s.BILL_ID.Contains(teamSearch));
                }

                if (isOnlyCurrent.GetValueOrDefault())
                {
                    staffMembers = staffMembers.Where(s => s.InPost);
                }

                if (isNotExternal.GetValueOrDefault())
                {
                    staffMembers = staffMembers.Where(s => s.BILL_ID != "External");
                }

                if (isOnlyAdmin.GetValueOrDefault())
                {                    
                    staffMembers = staffMembers.Where(s => s.CLINIC_SCHEDULER_GROUPS == "Admin");
                }

                if (isOnlyClinical.GetValueOrDefault())
                {
                    staffMembers = staffMembers.Where(s => s.CLINIC_SCHEDULER_GROUPS != "Admin" && s.CLINIC_SCHEDULER_GROUPS != null);
                    Console.WriteLine(staffMembers.Count());
                }

                _savm.staffMembers = staffMembers.ToList();

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "StaffMembers" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> StaffMemberDetails(string staffCode)
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Staff Member Details", "", _ip.GetIPAddress());

                _savm.staffMember = await _staffData.GetStaffMemberDetailsByStaffCode(staffCode);                
                _savm.staffMembers = await _staffData.GetStaffMemberListAll();
                _savm.teams = new List<string>();
                _savm.types = new List<string>();
                _savm.titles = await _titleData.GetTitlesList();

                foreach (var item in _savm.staffMembers)
                {
                    if (!_savm.teams.Contains(item.BILL_ID))
                    {
                        _savm.teams.Add(item.BILL_ID);
                    }
                    if(!_savm.types.Contains(item.CLINIC_SCHEDULER_GROUPS))
                    {
                        _savm.types.Add(item.CLINIC_SCHEDULER_GROUPS);
                    }
                }

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "StaffMembers" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> StaffMemberDetails(string staffCode, string title, string firstname, string lastname, string role, string team, 
            string type, DateTime startDate, DateTime endDate, bool isInPost, string? gmcNumber, bool? isSupervisor=false, bool? isSystemAdministrator=false)
        {

            string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Staff Member Details", "", _ip.GetIPAddress());

            int iSuccess = _crud.SysAdminCRUD("StaffMember", "Edit", 0, 0, 0, staffCode, title, firstname, lastname, User.Identity.Name, startDate, endDate,
                isSupervisor.GetValueOrDefault(), isSystemAdministrator.GetValueOrDefault(), false, 0, 0, 0, role, team, type, gmcNumber);

            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }

            return RedirectToAction("StaffMembers", new { message = "Changes saved.", success = true });
        }

        [HttpGet]
        public async Task<IActionResult> AddNewStaffMember()
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Staff Member", "", _ip.GetIPAddress());

                _savm.staffMembers = await _staffData.GetStaffMemberListAll();
                _savm.teams = new List<string>();
                _savm.types = new List<string>();
                _savm.titles = await _titleData.GetTitlesList();

                foreach (var item in _savm.staffMembers)
                {
                    if (!_savm.teams.Contains(item.BILL_ID))
                    {
                        _savm.teams.Add(item.BILL_ID);
                    }
                    if (!_savm.types.Contains(item.CLINIC_SCHEDULER_GROUPS))
                    {
                        _savm.types.Add(item.CLINIC_SCHEDULER_GROUPS);
                    }
                }

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "StaffMembers" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNewStaffMember(string loginName, string title, string firstname, string lastname, string role, string team,
            string type, DateTime startDate, string email, string? gmcNumber, bool? isSupervisor=false, bool? isSystemAdministrator = false)
        {
            string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Staff Member", "", _ip.GetIPAddress());

            int iSuccess = _crud.SysAdminCRUD("StaffMember", "Create", 0, 0, 0, loginName, title, firstname, lastname, User.Identity.Name, startDate, null,
                isSupervisor.GetValueOrDefault(), isSystemAdministrator.GetValueOrDefault(), false, 0, 0, 0, role, team, type, gmcNumber, email);

            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }

            StaffMember newStaff = await _staffData.GetStaffMemberDetails(loginName);
            string password = newStaff.PASSWORD;                       

            return RedirectToAction("StaffMembers", new { message = "New staff member added. Please advise new staff member of password: " + password, success = true });
            
        }

        [HttpGet]
        public async Task<IActionResult> Clinicians(string? message, bool? success)
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Clinicians", "", _ip.GetIPAddress());
                _savm.clinicians = new List<ExternalClinician>(); //because it can't load that many

                _savm.message = message;
                _savm.success = success.GetValueOrDefault();

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Clinicians" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Clinicians(string? firstNameSearch, string? lastNameSearch, bool? isOnlyCurrent = false, bool? isOnlyGP = false, bool? isOnlyNonGP = false)
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Clinicians", "", _ip.GetIPAddress());

                _savm.clinicians = new List<ExternalClinician>(); 

                if (firstNameSearch != null || lastNameSearch != null)
                {
                    _savm.clinicians = await _clinicianData.GetAllCliniciansList();

                    if (firstNameSearch != null)
                    {
                        _savm.clinicians = _savm.clinicians.Where(s => s.FIRST_NAME != null).ToList();
                        _savm.clinicians = _savm.clinicians.Where(s => s.FIRST_NAME.ToUpper().Contains(firstNameSearch.ToUpper())).ToList();
                    }

                    if (lastNameSearch != null)
                    {
                        _savm.clinicians = _savm.clinicians.Where(s => s.NAME != null).ToList();
                        _savm.clinicians = _savm.clinicians.Where(s => s.NAME.ToUpper().Contains(lastNameSearch.ToUpper())).ToList();
                    }

                    if (isOnlyCurrent.GetValueOrDefault())
                    {
                        _savm.clinicians = _savm.clinicians.Where(s => s.NON_ACTIVE == 0).ToList();
                    }

                    if (isOnlyGP.GetValueOrDefault())
                    {
                        _savm.clinicians = _savm.clinicians.Where(s => s.Is_Gp == -1).ToList();
                    }

                    if (isOnlyNonGP.GetValueOrDefault())
                    {
                        _savm.clinicians = _savm.clinicians.Where(s => s.Is_Gp == 0).ToList();
                    }
                }

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Clinicians" });
            }          
        }

        [HttpGet]
        public async Task<IActionResult> ClinicianDetails(string clinCode)
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Staff Member Details", "", _ip.GetIPAddress());

                _savm.clinician = await _clinicianData.GetClinicianDetails(clinCode);
                _savm.titles = await _titleData.GetTitlesList();
                var fac = await _facilityData.GetFacilityListAll();
                _savm.facilities = fac.Where(f => f.NONACTIVE == 0).ToList();

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ClinicianDetails" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClinicianDetails(string clinCode, string title, string firstName, string lastName, string facility, string speciality, string position, int isGP, int isNonActive)
        {

            string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Staff Member Details");

            int iSuccess = _crud.SysAdminCRUD("Clinician", "Edit", isGP, isNonActive, 0, clinCode, title, firstName, lastName, User.Identity.Name, null, null,
                false, false, false, 0, 0, 0, facility, speciality, position);

            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinician-edit(SQL)" }); }

            return RedirectToAction("Clinicians", new { message = "Changes saved.", success = true });
        }

        [HttpGet]
        public async Task<IActionResult> AddNewClinician()
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Staff Member", "", _ip.GetIPAddress());

                _savm.titles = await _titleData.GetTitlesList();
                var fac = await _facilityData.GetFacilityListAll();
                _savm.facilities = fac.Where(f => f.NONACTIVE == 0).ToList();
                
                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Clinician-Add" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNewClinician(string title, string firstName, string lastName, string facilityCode, string? jobTitle, string speciality, int isGP)
        {
            string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Staff Member", "", _ip.GetIPAddress());

            string clinCode = lastName + firstName.Substring(0, 1);
            
            if(_clinicianData.GetClinicianDetails(clinCode) != null)
            {
                int i = 1;

                CheckCode: //to make sure the code is unique
                if (_clinicianData.GetClinicianDetails(clinCode + i.ToString()) != null)
                {
                    i += 1;
                    goto CheckCode;
                }
                
                clinCode = clinCode + i.ToString();
            }

            int iSuccess = _crud.SysAdminCRUD("Clinician", "Create", isGP, 0, 0, clinCode, title, firstName, lastName, User.Identity.Name, null, null,
                false, false, false, 0, 0, 0, jobTitle, speciality, facilityCode);

            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinician-edit(SQL)" }); }

            return RedirectToAction("Clinicians", new { message = "New clinician added.", success = true });

        }


        [HttpGet]
        public async Task<IActionResult> Facilities(string? message, bool? success)
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Facilities", "", _ip.GetIPAddress());

                _savm.facilities = new List<ExternalFacility>(); //because it can't load that many

                _savm.message = message;
                _savm.success = success.GetValueOrDefault();

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Facilities" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Facilities(string? nameSearch, string? citySearch, string? codeSearch, bool? isOnlyCurrent = false, bool? isOnlyGP = false, bool? isOnlyNonGP = false)
        {            
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Facilities", "", _ip.GetIPAddress());
                _savm.facilities = new List<ExternalFacility>();
                var facList = await _facilityData.GetFacilityListAll();
                IQueryable<ExternalFacility> facs = facList.AsQueryable();

                if (nameSearch != null)
                {                    
                    facs = facs.Where(s => s.NAME != null);
                    facs = facs.Where(s => s.NAME.ToUpper().Contains(nameSearch.ToUpper()));
                }

                if (citySearch != null)
                {                    
                    facs = facs.Where(s => s.CITY != null);
                    facs = facs.Where(s => s.CITY.ToUpper().Contains(citySearch.ToUpper()));
                }

                if (codeSearch != null)
                {                    
                    facs = facs.Where(s => s.MasterFacilityCode.ToUpper().Contains(codeSearch.ToUpper()));
                }

                if (isOnlyCurrent.GetValueOrDefault())
                {
                    facs = facs.Where(s => s.NONACTIVE == 0);
                }

                if (isOnlyGP.GetValueOrDefault())
                {
                    facs = facs.Where(s => s.IS_GP_SURGERY == -1);
                }

                if (isOnlyNonGP.GetValueOrDefault())
                {
                    facs = facs.Where(s => s.IS_GP_SURGERY == 0);
                }

                _savm.facilities = facs.ToList();

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Facilities" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> FacilityDetails(string facCode)
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Facility Details", "", _ip.GetIPAddress());

                _savm.facility = await _facilityData.GetFacilityDetails(facCode);

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Facility Details" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> FacilityDetails(string facCode, string name, string address, string district, string city, string state, string zip, int nonActive, int isGP)
        {

            string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Facility Details", "", _ip.GetIPAddress());

            int iSuccess = _crud.SysAdminCRUD("Facility", "Edit", isGP, nonActive, 0, facCode, name, address, district, User.Identity.Name, null, null,
                false, false, false, 0, 0, 0, city, state, zip);

            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Facility-edit(SQL)" }); }

            return RedirectToAction("Facilities", new { message = "Changes saved.", success = true });
        }

        [HttpGet]
        public async Task<IActionResult> AddNewFacility()
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Facility", "", _ip.GetIPAddress());

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "New Facility" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNewFacility(string facCode, string name, string address, string district, string city, string state, string postCode, int nonActive, int isGP)
        {
            string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Facility", "", _ip.GetIPAddress());

            int iSuccess = _crud.SysAdminCRUD("Facility", "Create", isGP, nonActive, 0, facCode, name, address, district, User.Identity.Name, null, null,
                false, false, false, 0, 0, 0, city, state, postCode);

            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Facility-add(SQL)" }); }

            return RedirectToAction("Facilities", new { message = "New facility added.", success = true });

        }

        [HttpGet]
        public async Task<IActionResult> ClinicVenues(string? message, bool? success)
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Clinic Venues", "", _ip.GetIPAddress());

                _savm.venues = await _venueData.GetVenueList();

                _savm.message = message;
                _savm.success = success.GetValueOrDefault();

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Venues" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClinicVenues(string? codeSearch, string? nameSearch, bool? isOnlyCurrent = false) //I only added that variable because it throws a fit otherwise
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Clinic Venues", "", _ip.GetIPAddress());

                var venues = await _venueData.GetVenueList();

                if (nameSearch != null)
                {
                    venues = venues.Where(v => v.NAME.ToUpper().Contains(nameSearch.ToUpper())).ToList();
                }

                if(isOnlyCurrent.GetValueOrDefault())
                {
                    venues = venues.Where(v => v.NON_ACTIVE == 0).ToList();
                }

                _savm.venues = venues;


                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Venues" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VenueDetails(string clinCode)
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Clinic Venue Details", "", _ip.GetIPAddress());

                _savm.venue = await _venueData.GetVenueDetails(clinCode);

                CliniciansClinics clin = await _clinicDetailsData.GetCliniciansClinic(clinCode);

                if (clin != null)
                {
                    _savm.hasAssociatedClinicDetails = true;
                }

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "VenueDetails" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VenueDetails(string clinCode, string name, string location, string notes, string locationCode, int isNonActive)
        {

            string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Clinic Venue Details", "", _ip.GetIPAddress());

            int iSuccess = _crud.SysAdminCRUD("Venue", "Edit", isNonActive, 0, 0, clinCode, name, location, notes, User.Identity.Name, null, null,
                false, false, false, 0, 0, 0, locationCode, "", "");

            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Venue-add(SQL)" }); }

            return RedirectToAction("ClinicVenues", new { message = "Changes saved.", success = true });
        }

        [HttpGet]
        public async Task<IActionResult> AddNewVenue()
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Clinic Venue", "", _ip.GetIPAddress());

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "New Venue" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNewVenue(string clinCode, string name, string location, string notes, string locationCode)
        {
            string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Clinic Venue", "", _ip.GetIPAddress());

            if(notes == null) { notes = ""; }

            int iSuccess = _crud.SysAdminCRUD("Venue", "Create", 0, 0, 0, clinCode, name, location, notes, User.Identity.Name, null, null,
                false, false, false, 0, 0, 0, locationCode, "", "");

            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Venue-add(SQL)" }); }

            return RedirectToAction("ClinicVenues", new { message = "New clinic venue added.", success = true });

        }

        [HttpGet]
        public async Task<IActionResult> CliniciansClinics(string? message, bool? success)
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Clinic Details", "", _ip.GetIPAddress());

                _savm.cliniciansClinicList = await _clinicDetailsData.GetCliniciansClinicList();

                _savm.message = message;
                _savm.success = success.GetValueOrDefault();

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "CliniciansClinics" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CliniciansClinics(string? siteSearch, string? message, bool? success)
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Clinic Details", "", _ip.GetIPAddress());

                _savm.cliniciansClinicList = await _clinicDetailsData.GetCliniciansClinicList();

                if (siteSearch != null)
                {
                    _savm.cliniciansClinicList = _savm.cliniciansClinicList.Where(cd => cd.ClinicSite != null).ToList();
                    _savm.cliniciansClinicList = _savm.cliniciansClinicList.Where(cd => cd.ClinicSite.ToUpper().Contains(siteSearch.ToUpper())).ToList();
                }

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "CliniciansClinics" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ClinicDetails(string clinCode)
        {
            _savm.cliniciansClinic = await _clinicDetailsData.GetCliniciansClinic(clinCode);
            string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Clinic Details", "", _ip.GetIPAddress());
            
            _savm.venues = await _venueData.GetVenueList();
            _savm.staffMembers = await _staffData.GetStaffMemberListByRole("Admin");

            return View(_savm);
        }

        [HttpPost]
        public async Task<IActionResult> ClinicDetails(string clinCode, string? addressee, string? salutation, string? position, string? preAmble, string? postLude, string? copiesTo, string? site,
            string? telephone, string? address, string? town, string? county, string? postCode, string secretary, bool? callToBook = false, bool? includeSPR = false, bool? showDate = false, int? showLocalRef = 0)
        {
            _savm.cliniciansClinic = await _clinicDetailsData.GetCliniciansClinic(clinCode);
            string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Edit Clinic Details", "", _ip.GetIPAddress());

            int iSuccess = _crud.SysAdminCRUD("ClinicSetup", "Edit", 0, 0, 0, clinCode, addressee, salutation, preAmble, User.Identity.Name, null, null, callToBook, includeSPR,
                showDate, showLocalRef, 0, 0, postLude, position, telephone, address, town, county, postCode, secretary);

            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "ClinicSetup-edit(SQL)" }); }
            
            return RedirectToAction("CliniciansClinics", new { message = "Changes saved.", success = true });
        }

        [HttpGet]
        public async Task<IActionResult> AddNewCliniciansClinic(string? clinCodeToCreate)
        {
            try
            {
                string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Clinic Details", "", _ip.GetIPAddress());

                if(clinCodeToCreate != null) { _savm.clinCodeToCreate = clinCodeToCreate; }

                _savm.venues = await _venueData.GetVenueList();
                _savm.staffMembers = await _staffData.GetStaffMemberListByRole("Admin");

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "New Venue" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNewCliniciansClinic(string clinCode, string? addressee, string? salutation, string? position, string? preAmble, string? postLude, string? copiesTo, string? site, 
            string? telephone, string? address, string? town, string? county, string? postCode, string secretary, bool? callToBook = false, bool? includeSPR = false, bool? showDate = false, bool? showLocalRef = false)
        {
            string userStaffCode = await _staffData.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Clinic Details", "", _ip.GetIPAddress());

            int iShowLocalRef = 0;
            if(showLocalRef.GetValueOrDefault()) { iShowLocalRef = 1; }

            int iSuccess = _crud.SysAdminCRUD("ClinicSetup", "Create", 0,0,0,clinCode, addressee, salutation, preAmble, User.Identity.Name, null,null, callToBook, includeSPR, 
                showDate, iShowLocalRef,0,0,postLude, position, telephone, address, town, county, postCode, secretary);
            
            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "ClinicSetup-add(SQL)" }); }

            return RedirectToAction("CliniciansClinics", new { message = "New clinic details added.", success = true });
        }

        [HttpGet]
        public async Task<IActionResult> SetDutyClinicians(string? message, bool? success=false)
        {
            if(message != null)
            {
                _savm.message = message;
                _savm.success = success.GetValueOrDefault();
            }

            _savm.consultants = await _staffData.GetConsultantsList();
            _savm.gcs = await _staffData.GetGCList();            
            _savm.sprs = await _staffData.GetSpRList();
            _savm.dutyConsultant = _savm.consultants.FirstOrDefault(s => s.isDutyClinician == true);            
            _savm.dutySPR = _savm.sprs.FirstOrDefault(s => s.isDutyClinician == true);

            return View(_savm);
        }

        [HttpPost]
        public async Task<IActionResult> SetDutyClinicians(string? dutyCons = "", string? dutyGC = "", string? dutySPR = "")
        {
            int iSuccess = _crud.CallStoredProcedure("DutyClinician", "Set", 0, 0, 0, dutyCons, dutySPR, "", "", User.Identity.Name, null, null, false, false, 0, 0, 0, "", "", "",
                0, 0, 0, 0, 0, "", "");

            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DutyClinician-set(SQL)" }); }

            return RedirectToAction("SetDutyClinicians", new { message = "Updated.", success = true });
        }
    }
}
