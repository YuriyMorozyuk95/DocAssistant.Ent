﻿// Copyright (c) Microsoft. All rights reserved.

namespace Shared.Models;

public class SupportingContentRecord  
{  
    public SupportingContentRecord(string title, string content, string originUri = null)  
    {  
        Title = title;  
        Content = content;
        OriginUri = originUri;
    }  
  
    public string Title { get; set; }  
    public string Content { get; set; }
    public string OriginUri { get; set; } 
}  
 
public class ApproachResponse  
{
    public ApproachResponse()
    {
        Answer = string.Empty;
        Thoughts = string.Empty;
        DataPoints = Array.Empty<SupportingContentRecord>();
        CitationBaseUrl = string.Empty;
        Questions = Array.Empty<string?>();
        Error = string.Empty;
    }
    public ApproachResponse(string answer, string? thoughts, SupportingContentRecord[] dataPoints, string citationBaseUrl, string[] questions, string? error = null)  
    {  
        Answer = answer;  
        Thoughts = thoughts;  
        DataPoints = dataPoints;  
        CitationBaseUrl = citationBaseUrl;  
        Questions = questions;  
        Error = error;  
    }  
  
    public string Answer { get;set; }  
    public string? Thoughts { get; set; }  
    public SupportingContentRecord[] DataPoints { get; set; }  
    public string CitationBaseUrl { get; set; }  
    public string[] Questions { get; set; }  
    public string? Error { get; set; }  
}  
