﻿namespace SmartWayWebApi.Models;

public class Employee
{
    public string Name { get; set; }

    public string Surname { get; set; }

    public string Phone { get; set; }

    public int CompanyId { get; set; }
    
    public Passport Passport { get; set; }
    
    public Department Department { get; set; }
}