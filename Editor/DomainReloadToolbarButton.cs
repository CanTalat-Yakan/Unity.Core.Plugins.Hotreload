#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UnityEssentials
{
    [InitializeOnLoad]
    internal static class DomainReloadToolbarButton
    {
        static DomainReloadToolbarButton() =>
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

        private static void OnToolbarGUI()
        {
            bool busy = EditorApplication.isCompiling || EditorApplication.isUpdating;

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(busy))
            {
                GUIContent reloadContent = EditorGUIUtility.IconContent("Refresh");
                reloadContent.tooltip = busy ? "Domain Reload (busy)" : "Domain Reload";

                if (GUILayout.Button(reloadContent))
                    EditorUtility.RequestScriptReload();

                GUILayout.Space(5);
            }
        }
    }
}
#endif
