using WhatDoYouMeme.Api.Models;

namespace WhatDoYouMeme.Api.Dtos;

public record CaptionSummaryDto(
    int Id,
    string Content,
    MemeStyle MemeStyle
);