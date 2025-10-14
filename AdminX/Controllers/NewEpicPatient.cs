﻿using AdminX.Data;
using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class NewEpicPatient : Controller
    {
        ClinicalContext _context;
        AdminContext _adminContext;
        IPatientData _patientData;
        IPedigreeData _pedigreeData;
        INewPatientSearchData _searchData;
        IPatientSearchData _patientSearchData;
        IStaffUserData _staffUserData;
        IConfiguration _configuration;
        ICRUD _crud;
        NewEpicPatientVM _epvm;

        public NewEpicPatient(ClinicalContext context, AdminContext adminContext, IConfiguration configuration)
        {
            _context = context;
            _adminContext = adminContext;
            _patientData = new PatientData(_context);
            _searchData = new NewPatientSearchData(_adminContext);
            _epvm = new NewEpicPatientVM();
            _configuration = configuration;
            _crud = new CRUD(_configuration);
            _staffUserData = new StaffUserData(_context);
            _pedigreeData = new PedigreeData(_context);
            _patientSearchData = new PatientSearchData(_context);
        }

        public IActionResult Index()
        {
            _epvm.patientList = _patientData.GetPatientsWithoutCGUNumbers();

            return View(_epvm);
        }

        [HttpGet]
        public IActionResult EpicPatientSearch(int id)
        {
            _epvm.patient = _patientData.GetPatientDetails(id);            
            string staffCode = _staffUserData.GetStaffCode(User.Identity.Name);

            _crud.NewPatientSearch(_epvm.patient.FIRSTNAME, _epvm.patient.LASTNAME, _epvm.patient.DOB.GetValueOrDefault(), _epvm.patient.POSTCODE, _epvm.patient.SOCIAL_SECURITY, staffCode);

            int searchID = _searchData.GetPatientSearchID(staffCode);

            List<PatientSearchResults> searchResults = _searchData.GetPatientSearchResults(searchID);

            _epvm.patientSearchResultsList = searchResults.Where(r => r.ResultSource == "Patient").Where(p => p.MPI != id).ToList();
            _epvm.relativeSearchResultsList = searchResults.Where(r => r.ResultSource == "Relative").ToList();
            _epvm.pedigreeSearchResultsList = searchResults.Where(r => r.ResultSource == "Pedigree").ToList();

            return View(_epvm);
        }

        
        public IActionResult AssignCGUNumber(int id, string? fileNumber)
        {
            string cguNumber = "";

            if (fileNumber == null || fileNumber == "")
            {
                cguNumber = _pedigreeData.GetNextPedigreeNumber() + ".0";
            }
            else
            {
                List<Patient> patList = _patientSearchData.GetPatientsListByCGUNo(fileNumber); //get the next CGU point number
                int patientNumber = patList.Count();

                if (_patientData.GetPatientDetailsByCGUNo(fileNumber + "." + patientNumber.ToString()) == null)
                {
                    cguNumber = fileNumber + "." + patientNumber.ToString();
                }
            }

            int success = _crud.PatientAssignCGUNumber(id, cguNumber, User.Identity.Name);

            return RedirectToAction("PatientDetails", "Patient", new { id = id });
        }
    }
}
