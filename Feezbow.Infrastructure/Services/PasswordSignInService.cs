using Feezbow.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Feezbow.Infrastructure.Services;

public class PasswordSignInService(SignInManager<User> signInManager) : IPasswordSignInService
{
    public Task<SignInResult> CheckPasswordSignInAsync(
        User user,
        string password,
        bool lockoutOnFailure
    ) => signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
}
