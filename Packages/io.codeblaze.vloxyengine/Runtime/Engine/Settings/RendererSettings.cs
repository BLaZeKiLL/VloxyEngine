using System;

using UnityEngine;
using UnityEngine.Serialization;

namespace CodeBlaze.Vloxy.Engine.Settings {

    [Serializable]
    public class RendererSettings {

        public Material[] Materials;
        public bool CastShadows;

    }

}