using WhatDoYouMeme.Api.Models;
namespace WhatDoYouMeme.Api.Dtos;

public record GameSummaryDto (
    int Id,
    string JoinCode,
    GameStatus Status,
    DateTime CreatedAt
);