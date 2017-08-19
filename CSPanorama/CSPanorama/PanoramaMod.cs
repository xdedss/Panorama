using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICities;
using UnityEngine;

namespace CSPanorama
{
    public class PanoramaMod : IUserMod
    {
        string IUserMod.Description
        {
            get
            {
                return "Take a full view photo of your city.";
            }
        }

        string IUserMod.Name
        {
            get
            {
                return "Panorama";
            }
        }
    }

    public class PanoramaLoading : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if((mode == LoadMode.NewGame||mode == LoadMode.LoadGame) && GameObject.Find("PanoramaController") == null)
            {
                new GameObject("PanoramaController").AddComponent<PanoramaController>();
            }
        }
    }
}
