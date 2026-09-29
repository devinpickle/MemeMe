namespace WhatDoYouMeme.Api.Dtos;

public record JoinGameResponseDto(
    int PlayerId,
    string PlayerName,
    int GameId,
    string JoinCode,
    string AccessToken
);