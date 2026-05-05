using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
	public interface IEpicApptReferenceDataAsync
	{
		public Task<EpicApptReference> GetEpicAppointment(int refID);
		public Task<List<EpicApptReference>> GetEpicApptsList(int mpi);
	}

	public class EpicApptReferenceDataAsync : IEpicApptReferenceDataAsync
	{
		private readonly AdminContext _adminContext;

		public EpicApptReferenceDataAsync(AdminContext adminContext)
		{
			_adminContext = adminContext;
		}

		public async Task<EpicApptReference> GetEpicAppointment(int refID)
		{
			EpicApptReference appt = await _adminContext.EpicApptReference.FirstOrDefaultAsync(r => r.RefID == refID);

			return appt;
		}

		public async Task<List<EpicApptReference>> GetEpicApptsList(int mpi)
		{
			IQueryable<EpicApptReference> eappts = _adminContext.EpicApptReference.Where(r => r.MPI == mpi);

			return await eappts.ToListAsync();
		}
	}
}
