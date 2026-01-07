using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;

namespace SEBIZ.Domain.Contracts.MogoDbProductAPI.Domain.Contracts
{
    public record GameDto(string Id, string? Name, string? Description, double? Price, string? Genre, string? Developer, DateTime ReleaseDate, List<string>? Tags, string? ImageUrl);
    public record CreateGameDto(string? Name, string? Description, double? Price, string? Genre, string? Developer, DateTime ReleaseDate, List<string>? Tags, IFormFile? ImageFile);
    public record UpdateGameDto(string? Name, string? Description, double? Price, string? Genre, string? Developer, DateTime ReleaseDate, List<string>? Tags, IFormFile? ImageFile);
}
