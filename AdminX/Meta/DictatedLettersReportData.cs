using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface IDictatedLettersReportData
    {
        public List<DictatedlettersReportClinicians> GetReportClinicians();
        public List<DictatedLettersSecTeamReport> GetReportSecTeams();
        public List<DictatedLettersReport> GetDictatedLettersReport();
    }
    public class DictatedLettersReportData : IDictatedLettersReportData
    {
        
        private readonly AdminContext _adminContext;

        public DictatedLettersReportData(AdminContext adminContext)
        {            
            _adminContext = adminContext;
        }

        public List<DictatedlettersReportClinicians> GetReportClinicians()
        {
            List<DictatedlettersReportClinicians> report = _adminContext.DictatedlettersReportClinicians.OrderBy(r => r.Name).ToList();

            return report;
        }

        public List<DictatedLettersSecTeamReport> GetReportSecTeams()
        {
            List<DictatedLettersSecTeamReport> report = _adminContext.DictatedLettersSecTeamReport.ToList();

            return report;
        }

        public List<DictatedLettersReport> GetDictatedLettersReport()
        {
            List<DictatedLettersReport> report = _adminContext.DictatedLettersReport.OrderBy(r => r.Name).ToList();

            return report;
        }
    }
}