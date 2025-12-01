using JasperFx.Core.Reflection;
using Tuat.Helpers.Generics;

namespace Tuat.FlowAutomation;

public static class Generic
{
    private static readonly object _lockObject = new object();

    private static List<TypeDisplayName> _stepTypeDisplayNames = null!;
    public static List<TypeDisplayName> StepTypeDisplayNames
    {
        get
        {
            if (_stepTypeDisplayNames == null)
            {
                lock (_lockObject)
                {
                    if (_stepTypeDisplayNames == null)
                    {
                        var types = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(s => s.GetTypes())
                            .Where(p => p.BaseType == typeof(Step));

                        List<TypeDisplayName> items = [];
                        foreach (var type in types)
                        {
                            if (type.IsInterface || type.IsAbstract)
                            {
                                continue;
                            }

                            var attribute = type.GetAttribute<System.ComponentModel.DisplayNameAttribute>();
                            var editorControls = type.GetAllAttributes<System.ComponentModel.EditorAttribute>();
                            var bothComponents = editorControls.Select(x => Tuat.Helpers.Generics.Generic.ComponentType(x.EditorTypeName)).Where(x => x != null).ToList();

                            var settingsEditorControl = bothComponents.First();

                            items.Add(new TypeDisplayName
                            {
                                TypeName = type.FullName!,
                                Type = type,
                                DisplayName = attribute != null ? attribute.DisplayName : type.Name,
                                SettingsEditorType = settingsEditorControl?.FullName,
                                SettingsEditorComponentType = settingsEditorControl,
                            });
                        }
                        _stepTypeDisplayNames = items.OrderBy(x => x.DisplayName).ToList();

                    }
                }
            }
            return _stepTypeDisplayNames!;
        }
    }

}
