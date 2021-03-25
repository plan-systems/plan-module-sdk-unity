using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanSDK {
    
    public class LocalFS {
    
        public static string                ProcessPath(string path) {
        
            // Save work and allocations if no processing is needed
            if (path.Contains("/..") == false)
                return path;
                
            var parts = path.Split('/');
            int N = parts.Length;
            int dN = 0;
            for (int i = 1; i < N; i++) {
                if (parts[i] == "..") {
                    if (i-dN > 0)
                        dN += 2;
                } else if (dN > 0) {
                    parts[i-dN] = parts[i];
                }
            }
            N -= dN;
            
            var outPath = new System.Text.StringBuilder(path.Length);
            for (int i = 0; i < N; i++) {
                if (i > 0) {
                    outPath.Append('/');
                }
                outPath.Append(parts[i]);
            }
            return outPath.ToString();
        }
    }

}