using Shared.TableEntities;

namespace ClientApp.Pages;

public partial class UserEdit
{
    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Parameter]
    public string? UserId { get; set; }

    public UserEntity User { get; set; } = new UserEntity();

    protected string Message = string.Empty;
    protected string StatusClass = string.Empty;
    protected bool Saved;

    private IBrowserFile _selectedFile;

    protected override async Task OnInitializedAsync()
    {
        Saved = false;

        int.TryParse(UserId, out var employeeId);

        if (employeeId == 0) //new employee is being created
        {
            //add some defaults
            User = new UserEntity { };
        }
        else
        {
            User = await MockDataService.GetEmployeeDetails(int.Parse(UserId));
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

        if (User.Id?.Length > 0) //new
        {
            //TODO image adding
            //if (selectedFile != null)//take first image
            //{
            //    var file = selectedFile;
            //    Stream stream = file.OpenReadStream();
            //    MemoryStream ms = new();
            //    await stream.CopyToAsync(ms);
            //    stream.Close();

            //    User.ImageName = file.Name;
            //    User.ImageContent = ms.ToArray();
            //}

            var addedEmployee = await MockDataService.AddEmployee(User);
            if (addedEmployee != null)
            {
                StatusClass = "alert-success";
                Message = "New employee added successfully.";
                Saved = true;
            }
            else
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong adding the new employee. Please try again.";
                Saved = false;
            }
        }
        else
        {
            await MockDataService.UpdateEmployee(User);
            StatusClass = "alert-success";
            Message = "Employee updated successfully.";
            Saved = true;
        }
    }

    protected void HandleInvalidSubmit()
    {
        StatusClass = "alert-danger";
        Message = "There are some validation errors. Please try again.";
    }

    protected async Task DeleteEmployeeAsync()
    {
        await MockDataService.DeleteEmployee(User.Id);

        StatusClass = "alert-success";
        Message = "Deleted successfully";

        Saved = true;
    }

    protected void NavigateToOverview()
    {
        NavigationManager.NavigateTo("/users-page");
    }

}
