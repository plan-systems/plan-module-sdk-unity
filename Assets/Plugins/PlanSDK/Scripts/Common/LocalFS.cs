using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.IO;

namespace PlanSDK {
    
    public class LocalFS {
    
        static readonly Regex               IllegalFileChars;
        
        static LocalFS() {

            IllegalFileChars = new Regex(@"\/:*?""'&@<>|", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
            //new Regex(String.Format(@"^(CON|PRN|AUX|NUL|CLOCK\$|COM[1-9]|LPT[1-9])(?=\..|$)|(^(\.+|\s+)$)|((\.+|\s+)$)|([{0}])",                          
            //Regex.Escape(new String(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        }
        
        public static string                ProcessPath(string path) {
            path = path.Replace("//", "/");

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
        
        
        public static string                FilterName(string name, string invalidCharReplacement = "") {
            name = IllegalFileChars.Replace(name, invalidCharReplacement);
            name = name.Trim();
            return name;
        }
    }

}