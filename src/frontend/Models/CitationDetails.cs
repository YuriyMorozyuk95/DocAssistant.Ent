// Copyright (c) Microsoft. All rights reserved.

namespace ClientApp.Models;

public class CitationDetails  
{  
    public string Name { get; set; }  
    public string BaseUrl { get; set; }  
    public int Number { get; set; }
    public string OriginUrl { get; set; }
  
    public CitationDetails(string name, string baseUrl, int number = 0)  
    {  
        Name = name;  
        BaseUrl = baseUrl;
        Number = number;  
    }  
}  

