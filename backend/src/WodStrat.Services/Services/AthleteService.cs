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

    public AthleteService(IWodStratDatabase database)
    {
        _database = database;
    }

    /// <inheritdoc />
    public async Task<AthleteDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var athlete = await _database.Get<Athlete>()
            .Where(a => a.Id == id && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        return athlete?.ToDto();
    }

    /// <inheritdoc />
    public async Task<AthleteDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var athlete = await _database.Get<Athlete>()
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        return athlete?.ToDto();
    }

    /// <inheritdoc />
    public async Task<AthleteDto> CreateAsync(CreateAthleteDto dto, CancellationToken cancellationToken = default)
    {
        var entity = dto.ToEntity();

        _database.Add(entity);
        await _database.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    /// <inheritdoc />
    public async Task<AthleteDto?> UpdateAsync(Guid id, UpdateAthleteDto dto, CancellationToken cancellationToken = default)
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
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
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
