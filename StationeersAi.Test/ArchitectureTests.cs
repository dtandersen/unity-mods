using System;
using System.Linq;
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.TUnit;
using Shouldly;
using Caev;
using StationeersAi.Common;
using StationeersAi.Core;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace StationeersAi.Test;

/// <summary>
/// Guards the layering of the Unity-free libraries: the agent engine
/// (<c>StationeersAi.Ai</c>) sees game data only as per-turn arguments and
/// references <c>Common</c> alone; the domain (<c>Core</c>) may use the
/// engine but never the game; <c>Common</c> references nothing. None of them
/// may touch BepInEx, Harmony, Unity or Stationeers, so all logic can be
/// specified and tested without the game running.
/// </summary>
public class ArchitectureTests
{
    private static readonly string[] ForbiddenAssemblyPrefixes =
    {
        "BepInEx",
        "0Harmony",
        "HarmonyLib",
        "Assembly-CSharp",
        "UnityEngine",
        "Unity.",
    };

    // Regex alternative of the namespaces the core must never depend on.
    private const string ForbiddenNamespacePattern =
        @"^(BepInEx|HarmonyLib|UnityEngine|Unity|Assets\.Scripts|Reagents|Util\.Commands|WorldManager)";

    // Load the Unity-free architectures once; ArchUnitNET analyses the compiled IL.
    private static readonly Architecture CoreArchitecture = new ArchLoader()
        .LoadAssemblies(typeof(CoreAssembly).Assembly, typeof(CaevAssembly).Assembly, typeof(CommonAssembly).Assembly)
        .Build();

    [Test]
    public void Libraries_should_not_reference_forbidden_assemblies()
    {
        // Complement to the type-level ArchUnitNET rule below: catches assembly
        // references even when no type from the referenced assembly is used.
        var offenders = new[] { typeof(CoreAssembly).Assembly, typeof(CaevAssembly).Assembly, typeof(CommonAssembly).Assembly }
            .SelectMany(assembly => assembly.GetReferencedAssemblies())
            .Select(assembly => assembly.Name)
            .Where(name => ForbiddenAssemblyPrefixes.Any(prefix =>
                name.StartsWith(prefix, StringComparison.Ordinal)))
            .Distinct()
            .OrderBy(name => name)
            .ToArray();

        offenders.ShouldBeEmpty(
            $"The Unity-free libraries must not reference BepInEx/Stationeers/Unity assemblies. Found: {string.Join(", ", offenders)}");
    }

    [Test]
    public void Ai_should_reference_only_common()
    {
        // The engine sees game data only as per-turn arguments: besides the
        // framework it may reference shared primitives, nothing else.
        var offenders = typeof(CaevAssembly).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .Where(name => name.StartsWith("StationeersAi.", StringComparison.Ordinal) && name != CommonAssembly.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToArray();

        offenders.ShouldBeEmpty(
            $"Caev must reference only StationeersAi.Common. Found: {string.Join(", ", offenders)}");
    }

    [Test]
    public void Common_should_reference_nothing()
    {
        var offenders = typeof(CommonAssembly).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .Where(name => name.StartsWith("StationeersAi.", StringComparison.Ordinal))
            .Distinct()
            .OrderBy(name => name)
            .ToArray();

        offenders.ShouldBeEmpty(
            $"StationeersAi.Common must not reference other project assemblies. Found: {string.Join(", ", offenders)}");
    }

    [Test]
    public void Libraries_should_not_depend_on_bepinex_stationeers_or_unity_namespaces()
    {
        IArchRule rule = Types()
            .That().ResideInNamespaceMatching(@"^StationeersAi\.(Ai|Command|Common|Core)")
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(ForbiddenNamespacePattern)
            .Because("the Unity-free libraries must stay free of BepInEx/Stationeers/Unity so they are testable without the game");

        rule.Check(CoreArchitecture);
    }
}
