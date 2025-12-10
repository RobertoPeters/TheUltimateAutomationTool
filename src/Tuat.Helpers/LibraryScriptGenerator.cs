using System.Text;
using Tuat.Interfaces;

namespace Tuat.Helpers;

public static class LibraryScriptGenerator
{
    public static string GenerateScriptCode(IDataService dataService, int? rootLibraryId, List<int>? allLibraryIds)
    {
        var content = new StringBuilder();
        List<int> addedLibraryIds = [];

        if (allLibraryIds != null)
        {
            foreach (var libraryId in allLibraryIds)
            {
                GenerateScriptCode(content, dataService, libraryId, addedLibraryIds);
            }
        }

        if (rootLibraryId != null)
        {
            GenerateScriptCode(content, dataService, rootLibraryId.Value, addedLibraryIds);
        }

        return content.ToString();
    }

    public static void GenerateScriptCode(StringBuilder content, IDataService dataService, int rootLibraryId, List<int> addedLibraryIds)
    {
        if (addedLibraryIds.Contains(rootLibraryId))
        {
            return;
        }

        addedLibraryIds.Add(rootLibraryId);

        var libToInclude = dataService.GetLibraries().FirstOrDefault(l => l.Id == rootLibraryId);
        if (libToInclude == null)
        {
            return;
        }

        foreach (var libIdToInclude in libToInclude.IncludeScriptIds)
        {
            GenerateScriptCode(content, dataService, libIdToInclude, addedLibraryIds);
        }

        content.AppendLine();
        content.AppendLine(libToInclude.Script ?? "");
    }
}
