﻿using System.Text.Json.Serialization;
using Common.Models.DTO;

namespace Common.Models.Data;

public class PageToken
{
    [JsonPropertyName("term")]
    public string Term { get; set; }
    [JsonPropertyName("cursor")]
    public int Cursor { get; set; }
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
}

