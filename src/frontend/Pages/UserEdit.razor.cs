using Shared.TableEntities;

namespace ClientApp.Pages;

public partial class UserEdit
{
    private MudForm _form;

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Inject]
    public IUserApiClient UserApiClient { get; set; }
    [Inject]
    public required IJSRuntime JsRuntime { get; set; }

    [Inject]
    public ApiClient ApiClient { get; set; }

    [Parameter]
    public string? UserId { get; set; }

    [Parameter]
    public string? Email { get; set; }

    public UserEntity User { get; set; } = new UserEntity();

    protected string Message = string.Empty;
    protected string StatusClass = string.Empty;
    protected bool Saved;

    private IBrowserFile _selectedFile;

    protected override async Task OnInitializedAsync()
    {
        Saved = false;

        int.TryParse(UserId, out var userId);

        if (userId == 0) //new user is being created  
        {
            //add some defaults  
            User = new UserEntity { };
        }
        else
        {
            User = await UserApiClient.GetUserDetails(int.Parse(UserId), Email);
        }
    }

    private void OnInputFileChange(InputFileChangeEventArgs e)
    {
        _selectedFile = e.File;
        StateHasChanged();
    }

    protected async Task HandleValidSubmitAsync()
    {
        Saved = false;

        if (string.IsNullOrEmpty(User.Id)) //new  
        {
            User.Id = (new Random()).Next().ToString();
            var addedUser = await UserApiClient.AddUser(User);
            if (addedUser != null)
            {
                StatusClass = "alert-success";
                Message = "New user added successfully.";
                Saved = true;
            }
            else
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong adding the new user. Please try again.";
                Saved = false;
            }
        }
        else
        {
            await UserApiClient.UpdateUser(User);
            StatusClass = "alert-success";
            Message = "User updated successfully.";
            Saved = true;
        }
    }

    protected void HandleInvalidSubmit()
    {
        StatusClass = "alert-danger";
        Message = "There are some validation errors. Please try again.";
    }

    protected async Task DeleteUserAsync()
    {
        await UserApiClient.DeleteUser(User.Id, User.Email);

        StatusClass = "alert-success";
        Message = "Deleted successfully";

        Saved = true;
    }

    protected void NavigateToOverview()
    {
        NavigationManager.NavigateTo("/users-page");
    }

    private async Task UploadFilesAsync(IBrowserFile file)
    {
        if (file != null)
        {
            var cookie = await JsRuntime.InvokeAsync<string>("getCookie", "XSRF-TOKEN");

            var imageUrl = await ApiClient.UploadAvatarAsync(file, cookie);
            User.ImageUrl = imageUrl.Replace("\"", string.Empty);
        }
    }

}
