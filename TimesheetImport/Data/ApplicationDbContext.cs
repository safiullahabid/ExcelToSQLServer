
using Microsoft.EntityFrameworkCore;
using TimesheetImport.Models;

namespace TimesheetImport.Data
{
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
            {
            }

            public DbSet<EmployeeDetail> EmployeeDetails { get; set; }
            public DbSet<DailyRecord> DailyRecords { get; set; }
            public DbSet<TimesheetLog> TimesheetLogs { get; set; }
        }
    
}