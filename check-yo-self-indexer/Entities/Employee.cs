using System;

namespace check_yo_self_indexer.Server.Entities;

public class Employee
{
    public int EmployeeId { get; set; }

    public string LastName { get; set; }

    public string FirstName { get; set; }

    public decimal Salary { get; set; }

    public DateTime FirstPaycheckDate { get; set; }
}
