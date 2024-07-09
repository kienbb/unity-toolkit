using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityToolbarExtender.Examples
{
	static class ToolbarStyles
	{
		public static readonly GUIStyle commandButtonStyle;

		static ToolbarStyles()
		{
			commandButtonStyle = new GUIStyle("Command")
			{
				fontSize = 16,
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove,
				fontStyle = FontStyle.Bold,
				fixedWidth = 70
			};
		}
	}


	[InitializeOnLoad]
	public class SceneSwitchRightButton
	{
		static SceneSwitchRightButton()
		{
			ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUIRight);
		}
		static void OnToolbarGUIRight()
		{
			if (GUILayout.Button(new GUIContent($"PLAY", $"Play Games From Scene Splash"), ToolbarStyles.commandButtonStyle))
			{
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
					SceneHelper.StartScene("SplashScene");
			}
		}
	}


	[InitializeOnLoad]
	public class SceneSwitchLeftButton
	{
		static SceneSwitchLeftButton()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUILeft);
		}

		static void OnToolbarGUILeft()
		{
			GUILayout.FlexibleSpace();
   //         if (GUILayout.Button(new GUIContent($"BakeFx", $"Mở Scene Bake Fx"), ToolbarStyles.commandButtonStyle))
   //         {
   //             string[] guids = AssetDatabase.FindAssets("t:scene " + "Studio", null);
   //             string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
   //             if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
   //                 EditorSceneManager.OpenScene(scenePath);
   //         }

   //         if (GUILayout.Button(new GUIContent($"Map", $"Mở Scene Bake Light Để Làm Map"), ToolbarStyles.commandButtonStyle))
			//{
			//	string[] guids = AssetDatabase.FindAssets("t:scene " + "z_BakeLight", null);
			//	string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
			//	if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			//		EditorSceneManager.OpenScene(scenePath);
			//}

			//if (GUILayout.Button(new GUIContent($"Splash", $"Open Scene Splash"), ToolbarStyles.commandButtonStyle))
			//{
			//	string[] guids = AssetDatabase.FindAssets("t:scene " + "SplashScene", null);
			//	string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
			//	if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			//		EditorSceneManager.OpenScene(scenePath);
			//}

			//if (GUILayout.Button(new GUIContent($"Menu", $"Open Scene Menu"), ToolbarStyles.commandButtonStyle))
			//{
			//	string[] guids = AssetDatabase.FindAssets("t:scene " + "MenuScene", null);
			//	string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
			//	if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			//		EditorSceneManager.OpenScene(scenePath);
			//}

			//if (GUILayout.Button(new GUIContent($"Game", $"Open Scene Game"), ToolbarStyles.commandButtonStyle))
			//{
			//	string[] guids = AssetDatabase.FindAssets("t:scene " + "GameScene", null);
			//	string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
			//	if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			//		EditorSceneManager.OpenScene(scenePath);
			//}
		}
	}

	static class SceneHelper
	{
		static string sceneToOpen;

		public static void StartScene(string sceneName)
		{
			if (EditorApplication.isPlaying)
			{
				EditorApplication.isPlaying = false;
			}

			sceneToOpen = sceneName;
			EditorApplication.update += OnUpdate;
		}

		static void OnUpdate()
		{
			if (sceneToOpen == null ||
				EditorApplication.isPlaying || EditorApplication.isPaused ||
				EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			EditorApplication.update -= OnUpdate;

			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				// need to get scene via search because the path to the scene
				// file contains the package version so it'll change over time
				string[] guids = AssetDatabase.FindAssets("t:scene " + sceneToOpen, null);
				if (guids.Length == 0)
				{
					Debug.LogWarning("Couldn't find scene file");
				}
				else
				{
					string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
					EditorSceneManager.OpenScene(scenePath);
					EditorApplication.isPlaying = true;
				}
			}
			sceneToOpen = null;
		}
	}
}