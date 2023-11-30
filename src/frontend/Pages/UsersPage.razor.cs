using Shared.TableEntities;

namespace ClientApp.Pages;

public sealed partial class UsersPage
{
    public List<UserEntity> Users { get; set; } = default!;
    private UserEntity? _selectedEmployee;

    public string Title { get; set; } = "Users overview";
    public string Description { get; set; } = "users overview";

    protected override void OnInitialized()
    {
        Users = MockDataService.Users;
    }
}
