using UnityEngine;
using System;

namespace PlanSDK.CrateSDK {
        

    public class CrateMaker : MonoBehaviour {
    
        [HideInInspector]
        public int                          _version = 1;
        
        public Sprite                       CrateIcon;


        [Header("Crate Info")]
        public string                       CrateTitle          = "My Crate Title";
        public string                       CrateNameID         = "static-create-name-id";       
        
        public string                       HomeDomain          = "org-name.org";
        public string                       ShortDescription    = "";

        // Seconds UTC
        [HideInInspector]
        public long                         TimeCreated;         
        
        [Tooltip("A list of tags for this crate separated by commas")]
        public string                       Tags = "";       
        
        
        [Header("Build Info")]
        public int                          MajorVersion = 0;
        public int                          MinorVersion = 1;
        
        [SerializeField]
        int                                 _buildNumber = 1;
        


        public void                         IncrementBuildNum() {
            _buildNumber++;
        }
        
        public Crates.CrateInfo             ExportCrateInfo() {
            var info = new Crates.CrateInfo() {
                HomeDomain    = HomeDomain,
                NameID        = CrateNameID,
                Title         = CrateTitle,
                ShortDesc     = ShortDescription,
                Tags          = Tags,
                TimeCreated   = TimeCreated,
                TimeBuilt     = IssueSecondsUTC(),
                MajorVersion  = MajorVersion,
                MinorVersion  = MinorVersion,
                BuildNumber   = _buildNumber,
            };
            
            string dateStr = DateTime.Now.ToString("yyMMdd");
            info.BuildID = $"{dateStr}-{info.VersionID}";
            
            return info;
        }
                       

        
        public static string                FilterAssetID(string assetID) {
            assetID = LocalFS.FilterName(assetID);

            assetID.Trim();
            assetID = assetID.Replace("---", "-"); 
            assetID = assetID.Replace("  ", " "); 
            assetID = assetID.Replace("   ", " "); 
            return assetID;
        }
        
        public static long                  IssueSecondsUTC() {
            return (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
        

        void                                OnValidate() {
            
            if (TimeCreated == 0) {
                TimeCreated = IssueSecondsUTC();
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



    }

}