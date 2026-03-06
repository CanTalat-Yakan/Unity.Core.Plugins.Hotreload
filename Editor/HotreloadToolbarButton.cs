#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEssentials
{
    [InitializeOnLoad]
    internal static class HotreloadToolbarButton
    {
        static HotreloadToolbarButton() =>
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

        private static void OnToolbarGUI()
        {
            bool running = IsHotReloadRunning();
            bool busy = IsHotReloadBusy();

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(busy))
            {
                GUIContent recompileContent = GetRecompileIcon();
                recompileContent.tooltip = running ? "Recompile" : "Start Hot Reload";
                if (GUILayout.Button(recompileContent))
                    ExecuteStartOrRecompile();

                GUILayout.Space(5);
            }
        }

        internal static bool IsHotReloadRunning() =>
            GetEditorCodePatcherBool("Running");

        internal static bool IsHotReloadBusy() =>
            GetEditorCodePatcherBool("Starting") || GetEditorCodePatcherBool("Stopping");

        internal static void ExecuteStartOrRecompile()
        {
            if (IsHotReloadRunning())
            {
                InvokeHotReloadRunTabMethod("RecompileWithChecks");
                return;
            }

            InvokeEditorCodePatcherMethod("DownloadAndRun");
        }

        private static GUIContent GetRecompileIcon()
        {
            GUIContent icon = EditorGUIUtility.IconContent("Refresh");
            icon.tooltip = "Hot Reload";
            return icon;
        }

        private static bool GetEditorCodePatcherBool(string propertyName)
        {
            Type type = GetEditorCodePatcherType();
            if (type == null)
                return false;

            PropertyInfo prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            return prop?.GetValue(null) is bool value && value;
        }

        private static void InvokeEditorCodePatcherMethod(string methodName)
        {
            Type type = GetEditorCodePatcherType();
            MethodInfo method = type?.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
                return;

            ParameterInfo[] parameters = method.GetParameters();
            object[] args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                if (parameter.HasDefaultValue)
                    args[i] = parameter.DefaultValue;
                else if (parameter.ParameterType.IsValueType)
                    args[i] = Activator.CreateInstance(parameter.ParameterType);
                else
                    args[i] = null;
            }

            method.Invoke(null, args);
        }

        private static Type GetEditorCodePatcherType()
        {
            Assembly hotReloadEditorAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, "SingularityGroup.HotReload.Editor", StringComparison.Ordinal));

            return hotReloadEditorAssembly?.GetType("SingularityGroup.HotReload.Editor.EditorCodePatcher");
        }

        private static void InvokeHotReloadRunTabMethod(string methodName)
        {
            Type type = GetHotReloadRunTabType();
            MethodInfo method = type?.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
                return;

            method.Invoke(null, null);
        }

        private static Type GetHotReloadRunTabType()
        {
            Assembly hotReloadEditorAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, "SingularityGroup.HotReload.Editor", StringComparison.Ordinal));

            return hotReloadEditorAssembly?.GetType("SingularityGroup.HotReload.Editor.HotReloadRunTab");
        }
    }
}
#endif