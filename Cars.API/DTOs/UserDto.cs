namespace Cars.API.DTOs;

public record UserDto(
    string Id,
    string Username,
    string Email,
    string Token,
    IReadOnlyCollection<string> Roles);