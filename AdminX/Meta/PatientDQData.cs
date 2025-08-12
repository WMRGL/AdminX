using AdminX.Models;
using AdminX.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IPatientDQData
    {
        Task<List<MasterPatientTable>> GetPatientsCGUDBNotInEPICAsync(DateTime startDate, DateTime endDate);
        Task<List<EpicPatient>> GetPatientsEPICNotInCGUDBAsync(DateTime startDate, DateTime endDate);

    }

    public class PatientDQData : IPatientDQData
    {
        private readonly DQContext _dQContext;
        public PatientDQData(DQContext dQContext)
        {
            _dQContext = dQContext;
        }
        public async Task<List<MasterPatientTable>> GetPatientsCGUDBNotInEPICAsync(DateTime startDate, DateTime endDate)
        {
            var query = from app in _dQContext.CgudbAppointmentsDetails
                        join pat in _dQContext.MasterPatientTable on app.MPI equals pat.MPI
                        where app.type.Contains("ref") && app.complete == "active" && app.LogicalDelete == 0
                        && app.REFERRAL_DATE >= startDate && app.REFERRAL_DATE < endDate
                        && app.ClockStartDate != null && app.ClockStopDate == null
                        && !(
                            from epicPat in _dQContext.EpicPatients
                            join EpicReferrals in _dQContext.EpicReferrals on epicPat.localmrn equals EpicReferrals.localmrn
                            where EpicReferrals.ReferredToSpecialty == "Clinical Genetics"
                            && EpicReferrals.ReferralReceivedDate >= startDate && EpicReferrals.ReferralReceivedDate < endDate
                            && EF.Functions.Collate(epicPat.NHSNo.Trim(), "SQL_Latin1_General_CP1_CI_AS") == pat.SOCIAL_SECURITY.Trim()
                            select epicPat
                        ).Any()
                        orderby pat.FIRSTNAME
                        select new MasterPatientTable
                        {
                            MPI = pat.MPI,
                            FIRSTNAME = pat.FIRSTNAME,
                            LASTNAME = pat.LASTNAME,
                            SOCIAL_SECURITY = pat.SOCIAL_SECURITY,
                            CGU_No = pat.CGU_No,
                            DOB = pat.DOB,
                            REFERRAL_DATE = (DateTime)app.REFERRAL_DATE,
                            RefID = app.RefID,
                            type = app.type
                        };
            return await query.ToListAsync();
        }
        public async Task<List<EpicPatient>> GetPatientsEPICNotInCGUDBAsync(DateTime startDate, DateTime endDate)
        {
            var query = from epicpat in _dQContext.EpicPatients
                        join EpicReferrals in _dQContext.EpicReferrals on epicpat.localmrn equals EpicReferrals.localmrn
                        where EpicReferrals.ReferredToSpecialty == "Clinical Genetics"
                         && EpicReferrals.ReferralReceivedDate >= startDate && EpicReferrals.ReferralReceivedDate < endDate
                         && !(
                            from app in _dQContext.CgudbAppointmentsDetails
                            join pat in _dQContext.MasterPatientTable on app.MPI equals pat.MPI
                            where EF.Functions.Collate(epicpat.NHSNo.Trim(), "SQL_Latin1_General_CP1_CI_AS") == pat.SOCIAL_SECURITY.Trim()
                            && app.type.Contains("ref") && app.complete == "active" && app.LogicalDelete == 0
                            && app.REFERRAL_DATE >= startDate && app.REFERRAL_DATE < endDate
                            && app.ClockStartDate != null && app.ClockStopDate == null
                            select pat).Any()
                        orderby epicpat.Forename
                        select new EpicPatient
                        {
                            localmrn = epicpat.localmrn,
                            NHSNo = epicpat.NHSNo,
                            Forename = epicpat.Forename,
                            Surname = epicpat.Surname,
                            DOB = epicpat.DOB,
                            ReferralID = EpicReferrals.ReferralID,
                            ReferralReceivedDate = EpicReferrals.ReferralReceivedDate,
                            ReferredTo = EpicReferrals.ReferredTo,
                            ReferredBy = EpicReferrals.ReferredBy
                        };
            return await query.ToListAsync();
        }
    }
    
}
