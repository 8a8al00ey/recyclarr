using CliFx.Attributes;
using CliFx.Exceptions;
using JetBrains.Annotations;
using Recyclarr.Config;
using Serilog;
using TrashLib.Extensions;
using TrashLib.Sonarr;
using TrashLib.Sonarr.Config;
using TrashLib.Sonarr.QualityDefinition;
using TrashLib.Sonarr.ReleaseProfile;

namespace Recyclarr.Command;

[Command("sonarr", Description = "Perform operations on a Sonarr instance")]
[UsedImplicitly]
public class SonarrCommand : ServiceCommand
{
    [CommandOption("list-release-profiles", Description =
        "List available release profiles from the guide in YAML format.")]
    public bool ListReleaseProfiles { get; [UsedImplicitly] set; }

    // The default value is "empty" because I need to know when the user specifies the option but no value with it.
    // Discussed here: https://github.com/Tyrrrz/CliFx/discussions/128#discussioncomment-2647015
    [CommandOption("list-terms", Description =
        "For the given Release Profile Trash ID, list terms in it that can be filtered in YAML format. " +
        "Note that not every release profile has terms that may be filtered.")]
    public string? ListTerms { get; [UsedImplicitly] set; } = "empty";

    public override string Name => "Sonarr";

    public override async Task Process(IServiceLocatorProxy container)
    {
        await base.Process(container);

        var lister = container.Resolve<IReleaseProfileLister>();
        var profileUpdaterFactory = container.Resolve<Func<IReleaseProfileUpdater>>();
        var qualityUpdaterFactory = container.Resolve<Func<ISonarrQualityDefinitionUpdater>>();
        var configLoader = container.Resolve<IConfigurationLoader<SonarrConfiguration>>();
        var log = container.Resolve<ILogger>();

        if (ListReleaseProfiles)
        {
            lister.ListReleaseProfiles();
            return;
        }

        if (ListTerms != "empty")
        {
            if (!string.IsNullOrEmpty(ListTerms))
            {
                lister.ListTerms(ListTerms);
            }
            else
            {
                throw new CommandException(
                    "The --list-terms option was specified without a Release Profile Trash ID specified");
            }

            return;
        }

        foreach (var config in configLoader.LoadMany(Config, "sonarr"))
        {
            log.Information("Processing server {Url}", FlurlLogging.SanitizeUrl(config.BaseUrl));

            if (config.ReleaseProfiles.Count > 0)
            {
                await profileUpdaterFactory().Process(Preview, config);
            }

            if (config.QualityDefinition.HasValue)
            {
                await qualityUpdaterFactory().Process(Preview, config);
            }
        }
    }
}
