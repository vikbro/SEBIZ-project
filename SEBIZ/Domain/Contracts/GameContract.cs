using System;
using System.Collections.Generic;

namespace SEBIZ.Domain.Contracts
{
    public record GameDto(string Id, string? Name, string? Description, double? Price, string? Genre, string? Developer, DateTime ReleaseDate, List<string>? Tags, string? ImagePath, string CreatedById, string? FileName, string? GameFilePath, string? GameFileName);
    public record CreateGameDto(string? Name, string? Description, double? Price, string? Genre, string? Developer, DateTime ReleaseDate, List<string>? Tags, string? ImagePath, string? FileName, string? GameFilePath, string? GameFileName);
    public record UpdateGameDto(string? Name, string? Description, double? Price, string? Genre, string? Developer, DateTime ReleaseDate, List<string>? Tags, string? ImagePath, string? FileName, string? GameFilePath, string? GameFileName);
}
