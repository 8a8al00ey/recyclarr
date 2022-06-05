using System.IO.Abstractions;
using System.IO.Abstractions.Extensions;
using System.IO.Abstractions.TestingHelpers;
using AutoFixture.NUnit3;
using Common;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using TestLibrary.AutoFixture;
using TrashLib;

namespace Recyclarr.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class ConfigurationFinderTest
{
    [Test, AutoMockData]
    public void Return_path_next_to_executable_if_present(
        [Frozen] IAppContext appContext,
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        [Frozen] IAppPaths paths,
        ConfigurationFinder sut)
    {
        var basePath = fs.Path.Combine("base", "path");
        var baseYaml = fs.Path.Combine(basePath, "recyclarr.yml");

        paths.DefaultConfigFilename.Returns("recyclarr.yml");
        appContext.BaseDirectory.Returns(basePath);
        fs.AddFile(baseYaml, new MockFileData(""));

        var path = sut.FindConfigPath();

        path.Should().EndWith(baseYaml);
    }

    [Test, AutoMockData]
    public void Return_app_data_dir_location_if_base_directory_location_not_present(
        [Frozen] IAppContext appContext,
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        [Frozen] IAppPaths paths,
        ConfigurationFinder sut)
    {
        var appYaml = fs.Path.Combine("app", "data", "recyclarr.yml");

        paths.ConfigPath.Returns(appYaml);

        var path = sut.FindConfigPath();

        path.Should().EndWith(appYaml);
    }

    [Test, AutoMockData]
    public void Return_base_directory_location_if_both_files_are_present(
        [Frozen] IAppContext appContext,
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        [Frozen] IAppPaths paths,
        ConfigurationFinder sut)
    {
        var appPath = fs.Path.Combine("app", "data");
        var basePath = fs.Path.Combine("base", "path");
        var baseYaml = fs.Path.Combine(basePath, "recyclarr.yml");
        var appYaml = fs.Path.Combine(appPath, "recyclarr.yml");

        paths.DefaultConfigFilename.Returns("recyclarr.yml");
        appContext.BaseDirectory.Returns(basePath);
        fs.AddFile(baseYaml, new MockFileData(""));
        fs.AddFile(appYaml, new MockFileData(""));

        var path = sut.FindConfigPath();

        path.Should().EndWith(baseYaml);
    }

    [Test, AutoMockData]
    public void All_configs_includes_dir_and_default_yaml_when_exists(
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        [Frozen] IAppPaths paths,
        ConfigurationFinder sut)
    {
        var defaultYaml = fs.CurrentDirectory().SubDirectory("config").File("one.yml").FullName;
        var configsDir = fs.CurrentDirectory().SubDirectory("config").SubDirectory("configs");
        var configYaml1 = configsDir.File("two.yml").FullName;
        var configYaml2 = configsDir.File("three.yml").FullName;

        paths.ConfigPath.Returns(defaultYaml);
        paths.ConfigsDirectory.Returns(configsDir.FullName);

        fs.AddFile(defaultYaml, new MockFileData(""));
        fs.AddFile(configYaml1, new MockFileData(""));
        fs.AddFile(configYaml2, new MockFileData(""));

        var results = sut.FindAllConfigFiles();

        results.Should().BeEquivalentTo(defaultYaml, configYaml1, configYaml2);
    }

    [Test, AutoMockData]
    public void All_configs_includes_dir_only_when_default_yaml_not_exists(
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        [Frozen] IAppPaths paths,
        ConfigurationFinder sut)
    {
        // TODO - This implementation is copied from the test case above. Reimplement!!!
        var defaultYaml = fs.CurrentDirectory().SubDirectory("config").File("one.yml").FullName;
        var configsDir = fs.CurrentDirectory().SubDirectory("config").SubDirectory("configs");
        var configYaml1 = configsDir.File("two.yml").FullName;
        var configYaml2 = configsDir.File("three.yml").FullName;

        paths.ConfigPath.Returns(defaultYaml);
        paths.ConfigsDirectory.Returns(configsDir.FullName);

        fs.AddFile(configYaml1, new MockFileData(""));
        fs.AddFile(configYaml2, new MockFileData(""));

        var results = sut.FindAllConfigFiles();

        results.Should().BeEquivalentTo(configYaml1, configYaml2);
    }
}
