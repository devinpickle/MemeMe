using WhatDoYouMeme.Api.Models;

namespace WhatDoYouMeme.Api.Dtos;

public record SubmitCaptionDto(
    int PlayerId,
    string Content,
    MemeStyle Style
);
