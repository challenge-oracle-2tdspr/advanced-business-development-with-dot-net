using AgroTech.Application.DTOs;
namespace AgroTech.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDTO?> LoginAsync(LoginRequestDto request);
}