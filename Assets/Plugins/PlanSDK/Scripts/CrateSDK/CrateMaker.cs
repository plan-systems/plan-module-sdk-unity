using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlanSDK.CrateSDK {
        

    public class CrateMaker : MonoBehaviour {
    
        [HideInInspector]
        [SerializeField] int                _version;
        
        public Sprite                       CrateIcon;

        [Header("Publisher Info")]
        [Tooltip("This identifies a publisher and is not visible to the user.  A publisher must use the same ID for all its crates.  By convention, you should select an ICANN domain name that you own and control.")]
        public string                       PublisherID;
        [Tooltip("This identifies a publisher to the user and is for optics only.  It can be changed without any reprocussions.")]
        public string                       PublisherName;
        
        [Header("Crate Info")]
        [FormerlySerializedAs("CrateNameID")]
        [Tooltip("This identifies a crate (for a given publisher) and is not visible to the user.  It cannot be changed after a crate is deployed or else the new crate will not be linked to the previous version.  Because it is within a given publisher ID, avoid redundant name parts.")]
        public string                       CrateID             = "thing.sub-thing.detail";       
        [Tooltip("This identifies a crate to the user and is for optics only.  It can be changed without any reprocussions.")]
        [FormerlySerializedAs("CrateTitle")]
        public string                       CrateName           = "My Crate Name";
        
        public string                       ShortDescription    = "";

        // Seconds UTC
        [HideInInspector]
        public long                         TimeCreated;         
        
        [Tooltip("A list of tags for this crate separated by commas")]
        public string                       Tags = "";       
        
        [Tooltip("An optional URL intended for a human to learn more about this crate")]
        public string                       HomeURL;
        
        [Header("Build Info")]
        public int                          MajorVersion = 0;
        public int                          MinorVersion = 1;
        
        [SerializeField] int                _buildNumber = 1;
        

        public void                         IncrementBuildNum() {
            _buildNumber++;
        }
        
        public Crates.CrateInfo             ExportCrateInfo() {
            var info = new Crates.CrateInfo() {
                CrateSchema   = (int) Crates.CrateSchema.V100,
                CrateURI      = $"{PublisherID.Trim()}/{CrateID.Trim()}",
                PublisherName = (PublisherName != null) ? PublisherName : PublisherID,
                CrateName     = CrateName.Trim(),
                ShortDesc     = ShortDescription.Trim(),
                Tags          = Tags,
                TimeCreated   = TimeCreated,
                TimeBuilt     = IssueSecondsUTC(),
                MajorVersion  = MajorVersion,
                MinorVersion  = MinorVersion,
                BuildNumber   = _buildNumber,
                HomeURL       = HomeURL.Trim(),
            };
            
            if (String.IsNullOrWhiteSpace(HomeURL) == false)
                info.HomeURL = HomeURL.Trim();
            
            string dateStr = DateTime.Now.ToString("yyMMdd");
            info.BuildID = $"{dateStr}-{info.VersionID}";
            
            return info;
        }
                       

        

        
        public static long                  IssueSecondsUTC() {
            return (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
        

        void                                OnValidate() {
            
            if (_version < 2) {
                _version = 2;
            }
            
            if (String.IsNullOrEmpty(PublisherID)) {
                PublisherID = "plan-systems.org";
            } else {
                PublisherID = CrateItem.ValidateNameID(PublisherID);
            }
            
            if (TimeCreated == 0) {
                TimeCreated = IssueSecondsUTC();
            }
            
            if (String.IsNullOrWhiteSpace(HomeURL)) {
                HomeURL = "";
            }
            
            if (String.IsNullOrWhiteSpace(CrateID)) {
                CrateID = CrateName;
            }
            
            CrateID = CrateItem.ValidateNameID(CrateID);
            
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
            
            gameObject.name = $"|CRATE| {CrateName}";
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