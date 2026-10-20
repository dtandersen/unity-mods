using System;
using System.Collections.Generic;
using Shouldly;
using Sage.Repository;

namespace Sage.Test;

/// <summary>Name/id matching shared by the device tools.</summary>
public class ThingMatchingTests
{
    private sealed record ThingStub(string Name, long Id);

    private static readonly IReadOnlyList<ThingStub> Things = new[]
    {
        new ThingStub("Air Vent", 101),
        new ThingStub("Gas Sensor", 102),
        new ThingStub("Vent Controller", 103),
    };

    private static IReadOnlyList<ThingStub> Match(string device)
    {
        return ThingMatching.Match(Things, thing => thing.Name, thing => thing.Id, device);
    }

    [Test]
    public void Numeric_matches_reference_id()
    {
        Match("102").ShouldBe(new[] { new ThingStub("Gas Sensor", 102) });
    }

    [Test]
    public void Numeric_trims_whitespace()
    {
        Match("  101 ").ShouldBe(new[] { new ThingStub("Air Vent", 101) });
    }

    [Test]
    public void Unknown_id_matches_nothing()
    {
        Match("999").ShouldBeEmpty();
    }

    [Test]
    public void Exact_label_wins_case_insensitively()
    {
        Match("air vent").ShouldBe(new[] { new ThingStub("Air Vent", 101) });
    }

    [Test]
    public void Partial_label_matches()
    {
        Match("sensor").ShouldBe(new[] { new ThingStub("Gas Sensor", 102) });
    }

    [Test]
    public void Ambiguous_partial_matches_all()
    {
        Match("vent").Count.ShouldBe(2);
    }

    [Test]
    public void Blank_matches_nothing()
    {
        Match("  ").ShouldBeEmpty();
        Match(null).ShouldBeEmpty();
    }

    [Test]
    public void Throwing_accessors_are_skipped()
    {
        var things = new[]
        {
            new ThingStub("Air Vent", 101),
            new ThingStub("Bad", 102),
        };

        var matches = ThingMatching.Match(
            things,
            thing => thing.Name == "Bad" ? throw new InvalidOperationException("gone") : thing.Name,
            thing => thing.Id,
            "vent");

        matches.ShouldBe(new[] { new ThingStub("Air Vent", 101) });
    }
}
