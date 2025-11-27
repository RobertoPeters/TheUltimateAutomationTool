using JasperFx.Core.Reflection;

namespace Tuat.FlowAutomation;

public static class Generic
{
    public class TypeDisplayName
    {
        public string TypeName { get; set; } = null!;
        public Type Type { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? SettingsEditorType { get; set; } = null!;
        public Type? SettingsEditorComponentType { get; set; } = null!;
    }

    private static readonly object _lockObject = new object();

    private static Type? ComponentType(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }

        var asm = (from a in AppDomain.CurrentDomain.GetAssemblies()
                   where a.GetTypes().Any(x => x.FullName == typeName)
                   select a).FirstOrDefault();

        if (asm == null)
        {
            return null;
        }

        return asm.GetTypes().First(x => x.FullName == typeName);
    }

    private static List<TypeDisplayName> _clientTypeDisplayNames = null!;
    public static List<TypeDisplayName> ClientTypeDisplayNames
    {
        get
        {
            if (_clientTypeDisplayNames == null)
            {
                lock(_lockObject)
                {
                    if (_clientTypeDisplayNames == null)
                    {
                        var interfaceType = typeof(Tuat.Interfaces.IClientHandler);
                        var types = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(s => s.GetTypes())
                            .Where(p => interfaceType.IsAssignableFrom(p));

                        List<TypeDisplayName> items = [];
                        foreach (var type in types)
                        {
                            if (type .IsInterface || type.IsAbstract)
                            {
                                continue;
                            }

                            var attribute = type.GetAttribute<System.ComponentModel.DisplayNameAttribute>();
                            items.Add(new TypeDisplayName
                            {
                                TypeName = type.FullName!,
                                Type = type,
                                DisplayName = attribute != null ? attribute.DisplayName : type.Name,
                            });
                        }
                        _clientTypeDisplayNames = items;

                    }
                }
            }
            return _clientTypeDisplayNames!;
        }
    }

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
                            var bothComponents = editorControls.Select(x => ComponentType(x.EditorTypeName)).Where(x => x != null).ToList();

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
                        _stepTypeDisplayNames = items;

                    }
                }
            }
            return _stepTypeDisplayNames!;
        }
    }

}
