using System.Data.Odbc;
using SGS.Projects.Api.Models;

namespace SGS.Projects.Api.Services
{
    public class HanaOdbcService : IHanaOdbcService
    {
        private readonly string _connectionString;
        private readonly ILogger<HanaOdbcService> _logger;

        public HanaOdbcService(IConfiguration configuration, ILogger<HanaOdbcService> logger)
        {
            _connectionString = configuration.GetConnectionString("SapHana") 
                ?? throw new ArgumentNullException(nameof(configuration), "SapHana connection string not found");
            _logger = logger;
        }

        public async Task<IEnumerable<Timesheet>> GetTimesheetsAsync()
        {
            var timesheets = new List<Timesheet>();
            
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = @"
                    SELECT 
                        DocEntry,
                        DocNum,
                        U_Date as Date,
                        U_EmployeeId as EmployeeId,
                        U_ProjectId as ProjectId,
                        U_ActivityId as ActivityId,
                        U_Hours as Hours,
                        U_Description as Description,
                        U_Status as Status,
                        CreateDate as CreatedDate,
                        CreateTS as CreatedBy,
                        UpdateDate as ModifiedDate,
                        UpdateTS as ModifiedBy
                    FROM [@TIMESHEET]
                    ORDER BY U_Date DESC";
                
                using var command = new OdbcCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        DocNum = reader.GetString(1),
                        Date = reader.GetDateTime(2),
                        EmployeeId = reader.GetString(3),
                        ProjectId = reader.GetString(4),
                        ActivityId = reader.GetString(5),
                        Hours = reader.GetDecimal(6),
                        Description = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Status = reader.IsDBNull(8) ? null : reader.GetString(8),
                        CreatedDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                        CreatedBy = reader.IsDBNull(10) ? null : reader.GetString(10),
                        ModifiedDate = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                        ModifiedBy = reader.IsDBNull(12) ? null : reader.GetString(12)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets from SAP HANA");
                throw;
            }
            
            return timesheets;
        }

        public async Task<Timesheet?> GetTimesheetByIdAsync(int docEntry)
        {
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = @"
                    SELECT 
                        DocEntry,
                        DocNum,
                        U_Date as Date,
                        U_EmployeeId as EmployeeId,
                        U_ProjectId as ProjectId,
                        U_ActivityId as ActivityId,
                        U_Hours as Hours,
                        U_Description as Description,
                        U_Status as Status,
                        CreateDate as CreatedDate,
                        CreateTS as CreatedBy,
                        UpdateDate as ModifiedDate,
                        UpdateTS as ModifiedBy
                    FROM [@TIMESHEET]
                    WHERE DocEntry = ?";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@DocEntry", docEntry);
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        DocNum = reader.GetString(1),
                        Date = reader.GetDateTime(2),
                        EmployeeId = reader.GetString(3),
                        ProjectId = reader.GetString(4),
                        ActivityId = reader.GetString(5),
                        Hours = reader.GetDecimal(6),
                        Description = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Status = reader.IsDBNull(8) ? null : reader.GetString(8),
                        CreatedDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                        CreatedBy = reader.IsDBNull(10) ? null : reader.GetString(10),
                        ModifiedDate = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                        ModifiedBy = reader.IsDBNull(12) ? null : reader.GetString(12)
                    };
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheet with DocEntry {DocEntry} from SAP HANA", docEntry);
                throw;
            }
        }

        public async Task<IEnumerable<Timesheet>> GetTimesheetsByEmployeeAsync(string employeeId)
        {
            var timesheets = new List<Timesheet>();
            
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = @"
                    SELECT 
                        DocEntry,
                        DocNum,
                        U_Date as Date,
                        U_EmployeeId as EmployeeId,
                        U_ProjectId as ProjectId,
                        U_ActivityId as ActivityId,
                        U_Hours as Hours,
                        U_Description as Description,
                        U_Status as Status,
                        CreateDate as CreatedDate,
                        CreateTS as CreatedBy,
                        UpdateDate as ModifiedDate,
                        UpdateTS as ModifiedBy
                    FROM [@TIMESHEET]
                    WHERE U_EmployeeId = ?
                    ORDER BY U_Date DESC";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeId", employeeId);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        DocNum = reader.GetString(1),
                        Date = reader.GetDateTime(2),
                        EmployeeId = reader.GetString(3),
                        ProjectId = reader.GetString(4),
                        ActivityId = reader.GetString(5),
                        Hours = reader.GetDecimal(6),
                        Description = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Status = reader.IsDBNull(8) ? null : reader.GetString(8),
                        CreatedDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                        CreatedBy = reader.IsDBNull(10) ? null : reader.GetString(10),
                        ModifiedDate = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                        ModifiedBy = reader.IsDBNull(12) ? null : reader.GetString(12)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets for employee {EmployeeId} from SAP HANA", employeeId);
                throw;
            }
            
            return timesheets;
        }

        public async Task<IEnumerable<Timesheet>> GetTimesheetsByProjectAsync(string projectId)
        {
            var timesheets = new List<Timesheet>();
            
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = @"
                    SELECT 
                        DocEntry,
                        DocNum,
                        U_Date as Date,
                        U_EmployeeId as EmployeeId,
                        U_ProjectId as ProjectId,
                        U_ActivityId as ActivityId,
                        U_Hours as Hours,
                        U_Description as Description,
                        U_Status as Status,
                        CreateDate as CreatedDate,
                        CreateTS as CreatedBy,
                        UpdateDate as ModifiedDate,
                        UpdateTS as ModifiedBy
                    FROM [@TIMESHEET]
                    WHERE U_ProjectId = ?
                    ORDER BY U_Date DESC";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@ProjectId", projectId);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        DocNum = reader.GetString(1),
                        Date = reader.GetDateTime(2),
                        EmployeeId = reader.GetString(3),
                        ProjectId = reader.GetString(4),
                        ActivityId = reader.GetString(5),
                        Hours = reader.GetDecimal(6),
                        Description = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Status = reader.IsDBNull(8) ? null : reader.GetString(8),
                        CreatedDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                        CreatedBy = reader.IsDBNull(10) ? null : reader.GetString(10),
                        ModifiedDate = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                        ModifiedBy = reader.IsDBNull(12) ? null : reader.GetString(12)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets for project {ProjectId} from SAP HANA", projectId);
                throw;
            }
            
            return timesheets;
        }

        public async Task<IEnumerable<Timesheet>> GetTimesheetsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var timesheets = new List<Timesheet>();
            
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = @"
                    SELECT 
                        DocEntry,
                        DocNum,
                        U_Date as Date,
                        U_EmployeeId as EmployeeId,
                        U_ProjectId as ProjectId,
                        U_ActivityId as ActivityId,
                        U_Hours as Hours,
                        U_Description as Description,
                        U_Status as Status,
                        CreateDate as CreatedDate,
                        CreateTS as CreatedBy,
                        UpdateDate as ModifiedDate,
                        UpdateTS as ModifiedBy
                    FROM [@TIMESHEET]
                    WHERE U_Date BETWEEN ? AND ?
                    ORDER BY U_Date DESC";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        DocNum = reader.GetString(1),
                        Date = reader.GetDateTime(2),
                        EmployeeId = reader.GetString(3),
                        ProjectId = reader.GetString(4),
                        ActivityId = reader.GetString(5),
                        Hours = reader.GetDecimal(6),
                        Description = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Status = reader.IsDBNull(8) ? null : reader.GetString(8),
                        CreatedDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                        CreatedBy = reader.IsDBNull(10) ? null : reader.GetString(10),
                        ModifiedDate = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                        ModifiedBy = reader.IsDBNull(12) ? null : reader.GetString(12)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets for date range {StartDate} to {EndDate} from SAP HANA", startDate, endDate);
                throw;
            }
            
            return timesheets;
        }
    }
}
