using System;
using System.Collections.Generic;
using System.Linq;
using Sage.Entity;

namespace Sage.Repository;

/// <summary>In-memory Stationpedia repository. Used by tests.</summary>
public sealed class InMemoryStationpediaRepository : IStationpediaRepository
{
    private readonly List<StationpediaEntry> _entries;

    public InMemoryStationpediaRepository(IEnumerable<StationpediaEntry> entries)
    {
        _entries = (entries ?? throw new ArgumentNullException(nameof(entries))).ToList();
    }

    public void Add(StationpediaEntry entry)
    {
        _entries.Add(entry);
    }

    public void Update(StationpediaEntry entry)
    {
        _entries.RemoveAll(existing => existing.Key == entry.Key);
        _entries.Add(entry);
    }

    public IReadOnlyList<StationpediaEntry> GetAll()
    {
        return _entries;
    }

    public StationpediaEntry? GetByKey(string? key)
    {
        return _entries.FirstOrDefault(entry => entry.Key == key);
    }
}
