using SmartWayWebApi.Data.Implementation;
using SmartWayWebApi.Data.Interfaces;
using SmartWayWebApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IEmployeeRepo, EmployeeRepo>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapPost("api/v1/AddEmployee", async (IEmployeeRepo _employeeRepo, Employee employee) =>
{
    var result = await _employeeRepo.AddEmployee(employee); 
    
    return result is not 0  ? Results.Ok(result) : Results.BadRequest();
});

app.MapGet("api/v1/GetCompanyEmployee", async (IEmployeeRepo _employeeRepo, int companyId) =>
{
    var result = await _employeeRepo.GetCompanyEmployees(companyId);

    return result is not null ? Results.Ok(result) : Results.NotFound();
});

app.MapGet("api/v1/GetCompanyDepartmentEmployee",  async (IEmployeeRepo _employeeRepo, string departmentName)  =>
{
    var result = await  _employeeRepo.GetCompanyDepartmentEmployees(departmentName);
    
    return result is not null ? Results.Ok(result) : Results.NotFound();
});

app.MapDelete("api/v1/DeleteEmployee", async (IEmployeeRepo _employeeRepo, int id) => 
{ 
    var result = await _employeeRepo.DeleteEmployee(id);
    
    return result is not false ? Results.Ok(result) : Results.NotFound();
});

app.MapPatch("api/v1/UpdateEmployee", async (IEmployeeRepo _employeeRepo, EmployeeWithId employee) => 
{ 
    var result = await _employeeRepo.UpdateEmployee(employee);
    
    return result is not false ? Results.Ok(result) : Results.NotFound();
}); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();