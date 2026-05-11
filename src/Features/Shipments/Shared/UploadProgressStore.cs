using System.Collections.Concurrent;

namespace Shipment.Features.Shipments.Shared;

public sealed class UploadProgressStore
{
    private readonly ConcurrentDictionary<Guid, UploadProgress> store = new();

    public UploadProgress Create(Guid id)
    {
        var progress = new UploadProgress { UploadId = id };
        store[id] = progress;
        return progress;

    }

    public UploadProgress? GetId(Guid id) => store.TryGetValue(id, out var p) ? p : null;
}