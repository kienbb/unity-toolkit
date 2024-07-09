using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Debug = UnityEngine.Debug;

namespace AdOne.Editor
{
    public class AbiCustomMenu
    {
        [MenuItem("AdOne/Open Save Folder", priority = 40)]
        private static void OpenPersistentFolder()
        {
            //EditorUtility.RevealInFinder(Application.persistentDataPath);
            Process process = new Process();
            process.StartInfo.FileName = ((Application.platform == RuntimePlatform.WindowsEditor) ? "explorer.exe" : "open");
            process.StartInfo.Arguments = "file://" + Application.persistentDataPath;
            process.Start();
        }
    }


    public class ToyMenu : OdinEditorWindow
    {

        public double ADouble;

        [MenuItem("AdOne/Toy Menu AXZCASAQASD", priority = 100)]
        private static void ShowWindow()
        {
            var window = GetWindow<ToyMenu>();

            // Nifty little trick to quickly position the window in the middle of the editor.
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(400, 700);
        }
        [FoldoutGroup("ANIMATION CURVE")]
        public AnimationCurve curve;
        [FoldoutGroup("ANIMATION CURVE")]
        [Button(ButtonHeight = 50)]
        private void LogAllKeyframes()
        {
            if (curve == null)
                return;
            for (int i = 0; i < curve.keys.Length; i++)
            {
                PrintKeyFrame(curve.keys[i]);
            }
        }

        private void PrintKeyFrame(Keyframe key)
        {
            Debug.LogError($"time {key.time} | value {key.value} | inTangent {key.inTangent} | inWeight {key.inWeight}| outTangent {key.outTangent}| outWeight {key.outWeight} ");
        }

        //[FoldoutGroup("REWORK LEVEL HP")]
        //[Button]
        //void ReworkLevelCampaignHp()
        //{
        //    AssetDatabase.SaveAssets();
        //    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        //}

    }
}
