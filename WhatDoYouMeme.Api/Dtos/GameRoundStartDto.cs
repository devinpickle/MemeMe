using WhatDoYouMeme.Api.Models;

namespace WhatDoYouMeme.Api.Dtos;

public record GameRoundStartDto(
    string JoinCode,
    GameStatus Status,
    int RoundNumber,
    int JudgeId,
    int ImageId,
    RoundState State,
    bool GameFinished
);