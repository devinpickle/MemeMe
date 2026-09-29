using WhatDoYouMeme.Api.Models;

namespace WhatDoYouMeme.Api.Dtos;

public record GameLobbySummaryDto(
    string JoinCode,
    GameStatus Status,
    PlayerSummaryDto[] Players,
    int ImageCount
);