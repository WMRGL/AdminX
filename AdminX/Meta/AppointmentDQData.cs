using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;
namespace AdminX.Meta
{
    public interface IAppointmentDQData
    {
        Task<List<CgudbNotInEpic>> GetAppointmentsNotInEpicAsync(DateTime startDate, DateTime endDate);
        Task<List<EpicNotInCgudb>> GetAppointmentsNotInCgudbAsync(DateTime startDate, DateTime endDate);
    }
    
    public class AppointmentDQData : IAppointmentDQData
    {
        private readonly DQContext _dQContext;
        public AppointmentDQData(DQContext dQContext)
        {
            _dQContext = dQContext;
        }

        public async Task<List<CgudbNotInEpic>> GetAppointmentsNotInEpicAsync(DateTime startDate, DateTime endDate)
        {
            var query = from app in _dQContext.CgudbAppointmentsDetails
                        join pat in _dQContext.MasterPatientTable on app.MPI equals pat.MPI
                        join staff in _dQContext.Staff on app.STAFF_CODE_1 equals staff.STAFF_CODE
                        join clinic_location in _dQContext.ClinFacilities on app.FACILITY equals clinic_location.FACILITY
                        where (app.type.Contains("appt") || app.type == "MDT consultation" || app.type == "MDC Clinic" || app.type == "Tel. Consultation")
                              && app.BOOKED_DATE >= startDate && app.BOOKED_DATE < endDate
                              && (app.COUNSELED == null || !app.COUNSELED.StartsWith("Cancelled"))
                              && !(
                          from epicAppt in _dQContext.EpicAppointmentDetails
                          join epicPat in _dQContext.EpicPatients on epicAppt.localmrn equals epicPat.localmrn

                          where
                              epicAppt.SpecialtyName == "Clinical Genetics"
                              && EF.Functions.Collate(epicPat.NHSNo.Trim(), "SQL_Latin1_General_CP1_CI_AS") == pat.SOCIAL_SECURITY.Trim()
                              && epicAppt.AppointmentDate >= startDate 
                              && epicAppt.AppointmentDate < endDate
                              && epicAppt.AppointmentOutcome != "Cancelled"

                          select epicAppt
                      ).Any()
                        orderby pat.FIRSTNAME
                        select new CgudbNotInEpic
                        {
                            FirstName = pat.FIRSTNAME,
                            LastName = pat.LASTNAME,
                            SocialSecurity = pat.SOCIAL_SECURITY,
                            BookedDate = app.BOOKED_DATE,
                            BookedTime = app.BOOKED_TIME,  
                            Type = app.type,
                            Location = clinic_location.LOCATION,
                            CliniciansName = staff.NAME,
                            Attendance = app.COUNSELED,
                            ReferralDate = app.REFERRAL_DATE,
                            EstDurationMins = app.EST_DURATION_MINS,
                            Facility = app.FACILITY
                        };
            Console.WriteLine(query);
            return await query.ToListAsync();
        }

        public async Task<List<EpicNotInCgudb>> GetAppointmentsNotInCgudbAsync(DateTime startDate, DateTime endDate)
        {
            var query = from outpat in _dQContext.EpicAppointmentDetails
                        join epicpat in _dQContext.EpicPatients on outpat.localmrn equals epicpat.localmrn
                        where outpat.SpecialtyName == "Clinical Genetics"
                              && outpat.AppointmentDate >= startDate && outpat.AppointmentDate < endDate
                              && !(
                              from app in _dQContext.CgudbAppointmentsDetails
                              join pat in _dQContext.MasterPatientTable on app.MPI equals pat.MPI
                              where

                              EF.Functions.Collate(pat.SOCIAL_SECURITY, "SQL_Latin1_General_CP1_CI_AS") == epicpat.NHSNo.Trim()
                                                   && app.BOOKED_DATE >= startDate && app.BOOKED_DATE < endDate
                                                   && (app.type.Contains("appt") || app.type == "MDT consultation" || app.type == "MDC Clinic" || app.type == "Tel. Consultation")
                                                   && (app.COUNSELED == null || !app.COUNSELED.StartsWith("Cancelled"))
                                                   select app
                                                   ).Any()
                        orderby epicpat.Forename
                        select new EpicNotInCgudb
                        {
                            Forename = epicpat.Forename,
                            Surname = epicpat.Surname,
                            NHSNo = epicpat.NHSNo,
                            AppointmentDate = outpat.AppointmentDate,
                            AppointmentTime = outpat.AppointmentTime,
                            AppointmentLocation = outpat.AppointmentLocation,
                            ClinicCode = outpat.ClinicCode,
                            ClinicName = outpat.ClinicName,
                            ClinicleadClinicianType = outpat.ClinicleadClinicianType,
                            AppointmentAttendance = outpat.AppointmentAttendance,
                            AppointmentOutcome = outpat.AppointmentOutcome,
                            ProviderName = outpat.ProviderName,
                            specialtyname = outpat.SpecialtyName,
                            ConsultationMechanism = outpat.ConsultationMechanism
                        };
            Console.WriteLine(query);
            return await query.ToListAsync();
        }

    }
}
