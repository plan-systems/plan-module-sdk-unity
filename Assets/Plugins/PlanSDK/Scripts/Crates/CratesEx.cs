using UnityEngine;
using System;
using System.Text;
using System.IO;


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
        
        public string                       CrateBuildURI   { get => $"{CrateURI}/{BuildID}";    }
        
        public string                       PublisherID {
            get {
                var idx = CrateURI.LastIndexOf('/');
                return CrateURI.Substring(0, idx);
            }
        }
        
        public string                       CrateNameID {
            get {
                var idx = CrateURI.LastIndexOf('/');
                return CrateURI.Substring(idx + 1);
            }
        }
        
        public void                         SetURL(string url, StringBuilder scrap = null, string path = null) {
            if (scrap == null)
                scrap = new StringBuilder(200);
                
            scrap.Clear();
            scrap.Append(url);
            if (path != null)
                scrap.Replace("{.}", path);
            scrap.Replace("{CrateURI}", CrateURI);
            scrap.Replace("{CrateBuildID}", BuildID);
            scrap.Replace("{PlatformID}", kThisPlatformID);
                
            URL = scrap.ToString();
        }
                
        public bool                         IsUpdateOf(CrateInfo other) {
            if (CrateURI == other.CrateURI && BuildID != null && other != null && other.BuildID != null) {
                if (String.CompareOrdinal(BuildID, other.BuildID) > 0) 
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