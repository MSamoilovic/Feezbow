using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Domain.Services;

public class PantryService(
    IPantryRepository pantryRepository,
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork) : IPantryService
{
    public async Task<PantryItem> AddAsync(
        long projectId,
        long userId,
        string name,
        decimal quantity,
        string? unit,
        string? location,
        DateTime? expirationDate,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(projectId, userId, cancellationToken);

        var item = PantryItem.Create(projectId, name, quantity, unit, location, expirationDate, notes, userId);

        await pantryRepository.AddAsync(item, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return item;
    }

    public async Task<PantryItem> GetAsync(
        long pantryItemId, long userId, CancellationToken cancellationToken = default)
    {
        var item = await pantryRepository.GetByIdAsync(pantryItemId, cancellationToken)
            ?? throw new NotFoundException(nameof(PantryItem), pantryItemId);

        if (!item.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        return item;
    }

    public async Task<IReadOnlyList<PantryItem>> GetByProjectAsync(
        long projectId,
        long userId,
        string? search,
        string? location,
        int? expiringWithinDays,
        CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(projectId, userId, cancellationToken);
        return await pantryRepository.GetByProjectAsync(
            projectId, search, location, expiringWithinDays, cancellationToken);
    }

    public async Task<long> UpdateAsync(
        long pantryItemId,
        long userId,
        string? name,
        string? unit,
        string? location,
        DateTime? expirationDate,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var item = await LoadOwnedItemAsync(pantryItemId, userId, cancellationToken);
        item.Update(name, unit, location, expirationDate, notes, userId);
        await unitOfWork.CompleteAsync(cancellationToken);
        return item.ProjectId;
    }

    public async Task<long> AdjustQuantityAsync(
        long pantryItemId,
        long userId,
        decimal delta,
        CancellationToken cancellationToken = default)
    {
        var item = await LoadOwnedItemAsync(pantryItemId, userId, cancellationToken);
        item.AdjustQuantity(delta, userId);
        await unitOfWork.CompleteAsync(cancellationToken);
        return item.ProjectId;
    }

    public async Task<long> RemoveAsync(
        long pantryItemId, long userId, CancellationToken cancellationToken = default)
    {
        var item = await LoadOwnedItemAsync(pantryItemId, userId, cancellationToken);
        var projectId = item.ProjectId;

        item.MarkForRemoval(userId);
        pantryRepository.Remove(item);
        await unitOfWork.CompleteAsync(cancellationToken);

        return projectId;
    }

    private async Task EnsureMemberAsync(long projectId, long userId, CancellationToken ct)
    {
        var project = await projectRepository.GetProjectWithMembersAsync(projectId, ct)
            ?? throw new NotFoundException(nameof(Project), projectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");
    }

    private async Task<PantryItem> LoadOwnedItemAsync(long pantryItemId, long userId, CancellationToken ct)
    {
        var item = await pantryRepository.GetByIdAsync(pantryItemId, ct)
            ?? throw new NotFoundException(nameof(PantryItem), pantryItemId);

        if (!item.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        return item;
    }
}
