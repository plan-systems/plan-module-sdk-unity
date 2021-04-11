using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlanSDK.CrateSDK {
        
    public class CrateItem : CrateItemBase {
    
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
            
            if (String.IsNullOrWhiteSpace(AssetID)) {
                AssetID = CrateItem.ValidateNameID(gameObject.name.ToLowerInvariant());
            } else {
                AssetID = AssetID.Trim();
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
    
        static readonly Color               kVerticalGuideColor = new Color(0,1,0,.5f);
        static readonly Color               kGuideColor         = new Color(1,1,0,.6f);
        
        void                                OnDrawGizmosSelected() {
            var sz = AssetBounds.size;
            Gizmos.color = kGuideColor;

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
                    float segLen = 10f;
                    if (i < 2) {
                        ax = (i == 0) ? Vector3.right : Vector3.forward;
                    } else {
                        segLen = 100f;
                        Gizmos.color = kVerticalGuideColor;
                        ax = Vector3.up;
                    }
                    Gizmos.DrawLine(pos - segLen * ax,  pos -       ax);
                    Gizmos.DrawLine(pos + segLen * ax,  pos +       ax);
                    Gizmos.DrawLine(pos -    .5f * ax,  pos + .5f * ax);
                }
            }
        }
        
        
        
        static readonly Regex _NonNameIDChars = new Regex(@"[^a-zA-Z_.-]", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        public static string                ValidateNameID(string nameID, string invalidCharReplacement = "") {
            nameID = nameID.Trim().Replace(" ", "-");
            nameID = _NonNameIDChars.Replace(nameID, "");
            return nameID;
        }
        
        

    }

}