// Copyright (c) Microsoft. All rights reserved.

using System.Threading;

namespace ClientApp.Pages;

public sealed partial class Chat
{
    private string _userQuestion = "";
    private UserQuestion _currentQuestion;
    private string _lastReferenceQuestion = "";
    private bool _isReceivingResponse = false;

    private string _firstExample = "yo";

    private readonly Dictionary<UserQuestion, ApproachResponse> _questionAndAnswerMap = new();
    private bool _isLoadingPrompts;
    private Task _getCopilotPrompts;

    [Inject] public required ISessionStorageService SessionStorage { get; set; }

    [Inject] public required ApiClient ApiClient { get; set; }

    [CascadingParameter(Name = nameof(Settings))]
    public required RequestSettingsOverrides Settings { get; set; }

    [CascadingParameter(Name = nameof(CopilotPrompts))]
    public required CopilotPromptsRequestResponse CopilotPrompts { get; set; } = new CopilotPromptsRequestResponse();  

    [CascadingParameter(Name = nameof(IsReversed))]
    public required bool IsReversed { get; set; }

    protected override void OnInitialized()
    {
        // Instead of awaiting this async enumerable here, let's capture it in a task
        // and start it in the background. This way, we can await it in the UI.
        _getCopilotPrompts = OnGetCopilotPromptsClickedAsync();
    }

    private Task OnAskQuestionAsync(string question)
    {
        _userQuestion = question;
        return OnAskClickedAsync();
    }

    private async Task OnAskClickedAsync()
    {
        if (string.IsNullOrWhiteSpace(_userQuestion))
        {
            return;
        }

        _isReceivingResponse = true;
        _lastReferenceQuestion = _userQuestion;
        _currentQuestion = new(_userQuestion, DateTime.Now);
        _questionAndAnswerMap[_currentQuestion] = null;

        try
        {
            var history = _questionAndAnswerMap
                .Where(x => x.Value is not null)
                .Select(x => new ChatTurn(x.Key.Question, x.Value!.Answer))
                .ToList();

            history.Add(new ChatTurn(_userQuestion));

            var request = new ChatRequest(history.ToArray(), Settings.Approach, Settings.Overrides);
            var result = await ApiClient.ChatConversationAsync(request);

            _questionAndAnswerMap[_currentQuestion] = result.Response;
            if (result.IsSuccessful)
            {
                _userQuestion = "";
                _currentQuestion = default;
            }
        }
        finally
        {
            _isReceivingResponse = false;
        }
    }

    private void OnClearChat()
    {
        _userQuestion = _lastReferenceQuestion = "";
        _currentQuestion = default;
        _questionAndAnswerMap.Clear();
    }

    private async Task OnPostCopilotPromptsClickedAsync()
    {
        await ApiClient.PostCopilotPromptsServerDataAsync(CopilotPrompts);
    }

    private async Task OnGetCopilotPromptsClickedAsync()
    {
        _isLoadingPrompts = true;

        try
        {
            CopilotPrompts = await ApiClient.GetCopilotPromptsAsync();
        }
        finally
        {
            _isLoadingPrompts = false;
            StateHasChanged();
        }

    }
}
