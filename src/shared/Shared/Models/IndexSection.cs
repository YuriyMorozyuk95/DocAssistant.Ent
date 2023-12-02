// Copyright (c) Microsoft. All rights reserved.

namespace Shared.Models;

public readonly struct IndexSection
{
    public const string PermissionsFieldName = "permissions";

    public string Id { get; }
    public string Content { get; }
    public string SourcePage { get; }
    public string SourceFile { get; }
    public string[] Permissions { get; }

    public IndexSection(string id, string content, string sourcePage, string sourceFile, string[] permissions)
    {
        Id = id;
        Content = content;
        SourcePage = sourcePage;
        SourceFile = sourceFile;
        Permissions = permissions;
    }
}
