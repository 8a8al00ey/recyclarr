using System.IO.Abstractions.TestingHelpers;
using AutoFixture.NUnit3;
using CliFx.Infrastructure;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Recyclarr.Command;
using TestLibrary.AutoFixture;
using TrashLib;

// ReSharper disable MethodHasAsyncOverload

namespace Recyclarr.Tests.Command;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class CreateConfigCommandTest
{
    [Test, AutoMockData]
    public async Task Config_file_created_when_using_default_path(
        [Frozen] IAppPaths paths,
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        CreateConfigCommand cmd)
    {
        const string ymlPath = "path/recyclarr.yml";
        paths.ConfigPath.Returns(ymlPath);
        await cmd.ExecuteAsync(Substitute.For<IConsole>());

        var file = fs.GetFile(ymlPath);
        file.Should().NotBeNull();
        file.Contents.Should().NotBeEmpty();
    }

    [Test, AutoMockData]
    public async Task CreateConfig_SpecifyPath_FileIsCreated(
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        CreateConfigCommand cmd)
    {
        const string ymlPath = "some/other/path.yml";
        cmd.Path = ymlPath;

        await cmd.ExecuteAsync(Substitute.For<IConsole>()).ConfigureAwait(false);

        var file = fs.GetFile(ymlPath);
        file.Should().NotBeNull();
        file.Contents.Should().NotBeEmpty();
    }
}
