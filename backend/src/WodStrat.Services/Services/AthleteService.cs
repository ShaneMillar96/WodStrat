using Microsoft.EntityFrameworkCore;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Extensions;
using WodStrat.Services.Interfaces;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for athlete profile management.
/// </summary>
public class AthleteService : IAthleteService
{
    private readonly IWodStratDatabase _database;
    private readonly ICurrentUserService _currentUserService;

    public AthleteService(IWodStratDatabase database, ICurrentUserService currentUserService)
    {
        _database = database;
        _currentUserService = currentUserService;
    }

    /// <inheritdoc />
    public async Task<AthleteDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var athlete = await _database.Get<Athlete>()
            .Where(a => a.Id == id && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        return athlete?.ToDto();
    }

    /// <inheritdoc />
    public async Task<AthleteDto?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var athlete = await _database.Get<Athlete>()
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        return athlete?.ToDto();
    }

    /// <inheritdoc />
    public async Task<AthleteDto> CreateAsync(CreateAthleteDto dto, CancellationToken cancellationToken = default)
    {
        return await CreateAsync(dto, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AthleteDto> CreateAsync(CreateAthleteDto dto, int? userId, CancellationToken cancellationToken = default)
    {
        var entity = dto.ToEntity(userId);

        _database.Add(entity);
        await _database.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    /// <inheritdoc />
    public async Task<AthleteDto?> CreateForCurrentUserAsync(CreateAthleteDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();

        // Check if user already has an athlete profile
        var existingAthlete = await GetByUserIdAsync(userId, cancellationToken);
        if (existingAthlete != null)
        {
            return null; // User already has a profile
        }

        return await CreateAsync(dto, userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AthleteDto?> GetCurrentUserAthleteAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();
        return await GetByUserIdAsync(userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AthleteDto?> UpdateAsync(int id, UpdateAthleteDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _database.Get<Athlete>()
            .Where(a => a.Id == id && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            return null;

        dto.ApplyTo(entity);

        _database.Update(entity);
        await _database.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _database.Get<Athlete>()
            .Where(a => a.Id == id && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        _database.Update(entity);
        await _database.SaveChangesAsync(cancellationToken);

        return true;
    }
}
