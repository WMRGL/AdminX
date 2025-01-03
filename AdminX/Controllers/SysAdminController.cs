﻿using AdminX.Meta;
using AdminX.ViewModels;
using AspNetCoreGeneratedDocument;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using CPTest.Connections;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Mail;

namespace AdminX.Controllers
{
    public class SysAdminController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffData;
        private readonly IExternalClinicianData _clinicianData;
        private readonly IExternalFacilityData _facilityData;
        private readonly IClinicVenueData _venueData;
        private readonly ITitleData _titleData;
        private readonly SysAdminVM _savm;
        private readonly IAuditService _audit;
        private readonly IConstantsData _constants;
        private readonly ICRUD _crud;
        public SysAdminController(ClinicalContext context, DocumentContext docContext, IConfiguration config)
        {
            _clinContext = context;
            _docContext = docContext;
            _config = config;
            _staffData = new StaffUserData(_clinContext);
            _clinicianData = new ExternalClinicianData(_clinContext);
            _facilityData = new ExternalFacilityData(_clinContext);
            _venueData = new ClinicVenueData(_clinContext);
            _titleData = new TitleData(_clinContext);
            _savm = new SysAdminVM();
            _audit = new AuditService(_config);
            _constants = new ConstantsData(_docContext);
            _crud = new CRUD(_config);
        }

        [HttpGet]
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
                    _savm.staffMember = _staffData.GetStaffMemberDetails(User.Identity.Name);
                    string userStaffCode = _savm.staffMember.STAFF_CODE;
                    if(_constants.GetConstant("DeleteICPBtn", 1).Contains(User.Identity.Name.ToUpper()))
                    {
                        _savm.isEditStaff = true;
                    }                    

                    _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin");

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
                string userStaffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Staff Members");

                _savm.staffMembers = _staffData.GetStaffMemberListAll();
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
                string userStaffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Staff Members");

                _savm.staffMembers = _staffData.GetStaffMemberListAll().Where(s => s.BILL_ID != "Exclude").ToList();
                _savm.teams = new List<string>();

                foreach(var item in _savm.staffMembers)
                {
                    if(!_savm.teams.Contains(item.BILL_ID))
                    {
                        _savm.teams.Add(item.BILL_ID);
                    }
                }

                if (staffCodeSearch != null)
                {
                    _savm.staffMembers = _savm.staffMembers.Where(s => s.STAFF_CODE == staffCodeSearch).ToList();
                }

                if (nameSearch != null)
                {
                    _savm.staffMembers = _savm.staffMembers.Where(s => s.NAME.Contains(nameSearch)).ToList();
                }

                if (teamSearch != null)
                {
                    _savm.staffMembers = _savm.staffMembers.Where(s => s.BILL_ID != null).ToList();
                    _savm.staffMembers = _savm.staffMembers.Where(s => s.BILL_ID.Contains(teamSearch)).ToList();
                }

                if (isOnlyCurrent.GetValueOrDefault())
                {
                    _savm.staffMembers = _savm.staffMembers.Where(s => s.InPost).ToList();
                }

                if (isNotExternal.GetValueOrDefault())
                {
                    _savm.staffMembers = _savm.staffMembers.Where(s => s.BILL_ID != "External").ToList();
                }

                if (isOnlyAdmin.GetValueOrDefault())
                {                    
                    _savm.staffMembers = _savm.staffMembers.Where(s => s.CLINIC_SCHEDULER_GROUPS == "Admin").ToList();
                }

                if (isOnlyClinical.GetValueOrDefault())
                {
                    _savm.staffMembers = _savm.staffMembers.Where(s => s.CLINIC_SCHEDULER_GROUPS != "Admin" && s.CLINIC_SCHEDULER_GROUPS != null).ToList();
                }                

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
                string userStaffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Staff Member Details");

                _savm.staffMember = _staffData.GetStaffMemberDetailsByStaffCode(staffCode);                
                _savm.staffMembers = _staffData.GetStaffMemberListAll();
                _savm.teams = new List<string>();
                _savm.types = new List<string>();
                _savm.titles = _titleData.GetTitlesList();

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

            string userStaffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Staff Member Details");

            int iSuccess = _crud.CallStoredProcedure("StaffMember", "Edit", 0, 0, 0, staffCode, title, firstname, lastname, User.Identity.Name, startDate, endDate,
                isSupervisor.GetValueOrDefault(), isSystemAdministrator.GetValueOrDefault(), 0, 0, 0, role, team, type);

            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }

            return RedirectToAction("StaffMembers", new { message = "Changes saved.", success = true });
        }

        [HttpGet]
        public async Task<IActionResult> AddNewStaffMember()
        {
            try
            {
                string userStaffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Staff Member");

                _savm.staffMembers = _staffData.GetStaffMemberListAll();
                _savm.teams = new List<string>();
                _savm.types = new List<string>();
                _savm.titles = _titleData.GetTitlesList();

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
            string userStaffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
            _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - New Staff Member");

            int iSuccess = _crud.CallStoredProcedure("StaffMember", "Create", 0, 0, 0, loginName, title, firstname, lastname, User.Identity.Name, startDate, null,
                isSupervisor.GetValueOrDefault(), isSystemAdministrator.GetValueOrDefault(), 0, 0, 0, role, team, type, 0, 0, 0, 0, 0, gmcNumber, email);

            if (iSuccess == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }

            string password = _staffData.GetStaffMemberDetails(loginName).PASSWORD;
                       

            return RedirectToAction("StaffMembers", new { message = "New staff member added. Please advise new staff member of password: " + password, success = true });
            
        }

        [HttpGet]
        public async Task<IActionResult> Clinicians()
        {
            try
            {
                string userStaffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Clinicians");
                _savm.clinicians = new List<ExternalClinician>(); //because it can't load that many

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Clinicians" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Clinicians(string? firstNameSearch, string? lastNameSearch, bool? isOnlyCurrent = false, 
            bool? isOnlyGP = false, bool? isOnlyNonGP = false)
        {
            try
            {
                string userStaffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Clinicians");

                _savm.clinicians = new List<ExternalClinician>(); 

                if (firstNameSearch != null || lastNameSearch != null)
                {
                    _savm.clinicians = _clinicianData.GetAllCliniciansList();

                    if (firstNameSearch != null)
                    {
                        _savm.clinicians = _savm.clinicians.Where(s => s.FIRST_NAME != null).ToList();
                        _savm.clinicians = _savm.clinicians.Where(s => s.FIRST_NAME.Contains(firstNameSearch)).ToList();
                    }

                    if (lastNameSearch != null)
                    {
                        _savm.clinicians = _savm.clinicians.Where(s => s.NAME != null).ToList();
                        _savm.clinicians = _savm.clinicians.Where(s => s.NAME.Contains(lastNameSearch)).ToList();
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
        public async Task<IActionResult> Facilities(string? message, bool? success)
        {
            try
            {
                string userStaffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Facilities");

                _savm.facilities = new List<ExternalFacility>(); //because it can't load that many

                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Facilities" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Facilities(string? nameSearch)
        {            
            try
            {
                string userStaffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Facilities");
                _savm.facilities = new List<ExternalFacility>();

                if (nameSearch != null)
                {
                    _savm.facilities = _facilityData.GetFacilityListAll().Where(s => s.NAME != null).ToList();
                    _savm.facilities = _savm.facilities.Where(s => s.NAME.Contains(nameSearch)).ToList();
                }


                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Facilities" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ClinicVenues(string? message, bool? success)
        {
            try
            {
                string userStaffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Clinic Venues");

                _savm.venues = _venueData.GetVenueList();
                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Facilities" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClinicVenues(string? nameSearch)
        {
            try
            {
                string userStaffCode = _staffData.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - SysAdmin - Clinic Venues");

                _savm.venues = _venueData.GetVenueList();
                return View(_savm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Facilities" });
            }
        }


    }
}
