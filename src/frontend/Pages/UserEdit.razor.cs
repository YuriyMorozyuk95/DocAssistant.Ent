﻿using Shared.TableEntities;

namespace ClientApp.Pages;

public partial class UserEdit
{
    private MudForm _form;  

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    //TODO user instead of MockDataService
    [Inject]
    public UserApiClient UserApiClient { get; set; }

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

        int.TryParse(UserId, out var userId);

        if (userId == 0) //new user is being created  
        {
            //add some defaults  
            User = new UserEntity { };
        }
        else
        {
            User = await MockUserService.GetUserDetails(int.Parse(UserId));
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
            var addedUser = await MockUserService.AddUser(User);
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
            await MockUserService.UpdateUser(User);
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
        await MockUserService.DeleteUser(User.Id);

        StatusClass = "alert-success";
        Message = "Deleted successfully";

        Saved = true;
    }

    protected void NavigateToOverview()
    {
        NavigationManager.NavigateTo("/users-page");
    }
}
