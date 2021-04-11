
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

            var crateBuild = CrateBuild.NewBuild(buildParams);
            var buildDesc = $"<b><color=#008080>{crateBuild.Manifest.Info.CrateURI}</color></b>";

            var buildTargets = buildParams.BuildConfig.GetBuildTargets(); 
            Debug.Log($"{buildDesc} STARTING ({buildTargets.Count} targets)");

            crateBuild.RunExport();
                        
            {
                int successCount = 0;

                var outputDir = crateBuild.OutputPath;

                for (int j = 0; j < buildTargets.Count; j++) {
                    var target = buildTargets[j];

                    var platformDir = $"{outputDir}/{target.TargetString}";
                    if (Directory.Exists(platformDir)) {
                        Directory.Delete(platformDir, true);
                    }
                    Directory.CreateDirectory(platformDir);

                    crateBuild.PrepareToBuild(out var builds);

                    var bundleManifest = BuildPipeline.BuildAssetBundles(platformDir, builds, target.BundleOptions, target.Target);
                    if (bundleManifest == null) {
                        Debug.LogError($"<color=#ff8080>Asset bundle build failed for {platformDir}</color>");
                        return;
                    }
                    
    
                    // Delete junk files 
                    {
                        foreach (var filename in Directory.GetFiles(platformDir, "*.manifest", SearchOption.TopDirectoryOnly)) {
                            File.Delete(filename);
                        }

                        // Unity makes a cheese manifest object for the parent folder
                        File.Delete($"{platformDir}/{target.TargetString}");
                    }
                    
                    successCount++;

                    if (target.Logging) {
                        Debug.Log($"{buildDesc}: <color=#096009>{target.TargetString} complete</color> ({successCount}/{buildTargets.Count})");
                    }

                }
                


                if (successCount == buildTargets.Count) {
                    crateBuild.WriteManifests();

                    string info         = $"{outputDir}/{CrateInfo.kInfoFilename}";
                    string manifest     = $"{outputDir}/{CrateManifest.kManfestFilename}";
                    string manifestJSON = $"{outputDir}/{CrateManifest.kManfestFilename}.json";
                    
                    // Compress and finalize each platform crate
                    foreach (var target in buildTargets) {
                        var platformDir = $"{outputDir}/{target.TargetString}";
    
                        File.Copy(manifest,      $"{platformDir}/{CrateInfo.kInfoFilename}");
                        File.Copy(manifest,      $"{platformDir}/{CrateManifest.kManfestFilename}");
                        File.Copy(manifestJSON,  $"{platformDir}/{CrateManifest.kManfestFilename}.json");
                    
                        var dstZip = $"{outputDir}/{crateBuild.Manifest.Info.CrateNameID}__{crateBuild.Manifest.Info.BuildID}.{target.TargetString}.zip";
                        int[] zipProgress = new int[1];
                        var zipStatus = lzip.compressDir(platformDir, 9, dstZip, true, zipProgress, null, false, 0);
                        
                        if (zipStatus == 1) {
                            Directory.Delete(platformDir, true);
                        } else {
                            Debug.LogError($"lzip error {zipStatus} when creating '{dstZip}' from '{platformDir}'");
                            return;
                        }
                    }
                    
                    File.Delete(info);
                    File.Delete(manifest);
                    File.Delete(manifestJSON);
                
                    Debug.Log($"<b><color=#107010>BUILT {successCount} of {buildTargets.Count}</color></b> to '{crateBuild.OutputPath}'");
                    buildParams.Crate.IncrementBuildNum();
                    EditorUtility.SetDirty(buildParams.Crate);
                    AssetDatabase.SaveAssets();
                }

                crateBuild.Cleanup();
            }

        }
    }

    public class BundleBuild {
        public CrateBuild                   Crate;
        public BundleManifest               Manifest  = new BundleManifest();
        public Transform                    BundleRoot;              // Corresponding item representing this bundle in the scene


        public void                         ExportBundle() {

            RuntimePreviewGenerator.MarkTextureNonReadable = false;
            RuntimePreviewGenerator.BackgroundColor = new Color(0,0,0,0);
            RuntimePreviewGenerator.OrthographicMode = true;

            _bundleAssetDir = $"{Crate.AssetStagingDir}/{Manifest.BundleNameID}";
            CrateBuild.CreateAssetDir(_bundleAssetDir);

            _curPath = Manifest.BundleTitle;

            exportGroup(BundleRoot, "");
        }

        // Always relative to the project Asset dir (i.e. parent dir is project Assets dir)
        string                              _bundleAssetDir;

        // Is "" or includes trailing '/'
        string                              _curPath;

        void                                pushDir(string dirName) {
            _curPath = $"{_curPath}/{dirName}";
        }


        void                                popDir() {
            int len = _curPath.Length;
            int pos = _curPath.LastIndexOf('/');
            if (pos == len-1) {
                len--;
                pos = _curPath.LastIndexOf('/', len-1);
            }

            if (pos > 0) {
                _curPath = _curPath.Substring(0, pos);
            } else {
                _curPath = "";
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
                

            var assetName = item.name.Trim();
            var target = item.AssetTarget;

            if (String.IsNullOrEmpty(item.AssetNameID)) {
                Debug.LogWarning($"asset '{assetName}' skipped because AssetID is empty");
                return;
            }
                
            var entry = new AssetEntry {
                Bounds = item.AssetBounds,
                BrowsePath = $"{_curPath}/{assetName}",        // DEPRECATED
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

                Crate.IconBundle.Manifest.Assets.Add(new AssetEntry {
                    AssetFlags = AssetFlags.IsSprite,
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

        public CrateMaker                           Crate;
        public CrateBuildConfig                     BuildConfig;

        public                                      CrateBuildParams() {


        }

    }


    public class CrateBuild {
        public const string                         kDefaultIconBundleName = "!";

        // Bundle containing icons for this crate's assets.
        public BundleBuild                          IconBundle;
        
        public string                               OutputPath;                 // Absolute path of where build products go
        public string                               AssetStagingDir;            // Asset dir pathname of where tp export module assets (starts with "Assets/")
        public CrateBuildParams                     Params;

        public CrateManifest                        Manifest;
        public List<BundleBuild>                    Bundles = new List<BundleBuild>(10);
        
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

            Manifest = new CrateManifest() {
                Info = Params.Crate.ExportCrateInfo(),
            };


            Bundles.Clear();

            OutputPath = $"{Params.BuildConfig.ExpandedOutputPath}/{Manifest.Info.PublisherID}";
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
            IconBundle = CreateSubBundle(kDefaultIconBundleName, kDefaultIconBundleName);
            IconBundle.Manifest.LoadAllHint = true;
            Manifest.IconBundleName = IconBundle.Manifest.BundleNameID;
        }



        public bool                         AssetDirExists(string assetPath) {
            return Directory.Exists(Params.BuildConfig.ProjectDir + assetPath);
        }

        public static void                  CreateAssetDir(string assetPath) {
            if (AssetDatabase.IsValidFolder(assetPath))
                return;
                    
            // if (assetPath.StartsWith("Assets/")) {
            //     var assetSubPath = assetPath.Substring(7);
            //     Debug.Log($">>{assetSubPath}<<");
            //     if (Directory.Exists(assetSubPath))
            //         return;
            // }
                
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
        
            
        public BundleBuild                  CreateSubBundle(string bundleTitle, string bundleNameID = null) {
            bundleTitle = bundleTitle.Trim();
            if (String.IsNullOrWhiteSpace(bundleNameID)) {
                bundleNameID = LocalFS.FilterName(bundleTitle);
                bundleNameID = bundleNameID.Replace(" ", "-").ToLowerInvariant();
            }

            var bundle = new BundleBuild();
            bundle.Manifest.BundleTitle = bundleTitle;
            bundle.Manifest.BundleNameID = $"{Manifest.Info.CrateNameID}.{bundleNameID} {Manifest.Info.BuildID}";
            bundle.Crate = this;

            Bundles.Add(bundle);
            this.Manifest.Bundles.Add(bundle.Manifest);

            return bundle;
        }



        void                                AddBundles(Transform bundles) {

            int N = bundles.childCount;
            for (int i = 0; i < N; i++) {
                var bundleObj = bundles.GetChild(i);
                var bundleBuild = CreateSubBundle(bundleObj.name);
                bundleBuild.BundleRoot = bundleObj;
            }

        }

        void                                ExportBundles() {

            AddBundles(Params.Crate.transform);

            {
                var removeList = new List<BundleBuild>();
                foreach (var bundle in Bundles) {
                    bundle.ExportBundle();
                    if (bundle.Manifest.Assets.Count == 0) {
                        removeList.Add(bundle);
                    }
                }
    
                foreach (var i in removeList) {
                    this.Manifest.Bundles.Remove(i.Manifest);
                    Bundles.Remove(i);
                }
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
                IconBundle.Manifest.Assets.Add(new AssetEntry {
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
    
                Array.Sort<BundleManifest>(blist, (a, b) => a.BundleTitle.CompareTo(b.BundleTitle) ); 
                manifest.Bundles.Clear();
                manifest.Bundles.Add(blist);
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

                builds[i].assetBundleName = bundle.BundleNameID + ".assetbundle";

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
            
            {
                byte[] buf = gpb.MessageExtensions.ToByteArray(this.Manifest.Info);
                File.WriteAllBytes($"{OutputPath}/{CrateInfo.kInfoFilename}", buf);
            }
            
            {
                string manifestPath = $"{OutputPath}/{CrateManifest.kManfestFilename}";
                byte[] buf = gpb.MessageExtensions.ToByteArray(this.Manifest);
                File.WriteAllBytes(manifestPath, buf);

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