using AdminX.Data;
using AdminX.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IDictatedLettersReportDataAsync
    {
        public Task<List<DictatedlettersReportClinicians>> GetReportClinicians();
        public Task<List<DictatedLettersSecTeamReport>> GetReportSecTeams();
        public Task<List<DictatedLettersReport>> GetDictatedLettersReport();
    }
    public class DictatedLettersReportDataAsync : IDictatedLettersReportDataAsync
    {
        
        private readonly AdminContext _adminContext;

        public DictatedLettersReportDataAsync(AdminContext adminContext)
        {            
            _adminContext = adminContext;
        }

        public async Task<List<DictatedlettersReportClinicians>> GetReportClinicians()
        {
            List<DictatedlettersReportClinicians> report = await _adminContext.DictatedlettersReportClinicians.OrderBy(r => r.Name).ToListAsync();

            return report;
        }

        public async Task<List<DictatedLettersSecTeamReport>> GetReportSecTeams()
        {
            List<DictatedLettersSecTeamReport> report = await _adminContext.DictatedLettersSecTeamReport.ToListAsync();

            return report;
        }

        public async Task<List<DictatedLettersReport>> GetDictatedLettersReport()
        {
            List<DictatedLettersReport> report = await _adminContext.DictatedLettersReport.OrderBy(r => r.Name).ToListAsync();

            return report;
        }
    }
}