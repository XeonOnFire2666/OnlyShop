using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Common;

internal static class CategoryTree
{
    public static IReadOnlyCollection<Guid> GetSelfAndDescendantIds(IReadOnlyList<Category> all, Guid rootId)
    {
        var result = new HashSet<Guid> { rootId };
        var queue = new Queue<Guid>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var child in all.Where(c => c.ParentId == current))
            {
                if (result.Add(child.Id))
                {
                    queue.Enqueue(child.Id);
                }
            }
        }

        return result;
    }
}