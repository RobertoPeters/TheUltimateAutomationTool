using JasperFx.Core.Reflection;

namespace Tuat;

public static class Generic
{
    public class TypeDisplayName
    {
        public string TypeName { get; set; } = null!;
        public Type Type { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? SettingsEditorType { get; set; } = null!;
        public Type? SettingsEditorComponentType { get; set; } = null!;
        public string? EditorType { get; set; } = null!;
        public Type? EditorComponentType { get; set; } = null!;
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
                            var editorControl = type.GetAttribute<System.ComponentModel.EditorAttribute>();
                            items.Add(new TypeDisplayName
                            {
                                TypeName = type.FullName!,
                                Type = type,
                                DisplayName = attribute != null ? attribute.DisplayName : type.Name,
                                SettingsEditorType = editorControl?.EditorTypeName,
                                SettingsEditorComponentType = ComponentType(editorControl?.EditorTypeName)
                            });
                        }
                        _clientTypeDisplayNames = items.OrderBy(x => x.DisplayName).ToList();

                    }
                }
            }
            return _clientTypeDisplayNames!;
        }
    }

    private static List<TypeDisplayName> _automationTypeDisplayNames = null!;
    public static List<TypeDisplayName> AutomationTypeDisplayNames
    {
        get
        {
            if (_automationTypeDisplayNames == null)
            {
                lock (_lockObject)
                {
                    if (_automationTypeDisplayNames == null)
                    {
                        var interfaceType = typeof(Tuat.Interfaces.IAutomationHandler);
                        var types = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(s => s.GetTypes())
                            .Where(p => interfaceType.IsAssignableFrom(p));

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

                            var editorInterfaceType = typeof(Tuat.Interfaces.IAutomationEditor);
                            var settingsEditorInterfaceType = typeof(Tuat.Interfaces.IAutomationSettings);
                            var settingsEditorControl = bothComponents.First(x => settingsEditorInterfaceType.IsAssignableFrom(x));
                            var editorControl = bothComponents.First(x => editorInterfaceType.IsAssignableFrom(x));

                            items.Add(new TypeDisplayName
                            {
                                TypeName = type.FullName!,
                                Type = type,
                                DisplayName = attribute != null ? attribute.DisplayName : type.Name,
                                SettingsEditorType = settingsEditorControl?.FullName,
                                SettingsEditorComponentType = settingsEditorControl,
                                EditorType = editorControl?.FullName,
                                EditorComponentType = editorControl
                            });
                        }
                        _automationTypeDisplayNames = items.OrderBy(x => x.DisplayName).ToList();

                    }
                }
            }
            return _automationTypeDisplayNames!;
        }
    }

    private static List<TypeDisplayName> _scriptTypeDisplayNames = null!;
    public static List<TypeDisplayName> ScriptTypeDisplayNames
    {
        get
        {
            if (_scriptTypeDisplayNames == null)
            {
                lock (_lockObject)
                {
                    if (_scriptTypeDisplayNames == null)
                    {
                        var interfaceType = typeof(Tuat.Interfaces.IScriptEngine);
                        var types = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(s => s.GetTypes())
                            .Where(p => interfaceType.IsAssignableFrom(p));

                        List<TypeDisplayName> items = [];
                        foreach (var type in types)
                        {
                            if (type.IsInterface || type.IsAbstract)
                            {
                                continue;
                            }

                            var attribute = type.GetAttribute<System.ComponentModel.DisplayNameAttribute>();
                            var editorControl = type.GetAttribute<System.ComponentModel.EditorAttribute>();
                            items.Add(new TypeDisplayName
                            {
                                TypeName = type.FullName!,
                                Type = type,
                                DisplayName = attribute != null ? attribute.DisplayName : type.Name,
                                EditorType = editorControl?.EditorTypeName,
                                EditorComponentType = ComponentType(editorControl?.EditorTypeName)
                            });
                        }
                        _scriptTypeDisplayNames = items.OrderBy(x => x.DisplayName).ToList();

                    }
                }
            }
            return _scriptTypeDisplayNames!;
        }
    }

}
