using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Formats.Asn1;
using System.Security.AccessControl;
using System.Text;
using System.Xml;
using WebApplication1.Data;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeManagementContext _context;

        public EmployeeController(EmployeeManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get employees
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        [HttpGet("/Employees/GetEmployees")]
        public async Task<ActionResult<IEnumerable<EmployeeViewModel>>> GetEmployees(int pageSize, int pageNumber)
        {
            List<EmployeeViewModel> list = new List<EmployeeViewModel>();
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                _context.Database.OpenConnection();
                command.CommandText = "GetEmployees";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("pageSize", pageSize));
                command.Parameters.Add(new SqlParameter("pageNumber", pageNumber));
                var results = await command.ExecuteReaderAsync();
                while (results.Read())
                {
                    EmployeeViewModel e = new EmployeeViewModel() 
                    {
                        EmployeeID = results.GetValue("EmployeeID").ToString(),
                        EmployeeName = results.GetValue("EmployeeName").ToString(),
                        DayOfBirth = Convert.ToDateTime(results.GetValue("DayOfBirth")),
                        YearsOld = Convert.ToInt32(results.GetValue("YearsOld"))
                    };
                    list.Add(e);
                }
                _context.Database.CloseConnection();
            }
            return list;
        }

        #region --- Import Excel ---


        /// <summary>
        /// Import data from file excel (V2)
        /// </summary>
        /// <param name="upload"></param>
        /// <returns></returns>
        [HttpPost("/Employees/ImportByExcel1")]
        public async Task<ActionResult<bool>> ImportByExcel1(IFormFile upload)
        {
            try
            {
                if (upload != null && upload.Length > 0)
                {
                    Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    Stream stream = upload.OpenReadStream();
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        string result = string.Empty;
                        var sub = new StringBuilder();
                        int rows = 0;
                        while (reader.Read())
                        {
                            var sb = new StringBuilder();
                            sb.Append("<Data>");
                            sb.Append(string.Format("<EmployeeName>{0}</EmployeeName>", reader.GetValue(0).ToString()));
                            sb.Append(string.Format("<DayOfBirth>{0}</DayOfBirth>", reader.GetValue(1).ToString()));
                            sb.Append("</Data>");
                            sub.Append(sb.ToString());
                        }
                        using (var command = _context.Database.GetDbConnection().CreateCommand())
                        {
                            _context.Database.OpenConnection();
                            command.CommandText = "Import_data";
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("Data", sub.ToString()));
                            rows = await command.ExecuteNonQueryAsync();
                            _context.Database.CloseConnection();
                        }
                        if (rows > 0) return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Import data from excel file
        /// </summary>
        /// <param name="upload"></param>
        /// <returns></returns>
        [HttpPost("/Employees/ImportByExcel2")]
        public async Task<ActionResult<bool>> ImportByExcel2(IFormFile upload)
        {
            if (upload != null && upload.Length > 0)
            {
                List<Employee> list = new List<Employee>();
                Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                Stream stream = upload.OpenReadStream();
                var key = _context.Keys?.FirstOrDefault(x => x.TableName.Equals("Employees"));
                bool isExistKey = true;
                if (key == null)
                {
                    key = new Key() { KeyName = "NV", TableName = "Employees", LastKey = 1 };
                    isExistKey = false;
                }
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    try
                    {
                        while (reader.Read())
                        {
                            Employee e = new Employee();
                            key.LastKey++;
                            e.EmployeeID = string.Format("{0}_{1}", key.KeyName, key.LastKey);
                            e.EmployeeName = reader.GetValue(0).ToString();
                            e.DayOfBirth = Convert.ToDateTime(reader.GetValue(1));
                            _context.Add(e);
                        }

                        if (isExistKey)
                        {
                            _context.Update(key);
                        }
                        else
                        {
                            _context.Add(key);
                        }

                        if (await _context.SaveChangesAsync() > 0)
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        #endregion --- Import Excel ---
    }
}
