using AdminX.Models;
using AdminX.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IPatientDQData
    {
        Task<List<MasterPatientTable>> GetPatientsCGUDBNotInEPICAsync(DateTime startDate, DateTime endDate);
        Task<List<EpicPatient>> GetPatientsEPICNotInCGUDBAsync(DateTime startDate, DateTime endDate);
        Task<List<PatientMismatch>> GetPatientMismatchesAsync(DateTime startDate, DateTime endDate);

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
                            Address = epicpat.Address,
                            Postcode = epicpat.Postcode,
                            GenderIdentity = epicpat.GenderIdentity,
                            Sex = epicpat.Sex,
                            SexAssignedAtBirth = epicpat.SexAssignedAtBirth,
                            Ethnicity = epicpat.Ethnicity,
                            TelephoneNumber = epicpat.TelephoneNumber,
                            CurrentGPPracticeCode = epicpat.CurrentGPPracticeCode,
                            CurrentGPPracticeName = epicpat.CurrentGPPracticeName,
                            ReferralID = EpicReferrals.ReferralID,
                            ReferralReceivedDate = EpicReferrals.ReferralReceivedDate,
                            ReferredTo = EpicReferrals.ReferredTo,
                            ReferredBy = EpicReferrals.ReferredBy
                        };
            return await query.ToListAsync();
        }

        public async Task<List<PatientMismatch>> GetPatientMismatchesAsync(DateTime startDate, DateTime endDate)
        {
            var query =
                from cab_ref in _dQContext.EpicReferrals
                join cab_pat in _dQContext.EpicPatients on cab_ref.localmrn equals cab_pat.localmrn
                join cg_pat in _dQContext.MasterPatientTable on
                    EF.Functions.Collate(cab_pat.NHSNo, "SQL_Latin1_General_CP1_CI_AS") equals cg_pat.SOCIAL_SECURITY
                join cg_ref in _dQContext.CgudbAppointmentsDetails on cg_pat.MPI equals cg_ref.MPI

                where
                    cab_ref.ReferredToSpecialty.Contains("genetics")
                    && cg_ref.type.Contains("ref")
                    && cg_ref.complete == "Active"
                    && cg_ref.LogicalDelete == 0
                    && cg_ref.ClockStartDate != null
                    && cg_ref.ClockStopDate == null
                    && cab_ref.ReferralReceivedDate >= startDate && cab_ref.ReferralReceivedDate < endDate
                    && cg_ref.REFERRAL_DATE >= startDate && cg_ref.REFERRAL_DATE < endDate

                    && (
                        EF.Functions.Collate((cab_pat.NHSNo ?? ""), "SQL_Latin1_General_CP1_CI_AS") != (cg_pat.SOCIAL_SECURITY ?? "")
                        || EF.Functions.Collate((cab_pat.Forename ?? ""), "SQL_Latin1_General_CP1_CI_AS") != (cg_pat.FIRSTNAME ?? "")
                        || EF.Functions.Collate((cab_pat.Surname ?? ""), "SQL_Latin1_General_CP1_CI_AS") != (cg_pat.LASTNAME ?? "")
                        || cab_pat.DOB != cg_pat.DOB
                    )

                select new PatientMismatch
                {
                    LocalMRN = cab_pat.localmrn,
                    Caboodle_ReferralID = cab_ref.ReferralID,
                    Caboodle_NHSNo = cab_pat.NHSNo,
                    Caboodle_Forename = cab_pat.Forename,
                    Caboodle_Surname = cab_pat.Surname,
                    Caboodle_DOB = cab_pat.DOB,
                    CGU_No = cg_pat.CGU_No,
                    CGUDB_RefID = cg_ref.RefID.ToString(),
                    CGUDB_NHSNo = cg_pat.SOCIAL_SECURITY,
                    CGUDB_Forename = cg_pat.FIRSTNAME,
                    CGUDB_Surname = cg_pat.LASTNAME,
                    CGUDB_DOB = cg_pat.DOB,
                };

            return await query.ToListAsync();
        }
    }
    
}
