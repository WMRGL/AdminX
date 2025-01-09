using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Models;

namespace AdminX.Meta
{
	public interface IStaffMemberData
	{
		public StaffMember GetStaffDetails(string staff_code);
	}

	public class StaffMemberData: IStaffMemberData

	{
		private readonly ClinicalContext _clinContext;

		public StaffMemberData(ClinicalContext context)
		{
			_clinContext = context;
		}

		public StaffMember GetStaffDetails(string staff_code)
		{
			StaffMember staffMember = _clinContext.StaffMembers.FirstOrDefault(i => i.EMPLOYEE_NUMBER == staff_code); 
			return staffMember;
		}
	}


}
