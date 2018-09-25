using UnityEngine;
using UnityEditor;
using System;
using System.IO;

[InitializeOnLoad]
public class BuildHandler : EditorWindow
{
	static BuildHandler ()
	{
	}

	private static string projectName;

	private static string keystorePass;
	private static string aliasName;
	private static string aliasPass;
	private static int major = 1;
	private static int minor = 0;
	private static int build = 0;
	private static int revision = 0;
	private static Texture2D defaultIcon;
	private static Texture2D undergroundIcon;
	private static string bundleIdentifier;
	private static bool test;

	bool isHorizontalActive = true;

	void OnEnable ()
	{
		SetVersion ();
	}

	[MenuItem ("Window/Build Handler")]
	static void Init ()
	{
		EditorWindow.GetWindow (typeof (BuildHandler));
		SetVersion ();
	}

	#region User Interface
	void OnGUI ()
	{
		EditorGUILayout.Separator ();
		projectName = EditorGUILayout.TextField ("Project Name", projectName);
		Rect openFolder = EditorGUILayout.BeginHorizontal (GUILayout.MaxWidth (GUI.skin.label.CalcSize (new GUIContent ("Open Build Folder")).x));
		if (GUI.Button (openFolder, ""))
		{
			// TODO: Ensure that this works on Mac
			if (!Directory.Exists (Application.dataPath + "/../Builds"))
				Directory.CreateDirectory (Application.dataPath + "/../Builds");
			System.Diagnostics.Process.Start ("explorer.exe", (Application.dataPath + "/../Builds").Replace (@"/", @"\"));
		}
		EditorGUILayout.LabelField ("Open Build Folder");
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Separator ();

		Keystore ();
		AppSpecific ();
		IconConfiguration ();
		Versioning ();
		BuildButtons ();
	}

	private void AppSpecific ()
	{
		EditorGUILayout.Separator ();
		EditorGUILayout.Separator ();
		EditorGUILayout.LabelField ("App Specific Configuration");

		// Add any configuration for your app here
	}

	private void IconConfiguration ()
	{
		EditorGUILayout.Separator ();
		EditorGUILayout.Separator ();
		EditorGUILayout.LabelField ("Icon Configuration");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Default Icon", GUILayout.MaxWidth (GetWidthOfText ("Default Icon")));
		defaultIcon = (Texture2D)EditorGUILayout.ObjectField (defaultIcon, typeof (Texture2D), false, GUILayout.Width (75), GUILayout.Height (75));
		if (defaultIcon != null)
			EditorPrefs.SetString (projectName + "DefaultIcon", AssetDatabase.GetAssetPath (defaultIcon.GetInstanceID ()));
		else
			EditorPrefs.DeleteKey (projectName + "DefaultIcon");
		EditorGUILayout.LabelField ("Underground Icon", GUILayout.MaxWidth (GetWidthOfText ("Underground Icon")));
		undergroundIcon = (Texture2D)EditorGUILayout.ObjectField (undergroundIcon, typeof (Texture2D), false, GUILayout.Width (75), GUILayout.Height (75));
		if (undergroundIcon != null)
			EditorPrefs.SetString (projectName + "UndgdIcon", AssetDatabase.GetAssetPath (undergroundIcon.GetInstanceID ()));
		else
			EditorPrefs.DeleteKey (projectName + "UndgIcon");
		EditorGUILayout.EndHorizontal ();
	}

	private void Keystore ()
	{
		EditorGUILayout.Separator ();
		EditorGUILayout.Separator ();
		EditorGUILayout.LabelField ("Keystore Configuration");
		bundleIdentifier = EditorGUILayout.TextField ("Bundle Identifier", bundleIdentifier);
		keystorePass = EditorGUILayout.PasswordField (new GUIContent ("Keystore Password", "banonkey"), keystorePass);
		aliasName = EditorGUILayout.TextField (new GUIContent ("Alias Name", projectName != null ? projectName.ToLower () : "PLEASE SUPPLY PROJECT NAME"), aliasName);
		aliasPass = EditorGUILayout.PasswordField (new GUIContent ("Alias Password", "banonkey"), aliasPass);
	}

	private void Versioning ()
	{
		EditorGUILayout.Separator ();
		EditorGUILayout.Separator ();

		EditorGUILayout.LabelField ("Version");
		EditorGUILayout.BeginHorizontal ();
		major = EditorGUILayout.IntField (major, GUILayout.MaxWidth (20));
		minor = EditorGUILayout.IntField (minor, GUILayout.MaxWidth (20));
		build = EditorGUILayout.IntField (build, GUILayout.MaxWidth (20));
		revision = EditorGUILayout.IntField (revision, GUILayout.MaxWidth (60));
		Rect setVersionButton = EditorGUILayout.BeginHorizontal (GUILayout.MaxWidth (GUI.skin.label.CalcSize (new GUIContent ("Set Version From Editor Prefs")).x));
		if (GUI.Button (setVersionButton, GUIContent.none))
		{
			SetVersion ();
		}
		EditorGUILayout.LabelField ("Set Version From Editor Prefs");
		EditorGUILayout.EndHorizontal ();
		Rect setSettings = EditorGUILayout.BeginHorizontal (GUILayout.MaxWidth (GUI.skin.label.CalcSize (new GUIContent ("Set Editor Preferences")).x));
		if (GUI.Button (setSettings, GUIContent.none))
		{
			SetEditorPrefs ();
		}
		EditorGUILayout.LabelField ("Set Editor Preferences");
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Is Test Build? ", GUILayout.Width (GetWidthOfText ("Is Test Build? ")));
		test = EditorGUILayout.Toggle (test);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.Separator ();
		EditorGUILayout.Separator ();
	}

	private void BuildButtons ()
	{
		EditorGUILayout.BeginHorizontal ();
		if (CanBuildiOS ())
			DrawHorizontalButton ("Build iOS", BuildiOS, 4);
		else
			DrawHorizontalHelpBox (GetiOSBuildErrors (), MessageType.Error, 4);
		if (CanBuildAmazon ())
			DrawHorizontalButton ("Build Amazon", BuildAmazon, 4);
		else
			DrawHorizontalHelpBox (GetAmazonBuildErrors (), MessageType.Error, 4);
		if (CanBuildAmazonUnderground ())
			DrawHorizontalButton ("Build Amazon Underground", BuildAmazonUnderground, 4);
		else
			DrawHorizontalHelpBox (GetAmazonUndergroundBuildErrors (), MessageType.Error, 4);
		if (CanBuildGooglePlay ())
			DrawHorizontalButton ("Build Google", BuildGoogle, 4);
		else
			DrawHorizontalHelpBox (GetGooglePlayBuildErrors (), MessageType.Error, 4);
		if (isHorizontalActive)
			EditorGUILayout.EndHorizontal ();
	}

	private void DrawHorizontalButton (string label, Action function, int buttonsInRow)
	{
		bool isHorizActive = true;
		Rect button = EditorGUILayout.BeginHorizontal (GUILayout.MaxWidth (position.width / buttonsInRow));
		if (GUI.Button (button, ""))
		{
			function ();
			isHorizActive = false;
			return;
		}
		EditorGUILayout.LabelField (label);
		GUILayout.FlexibleSpace ();
		if (isHorizActive)
			EditorGUILayout.EndHorizontal ();
	}

	private void DrawHorizontalHelpBox (string error, MessageType type, int thingsInRow)
	{
		EditorGUILayout.BeginHorizontal (GUILayout.MaxWidth (position.width / thingsInRow));
		EditorGUILayout.HelpBox (error, type);
		EditorGUILayout.EndHorizontal ();
	}
	#endregion

	private static void SetVersion ()
	{
		if (!File.Exists ("Assets/ProjectName.txt"))
			File.WriteAllText ("Assets/ProjectName.txt", "");
		projectName = File.ReadAllText ("Assets/ProjectName.txt");
		if (EditorPrefs.HasKey (projectName + "Major"))
			major = EditorPrefs.GetInt (projectName + "Major");
		if (EditorPrefs.HasKey (projectName + "Minor"))
			minor = EditorPrefs.GetInt (projectName + "Minor");
		if (EditorPrefs.HasKey (projectName + "Build"))
			build = EditorPrefs.GetInt (projectName + "Build");
		if (EditorPrefs.HasKey (projectName + "Revision"))
			revision = EditorPrefs.GetInt (projectName + "Revision");
		Debug.Log ("[[Build Handler]] Set Version to: " + EditorPrefs.GetInt (projectName + "Major").ToString () + "." + EditorPrefs.GetInt (projectName + "Minor").ToString () + "." +
					EditorPrefs.GetInt (projectName + "Build").ToString () + "." + EditorPrefs.GetInt (projectName + "Revision").ToString ());
		if (EditorPrefs.HasKey (projectName + "UndgdIcon"))
			undergroundIcon = (Texture2D)AssetDatabase.LoadAssetAtPath (EditorPrefs.GetString (projectName + "UndgdIcon"), typeof (Texture2D));
		if (EditorPrefs.HasKey (projectName + "DefaultIcon"))
			defaultIcon = (Texture2D)AssetDatabase.LoadAssetAtPath (EditorPrefs.GetString (projectName + "DefaultIcon"), typeof (Texture2D));
		if (EditorPrefs.HasKey (projectName + "BundleIdentifier"))
			bundleIdentifier = EditorPrefs.GetString (projectName + "BundleIdentifier");
		if (EditorPrefs.HasKey (projectName + "KeystorePassword"))
			keystorePass = EditorPrefs.GetString (projectName + "KeystorePassword");
		if (EditorPrefs.HasKey (projectName + "AliasName"))
			aliasName = EditorPrefs.GetString (projectName + "AliasName");
		if (EditorPrefs.HasKey (projectName + "AliasPass"))
			aliasPass = EditorPrefs.GetString (projectName + "AliasPass");
	}

	bool CanBuildiOS ()
	{
		return true;
	}

	string GetiOSBuildErrors ()
	{
		return "";
	}

	bool CanBuildAmazon ()
	{
		return (defaultIcon != null && keystorePass == "banonkey" && aliasPass == "banonkey" && aliasName == projectName.ToLower ());
	}

	string GetAmazonBuildErrors ()
	{
		string errors = "The following fields must be set: \n";
		if (defaultIcon == null)
			errors += "-Default Icon\n";
		if (keystorePass != "banonkey")
			errors += "-Keystore Pass\n";
		if (aliasPass != "banonkey")
			errors += "-Alias Pass\n";
		if (projectName == null || aliasName != projectName.ToLower ())
			errors += "-Alias Name\n";
		errors = errors.Substring (0, errors.Length - 1);
		return errors;
	}

	bool CanBuildAmazonUnderground ()
	{
		return (undergroundIcon != null && keystorePass == "banonkey" && aliasPass == "banonkey" && aliasName == projectName.ToLower ());
	}

	string GetAmazonUndergroundBuildErrors ()
	{
		string errors = "The following fields must be set: \n";
		if (undergroundIcon == null)
			errors += "-Underground Icon\n";
		if (keystorePass != "banonkey")
			errors += "-Keystore Pass\n";
		if (aliasPass != "banonkey")
			errors += "-Alias Pass\n";
		if (projectName == null || aliasName != projectName.ToLower ())
			errors += "-Alias Name\n";
		errors = errors.Substring (0, errors.Length - 1);
		return errors;
	}

	bool CanBuildGooglePlay ()
	{
		return (defaultIcon != null && keystorePass == "banonkey" && aliasPass == "banonkey" && aliasName == projectName.ToLower ());
	}

	string GetGooglePlayBuildErrors ()
	{
		string errors = "The following fields must be set: \n";
		if (defaultIcon == null)
			errors += "-Default Icon\n";
		if (keystorePass != "banonkey")
			errors += "-Keystore Pass\n";
		if (aliasPass != "banonkey")
			errors += "-Alias Pass\n";
		if (projectName == null || aliasName != projectName.ToLower ())
			errors += "-Alias Name\n";
		errors = errors.Substring (0, errors.Length - 1);
		return errors;
	}

	void BuildiOS ()
	{
		SetDefines ("IOS", test ? "TEST_BUILD" : "");
		PlayerSettings.bundleVersion = major.ToString () + minor.ToString () + build.ToString ();
		//PlayerSettings.shortBundleVersion = major.ToString () + "." + minor.ToString () + "." + build.ToString ();
		EditorUserBuildSettings.SwitchActiveBuildTarget (BuildTarget.iOS);
		Build (BuildTarget.iOS, "Builds/" + projectName + "(iOS)");
	}

	void BuildAmazon ()
	{
		SetDefines ("AMAZON", test ? "TEST_BUILD" : "");
		PlayerSettings.SetIconsForTargetGroup (BuildTargetGroup.Unknown, new Texture2D [] { defaultIcon });
		ConfigureInApp (false);
		ConfigureAndroidPlayerSettings ();
		EditorUserBuildSettings.SwitchActiveBuildTarget (BuildTarget.Android);
		Build (BuildTarget.Android, "Builds/" + projectName + "(Amazon)");
	}

	void BuildAmazonUnderground ()
	{
		SetDefines ("AMAZON", "UNDERGROUND", test ? "TEST_BUILD" : "");
		PlayerSettings.SetIconsForTargetGroup (BuildTargetGroup.Unknown, new Texture2D [] { undergroundIcon });
		ConfigureInApp (true);
		ConfigureAndroidPlayerSettings (true);
		EditorUserBuildSettings.SwitchActiveBuildTarget (BuildTarget.Android);
		Build (BuildTarget.Android, "Builds/" + projectName + "(Amazon Underground)");
	}

	void BuildGoogle ()
	{
		SetDefines ("GOOGLE", test ? "TEST_BUILD" : "");
		PlayerSettings.SetIconsForTargetGroup (BuildTargetGroup.Unknown, new Texture2D [] { defaultIcon });
		ConfigureInApp (false);
		ConfigureAndroidPlayerSettings ();
		EditorUserBuildSettings.SwitchActiveBuildTarget (BuildTarget.Android);
		Build (BuildTarget.Android, "Builds/" + projectName + "(Google)");
	}

	public void Build (BuildTarget target, string output)
	{
		//This helps with getting the version number base to start from
		var settingsPath = Path.GetDirectoryName (Application.dataPath);
		settingsPath = Path.Combine (settingsPath, "ProjectSettings");
		settingsPath = Path.Combine (settingsPath, "ProjectSettings.asset");
		//Throw error if we can't find anything
		if (!File.Exists (settingsPath))
		{
			Debug.LogError ("Couldn't find project settings file.");
			return;
		}

		// Make sure the paths exist before building.
		try
		{
			Directory.CreateDirectory (output);
		}
		catch
		{
			Debug.LogError ("Failed to create directories: " + output);
		}

		var lines = File.ReadAllLines (settingsPath);
		if (!lines [0].StartsWith ("%YAML"))
		{
			Debug.LogError ("Project settings file needs to be serialized as a text asset. (Check 'Project Settings->Editor')");
			return;
		}

		System.Version version = new System.Version (major, minor, build, revision);
		SetEditorPrefs ();

		//Lets start writing the files to the log and such
		File.WriteAllLines (settingsPath, lines);
		Debug.Log ("-----BUILD-----");
		Debug.Log ("Build version: " + version);
		Debug.Log ("Building Platform: " + target.ToString ());

		//These are to speed things up a little bit.
		string projectNameFile = null;
		Debug.Log (target.ToString ());
		if (target.ToString () == "WebPlayer")
		{
			projectNameFile = projectName + "_" + version;
		}
		else if (target.ToString () == "StandaloneWindows")
		{
			projectNameFile = projectName + "_" + version + ".exe";
		}
		else if (target.ToString () == "StandaloneOSXUniversal")
		{
			projectNameFile = projectName + "_" + version + ".app";
		}
		else if (target.ToString () == "Android")
		{
			string app;
			if (output.Contains ("Underground"))
				app = "U";
			else if (output.Contains ("Amazon"))
				app = "A";
			else
				app = "G";
			projectNameFile = projectName + "_" + version + app + ".apk";
		}
		else if (target.ToString () == "iOS")
		{
			projectNameFile = projectName + "_" + version;
		}
		string [] level_list = FindScenes ();
		Debug.Log ("Scenes to be processed: " + level_list.Length);

		foreach (string s in level_list)
		{
			string cutdown_level_name = s.Remove (s.IndexOf (".unity"));
			Debug.Log ("   " + cutdown_level_name);
		}

		Debug.Log (output + "/" + projectNameFile);
		string results = BuildPipeline.BuildPlayer (level_list, output + "/" + projectNameFile, target, BuildOptions.None).ToString ();
		if (results.Length == 0)
			Debug.Log ("No Build Errors");
		else
			Debug.Log ("Build Error:" + results);
	}

	void SetEditorPrefs ()
	{
		EditorPrefs.SetInt (projectName + "Major", major);
		EditorPrefs.SetInt (projectName + "Minor", minor);
		EditorPrefs.SetInt (projectName + "Build", build);
		EditorPrefs.SetInt (projectName + "Revision", revision);
		EditorPrefs.SetString (projectName + "BundleIdentifier", bundleIdentifier);
		EditorPrefs.SetString (projectName + "KeystorePassword", keystorePass);
		EditorPrefs.SetString (projectName + "AliasName", aliasName);
		EditorPrefs.SetString (projectName + "AliasPass", aliasPass);
	}

	//Find the scenes so we can add them to the log
	public static string [] FindScenes ()
	{
		int num_scenes = 0;

		// Count active scenes.
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (scene.enabled)
				num_scenes++;
		}

		// Build the list of scenes.
		string [] scenes = new string [num_scenes];

		int x = 0;
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (scene.enabled)
				scenes [x++] = scene.path;
		}

		return (scenes);
	}

	private void SetDefines (params string [] defines)
	{
		File.WriteAllText ("Assets/ProjectName.txt", projectName);
		string defs = "";
		for (int i = 0; i < defines.Length; i++)
			if (defines [i] != "")
				defs += "-define:" + defines [i] + "\n";
		File.WriteAllText ("Assets/smcs.rsp", defs);
	}

	private void ConfigureAndroidPlayerSettings (bool underground = false)
	{
		PlayerSettings.Android.keystorePass = keystorePass;
		PlayerSettings.Android.keyaliasName = aliasName;
		PlayerSettings.Android.keyaliasPass = aliasPass;
		PlayerSettings.bundleVersion = major.ToString () + minor.ToString () + build.ToString ();
		//PlayerSettings.shortBundleVersion = major.ToString () + "." + minor.ToString () + "." + build.ToString ();
		PlayerSettings.Android.bundleVersionCode = System.Convert.ToInt32 (major.ToString () + minor.ToString () + build.ToString ());
		PlayerSettings.applicationIdentifier = bundleIdentifier + (underground ? ".underground" : "");
	}

	private void ConfigureInApp (bool underground)
	{
		//Unibill.Impl.BillingPlatform platform, 
		/*
		InventoryEditor window = (InventoryEditor)InventoryEditor.ShowWindow ();
		for (int i = 0; i < InventoryEditor.Items.Count; i++)
		{
			if (underground)
			{
				if (!InventoryEditor.Items [i].item.Id.Contains (".underground"))
				{
					InventoryEditor.Items [i].item.Id += ".underground";
				}
			}
			else
			{
				InventoryEditor.Items [i].item.Id = InventoryEditor.Items [i].item.Id.Replace (".underground", "");
			}
		}
		window.SetAndroidBillingPlatform (platform);
		window.Close ();
		*/
	}

	float GetWidthOfText (string text)
	{
		return GUI.skin.label.CalcSize (new GUIContent (text)).x;
	}
}