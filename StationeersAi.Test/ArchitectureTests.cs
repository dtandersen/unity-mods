using System;
using System.Linq;
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.TUnit;
using Shouldly;
using StationeersAi.Core;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace StationeersAi.Test;

/// <summary>
/// Guards the defining constraint of the core library:
/// <c>StationeersAi.Core</c> must stay free of BepInEx, Harmony, Unity and
/// Stationeers so that all logic can be specified and tested without the game
/// running.
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

    // Load the core architecture once; ArchUnitNET analyses the compiled IL.
    private static readonly Architecture CoreArchitecture = new ArchLoader()
        .LoadAssemblies(typeof(CoreAssembly).Assembly)
        .Build();

    [Test]
    public void Core_should_not_reference_forbidden_assemblies()
    {
        // Complement to the type-level ArchUnitNET rule below: catches assembly
        // references even when no type from the referenced assembly is used.
        var offenders = typeof(CoreAssembly).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .Where(name => ForbiddenAssemblyPrefixes.Any(prefix =>
                name.StartsWith(prefix, StringComparison.Ordinal)))
            .OrderBy(name => name)
            .ToArray();

        offenders.ShouldBeEmpty(
            $"StationeersAi.Core must not reference BepInEx/Stationeers/Unity assemblies. Found: {string.Join(", ", offenders)}");
    }

    [Test]
    public void Core_should_not_depend_on_bepinex_stationeers_or_unity_namespaces()
    {
        IArchRule rule = Types()
            .That().ResideInNamespaceMatching(@"^StationeersAi\.Core")
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(ForbiddenNamespacePattern)
            .Because("StationeersAi.Core must stay free of BepInEx/Stationeers/Unity so it is testable without the game");

        rule.Check(CoreArchitecture);
    }
}
