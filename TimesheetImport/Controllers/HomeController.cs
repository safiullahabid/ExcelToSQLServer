using Microsoft.AspNetCore.Mvc;
using TimesheetImport.Data;
using System.Diagnostics;
using System.Data;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using TimesheetImport.Models;
using TimesheetImport.ViewModels;

namespace TimesheetImport.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            /*if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please upload a file.");
                return View();
            }*/


            if (file != null && file.Length > 0 && (file.ContentType == "application/vnd.ms-excel" || file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
            {
                //TimeSheet Logs Creation
                var timesheetLog = new TimesheetLog
                {
                    FileName = file.FileName
                };
                _context.TimesheetLogs.Add(timesheetLog);
                await _context.SaveChangesAsync();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    var dataSet = ExcelHelper.ReadExcelFile(stream);
                    var tables = dataSet.Tables;

                    //string dropDownValue = GetDropdownValue(file);
                    string dropDownValue = "";



                    var dataTable = dataSet.Tables[0];
                    Guid empGuid;
                    var employeeId = Convert.ToInt32(dataTable.Rows[5][2]);
                    //Employee Existence Check
                    //var existingEmployee = await _context.EmployeeDetails.FirstOrDefaultAsync(e => e.EmpId == employeeId);
                    var existingEmployee = await _context.EmployeeDetails.Include(e => e.DailyRecords).FirstOrDefaultAsync(e => e.EmpId == employeeId);
                    if (existingEmployee == null)
                    {
                        // Read employee info
                        empGuid = Guid.NewGuid();
                        var employee = new EmployeeDetail
                        {
                            Id = empGuid,
                            EmpId = employeeId,
                            Title = dropDownValue,
                            EmployeeName = dataTable.Rows[4][1].ToString(),
                            Supervisor = dataTable.Rows[5][8].ToString(),
                            TimeKeeper = dataTable.Rows[6][8].ToString(),
                            Department = dataTable.Rows[7][2].ToString(),
                            PhoneNo = dataTable.Rows[6][1].ToString()
                        };
                        _context.EmployeeDetails.Add(employee);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        empGuid = existingEmployee.Id;
                    }


                    try
                    {
                        var dailyRecords = new List<DailyRecord>();
                        for (int row = 15; row <= 33; row++)
                        {
                            if (row < 22 || row > 26)
                            {
                                if (!DateTime.TryParse(dataTable.Rows[row][0].ToString(), out DateTime date))
                                {
                                    continue;
                                }
                                var record = new DailyRecord
                                {
                                    Id = Guid.NewGuid(),
                                    EmployeeDetailId = empGuid,
                                    DayDate = DateTime.Parse(dataTable.Rows[row][0].ToString()),
                                    InTime = string.IsNullOrWhiteSpace(dataTable.Rows[row][2].ToString()) ? (TimeSpan?)null : DateTime.Parse(dataTable.Rows[row][2].ToString()).TimeOfDay,
                                    LunchOut = string.IsNullOrWhiteSpace(dataTable.Rows[row][3].ToString()) ? (TimeSpan?)null : DateTime.Parse(dataTable.Rows[row][3].ToString()).TimeOfDay,
                                    LunchIn = string.IsNullOrWhiteSpace(dataTable.Rows[row][4].ToString()) ? (TimeSpan?)null : DateTime.Parse(dataTable.Rows[row][4].ToString()).TimeOfDay,
                                    OutTime = string.IsNullOrWhiteSpace(dataTable.Rows[row][5].ToString()) ? (TimeSpan?)null : DateTime.Parse(dataTable.Rows[row][5].ToString()).TimeOfDay,
                                    HoursWorked = string.IsNullOrWhiteSpace(dataTable.Rows[row][6].ToString()) ? (TimeSpan?)null : DateTime.Parse(dataTable.Rows[row][6].ToString()).TimeOfDay,
                                    SickLeave = string.IsNullOrWhiteSpace(dataTable.Rows[row][7].ToString()) ? 0 : double.Parse(dataTable.Rows[row][7].ToString()),
                                    AnnualLeave = string.IsNullOrWhiteSpace(dataTable.Rows[row][8].ToString()) ? 0 : double.Parse(dataTable.Rows[row][8].ToString()),
                                    UHLeave = string.IsNullOrWhiteSpace(dataTable.Rows[row][9].ToString()) ? 0 : double.Parse(dataTable.Rows[row][9].ToString()),
                                    OtherLeave = string.IsNullOrWhiteSpace(dataTable.Rows[row][10].ToString()) ? 0 : double.Parse(dataTable.Rows[row][10].ToString()),
                                    OtherLeaveHR = string.IsNullOrWhiteSpace(dataTable.Rows[row][11].ToString()) ? 0 : double.Parse(dataTable.Rows[row][11].ToString()),
                                    CompTime = string.IsNullOrWhiteSpace(dataTable.Rows[row][12].ToString()) ? 0 : double.Parse(dataTable.Rows[row][12].ToString()),
                                    OverTime = string.IsNullOrWhiteSpace(dataTable.Rows[row][13].ToString()) ? 0 : double.Parse(dataTable.Rows[row][13].ToString())
                                };

                                dailyRecords.Add(record);

                            }
                        }
                        //********************************

                        var recordDates = dailyRecords.Select(dr => dr.DayDate).ToList();

                        // Fetch all existing records for those dates
                        var existingRecords = await _context.DailyRecords
                            .Where(dr => dr.EmployeeDetailId == existingEmployee.Id && recordDates.Contains(dr.DayDate))
                            .ToListAsync();

                        // Dictionary to quickly find existing records by date
                        var existingRecordsDict = existingRecords.ToDictionary(dr => dr.DayDate, dr => dr);


                        //********************************

                        //foreach (var item in dailyRecords)
                        //{

                        //    if (existingEmployee != null && existingEmployee.DailyRecords.OrderBy(x => x.DayDate).Any(x => x.DayDate == item.DayDate))
                        //    {
                        //        _context.DailyRecords.Update(item);
                        //    }
                        //    /*if (existingRecordsDict.TryGetValue(item.DayDate, out var existingRecord))
                        //    {
                        //        // Update the existing record with the new values from item
                        //        _context.Entry(existingRecord).CurrentValues.SetValues(item);
                        //    }*/
                        //    else
                        //    {
                        //        _context.DailyRecords.Add(item);
                        //    }
                        //}

                        var dailyRecordsVM = new DailyRecordsVM
                        {
                            DailyRecords = existingRecords
                        };

                        if (existingRecords.Count() > 0)
                        {
                            if (ModelState.IsValid)
                            {
                                foreach (var item in dailyRecordsVM.DailyRecords)
                                {
                                    _context.Update(item);
                                }
                                await _context.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            _context.DailyRecords.AddRange(dailyRecords);
                        }
                        await _context.SaveChangesAsync();

                        return Json(new { success = true, message = "File(s) upload Complete." });
                    }
                    catch (Exception ex)
                    {
                        // On error, return a JSON response with error details
                        return Json(new { success = false, message = "Error uploading file(s): " + ex.Message });
                    }
                    //return RedirectToAction("Index");
                }
            }
            else
            {
                return Json(new { success = false, message = "Please provide only MS Excel file." });
            }

            //return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}

public static class ExcelHelper
{
    public static DataSet ReadExcelFile(Stream stream)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using (var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
        {
            // Use the AsDataSet method to read the file content as a DataSet
            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = false // Use the first row as column names
                }
            });

            return result;
        }
    }
}