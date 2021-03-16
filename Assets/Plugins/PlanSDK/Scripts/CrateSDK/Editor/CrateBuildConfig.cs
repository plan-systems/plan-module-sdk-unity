
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace PlanSDK.CrateSDK {


	public struct CrateBuildTarget {
	
        public CrateBuildTarget(BuildTarget target, string targetString, BuildAssetBundleOptions bundleOptions) {
            Target = target;
            TargetString = targetString;
            BundleOptions = bundleOptions;

            Logging = true;
        }
        
        public BuildTarget             Target { get; private set; }
        public string                  TargetString { get; private set; }
        public BuildAssetBundleOptions BundleOptions { get; private set; }
        public bool                    Logging { get; private set; }
    
	}


    // Generally, a player is built with exactly matching data.  This is designed so that:
    // 1) Config file simply points to the right folders to load asset bundles from, so you can change CDNs if you like.
    // 2) Player will have a version number burned a scriptable object in its Resources folder that points to the manifest it should read.
    // 3) Manifest files have a version number in their filename, so they fully describe the data files that went with that version (may be shared with old or new builds)
    // 4) Older and newer builds will function side-by-side with each other as long as you don't delete or rename any asset bundles or manifests.

    // Place an CrateBuildConfig in an Editor/Resources folder.  It is really only there to make it easy to configure and generate builds--the asset is not used at runtime.
    // Name this asset "CrateBuildConfig"
    public class CrateBuildConfig : ScriptableObject {
    
        static public string                kBuildConfigPath = "CrateBuildConfig";

        [Tooltip("Turn this on to see asset bundle logging.")]
        public bool                         DoLogging = true;

        [Tooltip("Enabling this causes extra info to be displayed and temp items are not cleaned up")]
        public bool                         DebugBuild = false;

        [Tooltip("Specifies the output dir where modules are built")]
        public string                       CrateOutputPath = "{ProjectDir}/../__builds";

        public string                       ExpandedOutputPath {
            get {
                var outDir = CrateOutputPath.Replace("{ProjectDir}", this.ProjectDir);
                outDir = outDir.Replace("//", "/");
                return outDir;
            }
        }

        [Tooltip("Specifies where intermediary assets are places in preparation for a module export.  This path must be in your project's Assets.")]
        public string                       AssetStagingPath = "Assets/_asset-staging";

        public PlatformOptions[] platforms = new PlatformOptions[] {
            new PlatformOptions(BuildTarget.StandaloneWindows64),
            new PlatformOptions(BuildTarget.WSAPlayer),
            new PlatformOptions(BuildTarget.StandaloneOSX),
            new PlatformOptions(BuildTarget.Stadia),
            new PlatformOptions(BuildTarget.StandaloneLinux64),
            new PlatformOptions(BuildTarget.Android),
            new PlatformOptions(BuildTarget.iOS),
            new PlatformOptions(BuildTarget.Lumin),
        };

        // "Users/me/UnityProjectDir/"  (includes trailing slash)
        string                                      _projectDir;
        string                                      _assetPath;
        string                                      _expandedOutputPath;

        // Absolute path of Unity Asset dir plus trailing slash
        public string                               AssetPath {
            get {
                if (String.IsNullOrEmpty(_assetPath)) {
                    _assetPath = Application.dataPath + "/";
                }
                return _assetPath;
            }
        }

        // Absolute path of Unity project dir plus trailing slash
        public string                               ProjectDir {
            get {
                if (String.IsNullOrEmpty(_projectDir)) {
                    _projectDir = this.AssetPath;
                    if (_projectDir.EndsWith("/Assets/")) {
                        _projectDir = _projectDir.Substring(0, _projectDir.Length - 7);
                    }
                }
                return _projectDir;
            }
        }   


        // Each platform gets its own unique space to set options.
        [Serializable]
        public class PlatformOptions {
    
            // This must come first to show up in the Inspector as the drop-down label.
            public string                   Name;			
            public BuildTarget              Platform;
        
            // This controls whether this platform is AVAILABLE, and if they someone does a Build All, or even an explicit Build xxxxx, 
            // if this is disabled in the ScriptableObject, it will not build.  So these are pretty important to set for your project.
            // If you really want to build everything, leave everything on.  Unity gives us no way to know what platforms you have installed, 
            // though, which is why it is done this way.
            [Tooltip("If enabled, this platform will be built.  You must have the appropriate SDK installed for this to work, obviously.")]
            public bool                     TargetEnabled = true;

            // This creates the default options for all platforms.  If you want to manually set options per-platform, do it in the code below explicitly.
            // Some of these are disabled because there's no real use in using them.
            [Space]
            [Header("Asset Bundle Options")]
            public bool Uncompressed = false;
            public bool ForceRebuild = false;
            public bool DisableWriteTypeTree = false;
            public bool IgnoreTypeTreeChanges = false;
            public bool ChunkBasedCompression = true;
            public bool AssetBundleStripUnityVersion = true;
            public bool StrictMode = true;
            public bool DeterministicAssetBundle = true;
            public bool DisableLoadAssetByFileName = true;
            public bool DisableLoadAssetByFileNameWithExtension = true;


            //-------------------
            // This combines the settings saved in the CrateBuildConfig scriptable object to control the build of asset bundles.
            public BuildAssetBundleOptions GenerateBundleOptionsFromSettings()
            {
                BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
                if (Uncompressed)
                    options |= BuildAssetBundleOptions.UncompressedAssetBundle;
                if (DisableWriteTypeTree)
                    options |= BuildAssetBundleOptions.DisableWriteTypeTree;
                if (DeterministicAssetBundle)
                    options |= BuildAssetBundleOptions.DeterministicAssetBundle;
                if (ForceRebuild)
                    options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
                if (IgnoreTypeTreeChanges)
                    options |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;
                if (ChunkBasedCompression)
                    options |= BuildAssetBundleOptions.ChunkBasedCompression;
                if (StrictMode)
                    options |= BuildAssetBundleOptions.StrictMode;
                if (DisableLoadAssetByFileName)
                    options |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
                if (DisableLoadAssetByFileNameWithExtension)
                    options |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
                if (AssetBundleStripUnityVersion)
                    options |= BuildAssetBundleOptions.AssetBundleStripUnityVersion;

                return options;
            } 

            // Constructor to set name properly.
            public PlatformOptions(BuildTarget platform) {
                Platform = platform;
                Name = GetPlatformName(platform);
            }
        }


        // This lets us get the build target string, which should show up as the matching string above at runtime.
        static public string GetPlatformName(BuildTarget bt)
        {
            switch (bt)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.Stadia:
                    return "stadia";
                case BuildTarget.StandaloneLinux64:
                    return "linux64";
                case BuildTarget.StandaloneOSX:
                    return "macOS";
                case BuildTarget.StandaloneWindows:
                    return "win32";
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.WSAPlayer:
                    return "winstore";
                case BuildTarget.tvOS:
                    return "tvos";
                case BuildTarget.Lumin:
                    return "magicleap";
                case BuildTarget.WebGL:
                    return "webgl";
                case BuildTarget.PS4:
                    return "ps4";
                case BuildTarget.Switch:
                    return "switch";
                case BuildTarget.XboxOne:
                    return "xboxone";
            }
            Debug.Assert(false, "Requested platform is unknown: "+bt);
            return "unknown";
        }

        
        //-------------------

        public List<CrateBuildTarget> GetBuildTargets() {
    
            if (string.IsNullOrEmpty(Application.productName)) 
                Debug.LogError("Product Name is not set.  Do this in Edit->ProjectSettings->Player");

            // Build a configuration dictionary with all the settings stored PER-PLATFORM, for easy extraction.
            var buildTargets = new List<CrateBuildTarget>();
            foreach (PlatformOptions po in platforms) {
                if (po.TargetEnabled) {

                    if (po.Platform == BuildTarget.WebGL)
                        po.DisableWriteTypeTree = false;

                    // Generated straight from checkboxes in the ScriptableObject.
                    BuildAssetBundleOptions bundleOptions = po.GenerateBundleOptionsFromSettings();

                    var builtTarget = new CrateBuildTarget(po.Platform, GetPlatformName(po.Platform), bundleOptions);

                    buildTargets.Add(builtTarget);
                }
            }
        
            return buildTargets;
        }


        //-------------------
        // Convenient way to get the build config asset.
        static public CrateBuildConfig        CurrentConfig(bool reload = false) {

            if (reload) {
                _cachedConfig = null;
            }

            if (_cachedConfig != null)
                return _cachedConfig;
        
            // Try to load the BuildSettings scriptable object, which contains the build version, final manifest URL, etc.
            _cachedConfig = LoadBuildConfig();
            if (_cachedConfig==null) Debug.LogError("<color=#ff8080>No {kBuildConfigPath} object found.</color>");

            return _cachedConfig;
        }

        static CrateBuildConfig               LoadBuildConfig() {
            var config = Resources.Load<CrateBuildConfig>(CrateBuildConfig.kBuildConfigPath);
            if (config == null) {
                config = Resources.Load<CrateBuildConfig>("ModuleBuildConfig");        // Try the legacy name
            }
            return config;
        }

        static CrateBuildConfig _cachedConfig = null;


		const string kRevealBuildConfigs                  = "Tools/PLAN/Reveal Build Configs";

        // Easier to configure stuff if it's broken out in an obvious place like this.
        static public string                GetSDKFolder() {
    
            // Allow user to move things around without having to change the code
            string[] matchList = Directory.GetDirectories(Application.dataPath, "Plan*SDK", SearchOption.AllDirectories);
            if (matchList.Length == 0) {
                Debug.Assert(matchList.Length > 0, "failed to locate PLAN SDK dir");
            }

            string sdkDir = matchList[0].Replace(Application.dataPath, "");
            sdkDir = sdkDir.Replace('\\', '/');
            return sdkDir;
        }

        static public string                GetResourcesDir() {
            string sdkDir = $"Assets{GetSDKFolder()}";

            string resDir = $"{sdkDir}/Editor/Resources";
            CrateBuild.CreateAssetDir(resDir);

            return resDir;
        }

        [MenuItem(kRevealBuildConfigs, false, 204)]
        static public void RevealBuildConfigs() {
    
            var config = LoadBuildConfig();
            if (config==null) {
                config = new CrateBuildConfig();
                var resDir = GetResourcesDir();
                string configPath = $"{resDir}/{kBuildConfigPath}.asset";
                AssetDatabase.CreateAsset(config, configPath);
                AssetDatabase.SaveAssets();

                config = Resources.Load<CrateBuildConfig>(kBuildConfigPath);
            }

            AssetDatabase.Refresh();
            Selection.activeObject = config;
        }
        
    }
}

