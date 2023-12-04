
// Copyright (c) Microsoft. All rights reserved.  
using Shared.TableEntities;

namespace ClientApp.Components;

public sealed partial class PermissionMultiSelect
{
    [Inject]
    public IPermissionApiClient PermissionApiClient { get; set; }
    [Parameter]
    public List<PermissionEntity> SelectedItems { get; set; } = new();
    [Parameter]  
    public EventCallback<List<PermissionEntity>> SelectedItemsChanged { get; set; }
    [Parameter]  
    public string Label { get; set; }
    [Parameter]  
    public bool IsEnabled { get; set; }  

    public Type PermissionEntityType { get; set; } = typeof(PermissionEntity);
    private List<PermissionEntity> _items = new();
    private bool _isInitialized;
    protected override async Task OnInitializedAsync()  
    {
        _items = (await PermissionApiClient.GetAllPermissions()).ToList();
        _isInitialized = true;
        StateHasChanged();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
    }

    private async Task OnSelectedItemsChangedAsync(IEnumerable<PermissionEntity> arg)
    {
        if (_isInitialized)
        {
            var argsIds = arg.Select(x => x.Id).ToList();
            var selectedItemsFromItems = _items.Where(x => argsIds.Contains(x.Id)).ToList();

            SelectedItems = selectedItemsFromItems;  
            await SelectedItemsChanged.InvokeAsync(SelectedItems);
            StateHasChanged();
        }
    }
}
