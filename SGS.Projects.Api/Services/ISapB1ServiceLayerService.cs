using SGS.Projects.Api.Models;

namespace SGS.Projects.Api.Services
{
    public interface ISapB1ServiceLayerService
    {
        Task<Timesheet> CreateTimesheetAsync(TimesheetCreateRequest request);
        Task<Timesheet> UpdateTimesheetAsync(TimesheetUpdateRequest request);
        Task<bool> DeleteTimesheetAsync(int docEntry);
        Task<string> GetSessionIdAsync();
    }
}
