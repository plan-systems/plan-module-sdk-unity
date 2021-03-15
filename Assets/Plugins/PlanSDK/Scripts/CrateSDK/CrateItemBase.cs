using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanSDK.CrateSDK {
    
    public class CrateItemBase : MonoBehaviour {
    

        // This should be edited to 2 once all existing modules are up to date
        [HideInInspector]
        public int                          _version = 1;
        
        [Tooltip("AssetNameID permanently identifies this asset.  Changing this ID after this asset has been published will cause references to this asset to break.")]
        public string                       AssetNameID;

        [Tooltip("Assets that are private are not visible to users browsing the module")]
        public bool                         IsPrivate;
        
        [Tooltip("A short phase or sentence decribing this item")]
        public string                       ShortDescription;
        
        [Tooltip("When this is enabled, this asset will be omitted from the build")]
        public bool                         ExcludeFromBuild;
        
        [HideInInspector]
        public bool                         AutoGenerateIcon = true;
        

        [HideInInspector]
        public Sprite                       CustomIcon;


        public virtual string               ExportTagList() {
            return "";
        }

        
    }

}