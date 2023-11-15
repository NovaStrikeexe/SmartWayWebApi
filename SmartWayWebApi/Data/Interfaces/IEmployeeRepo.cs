using SmartWayWebApi.Models;
using SmartWayWebApi.DTO;

namespace SmartWayWebApi.Data.Interfaces;

public interface IEmployeeRepo
{
    Task<int> AddEmployee(Employee employee);
    
    Task<bool> DeleteEmployee(int id);
    
    Task<List<EmployeeWithId>> GetCompanyDepartmentEmployees(string companyName);
    
    Task<List<EmployeeWithId>> GetCompanyEmployees(int companyId);
    
    Task<bool> UpdateEmployee(EmployeeWithId employee);
}