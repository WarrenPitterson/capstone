using BookWorm.Api.Entities;

namespace BookWorm.Api.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
