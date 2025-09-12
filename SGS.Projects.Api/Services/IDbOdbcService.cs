using SGS.Projects.Api.Models;

namespace SGS.Projects.Api.Services
{
    public interface IDbOdbcService
    {
        Task<IEnumerable<Timesheet>> GetTimesheetsAsync();
        Task<Timesheet?> GetTimesheetByIdAsync(int docEntry);
        Task<IEnumerable<Timesheet>> GetTimesheetsByEmployeeAsync(string employeeId);
        Task<IEnumerable<Timesheet>> GetTimesheetsByProjectAsync(string projectId);
        Task<IEnumerable<Timesheet>> GetTimesheetsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Timesheet>> GetTimesheetsByEmployeeAndDateRangeAsync(string employeeId, DateTime startDate, DateTime endDate);
        Task<string> GetNextTimesheetCodeAsync();

        // Lookups
        Task<IEnumerable<CustomerSummary>> GetCustomersAsync();
        Task<IEnumerable<ContactSummary>> GetContactsByCustomerAsync(string cardCode);
        Task<IEnumerable<ProjectSummary>> GetProjectsAsync();
        Task<IEnumerable<ActivitySummary>> GetActivitiesByProjectAsync(string projectCode);
        Task<IEnumerable<ProjectSummary>> GetProjectsByCustomerAsync(string cardCode);
        Task<IEnumerable<ResourceSummary>> GetResourcesAsync();

        // Aggregations
        Task<ActivityTimeTotal?> GetActivityTimeTotAsync(string projectId, string activityId);
    }
}
