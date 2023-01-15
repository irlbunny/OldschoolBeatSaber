using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace OldschoolBeatSaber.Utilities
{
    internal static class ResourceUtility
    {
        private static Dictionary<string, UnityEngine.Object> _cachedObjs = new();

        private static Mesh _tubeMesh;
        public static Mesh TubeMesh => _tubeMesh ??= new Mesh
        {
            name = "Tube",
            vertices = new Vector3[]
            {
                new(1, -1, -1),
                new(1, -1, 1),
                new(1, 1, -1),
                new(1, 1, 1),
                new(-1, -1, -1),
                new(-1, -1, 1),
                new(-1, 1, -1),
                new(-1, 1, 1),
                new(1, 1, -1),
                new(1, 1, 1),
                new(-1, 1, -1),
                new(-1, 1, 1),
                new(-1, -1, -1),
                new(1, -1, -1),
                new(1, -1, 1),
                new(-1, -1, 1),
                new(1, 1, -1),
                new(1, -1, -1),
                new(-1, -1, -1),
                new(-1, 1, -1),
                new(-1, 1, 1),
                new(-1, -1, 1),
                new(1, -1, 1),
                new(1, 1, 1)
            },
            triangles = new[]
            {
                0, 2, 3,
                0, 3, 1,
                8, 6, 7,
                8, 7, 9,
                10, 4, 5,
                10, 5, 11,
                12, 13, 14,
                12, 14, 15,
                16, 17, 18,
                16, 18, 19,
                20, 21, 22,
                20, 22, 23
            }
        };

        private static Material _bigSmokeParticleMaterial;
        public static Material BigSmokeParticleMaterial => _bigSmokeParticleMaterial ??= FindMaterial("BigSmokeParticle");

        private static Material _darkEnvironmentMaterial;
        public static Material DarkEnvironmentMaterial => _darkEnvironmentMaterial ??= FindMaterial("DarkEnvironmentSimple");

        private static Material _envLightMaterial;
        public static Material EnvLightMaterial => _envLightMaterial ??= FindMaterial("EnvLight", "ENABLE_HEIGHT_FOG");

        private static Material _envLightOpaqueMaterial;
        public static Material EnvLightOpaqueMaterial => _envLightOpaqueMaterial ??= FindMaterial("EnvLightOpaque", "ENABLE_HEIGHT_FOG");

        private static Material _sparkleMaterial;
        public static Material SparkleMaterial => _sparkleMaterial ??= FindMaterial("Sparkle");

        private static Material _uiNoGlowMaterial;
        public static Material UINoGlowMaterial => _uiNoGlowMaterial ??= FindMaterial("UINoGlow");

        private static Material _uiFogBGMaterial;
        public static Material UIFogBGMaterial => _uiFogBGMaterial ??= FindMaterial("UIFogBG");

        public static T Find<T>(string name) where T : UnityEngine.Object
        {
            var cachedKey = $"{typeof(T).Name}_{name}";
            if (_cachedObjs.TryGetValue(cachedKey, out var cachedObj))
                return (T)cachedObj;

            var obj = Resources.FindObjectsOfTypeAll<T>()
                .Where(x => x != null)
                .FirstOrDefault(x => x.name == name);
            _cachedObjs[cachedKey] = obj;

            return obj;
        }

        public static Material FindMaterial(string name, params string[] disabledKeywords)
        {
            var material = Find<Material>(name);
            if (material != null && disabledKeywords != null && disabledKeywords.Length > 0)
            {
                material = UnityEngine.Object.Instantiate(material);
                foreach (var disabledKeyword in disabledKeywords)
                {
                    material.DisableKeyword(disabledKeyword);
                }
            }
            return material;
        }

        public static void ReplaceMaterialsInChildren(this GameObject gameObject)
        {
            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                ReplaceRendererMaterials(renderer);
            }

            foreach (var image in gameObject.GetComponentsInChildren<Image>(true))
            {
                ReplaceImageMaterial(image);
            }
        }

        // UIFogBG

        private static void ReplaceRendererMaterials(Renderer renderer)
        {
            var materials = renderer.materials;
            var materialsUpdated = false;
            for (var i = 0; i < materials.Length; i++)
            {
                switch (materials[i].name)
                {
                    case "_Replace_DarkEnvironment (Instance)":
                        materials[i] = DarkEnvironmentMaterial;
                        materialsUpdated = true;
                        break;

                    case "_Replace_EnvLightOpaque (Instance)":
                        materials[i] = EnvLightOpaqueMaterial;
                        materialsUpdated = true;
                        break;

                    case "_Replace_EnvLight (Instance)":
                        materials[i] = EnvLightMaterial;
                        materialsUpdated = true;
                        break;

                    case "_Replace_Sparkle (Instance)":
                        materials[i] = SparkleMaterial;
                        materialsUpdated = true;
                        break;
                }
            }

            if (materialsUpdated)
                renderer.sharedMaterials = materials;
        }

        private static void ReplaceImageMaterial(Image image)
        {
            switch (image.material.name)
            {
                case "_Replace_UINoGlow":
                    image.material = UINoGlowMaterial;
                    break;

                case "_Replace_UIFogBG":
                    image.material = UIFogBGMaterial;
                    break;
            }
        }

        public static AudioClip GetAudioClip(string filePath)
        {
            using (var www = UnityWebRequestMultimedia.GetAudioClip(FileHelpers.GetEscapedURLForFilePath(filePath), AudioType.UNKNOWN))
            {
                var request = www.SendWebRequest();
                while (!request.isDone)
                { }

                if (!www.isNetworkError)
                {
                    var audioClip = DownloadHandlerAudioClip.GetContent(www);
                    if (audioClip != null && audioClip.loadState == AudioDataLoadState.Loaded)
                        return audioClip;
                }
            }

            return null;
        }

        public static AudioClip GetEmbeddedResourceAudioClip(string fileName)
        {
            var audioPath = Path.Combine(Plugin.AudioPath, fileName);

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"OldschoolBeatSaber.Resources.{fileName}"))
            {
                File.Delete(audioPath);

                using (var fileStream = new FileStream(audioPath, FileMode.CreateNew))
                {
                    stream.CopyTo(fileStream);
                }
            }

            return GetAudioClip(audioPath);
        }
    }
}
