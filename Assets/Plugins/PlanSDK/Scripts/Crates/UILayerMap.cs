
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace PlanSDK.Pmod {

    [Serializable]
    public struct UIAssetRef {
        public string                       keyID;
        public string                       assetURI;
        public Texture                      texture;
        public Transform                    transform;
        //public CrateItem                     
    }

    [Serializable]
	[CreateAssetMenu(menuName = "PLAN/UILayerMap")]
    public class UILayerMap : ScriptableObject {
    
        [HideInInspector]
        public int                          _version = 1;
        
        public UIAssetRef[]                 Entries;
    

    }
    
}
