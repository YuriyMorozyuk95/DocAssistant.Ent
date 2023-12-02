// Copyright (c) Microsoft. All rights reserved.

using Shared.TableEntities;

public class MockUserService
{
    private static List<UserEntity>? _users = default!;

    public static List<UserEntity>? Users
    {
        get
        {
            _users ??= InitializeMockUsers();

            return _users;
        }
    }

    public static List<UserEntity> InitializeMockUsers()
    {
        var e1 = new UserEntity
        {
            Id = "1",
            Email = "netflixhome2024@gmail.com",
            FirstName = "Oleg",
            LastName = "Sedlaruk",
            ImageUrl = "https://hacktogatherstorage.blob.core.windows.net/avatars/Oleg.jpg",
            Permissions = MockPermissionService.GetPermissions(),
        };

        var e2 = new UserEntity
        {
            Id = "2",
            Email = "Victoria.naffato@gmail.com",
            FirstName = "Victoria",
            LastName = "Naffato",
            ImageUrl = "https://hacktogatherstorage.blob.core.windows.net/avatars/Victoria.jpg",
            Permissions = new List<PermissionEntity> { MockPermissionService.GetPermissionById("2"), MockPermissionService.GetPermissionById("3") },
        };

        var e3 = new UserEntity
        {
            Id = "3",
            Email = "Yuriy.Morozyuk.95@gmail.com",
            FirstName = "Yurii",
            LastName = "Moroziuk",
            ImageUrl = "https://hacktogatherstorage.blob.core.windows.net/avatars/Yurii.jpg",
            Permissions = new List<PermissionEntity> { MockPermissionService.GetPermissionById("1") },
        };

        return new List<UserEntity>() { e1, e2, e3 };
    }

    public static Task<UserEntity> AddUser(UserEntity userEntity)
    {
        Users.Add(userEntity);

        return Task.FromResult(userEntity);
    }

    public static Task<IEnumerable<UserEntity>> GetAllUsers(bool refreshRequired = false)
    {
        return Task.Run(() => Users.AsEnumerable());
    }
    public static Task<UserEntity> GetUserDetails(int userId)
    {
        return Task.Run(() => Users.FirstOrDefault(e => e.Id == userId.ToString()));
    }
    public static Task UpdateUser(UserEntity user)
    {
        return Task.Run(() =>
        {
            var userToUpdate = Users.FirstOrDefault(e => e.Id == user.Id);
            if (userToUpdate != null)
            {
                userToUpdate.FirstName = user.FirstName;
                userToUpdate.LastName = user.LastName;
                userToUpdate.Email = user.Email;
            }
        });
    }
    public static Task DeleteUser(string userId)
    {
        return Task.Run(() =>
        {
            var userToDelete = Users.FirstOrDefault(e => e.Id == userId.ToString());
            if (userToDelete != null)
            {
                Users.Remove(userToDelete);
            }
        });
    }
}
