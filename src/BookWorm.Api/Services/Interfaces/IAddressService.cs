using BookWorm.Api.Models;

namespace BookWorm.Api.Services.Interfaces;

public interface IAddressService
{
    Task<List<AddressDto>> GetUserAddressesAsync(int userId);
    Task<AddressDto> CreateAddressAsync(int userId, CreateAddressRequest request);
    Task<AddressDto> UpdateAddressAsync(int userId, int addressId, CreateAddressRequest request);
    Task DeleteAddressAsync(int userId, int addressId);
}
