﻿using System;
using System.IO;
using System.IO.Abstractions;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Trash.Config;
using Trash.Radarr;
using YamlDotNet.Core;
using YamlDotNet.Serialization.ObjectFactories;

namespace Trash.Tests.Radarr
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class RadarrConfigurationTest
    {
        [Test]
        public void Custom_format_names_list_is_required()
        {
            const string testYaml = @"
radarr:
  - api_key: abc
    base_url: xyz
    custom_formats:
      - quality_profiles:
          - name: MyProfile
";

            var configLoader = new ConfigurationLoader<RadarrConfiguration>(
                Substitute.For<IConfigurationProvider>(),
                Substitute.For<IFileSystem>(), new DefaultObjectFactory());

            Action act = () => configLoader.LoadFromStream(new StringReader(testYaml), "radarr");

            act.Should().Throw<YamlException>();
        }

        [Test]
        public void Quality_definition_type_is_required()
        {
            const string yaml = @"
radarr:
- base_url: a
  api_key: b
  quality_definition:
    preferred_ratio: 0.5
";
            var loader = new ConfigurationLoader<RadarrConfiguration>(
                Substitute.For<IConfigurationProvider>(),
                Substitute.For<IFileSystem>(),
                new DefaultObjectFactory());

            Action act = () => loader.LoadFromStream(new StringReader(yaml), "radarr");

            act.Should().Throw<YamlException>()
                .WithMessage("*'type' is required for 'quality_definition'");
        }

        [Test]
        public void Quality_profile_name_is_required()
        {
            const string testYaml = @"
radarr:
  - api_key: abc
    base_url: xyz
    custom_formats:
      - names: [one, two]
        quality_profiles:
          - score: 100
";

            var configLoader = new ConfigurationLoader<RadarrConfiguration>(
                Substitute.For<IConfigurationProvider>(),
                Substitute.For<IFileSystem>(), new DefaultObjectFactory());

            Action act = () => configLoader.LoadFromStream(new StringReader(testYaml), "radarr");

            act.Should().Throw<YamlException>();
        }
    }
}
