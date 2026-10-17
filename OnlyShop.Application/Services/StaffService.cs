using OnlyShop.Application.Abstractions.Infrastructure;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Common.Exceptions;
using OnlyShop.Application.Dtos;
using OnlyShop.Application.Mapping;
using OnlyShop.Core.Common;
using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Services;

internal sealed class StaffService(
    IStaffRepository staff,
    IPasswordHasher hasher,
    IUnitOfWork uow) : IStaffService
{
    private const int MinPasswordLength = 8;

    public async Task<StaffDto> AuthenticateAsync(string email, string password, CancellationToken ct = default)
    {
        var member = await staff.GetByEmailAsync(NormalizeEmail(email), ct);

        if (member is null || !member.IsActive || !hasher.Verify(member.PasswordHash, password))
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        member.RegisterLogin();
        await uow.SaveChangesAsync(ct);

        return member.ToDto();
    }

    public async Task<IReadOnlyList<StaffDto>> GetAllAsync(CancellationToken ct = default)
    {
        var all = await staff.GetAllAsync(ct);
        return all.OrderBy(s => s.Name).Select(s => s.ToDto()).ToList();
    }

    public async Task<StaffDto> CreateAsync(CreateStaffRequest request, CancellationToken ct = default)
    {
        var email = NormalizeEmail(request.Email);

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email is required");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new DomainException("Name is required");
        }

        if (request.Password is null || request.Password.Length < MinPasswordLength)
        {
            throw new DomainException($"Password must be at least {MinPasswordLength} characters");
        }

        if (await staff.EmailExistsAsync(email, ct))
        {
            throw new DomainException("A staff member with this email already exists");
        }

        var member = new StaffMember
        {
            Email = email,
            PasswordHash = hasher.Hash(request.Password),
            Name = request.Name.Trim(),
            Role = request.Role
        };

        staff.Add(member);
        await uow.SaveChangesAsync(ct);

        return member.ToDto();
    }

    public async Task SetActiveAsync(Guid staffId, bool isActive, CancellationToken ct = default)
    {
        var member = await LoadAsync(staffId, ct);
        member.IsActive = isActive;
        member.UpdatedAt = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);
    }

    public async Task LinkTelegramAsync(Guid staffId, long telegramId, CancellationToken ct = default)
    {
        var member = await LoadAsync(staffId, ct);
        member.TelegramId = telegramId;
        member.UpdatedAt = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);
    }

    public async Task UpdateTelegramIdAsync(Guid staffId, long? telegramId, CancellationToken ct = default)
    {
        var staff = await staffRepository.GetByIdAsync(staffId, ct)
            ?? throw new NotFoundException(nameof(StaffMember), staffId);

        staff.TelegramId = telegramId;
        await uow.SaveChangesAsync(ct);
    }

    private async Task<StaffMember> LoadAsync(Guid staffId, CancellationToken ct) =>
        await staff.GetByIdAsync(staffId, ct)
            ?? throw new NotFoundException(nameof(StaffMember), staffId);

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}