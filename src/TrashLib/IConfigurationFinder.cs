namespace TrashLib;

public interface IConfigurationFinder
{
    string FindConfigPath();
    IEnumerable<string> FindAllConfigFiles();
}
