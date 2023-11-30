// Copyright (c) Microsoft. All rights reserved.

using Shared.TableEntities;

public class MockDataService
{
    private static List<UserEntity>? _users = default!;

    public static List<UserEntity>? Users
    {
        get
        {
            _users ??= InitializeMockEmployees();

            return _users;
        }
    }

    private static List<UserEntity> InitializeMockEmployees()
    {
        var e1 = new UserEntity
        {
            Id = "1",
            Email = "bethany@bethanyspieshop.com",
            FirstName = "Bethany",
            LastName = "Smith",
        };

        var e2 = new UserEntity
        {
            Id = "2",
            Email = "gill@bethanyspieshop.com",
            FirstName = "Gill",
            LastName = "Cleeren",
        };

        var e3 = new UserEntity
        {
            Id = "3",
            Email = "gill@bethanyspieshop.com",
            FirstName = "Jane",
            LastName = String.Empty,
        };

        return new List<UserEntity>() { e1, e2, e3 };
    }

    public static Task<UserEntity> AddEmployee(UserEntity userEntity)
    {
        Users.Add(userEntity);

        return Task.FromResult(userEntity);
    }

    public static Task<IEnumerable<UserEntity>> GetAllEmployees(bool refreshRequired = false)
    {
        return Task.Run(() => Users.AsEnumerable());
    }
    public static Task<UserEntity> GetEmployeeDetails(int employeeId)
    {
        return Task.Run(() => Users.FirstOrDefault(e => e.Id == employeeId.ToString()));
    }
    public static Task UpdateEmployee(UserEntity employee)
    {
        return Task.Run(() =>
        {
            var employeeToUpdate = Users.FirstOrDefault(e => e.Id == employee.Id);
            if (employeeToUpdate != null)
            {
                employeeToUpdate.FirstName = employee.FirstName;
                employeeToUpdate.LastName = employee.LastName;
                employeeToUpdate.Email = employee.Email;
            }
        });
    }
    public static Task DeleteEmployee(string employeeId)
    {
        return Task.Run(() =>
        {
            var employeeToDelete = Users.FirstOrDefault(e => e.Id == employeeId.ToString());
            if (employeeToDelete != null)
            {
                Users.Remove(employeeToDelete);
            }
        });
    }
}
