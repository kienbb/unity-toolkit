using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FTCustomEditorWindow : OdinMenuEditorWindow
{
    [MenuItem("FreeTime/Custom Editor")]
    private static void OpenWindow()
    {
        GetWindow<FTCustomEditorWindow>().Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Selection.SupportsMultiSelect = false;

        tree.Add("Settings", GeneralDrawerConfig.Instance);
        tree.Add("Utilities", new TextureUtilityEditor());
        tree.AddAllAssetsAtPath("Odin Settings", "Assets/Plugins/Sirenix", typeof(ScriptableObject), true, true);
        return tree;
    }
}

public class TextureUtilityEditor
{
    [BoxGroup("Tool"), HideLabel, EnumToggleButtons]
    public Tool Tool;

    public List<Texture> Textures;

    [Button(ButtonSizes.Large), HideIf("Tool", Tool.Rotate)]
    public void SomeAction() { }

    [Button(ButtonSizes.Large), ShowIf("Tool", Tool.Rotate)]
    public void SomeOtherAction() { }
}