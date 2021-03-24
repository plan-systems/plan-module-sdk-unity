using UnityEngine;
using System;

namespace PlanSDK.CrateSDK {
        

    public class CrateMaker : MonoBehaviour {
    
        [HideInInspector]
        public int                          _version = 1;
        
        public Sprite                       CrateIcon;


        [Header("Crate Info")]
        public string                       CrateTitle          = "org-name.org";
        public string                       CrateNameID         = "my-create-name-id";       
        
        public string                       HomeDomain          = "org-name.org";
        public string                       ShortDescription    = "";

        
        [Tooltip("A list of tags for this crate separated by commas")]
        public string                       Tags = "";       
        
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
            assetID = assetID.Replace("---", "-"); 
            assetID = assetID.Replace("  ", " "); 
            assetID = assetID.Replace("   ", " "); 
            return assetID;
        }
        

        void                                OnValidate() {

            
            if (String.IsNullOrWhiteSpace(_buildID)) {
                _buildID = "{yyMMdd}";
            }
            
            if (String.IsNullOrWhiteSpace(CrateNameID)) {
                CrateNameID = CrateTitle;
            }
            
            if (String.IsNullOrWhiteSpace(CrateNameID)) {
                CrateNameID = FilterAssetID(CrateNameID);
            }
            
            if (String.IsNullOrEmpty(Tags) == false) {
                bool changed = false;
                var tags = Tags.Split(',');
                for (int i = 0; i < tags.Length; i++) {
                    var filtered = tags[i].Trim();
                    if (filtered != tags[i]) {
                        changed = true;
                        tags[i] = filtered;
                    }
                }
                if (changed) {
                    var builder = new System.Text.StringBuilder();
                    for (int i = 0; i < tags.Length; i++) {
                        if (i > 0)
                            builder.Append(',');
                        builder.Append(tags[i]);
                    }
                    Tags = builder.ToString();
                }
            }
            
            gameObject.name = $"CRATE ({CrateNameID})";
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