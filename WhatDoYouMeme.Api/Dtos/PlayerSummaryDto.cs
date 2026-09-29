using WhatDoYouMeme.Api.Models;

namespace WhatDoYouMeme.Api.Dtos;

public record PlayerSummaryDto(
    int Id,
    string Name,
    int Score,
    bool IsHost
);