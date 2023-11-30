using Microsoft.AspNetCore.Components;

using Shared.TableEntities;

namespace ClientApp.Pages;

    public partial class UserDetail
    {
        [Parameter]
        public string Id { get; set; }
        public UserEntity User { get; set; } = new UserEntity();

        protected override Task OnInitializedAsync()
        {
            User = MockDataService.Users.FirstOrDefault(e => e.Id == Id);

            return base.OnInitializedAsync();
        }
    }
