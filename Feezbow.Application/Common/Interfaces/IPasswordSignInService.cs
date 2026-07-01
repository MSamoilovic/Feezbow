using Feezbow.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Feezbow.Infrastructure.Services;

public interface IPasswordSignInService
{
    Task<SignInResult> CheckPasswordSignInAsync(User user, string password, bool lockoutOnFailure);
}
