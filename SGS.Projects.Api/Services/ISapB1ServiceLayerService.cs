using SGS.Projects.Api.Models;

namespace SGS.Projects.Api.Services
{
    public interface ISapB1ServiceLayerService : IDisposable
    {
        Task<string> GetSessionIdAsync();
        Task<List<Timesheet>> GetTimesheetsAsync();
        Task<Timesheet?> GetTimesheetAsync(string code);
        Task<Timesheet> CreateTimesheetAsync(TimesheetCreateRequest request);
        Task<Timesheet> UpdateTimesheetAsync(TimesheetUpdateRequest request);
        Task<bool> DeleteTimesheetAsync(string code);
    }
}
