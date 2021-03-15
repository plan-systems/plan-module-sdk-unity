
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace PlanSDK.Pmod {


    [Serializable]
	[CreateAssetMenu(menuName = "PLAN/UIThemeDef")]
    public class UIThemeDef : ScriptableObject {
    
        [HideInInspector]
        public int                          _version = 1;

        public UILayerMap[]                 Layers;

    }
    
}
