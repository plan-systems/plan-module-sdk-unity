using UnityEngine;
using System.Collections;
using System;

namespace Plan {
        

    public class PlanModule : MonoBehaviour {
    
        public Sprite                       ModuleIcon;

        [Header("ModuleURI")]
        public string                       ModuleDomain    = "org-name.org";
        public string                       ModuleName      = "my-module-name";

        // public string                       NextBuildInfo {
        //     get {
        //         if (String.IsNullOrEmpty(_buildInfo)) {
        //             //buildInfo = $"Next build: {DateTime.Now.Year}.{_curBuildNumber:00}";
        //             _buildInfo = $"Next build: {DateTime.UtcNow.Ticks:x08}";
        //         }
        //         return _buildInfo;
        //     }
        // }


        public string                       BuildSuffix {
            get {
                var ticks = DateTime.UtcNow.Ticks / 10000000;
                var unixSecs = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                return $"{unixSecs:x08}";
            }
        }


        int                                 _curBuildNumber = 1;


        public void                         IncrementBuildNum() {
            _curBuildNumber++;
        }



    }

}