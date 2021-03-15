
using UnityEngine;

namespace PlanSDK.Crates {

    public partial class AssetEntry {

        public Bounds                       Bounds {
            get {
                return new Bounds(
                    new Vector3(centerX_, centerY_, centerZ_),
                    new Vector3(2*extentsX_, 2*extentsY_, 2*extentsZ_)
                );
            }
            set {
                const float kEpsilon = 1e-7f;

                var cen = value.center;
                centerX_ = (Mathf.Abs(cen.x) > kEpsilon) ? cen.x : 0;
                centerY_ = (Mathf.Abs(cen.y) > kEpsilon) ? cen.y : 0;
                centerZ_ = (Mathf.Abs(cen.z) > kEpsilon) ? cen.z : 0;

                var ext = value.extents;
                extentsX_ = (Mathf.Abs(ext.x) > kEpsilon) ? ext.x : 0;
                extentsY_ = (Mathf.Abs(ext.y) > kEpsilon) ? ext.y : 0;
                extentsZ_ = (Mathf.Abs(ext.z) > kEpsilon) ? ext.z : 0;
            }
        }


    }

}