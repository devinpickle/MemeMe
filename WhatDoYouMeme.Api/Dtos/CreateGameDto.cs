using System.ComponentModel.DataAnnotations;
using WhatDoYouMeme.Api.Models;

namespace WhatDoYouMeme.Api.Dtos;

public record CreateGameDto (
    string HostName,
    int NumberOfRounds
);