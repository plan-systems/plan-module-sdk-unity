using UnityEngine;
using System.Collections;
using System;

namespace PlanSDK.CrateSDK {
        

    public class CrateMaker : MonoBehaviour {
    
        [HideInInspector]
        public int                          _version = 1;
        
        public Sprite                       CrateIcon;


        [Header("Create Info")]
        public string                       CrateTitle       = "org-name.org";
        public string                       HomeDomain       = "org-name.org";

        [HideInInspector]
        public string                       ModuleName      = "my-module-name";     // DEPRECATED
        [HideInInspector]
        public string                       ModuleDomain    = "org-name.org";       // DEPRECATED
        [HideInInspector]
        public Sprite                       ModuleIcon;                             // DEPRECATED
        

        public string                       CrateNameID     = "my-pack-id";
        
        [SerializeField]
        string                              _buildID;
        
        [HideInInspector]
        [SerializeField]
        int                                 _buildNumber;
        
        public string                       IssueBuildID() {
            string buildID = _buildID;
            if (_buildID.Contains("{") && _buildID.Contains("}")) {
                buildID = buildID.Replace("{", "").Replace("}", "");
                buildID = DateTime.Now.ToString(buildID).Substring(1);
            }
            _buildNumber++;
            return buildID;
        }
        
        
        public static string                FilterAssetID(string assetID) {
            const string kIllegal = "\\/:*?\"'\'&@<>|!"; 
            assetID = assetID.ToLower().Replace(" ", "-"); 

            foreach (char c in kIllegal) {
                assetID = assetID.Replace(c.ToString(), ""); 
            }
            assetID.Trim();
            assetID = assetID.Replace("  ", " "); 
            assetID = assetID.Replace("   ", " "); 
            return assetID;
        }
        

        void                                OnValidate() {

            if (_version == 1) {
                _version = 2;
                
                HomeDomain = ModuleDomain;
                CrateIcon = ModuleIcon;
                CrateTitle = ModuleName;
            }
            
            if (String.IsNullOrWhiteSpace(_buildID)) {
                _buildID = "{yyMMdd}";
            }
            
            if (String.IsNullOrWhiteSpace(CrateNameID)) {
                CrateNameID = CrateTitle;
            }
            
            if (String.IsNullOrWhiteSpace(CrateNameID)) {
                CrateNameID = FilterAssetID(CrateNameID);
            }
            
            gameObject.name = $"{HomeDomain}/{CrateNameID}";
        }
        
        // public string                       NextBuildInfo {
        //     get {
        //         if (String.IsNullOrEmpty(_buildInfo)) {
        //             //buildInfo = $"Next build: {DateTime.Now.Year}.{_curBuildNumber:00}";
        //             _buildInfo = $"Next build: {DateTime.UtcNow.Ticks:x08}";
        //         }
        //         return _buildInfo;
        //     }
        // }


        public string                       BuildSuffix {
            get {
                var ticks = DateTime.UtcNow.Ticks / 10000000;
                var unixSecs = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                return $"{unixSecs:x08}";
            }
        }


        int                                 _curBuildNumber = 1;


        public void                         IncrementBuildNum() {
            _curBuildNumber++;
        }



    }

}