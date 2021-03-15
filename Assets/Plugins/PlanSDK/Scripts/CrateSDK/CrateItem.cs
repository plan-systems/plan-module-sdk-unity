using System;
using UnityEngine;
using System.Collections;

namespace PlanSDK.CrateSDK {
        
    public class CrateItem : CrateItemBase {
    
        [HideInInspector]
        public bool                         IsGlyph;        // DEPRECATED -- REMOVE IN V3
        [HideInInspector]
        public bool                         IsStruct;       // DEPRECATED -- REMOVE IN V3
        
        [HideInInspector]
        public Bounds                       AssetBounds;
        
        // FUTURE: use a 0..1 value that helps the engine to decide which mode to view.
        // i.e. if placing a cat from 1000m, auto scale is likely desired.  If placing from 5 meters away, probably not.
        [HideInInspector]
        public bool                         AutoScaleByDefault;
        
        // FUTURE: auto generate this based on if the asset contains a nav surface 
        [HideInInspector]
        public bool                         IsSurface;

        
        public Transform                    AssetTarget {
            get => (transform.childCount > 0) ? transform.GetChild(0) : null;
        }


        Skybox                              _skybox;
        public Skybox                       Skybox {
            get => GetComponent<Skybox>();
        }
        
        void                                OnValidate() {
            if (_version <= 1) {
                AutoScaleByDefault = IsGlyph;
                IsSurface = IsStruct;
                Debug.Log($"Updated '{name}' from version {_version} to {2}");
                _version = 2;
            }
            
            if (String.IsNullOrWhiteSpace(AssetNameID)) {
                AssetNameID = CrateMaker.FilterAssetID(gameObject.name.ToLowerInvariant());
            }
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
            Gizmos.color = Color.yellow;

            if (sz.x > 0) {
                // Draw a yellow cube at the transform position.
                // Add a slight amount so they don't get stomped on
                sz += .05f * Vector3.one;
                Gizmos.DrawWireCube(transform.position + AssetBounds.center, sz);
            }
            
            {
                var pos = transform.position;
                Gizmos.DrawSphere(pos, .05f);
                for (int i = 0; i < 3; i++) {
                    Vector3 ax;
                    if (i < 2) {
                        ax = (i == 0) ? Vector3.right : Vector3.forward;
                    } else {
                        Gizmos.color = Color.green;
                        ax = Vector3.up;
                    }
                    Gizmos.DrawLine(pos - 10f * ax,  pos - ax);
                    Gizmos.DrawLine(pos + 10f * ax,  pos + ax);
                    Gizmos.DrawLine(pos - .5f * ax,  pos + .5f * ax);
                }
            }
        }

    }

}