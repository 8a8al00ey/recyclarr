using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Autofac;
using Autofac.Core;
using CliFx.Infrastructure;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using VersionControl;

namespace Recyclarr.Tests;

public record ServiceFactoryWrapper(Type Service, Action<ILifetimeScope> Instantiate);

public static class FactoryForService<TService>
{
    public static ServiceFactoryWrapper WithArgs<TP1>(TP1 arg1 = default!)
    {
        return new ServiceFactoryWrapper(typeof(TService),
            c => c.Resolve<Func<TP1, TService>>().Invoke(arg1));
    }
}

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class CompositionRootTest
{
    private static readonly List<ServiceFactoryWrapper> FactoryTests = new()
    {
        FactoryForService<IGitRepository>.WithArgs("path")
    };

    [TestCaseSource(typeof(CompositionRootTest), nameof(FactoryTests))]
    public void Service_requiring_factory_should_be_instantiable(ServiceFactoryWrapper service)
    {
        var act = () =>
        {
            using var container = new CompositionRoot().Setup("", Substitute.For<IConsole>(), default).Container;
            service.Instantiate(container);
        };

        act.Should().NotThrow();
    }

    // Warning CA1812 : CompositionRootTest.ConcreteTypeEnumerator is an internal class that is apparently never
    // instantiated.
    [SuppressMessage("Performance", "CA1812",
        Justification = "Created via reflection by TestCaseSource attribute"
    )]
    private sealed class ConcreteTypeEnumerator : IEnumerable
    {
        private readonly ILifetimeScope _container;

        public ConcreteTypeEnumerator()
        {
            _container = new CompositionRoot().Setup("", Substitute.For<IConsole>(), default).Container;
        }

        public IEnumerator GetEnumerator()
        {
            return _container.ComponentRegistry.Registrations
                .SelectMany(x => x.Services)
                .OfType<TypedService>()
                .Select(x => x.ServiceType)
                .Distinct()
                .Except(FactoryTests.Select(x => x.Service))
                .Where(x => x.FullName == null || !x.FullName.StartsWith("Autofac."))
                .GetEnumerator();
        }
    }

    [TestCaseSource(typeof(ConcreteTypeEnumerator))]
    public void Service_should_be_instantiable(Type service)
    {
        using var container = new CompositionRoot().Setup("", Substitute.For<IConsole>(), default).Container;
        container.Invoking(c => c.Resolve(service))
            .Should().NotThrow()
            .And.NotBeNull();
    }
}
