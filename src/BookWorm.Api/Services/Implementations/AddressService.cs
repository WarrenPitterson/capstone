using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Exceptions;
using BookWorm.Api.Models;
using BookWorm.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Api.Services.Implementations;

public class AddressService : IAddressService
{
    private readonly BookWormDbContext _dbContext;

    public AddressService(BookWormDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AddressDto>> GetUserAddressesAsync(int userId)
    {
        return await _dbContext.Addresses
            .Where(a => a.UserId == userId && a.IsSaved)
            .OrderByDescending(a => a.Id)
            .Select(a => MapToAddressDto(a))
            .ToListAsync();
    }

    public async Task<AddressDto> CreateAddressAsync(int userId, CreateAddressRequest request)
    {
        var address = new Address
        {
            UserId = userId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Line1 = request.Line1.Trim(),
            Line2 = request.Line2?.Trim(),
            City = request.City.Trim(),
            Pin = request.Pin.Trim(),
            State = request.State.Trim(),
            Country = request.Country.Trim(),
            PhoneCountryCode = request.PhoneCountryCode?.Trim() ?? "+91",
            Phone = request.Phone.Trim(),
            IsSaved = request.IsSaved
        };

        await _dbContext.Addresses.AddAsync(address);
        await _dbContext.SaveChangesAsync();

        return MapToAddressDto(address);
    }

    public async Task<AddressDto> UpdateAddressAsync(int userId, int addressId, CreateAddressRequest request)
    {
        var address = await _dbContext.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

        if (address == null)
        {
            throw new NotFoundException($"Address with ID {addressId} not found.");
        }

        address.FirstName = request.FirstName.Trim();
        address.LastName = request.LastName.Trim();
        address.Email = request.Email.Trim().ToLowerInvariant();
        address.Line1 = request.Line1.Trim();
        address.Line2 = request.Line2?.Trim();
        address.City = request.City.Trim();
        address.Pin = request.Pin.Trim();
        address.State = request.State.Trim();
        address.Country = request.Country.Trim();
        address.PhoneCountryCode = request.PhoneCountryCode?.Trim() ?? "+91";
        address.Phone = request.Phone.Trim();
        address.IsSaved = request.IsSaved;

        await _dbContext.SaveChangesAsync();

        return MapToAddressDto(address);
    }

    public async Task DeleteAddressAsync(int userId, int addressId)
    {
        var address = await _dbContext.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

        if (address == null)
        {
            throw new NotFoundException($"Address with ID {addressId} not found.");
        }

        _dbContext.Addresses.Remove(address);
        await _dbContext.SaveChangesAsync();
    }

    private static AddressDto MapToAddressDto(Address a)
    {
        return new AddressDto
        {
            Id = a.Id,
            UserId = a.UserId,
            FirstName = a.FirstName,
            LastName = a.LastName,
            Email = a.Email,
            Line1 = a.Line1,
            Line2 = a.Line2,
            City = a.City,
            Pin = a.Pin,
            State = a.State,
            Country = a.Country,
            PhoneCountryCode = a.PhoneCountryCode,
            Phone = a.Phone,
            IsSaved = a.IsSaved
        };
    }
}
