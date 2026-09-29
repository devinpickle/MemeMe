using WhatDoYouMeme.Api.Models;

namespace WhatDoYouMeme.Api.Dtos;

public record RoundFinishSummaryDto(
    RoundState State,
    int WinnerId,
    string WinnerName,
    int WinnerScore,
    string winningCaption,
    string ImageFileName,
    bool GameFinished
);