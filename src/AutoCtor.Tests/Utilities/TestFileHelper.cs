using System.Xml.Linq;

public static class TestFileHelper
{
    public static DirectoryInfo? BaseDir { get; } = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent;

    private static XDocument? _packageVersionDoc;
    private static XDocument PackageVersionDoc
    {
        get
        {
            if (_packageVersionDoc is null)
            {
                var srcDir = BaseDir?.Parent;
                var packageVersionsFile = Path.Combine(srcDir?.FullName ?? "", "Directory.Packages.props");
                _packageVersionDoc = XDocument.Load(packageVersionsFile);
            }
            return _packageVersionDoc;
        }
    }

    public static string GetPackageVersion(string packageName)
    {
        var versionXml = PackageVersionDoc.Root?.Descendants("PackageVersion")
            .FirstOrDefault(el => el.Attribute("Include")?.Value == packageName);

        return versionXml?.Attribute("Version")?.Value
            ?? throw new Exception($"{packageName} missing from Directory.Packages.props");
    }
}
