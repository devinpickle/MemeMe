using System.ComponentModel.DataAnnotations;
using WhatDoYouMeme.Api.Models;

namespace WhatDoYouMeme.Api.Dtos;

public record UpdateGameDto (
    [Required][StringLength(4)] string JoinCode,
    [Range(1, 50)] GameStatus Status,
    DateTime CreatedAt
);