using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;


namespace PlanSDK.Pmod {
        
    [CustomEditor(typeof(UILayerMap))]
    public partial class UILayerMap_Edit : Editor {
    
        // SerializedProperty                  _autoScale;
        // SerializedProperty                  _isSurface;
        // SerializedProperty                  _isPublic;
        

        //static GUIStyle                     _boxStyle = EditorStyles.helpBox;
        
        void                                OnEnable() {
            

        }
        
        public override void                OnInspectorGUI() {
            serializedObject.Update();
            
            DrawDefaultInspector();
        }
    }
    
}