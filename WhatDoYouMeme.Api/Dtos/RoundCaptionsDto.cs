namespace WhatDoYouMeme.Api.Dtos;

public record RoundCaptionsDto
(
    int RoundId,
    List<CaptionSummaryDto> Captions,
    int? SelectedCaptionId
);