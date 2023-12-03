using Shared.TableEntities;

namespace ClientApp.Components;

public partial class UserCard
{
    [Parameter]
    public UserEntity User { get; set; } = default!;
}
