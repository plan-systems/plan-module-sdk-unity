
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Collections.Generic;

using PlanSDK.Crates;
using gpb = global::Google.Protobuf;


namespace PlanSDK.CrateSDK {


    public class BundleBuilder {

        public static void                         BuildModule(CrateBuildParams buildParams) {

            if (EditorApplication.isPlaying || EditorApplication.isCompiling) {
                UnityEngine.Debug.LogWarning("<color=#ff8080>Cannot build while running or compiling.</color>");
                return;
            }

            buildParams.BuildConfig = CrateBuildConfig.CurrentConfig(true);

            var modBuild = CrateBuild.NewBuild(buildParams);
            Debug.Log($"Building {modBuild.OutputPath}");

            var buildTargets = buildParams.BuildConfig.GetBuildTargets(); 

            modBuild.RunExport();


            {
                int successCount = 0;

                foreach (var target in buildTargets) {

                    var outputDir = $"{modBuild.OutputPath}/{target.TargetString}";
                    Directory.CreateDirectory(outputDir);

					// ABBundleBuilder.DoBuildBundles(target.SrcBundleFolder, target.BuildVersion, target.DestBundleFolder, target.BundleOptions, target.Target, target.TargetString, target.Logging, target.IgnoreEndsWith, target.IgnoreContains, target.IgnoreExact);

                    modBuild.PrepareToBuild(out var builds);

                    var bundleManifest = BuildPipeline.BuildAssetBundles(outputDir, builds, target.BundleOptions, target.Target);
                    if (bundleManifest == null) {
                        Debug.LogError($"<color=#ff8080>Asset bundle build failed for {outputDir}</color>");
                        return;
                    }

                    successCount++;

                    // Delete junk files 
                    {
                        foreach (var filename in Directory.GetFiles(outputDir, "*.manifest", SearchOption.TopDirectoryOnly)) {
                            File.Delete(filename);
                        }

                        // Unity makes a cheese manifest object for the parent folder
                        File.Delete($"{outputDir}/{target.TargetString}");
                    }
                }

                if (successCount == buildTargets.Count) {
                    modBuild.WriteManifests();

                    Debug.Log($"<color=#107010>{successCount} of {buildTargets.Count} targets built to {modBuild.OutputPath}</color>");
                    buildParams.Crate.IncrementBuildNum();
                    EditorUtility.SetDirty(buildParams.Crate);
                    AssetDatabase.SaveAssets();
                }

                modBuild.Cleanup();
            }

        }
    }

    public class BundleBuild {
        public CrateBuild                   Crate;
        public BundleManifest               Manifest    = new BundleManifest();
        public Transform                    BundleRoot;              // Corresponding item representing this bundle in the scene





        public void                         ExportBundle() {

            RuntimePreviewGenerator.MarkTextureNonReadable = false;
            RuntimePreviewGenerator.BackgroundColor = new Color(0,0,0,0);
            RuntimePreviewGenerator.OrthographicMode = true;

            _bundleAssetDir = $"{Crate.AssetStagingDir}";

            // Bundle asset URI are relative to the bundle, so create the dirs, but the root bundle URI is ""
            _curURI = "";

            pushDir(Manifest.BundlePublicName);

            exportGroup(BundleRoot, "");
        }

        // Always relative to the project Asset dir (i.e. parent dir is project Assets dir)
        string                              _bundleAssetDir;

        // Is "" or includes trailing '/'
        string                              _curURI;

        void                                pushDir(string dirName) {
            if (_curURI == "") {
                _curURI = dirName;
            } else {
                _curURI = $"{_curURI}/{dirName}";
            }

            CrateBuild.CreateAssetDir($"{_bundleAssetDir}/{_curURI}");
        }


        void                                popDir() {
            int len = _curURI.Length;
            int pos = _curURI.LastIndexOf('/');
            if (pos == len-1) {
                len--;
                pos = _curURI.LastIndexOf('/', len-1);
            }

            if (pos > 0) {
                _curURI = _curURI.Substring(0, pos);
            } else {
                _curURI = "";
            }
        }



        string                              renderAssetIcon(Transform asset, AssetEntry entry) {
            string iconPathname = null;

            Transform target = asset;

            RuntimePreviewGenerator.PreviewDirection = new Vector3(0, 0, -1);

            var iconTex = RuntimePreviewGenerator.GenerateModelPreview(target, 128, entry.Bounds);
            if (iconTex != null) {

                iconPathname = $"{_bundleAssetDir}/{entry.NameID}.icon.png";
                Crate.WriteProjectFile(iconPathname, iconTex.EncodeToPNG());
                AssetDatabase.ImportAsset(iconPathname, ImportAssetOptions.ForceSynchronousImport);

                // TODO see time 6:00 in https://www.youtube.com/watch?v=-bxaYugwVL4&feature=emb_logo for having default filter settings for UI textures
                var importer = AssetImporter.GetAtPath(iconPathname);

                TextureImporter ti = importer as TextureImporter;
                ti.textureType = TextureImporterType.Sprite;
                ti.alphaIsTransparency = true;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.alphaSource = TextureImporterAlphaSource.FromInput;
                ti.filterMode = FilterMode.Bilinear;
                ti.mipmapEnabled = true;
                ti.maxTextureSize = 128;
                AssetDatabase.ImportAsset(iconPathname);
            }

            return iconPathname;
        }





        void                                  exportModuleAsset(CrateItem item) {
        
            if (item.ExcludeFromBuild)
                return;
                

            var assetName = item.name;
            var target = item.AssetTarget;

            if (String.IsNullOrEmpty(item.AssetNameID)) {
                Debug.LogWarning($"asset '{assetName}' skipped because AssetID is empty");
            }
                
            assetName = item.name;
            var entry = new AssetEntry {
                Bounds = item.AssetBounds,
                BrowsePath = $"{_curURI}/{assetName}",        // DEPRECATED
                NameID = item.AssetNameID,
                Name = assetName,
                Tags = item.ExportTagList(),
            };

            if (item.IsPrivate) {
                entry.AssetFlags |= AssetFlags.IsPrivate;
            }

            if (String.IsNullOrWhiteSpace(item.ShortDescription) == false) {
                entry.ShortDesc = item.ShortDescription;
            }
                    
            string err = null;

            var skybox = item.GetComponent<Skybox>();
            if (skybox != null) {
                entry.AssetFlags |= AssetFlags.IsSkybox;
                entry.LocalURI = AssetDatabase.GetAssetPath(skybox.material);

                var iconSkin = Crate.SkyboxIconSkin;
                iconSkin.sharedMaterial = skybox.material;

                target = iconSkin.transform;

            } else {
                if (item.IsSurface) {
                    entry.AssetFlags |= AssetFlags.IsSurface;
                }
                if (item.AutoScaleByDefault) {
                    entry.AssetFlags |= AssetFlags.AutoScale;
                }
                
                entry.AssetFlags |= AssetFlags.IsPlaceable;
            }
            
            if (entry.AssetFlags.HasFlag(AssetFlags.IsPlaceable)) {
                string prefabPathname = $"{_bundleAssetDir}/{entry.NameID}.prefab";
                entry.LocalURI = prefabPathname;

                //Vector3 savedPos = asset.localPosition;
                //asset.localPosition = Vector2.zero;
                Debug.Assert(target != null, $"item {entry.BrowsePath} has no asset to export");

                // Vector3 pos = asset.localPosition;
                // asset.localPosition = bounds.center;
                PrefabUtility.SaveAsPrefabAsset(target.gameObject, prefabPathname, out bool success);
                if (!success) {
                    err = $"failed to export {entry.BrowsePath} as prefab '{prefabPathname}'";
                }
            }

            string iconPath = null;
            if (item.AutoGenerateIcon) {
                iconPath = renderAssetIcon(target, entry);
            } else if (item.CustomIcon != null) {
                iconPath = AssetDatabase.GetAssetPath(item.CustomIcon);
            }

            // FUTURE: IconIDs are just 16 byte GUIDs that multuple mod assets reference
            if (String.IsNullOrEmpty(iconPath) == false) {
                entry.AssetFlags |= AssetFlags.HasIcon;

                Crate.BrowserBundle.Manifest.Assets.Add(new AssetEntry {
                    AssetFlags = /*AssetFlags.IsIcon |*/ AssetFlags.IsSprite,
                    LocalURI = iconPath,
                    NameID = item.AssetNameID,
                });
            }

            //asset.localPosition = savedPos;

            if (err == null) {
                this.Manifest.Assets.Add(entry);
            } else {
                Debug.LogError(err);
            }
        }


        void                                exportGroup(Transform groupItem, string groupName) {
            int N = groupItem?.childCount ?? 0;
            if (N == 0) 
                return;

            if (groupName == null) {
                groupName = groupItem.name;
                int extPos = groupName.LastIndexOf('.');
                string ext = (extPos >= 0) ? groupName.Substring(extPos) : "";
                groupName = groupName.Substring(0, groupName.Length - ext.Length);
            }

            if (groupName.Length > 0) {
                pushDir(groupName);
            }

            for (int j = 0; j < N; j++) {
                Transform subItem = groupItem.GetChild(j);

                var mass = subItem.GetComponent<CrateItem>();
                if (mass == null) {
                    exportGroup(subItem, null);
                } else {
                    exportModuleAsset(mass);
                }
            }

            if (groupName.Length > 0) {
                popDir();
            }
        }
    }

    public class CrateBuildParams {


        // // Absolute path of Unity project dir plus trailing slash
        // // e.g. "/Users/aomeara/MyUnityProjectDir/" 
        // public string                               ProjectDir;             // "Users/me/UnityProjectDir/"  (includes trailing slash)

        // // Absolute path of Unity asset dir plus trailing slash
        // public string                               AssetDir;

        public string                               BuildSuffix;

        public CrateMaker                           Crate;
        public CrateBuildConfig                     BuildConfig;

        public                                      CrateBuildParams() {


        }

    }


    public class CrateBuild {
        public static readonly string               kBrowserBundleDefaultName = "!";

        // Bundle containing all this module's browsable assets, etc.
        public BundleBuild                          BrowserBundle;
        
        public string                               OutputPath;                 // Absolute path of where build products go
        public string                               AssetStagingDir;            // Asset dir pathname of where tp export module assets (starts with "Assets/")
        public CrateBuildParams                     Params;

        public CrateManifest                        Manifest;
        public Dictionary<string, BundleBuild>      Bundles = new Dictionary<string, BundleBuild>(10);

        public MeshRenderer                         SkyboxIconSkin;


        public static CrateBuild            NewBuild(CrateBuildParams buildParams) {

            var build = new CrateBuild();

            build.Params = buildParams;

            build.initBuild();

            return build;
        }

        void                                removeAssetStagingDir() {
            if (AssetDirExists(AssetStagingDir)) {
                AssetDatabase.DeleteAsset(AssetStagingDir);
                //Directory.Delete(Params.ProjectDir + AssetExportDir, true);
            }
        }


        void                                initBuild() {

            Manifest = new CrateManifest();
            Manifest.HomeDomain         = Params.Crate.HomeDomain;
            Manifest.CrateNameID        = Params.Crate.CrateNameID;
            Manifest.Tags               = Params.Crate.Tags;
            Manifest.ShortDesc          = Params.Crate.ShortDescription;
            Manifest.BuildID            = Params.Crate.IssueBuildID();
            Manifest.CrateTitle         = Params.Crate.CrateTitle;
            Manifest.BrowserBundleName  = kBrowserBundleDefaultName;

            Bundles.Clear();

            {
                var outDir = Params.BuildConfig.ExpandedOutputPath;
                Directory.CreateDirectory(outDir);
                outDir = $"{outDir}/{Manifest.HomeDomain}";
                Directory.CreateDirectory(outDir);
                OutputPath = $"{outDir}/{Manifest.CrateNameID}";
            }
            if (Directory.Exists(OutputPath)) {
                Directory.Delete(OutputPath, true);
            }
            Directory.CreateDirectory(OutputPath);
        

            AssetStagingDir = Params.BuildConfig.AssetStagingPath;
            removeAssetStagingDir();
            CreateAssetDir(AssetStagingDir);

            {
                if (SkyboxIconSkin != null) {
                    UnityEngine.Object.DestroyImmediate(SkyboxIconSkin);
                    SkyboxIconSkin = null;
                }
                var prefab = Resources.Load<MeshRenderer>("SkyboxIconSkin");
                SkyboxIconSkin = UnityEngine.Object.Instantiate<MeshRenderer>(prefab); //, modulesObj, false);
                SkyboxIconSkin.gameObject.hideFlags = HideFlags.HideAndDontSave; //HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            // Set up browser bundle
            BrowserBundle = GetBundle(Manifest.BrowserBundleName, true);
            BrowserBundle.Manifest.LoadAllHint = true;
        }



        public bool                         AssetDirExists(string assetPath) {
            return Directory.Exists(Params.BuildConfig.ProjectDir + assetPath);
        }

        public static void                  CreateAssetDir(string assetPath) {
            int pos = assetPath.LastIndexOf('/');

            int len = assetPath.Length;
            if (pos == len-1) {
                len--;
                pos = assetPath.LastIndexOf('/', len-1);
            }

            string leafName = assetPath.Substring(pos+1, len-pos-1);
            string parentName;
            if (pos > 0) {
                parentName = assetPath.Substring(0, pos);
            } else {
                parentName = "Assets";
            }

            AssetDatabase.CreateFolder(parentName, leafName);
        }

        public void                         WriteProjectFile(string assetPath, byte[] data) {
            File.WriteAllBytes(Params.BuildConfig.ProjectDir + assetPath, data);
        }

        public BundleBuild                  CreateBundle(Transform bundleObj) {
            var bundle = GetBundle(bundleObj.name, true);
            bundle.BundleRoot = bundleObj;

            return bundle;
        }

        public BundleBuild                  GetBundle(string bundleBuildName, bool autoCreate) {
            Bundles.TryGetValue(bundleBuildName.ToLowerInvariant(), out var bundle);
        
            if (bundle == null && autoCreate) {
                bundle = new BundleBuild();

                int extPos = bundleBuildName.IndexOf('.');
                if (extPos < 0)
                    extPos = bundleBuildName.Length;

                bundle.Manifest.BundlePublicName = bundleBuildName.Substring(0, extPos);
                bundle.Manifest.BundleBuildName  = bundleBuildName.ToLowerInvariant();
                bundle.Crate = this;

                Bundles[bundle.Manifest.BundleBuildName] = bundle;
                this.Manifest.Bundles.Add(bundle.Manifest);
            }

            return bundle;
        }

        void                                AddModuleIcon() {




        }

        void                                AddBundles(Transform bundles) {

            int N = bundles.childCount;
            for (int i = 0; i < N; i++) {
                var bundleObj = bundles.GetChild(i);
                CreateBundle(bundleObj);                
            }

        }

        void                                ExportBundles() {

            AddBundles(Params.Crate.transform);

            var removeList = new List<string>();
            foreach (var kv in Bundles) {
                var bundle = kv.Value;
                bundle.ExportBundle();
                if (bundle.Manifest.Assets.Count == 0) {
                    removeList.Add(kv.Key);
                }
            }

            foreach (var i in removeList) {
                Bundles.Remove(i);
            }

            if (SkyboxIconSkin != null) {
                UnityEngine.Object.DestroyImmediate(SkyboxIconSkin);
                SkyboxIconSkin = null;
            }

        }


        public void                         RunExport() {

            // Export module icon if set
            if (Params.Crate.CrateIcon != null) {
                string spriteAssetPath = AssetDatabase.GetAssetPath(Params.Crate.CrateIcon);
                BrowserBundle.Manifest.Assets.Add(new AssetEntry {
                    AssetFlags = AssetFlags.IsSprite,
                    NameID = CrateManifest.kCrateIconNameID,
                    LocalURI = spriteAssetPath,
                });
            }
            
            ExportBundles();

            finalizeManifests();
        }





        void                                finalizeManifests() {
            var manifest = this.Manifest;

            int bundleCount = manifest.Bundles.Count;

            // Reorder bundles alphabetically
            {
                var blist = new BundleManifest[bundleCount];
                manifest.Bundles.CopyTo(blist, 0);
    
                Array.Sort<BundleManifest>(blist, (a, b) => a.BundleBuildName.CompareTo(b.BundleBuildName) ); 
                manifest.Bundles.Clear();
                manifest.Bundles.Add(blist);
            }

            // Add the build suffix to each bundle (thanks Unity, for erroring when two totally different assetbundles share the same name, wtf)
            foreach (var bundle in manifest.Bundles) {
                bundle.BundleBuildName = $"{bundle.BundleBuildName}.{Params.Crate.BuildSuffix}";
            }

            // Sort assets alphabetically in each bundle
            foreach (var bundle in manifest.Bundles) {
                int assetCount = bundle.Assets.Count;

                var asslist = new AssetEntry[assetCount];
                bundle.Assets.CopyTo(asslist, 0);
    
                Array.Sort<AssetEntry>(asslist, (a, b) => a.BrowsePath.CompareTo(b.BrowsePath) ); 
                bundle.Assets.Clear();
                bundle.Assets.Add(asslist);
            }
        }



        public void                         PrepareToBuild(out AssetBundleBuild[] builds) {
            var manifest = this.Manifest;
            int bundleCount = manifest.Bundles.Count;

            // // Create an asset bundle for each folder containing all the child assets inside it.
            // if (bundleCount == 0) {
            //     Debug.Log("<color=#ff8080>["+platformString+"] No modules to build: No subfolders in "+moduleToBuildDir+"</color>");
            //     return true;
            // }

            // Transfer asset list into builds[]
            builds = new AssetBundleBuild[bundleCount];
            for (int i = 0; i < bundleCount; i++) {
                var bundle = manifest.Bundles[i];
                int N = bundle.Assets.Count;

                builds[i].assetBundleName = bundle.BundleBuildName + ".assetbundle";

                builds[i].assetNames       = new string[N];
                builds[i].addressableNames = new string[N];
                for (int j = 0; j < N; j++) {
                    builds[i].assetNames[j]       = bundle.Assets[j].LocalURI;
                    builds[i].addressableNames[j] = bundle.Assets[j].NameID;
                }
            }
        }


        public void                         WriteManifests() {

            // Clear LocalURI since we were just using it to store the asset file pathname
            foreach (var bundle in Manifest.Bundles) {
                int N = bundle.Assets.Count;
                for (int i = 0; i < N; i++) {
                    bundle.Assets[i].LocalURI = "";
                }
            }

            string manifestPath = $"{OutputPath}/_manifest.pb";
            byte[] buf = gpb.MessageExtensions.ToByteArray(this.Manifest);
            File.WriteAllBytes(manifestPath, buf);

            {
                var jsonFormater = new gpb.JsonFormatter(new gpb.JsonFormatter.Settings(true));
                string manifestJson = jsonFormater.Format(this.Manifest);

                //var parsed = SimpleJSON.JSON.Parse(manifestJson);
                //parsed.SaveToBinaryFile($"{manifestPath}.json");

                File.WriteAllText($"{manifestPath}.json", manifestJson, System.Text.Encoding.UTF8);
            }
        }



        public void                         Cleanup() {
            if (Params.BuildConfig.DebugBuild == false) {
                removeAssetStagingDir();
            }
        }
    }


}