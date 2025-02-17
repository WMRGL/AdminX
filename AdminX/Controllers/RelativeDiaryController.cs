using AdminX.Meta;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class RelativeDiaryController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;
        private readonly IRelativeDiaryData _relDiaryData;
        private readonly IRelativeData _relData;
        private readonly IPatientData _patientData;
        private readonly ICRUD _crud;
        private readonly RelativeDiaryVM _rdvm;
        public RelativeDiaryController(ClinicalContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _relDiaryData = new RelativeDiaryData(_context);
            _relData = new RelativeData(_context);
            _patientData = new PatientData(_context);
            _crud = new CRUD(_config);
            _rdvm = new RelativeDiaryVM();
        }
        public IActionResult Index(int relID)
        {
            _rdvm.relative = _relData.GetRelativeDetails(relID);
            _rdvm.relativeDiaryList = _relDiaryData.GetRelativeDiaryList(relID);
            _rdvm.patient = _patientData.GetPatientDetailsByWMFACSID(_rdvm.relative.WMFACSID);

            return View(_rdvm);
        }
    }
}
