﻿using Shared.TableEntities;

namespace ClientApp.Pages;

public sealed partial class UsersPage
{
    [Inject]
    private NavigationManager NavigationManager { get; set; }

    public List<UserEntity> Users { get; set; } = default!;
    private UserEntity? _selectedUser;

    public string Title { get; set; } = "Users overview";
    public string Description { get; set; } = "users overview";

    protected override void OnInitialized()
    {
        Users = MockUserService.Users;
    }
}
