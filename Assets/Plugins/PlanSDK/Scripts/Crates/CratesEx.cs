using UnityEngine;



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