using SGS.Projects.Api.Models;

namespace SGS.Projects.Api.Services
{
    public interface IHanaOdbcService
    {
        Task<IEnumerable<Timesheet>> GetTimesheetsAsync();
        Task<Timesheet?> GetTimesheetByIdAsync(int docEntry);
        Task<IEnumerable<Timesheet>> GetTimesheetsByEmployeeAsync(string employeeId);
        Task<IEnumerable<Timesheet>> GetTimesheetsByProjectAsync(string projectId);
        Task<IEnumerable<Timesheet>> GetTimesheetsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
