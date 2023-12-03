
// Copyright (c) Microsoft. All rights reserved.  
using Shared.TableEntities;

namespace ClientApp.Components;

public sealed partial class PermissionMultiSelect
{
    [Inject]
    public PermissionApiClient PermissionApiClient { get; set; }
    [Parameter]
    public List<PermissionEntity> SelectedItems { get; set; } = new List<PermissionEntity>();
    [Parameter]  
    public EventCallback<List<PermissionEntity>> SelectedItemsChanged { get; set; }
    [Parameter]  
    public string Label { get; set; }
    [Parameter]  
    public bool IsEnabled { get; set; }  

    public Type PermissionEntityType { get; set; } = typeof(PermissionEntity);
    private List<PermissionEntity> _items;

    protected override async Task OnInitializedAsync()  
    {
        var perm = MockPermissionService.GetPermissions();
        await PermissionApiClient.SaveChanges(perm);
        _items = (List<PermissionEntity>)await PermissionApiClient.GetAllPermissions();
    }  

    private async Task OnSelectedItemsChangedAsync(IEnumerable<PermissionEntity> arg)
    {
        SelectedItems = arg.ToList();  
        await SelectedItemsChanged.InvokeAsync(SelectedItems);
    }
}
