using UnityEngine;
using System;
using System.Text;

namespace PlanSDK.Crates {

    public sealed partial class CrateManifest {
        public const string                 kCrateIconNameID = ".crate-icon";
    
        public const string                 kTagPrivate = "private";

        public const string                 kManfestFilename = "_CrateManifest.pb";

    }


    public sealed partial class CrateInfo {
        public const string                 kInfoFilename    = "_CrateInfo.pb";

        public string                       VersionID {
            get => $"v{MajorVersion}.{MinorVersion}.{BuildNumber}";
        }
        
        // Symbolically denotes the most recent BuildID of a crate (instead of an explicit BuildID)
        public const string                 kMostRecentBuildID = "_";
        
        public string                       CrateURI        { get => $"{HomeDomain}/{NameID}";              }
        public string                       CrateBuildURI   { get => $"{HomeDomain}/{NameID}/{BuildID}";    }
        
        public void                         SetURL(string url, StringBuilder scrap = null, string path = null) {
            if (scrap == null)
                scrap = new StringBuilder(200);
                
            scrap.Clear();
            scrap.Append(url);
            if (path != null)
                scrap.Replace("{.}", path);
            scrap.Replace("{CrateDomain}", HomeDomain);
            scrap.Replace("{CrateNameID}", NameID);
            scrap.Replace("{CrateBuildID}", BuildID);
            scrap.Replace("{PlatformID}", kThisPlatformID);
                
            URL = scrap.ToString();
        }
                
        public bool                         IsNewerOrEqualTo(string buildID) {
            if (BuildID != null && buildID != null) {
                if (String.CompareOrdinal(BuildID, buildID) >= 0) 
                    return true;
            }
            return false;
        }
        
        public bool                         IsOlderThan(string buildID) {
            if (BuildID != null && buildID != null) {
                if (String.CompareOrdinal(BuildID, buildID) < 0) 
                    return true;
            }
            return false;
        }
        
        public string                       TimeBuiltString {
            get => DateTimeOffset.FromUnixTimeSeconds(TimeBuilt).DateTime.ToShortDateString();
        }
        
        public const string                 kThisPlatformID =
            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                "Windows";
            #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                "macOS";
            #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                "Linux";
            #elif UNITY_IOS
                "iOS";
            #elif UNITY_ANDROID
                "Android";
            #else
                #error "Unsupported build platform, son!"
            #endif
            
       
    }

}    