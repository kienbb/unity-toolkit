using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace AdOne.Editor
{
    public class CustomPrebuildOrder : IPreprocessBuildWithReport
    {
        #region Properties

        #endregion

        #region Functions

        #endregion
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            //ToyMenu.ConvertAllTablesToBinary();
            //SkinAssetManager.Instance.LoadAllTowerSkinGameObject();

            //AssetDatabase.SaveAssets();
        }
    }
}
