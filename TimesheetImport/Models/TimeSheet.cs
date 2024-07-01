using System.ComponentModel.DataAnnotations;

namespace TimesheetImport.Models
{
    public class EmployeeDetail
        {
            [Key]
            public Guid Id { get; set; }
            public int EmpId { get; set; }
            public string? EmployeeName { get; set; }
            public string? Title { get; set; }
            public string? Supervisor { get; set; }
            public string? PhoneNo { get; set; }
            public string? TimeKeeper { get; set; }
            public string? Department { get; set; }
            public ICollection<DailyRecord> DailyRecords { get; set; }
        }
        public class DailyRecord
        {
            [Key]
            public Guid Id { get; set; }
            public DateTime DayDate { get; set; }
            public TimeSpan? InTime { get; set; }
            public TimeSpan? LunchOut { get; set; }
            public TimeSpan? LunchIn { get; set; }
            public TimeSpan? OutTime { get; set; }
            public TimeSpan? HoursWorked { get; set; }
            public double SickLeave { get; set; } = 0;
            public double AnnualLeave { get; set; } = 0;
            public double UHLeave { get; set; } = 0;
            public double OtherLeave { get; set; } = 0;
            public double OtherLeaveHR { get; set; } = 0;
            public double CompTime { get; set; } = 0;
            public double OverTime { get; set; } = 0;

            public Guid EmployeeDetailId { get; set; }
        }

        public class TimesheetLog
        {
            [Key]
            public int Id { get; set; }
            public string? FileName { get; set; }
            public DateTime? CreatedAt { get; set; } = DateTime.Now;

        }
    }