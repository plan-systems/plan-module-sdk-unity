using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.IO;


namespace Plan {
        
    [CustomEditor(typeof(PlanModule))]
    public class PlanModule_Edit : Editor {
    

        public override void                OnInspectorGUI() {
            DrawDefaultInspector();

            var planMod = target as PlanModule;



            if (GUILayout.Button("Build Module") ) {
            
                var build = new ModuleBuildParams();
                build.PlanMod = planMod;
                build.BuildSuffix = planMod.BuildSuffix;

                EditorDispatcher.Dispatch(() => {
                    BundleBuilder.BuildModule(build);
                });

            }

            var buildConfig = ModBuildConfig.CurrentConfig();
            var outputPath = $"{buildConfig.ExpandedOutputPath}/{planMod.ModuleDomain}";
            EditorGUILayout.HelpBox(outputPath, MessageType.None);

            if (Directory.Exists(outputPath)) {
                if (GUILayout.Button("Reveal in Finder") ) {
                    EditorUtility.RevealInFinder(outputPath);
                }
            }

        }

   


    }



}
