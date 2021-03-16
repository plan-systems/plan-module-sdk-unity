using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;


namespace PlanSDK.CrateSDK {
        
    [CustomEditor(typeof(CrateItem))]
    public partial class CrateItem_Edit : Editor {
    
        SerializedProperty                  _autoScale;
        SerializedProperty                  _isSurface;
        
        static GUIStyle                     _boxStyle; // = EditorStyles.helpBox;
        
        void                                OnEnable() {
            
            _autoScale = serializedObject.FindProperty("AutoScaleByDefault");
            _isSurface = serializedObject.FindProperty("IsSurface");


            // SerializedProperty vers = serializedObject.FindProperty("_version");
            // if (vers.intValue == 1) {
            //     _autoScale.boolValue = serializedObject.FindProperty("IsGlyph").boolValue;
            //     _isSurface.boolValue = serializedObject.FindProperty("IsStruct").boolValue;
            //     Debug.Log($"Updated '{target.name}' from version {vers.intValue} to {2}");
            //     vers.intValue = 2;
                
            //     serializedObject.ApplyModifiedProperties();
            // }

        }
        
        public override void                OnInspectorGUI() {
            serializedObject.Update();
            
            DrawDefaultInspector();

            //EditorGUI.PropertyField(position, property, label, true);
   
         
            var mass = target as CrateItem;

            {
                EditorGUI.BeginChangeCheck();
                bool autoIcon = EditorGUILayout.Toggle("Auto generate icon",  mass.AutoGenerateIcon);
                if (EditorGUI.EndChangeCheck()) {
                    mass.AutoGenerateIcon = autoIcon;
                }

                    
                if (autoIcon == false) {
                    EditorGUI.BeginChangeCheck();
                    var customIcon = (Sprite) EditorGUILayout.ObjectField("Custom Icon", mass.CustomIcon, typeof(Sprite), true);

                    if (EditorGUI.EndChangeCheck()) {

                        if (mass.CustomIcon != customIcon && customIcon != null) {
                            mass.AutoGenerateIcon = false;
                        }

                        mass.CustomIcon = customIcon;
                    }
                }
            }


            //EditorGUILayout.HelpBox(CrateMaker.NextBuildInfo, MessageType.Info);
            if (mass.Skybox != null) {
                EditorGUILayout.HelpBox("IsSkybox", MessageType.Info);
            } else {
//                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(_autoScale);
                EditorGUILayout.PropertyField(_isSurface);

                // if (EditorGUI.EndChangeCheck()) {
                //     Undo.RecordObject(mass, "change module asset flags");
                //     mass.IsGlyph  = isGlyph;
                //     mass.IsStruct = isStruct;
                // }
                if (_boxStyle == null) {
                    _boxStyle = new GUIStyle(GUI.skin.box);
                    _boxStyle.padding = new RectOffset(15, 10, 5, 5);
                }
                
                EditorGUILayout.LabelField("Extents", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(_boxStyle);
                {
                    EditorGUI.BeginChangeCheck();

                    var center  = EditorGUILayout.Vector3Field("Center",  mass.AssetBounds.center);
                    var extents = EditorGUILayout.Vector3Field("Extents", mass.AssetBounds.extents);

                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(mass, "change module asset bounds");
                        mass.AssetBounds.center  = center;
                        mass.AssetBounds.extents = extents;
                    }

                    if (GUILayout.Button("Regen Extents") ) {
                        regenExtents();
                    }
                }
                EditorGUILayout.EndVertical();

            }

           
            serializedObject.ApplyModifiedProperties();
            // EditorGUILayout.HelpBox("This is a help box", MessageType.Info);

            //             Rect r = (Rect)EditorGUILayout.BeginVertical();
            // if (GUI.Button(r, GUIContent.none))
            //     Debug.Log("Go here");
            // GUILayout.Label("I'm inside the button");
            // GUILayout.Label("So am I");
            // EditorGUILayout.EndVertical();


        }


        void                                regenExtents() {
            var mass = target as CrateItem;
            var bounds = new Bounds();
            int boundsCount = 0;
            var asset = mass.AssetTarget;
            if (asset != null) {
                deepRegenBounds(mass.transform, 5, ref bounds, ref boundsCount);
            }

            if (boundsCount > 0) {
                bounds.center = bounds.center - asset.position + asset.localPosition;
            }
            mass.AssetBounds = bounds;
        }


        static void                         deepRegenBounds(Transform item, int depthRemain, ref Bounds bounds, ref int boundsCount) {

            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer != null) {
                var subBounds = renderer.bounds;
                if (boundsCount > 0) {
                    bounds.Encapsulate(subBounds);
                } else {
                    bounds = subBounds;
                }
                boundsCount++;
            }
            var terrain = item.GetComponent<TerrainCollider>();
            if (terrain != null) {
                var subBounds = terrain.bounds;
                if (boundsCount > 0) {
                    bounds.Encapsulate(subBounds);
                } else {
                    bounds = subBounds;
                }
                boundsCount++;
            }

            if (depthRemain > 0) {
                int N = item.childCount;
                for (int i = 0; i < N; i++) {
                    var sub = item.GetChild(i);
                    deepRegenBounds(sub, depthRemain-1, ref bounds, ref boundsCount);
                }
            }
        }

    }



}
