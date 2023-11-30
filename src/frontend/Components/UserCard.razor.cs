using Shared.TableEntities;

namespace ClientApp.Components;

public partial class UserCard
{
    [Parameter]
    public UserEntity User { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    protected override void OnInitialized()
    {
        //TODO update validation
        //if (string.IsNullOrEmpty(User.LastName))
        //{
        //    throw new Exception("Last name can't be empty");
        //}
    }

    public void NavigateToDetails(UserEntity selectedEmployee)
    {
        NavigationManager.NavigateTo($"/user-detail/{selectedEmployee.Id}");
    }
}
