using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Query;

namespace StationeersAi.Command;

/// <summary>Ask for items by name fragment.</summary>
public sealed record SearchItemsRequest(string? Query);

/// <summary>The items that matched; empty when none did.</summary>
public sealed record ItemsResult(Status Status, List<string> Errors, IReadOnlyList<Item> Items) : BaseResult(Status, Errors);

/// <summary>Domain operation: search the item catalogue.</summary>
public sealed class SearchItemsCommand : Command<SearchItemsRequest, ItemsResult>
{
    private readonly ItemIndex _items;

    public SearchItemsCommand(ItemIndex items)
    {
        _items = items;
    }

    public ItemsResult Execute(SearchItemsRequest request)
    {
        return new ItemsResult(Status.SUCCESS, new List<string>(), _items.Search(request.Query));
    }
}
