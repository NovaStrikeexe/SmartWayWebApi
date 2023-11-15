using System.Data;
using Dapper;
using Npgsql;
using SmartWayWebApi.Data.Interfaces;
using SmartWayWebApi.DTO;
using SmartWayWebApi.Models;

namespace SmartWayWebApi.Data.Implementation;

public class EmployeeRepo : IEmployeeRepo
{
    private readonly ILogger<EmployeeRepo> _logger;
    private readonly string _connectionString;

    public EmployeeRepo(ILogger<EmployeeRepo> logger)
    {
        _logger = logger;
        _connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=SmartWayDB;";
    }


    public async Task<int> AddEmployee(Employee employee)
    {
        var employeeId = 0;

        var employeeDto = new EmployeeAddDto()
        {
            Name = employee.Name,
            Surname = employee.Surname,
            Phone = employee.Phone,
            CompanyId = employee.CompanyId,
            PassportType = employee.Passport.Type,
            PassportNumber = employee.Passport.Number,
            DepartmentName = employee.Department.Name,
            DepartmentPhone = employee.Department.Phone
        };
        try
        {
            using IDbConnection connection = new NpgsqlConnection(_connectionString);
            const string query = "INSERT INTO " + "T_Employee " +
                                 "(Name, Surname, Phone, CompanyId, PassportType, " +
                                 "PassportNumber, DepartmentName, DepartmentPhone) " +
                                 "VALUES (@Name, @Surname, @Phone, @CompanyId, @PassportType, " +
                                 "@PassportNumber, @DepartmentName, @DepartmentPhone) RETURNING Id;";

            employeeId = await connection.ExecuteScalarAsync<int>(query, employeeDto);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"{nameof(EmployeeRepo)}.{nameof(AddEmployee)}");
        }
        
        return employeeId;
    }

    public async Task<bool> DeleteEmployee(int id)
    {
        var result = false;
        
        try
        {
            using IDbConnection connection = new NpgsqlConnection(_connectionString);
            const string query = "DELETE FROM T_Employee WHERE Id = @Id";
            
            await connection.ExecuteAsync(query, new { Id = id });
            
            result = true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"{nameof(EmployeeRepo)}.{nameof(DeleteEmployee)}");
        }

        return result;
    }

    public async Task<List<EmployeeWithId>> GetCompanyDepartmentEmployees(string departmentName)
    {
        IEnumerable<Employee>? result = null;
        try
        {
            using IDbConnection connection = new NpgsqlConnection(_connectionString);
            const string query = "SELECT * FROM T_Employee WHERE DepartmentName = @DepartmentName";

            var listEmployee = await connection.QueryAsync<EmployeeDto>(query, new { DepartmentName = departmentName });

            result = ConvertToEmployee(listEmployee);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"{nameof(EmployeeRepo)}.{nameof(UpdateEmployee)}");
        }


        return (List<EmployeeWithId>)result;
    }

    public async Task<List<EmployeeWithId>>? GetCompanyEmployees(int companyId)
    {
        IEnumerable<Employee>? result = null;

        try
        {
            using IDbConnection connection = new NpgsqlConnection(_connectionString);
            const string query = "SELECT * FROM T_Employee WHERE CompanyId = @CompanyId";

            var listEmployee = await connection.QueryAsync<EmployeeDto>(query, new { CompanyId = companyId });
            
            result = ConvertToEmployee(listEmployee);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"{nameof(EmployeeRepo)}.{nameof(GetCompanyEmployees)}");
        }
        

        return (List<EmployeeWithId>)result;
    }
    
    public async Task<bool> UpdateEmployee(EmployeeWithId employee)
    {
        var result = false;
        
        try
        {
            using IDbConnection connection = new NpgsqlConnection(_connectionString);

            var employeeDto = ConvertToEmployeeDto(employee);

            const string getQuery = "SELECT * FROM T_Employee WHERE Id = @Id";
            
            var existingEmployee = await connection.QueryAsync<EmployeeDto>(getQuery, employeeDto);;
            
            VailideteFields(employee, employeeDto, existingEmployee);
            
            const string query = "UPDATE T_Employee SET Name = @Name, Surname = @Surname, Phone = @Phone, " +
                                 "PassportType = @PassportType, PassportNumber = @PassportNumber, " +
                                 "DepartmentName = @DepartmentName, DepartmentPhone = @DepartmentPhone " +
                                 "WHERE Id = @Id";

            await connection.ExecuteAsync(query, employeeDto);

            result = true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"{nameof(EmployeeRepo)}.{nameof(UpdateEmployee)}");
        }

        return result;
    }

    private static EmployeeDto ConvertToEmployeeDto(EmployeeWithId employee)
    =>  new() 
    {
        Id = employee.Id,
        Name = employee.Name,
        Surname = employee.Surname,
        Phone = employee.Phone,
        CompanyId = employee.CompanyId,
        PassportType = employee.Passport.Type,
        PassportNumber = employee.Passport.Number,
        DepartmentName = employee.Department.Name,
        DepartmentPhone = employee.Department.Phone
    };

    private static void VailideteFields(Employee employee, EmployeeDto employeeDto, IEnumerable<EmployeeDto> existingEmployee)
    {
        foreach (var empl in existingEmployee)
        {
            if (!string.IsNullOrEmpty(employeeDto.Name))
            {
                empl.Name = employeeDto.Name;
            }

            if (!string.IsNullOrEmpty(employeeDto.Surname))
            {
                empl.Surname = employeeDto.Surname;
            }

            if (!string.IsNullOrEmpty(employeeDto.Phone))
            {
                empl.Phone = employeeDto.Phone;
            }

            if (employee.Passport != null)
            {
                if (!string.IsNullOrEmpty(employeeDto.PassportType))
                {
                    empl.PassportType = employee.Passport.Type;
                }

                if (!string.IsNullOrEmpty(employee.Passport.Number))
                {
                    empl.PassportNumber = employee.Passport.Number;
                }
            }

            if (employee.Department != null)
            {
                if (!string.IsNullOrEmpty(employee.Department.Name))
                {
                    empl.DepartmentName = employee.Department.Name;
                }

                if (!string.IsNullOrEmpty(employee.Department.Phone))
                {
                    empl.DepartmentPhone = employee.Department.Phone;
                }
            }
        }
       
    }

    private static IEnumerable<EmployeeWithId> ConvertToEmployee(IEnumerable<EmployeeDto> listEmployee)
    =>  listEmployee.Select(employee => new EmployeeWithId
        {
            Id = employee.Id,
            Name = employee.Name,
            Surname = employee.Surname,
            Phone = employee.Phone,
            CompanyId = employee.CompanyId,
            Passport = new Passport { Type = employee.PassportType, Number = employee.PassportNumber },
            Department = new Department { Name = employee.DepartmentName, Phone = employee.DepartmentPhone }
        })
        .ToList();
}