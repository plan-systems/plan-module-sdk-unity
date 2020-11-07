using UnityEngine;
using System.Collections;

namespace Plan {
        
    public class ModAsset : MonoBehaviour {
    
        [HideInInspector]
        public bool                         AutoGenerateIcon = true;

        [HideInInspector]
        public Sprite                       CustomIcon;

        [HideInInspector]
        public bool                         IsGlyph;
        [HideInInspector]
        public bool                         IsStruct;
        [HideInInspector]
        public Bounds                       AssetBounds;

        public Transform                    AssetTarget {
            get => (transform.childCount > 0) ? transform.GetChild(0) : null;
        }


        Skybox                              _skybox;
        public Skybox                       Skybox {
            get => GetComponent<Skybox>();
        }

        // void                                OnValidate() {
        //     _skybox = GetComponent<Skybox>();
        //     if (IsGlyph == false && IsStruct == false && _skybox == null) {
        //         var asset = AssetTarget;
        //         if (asset != null) {
        //             IsGlyph = true;

        //             if (asset.GetComponent<ParticleSystem>()) {
        //                 if (AutoGenerateIcon) {
        //                     AutoGenerateIcon = false;
        //                 }
        //             }
        //         }
        //     }
        // }

        void                                OnDrawGizmosSelected() {
            var sz = AssetBounds.size;

            if (sz.x > 0) {
                // Draw a yellow cube at the transform position
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position + AssetBounds.center, AssetBounds.size);
            }
        }

    }

}