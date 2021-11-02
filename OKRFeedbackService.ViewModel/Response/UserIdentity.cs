using System.Collections.Generic;

namespace OKRFeedbackService.ViewModel.Response
{
    public class UserIdentity
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long RoleId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmailId { get; set; }
        public bool IsActive { get; set; }
        public string ReportingTo { get; set; }
        public string ImageDetail { get; set; }
        public long OrganisationId { get; set; }
        public List<UserRolePermission> RolePermissions { get; set; }
    }

    public class UserRolePermission
    {
        public long ModuleId { get; set; }
        public string ModuleName { get; set; }
        public List<PermissionDetailModel> Permissions { get; set; }
    }

    public class PermissionDetailModel
    {
        public long PermissionId { get; set; }
        public string PermissionName { get; set; }
        public bool Status { get; set; }
    }
}
