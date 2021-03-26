using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.IO;


namespace PlanSDK.CrateSDK {
        
    [CustomEditor(typeof(CrateMaker))]
    public class CrateMaker_Edit : Editor {
    

        public override void                OnInspectorGUI() {
            DrawDefaultInspector();

            var crate = target as CrateMaker;

            if (GUILayout.Button("Build Crate") ) {
            
                var build = new CrateBuildParams();
                build.Crate = crate;
                build.BuildSuffix = crate.BuildSuffix;
                
                RenameEmptySubs(crate.transform);

                EditorDispatcher.Dispatch(() => {
                    BundleBuilder.BuildModule(build);
                });

            }

            var buildConfig = CrateBuildConfig.CurrentConfig();
            var outputPath = $"{buildConfig.ExpandedOutputPath}/{crate.HomeDomain}";
            EditorGUILayout.HelpBox(outputPath, MessageType.None);

            if (Directory.Exists(outputPath)) {
                if (GUILayout.Button("Reveal in Finder") ) {
                    EditorUtility.RevealInFinder(LocalFS.ProcessPath(outputPath));
                }
            }

        }

   

        public static void                 RenameEmptySubs(Transform obj, string nonNilName = "-") {
            if (String.IsNullOrWhiteSpace(obj.gameObject.name)) {
                obj.gameObject.name = nonNilName;
            }
            for (int i = 0; i < obj.childCount; i++ ) {
                RenameEmptySubs(obj.GetChild(i), nonNilName);
            }
        }


    }



}
