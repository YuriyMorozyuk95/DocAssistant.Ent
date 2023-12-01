// Copyright (c) Microsoft. All rights reserved.

using Shared.TableEntities;

namespace Shared.Models;

public record SearchParameters
{
    public bool SemanticRanker { get; set; } = false;

    public string RetrievalMode { get; set; } = "Vector"; // available option: Text, Vector, Hybrid

    public bool? SemanticCaptions { get; set; }
    public string? ExcludeCategory { get; set; }
    public int? Top { get; set; } = 3;
    public int? Temperature { get; set; }
    public string? PromptTemplate { get; set; }
    public string? PromptTemplatePrefix { get; set; }
    public string? PromptTemplateSuffix { get; set; }
    public bool SuggestFollowupQuestions { get; set; } = true;
    public List<PermissionEntity> SelectedPermissionList { get; set; } = new(); //TODO read from User

    public string[] Permissions => SelectedPermissionList.Select(p => p.Name).ToArray();
}
