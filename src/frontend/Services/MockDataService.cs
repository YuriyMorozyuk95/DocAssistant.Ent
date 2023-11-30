// Copyright (c) Microsoft. All rights reserved.

using Shared.TableEntities;

public class MockDataService
{
    private static List<UserEntity>? _employees = default!;

    public static List<UserEntity>? Employees
    {
        get
        {

            _employees ??= InitializeMockEmployees();

            return _employees;
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
}
