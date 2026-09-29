namespace WhatDoYouMeme.Api.Dtos;

public record GameResultsDto(
    PlayerSummaryDto[] Winners,
    PlayerSummaryDto[] PlayerSummaryDtos
);