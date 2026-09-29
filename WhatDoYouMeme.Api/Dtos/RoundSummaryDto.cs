using WhatDoYouMeme.Api.Models;

namespace WhatDoYouMeme.Api.Dtos;

public record RoundSummaryDto(
    int RoundNumber,
    RoundState State,
    int JudgeId,
    int ImageId,
    string ImageFileName,
    int SubmittedCaptionCount,
    int CaptionsExpected,
    int? SelectedCaptionId,
    string? SelectedCaptionContent,
    MemeStyle? SelectedCaptionStyle,
    string? WinnerName,
    int? WinnerId,
    int? WinnerScore,
    bool GameFinished
);