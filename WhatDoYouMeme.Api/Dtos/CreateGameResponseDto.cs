namespace WhatDoYouMeme.Api.Dtos;

public record CreateGameResponseDto(
    int GameId,
    string JoinCode,
    int PlayerId,
    string AccessToken
);