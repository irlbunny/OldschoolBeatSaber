using IPA;
using IPA.Loader;
using IPA.Utilities;
using OldschoolBeatSaber.Installers;
using OldschoolBeatSaber.Utilities;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using System.IO;
using System.Reflection;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace OldschoolBeatSaber
{
    [Plugin(RuntimeOptions.DynamicInit), Slog]
    public class Plugin
    {
        internal static string DataPath { get; private set; } = Path.Combine(UnityGame.UserDataPath, "OldschoolBeatSaber");
        internal static string AudioPath { get; private set; } = Path.Combine(DataPath, "Audio");

        internal static AssetBundle Content { get; private set; }

        internal static GameObject MenuCorePrefab { get; private set; }
        internal static GameObject MenuEnvironmentPrefab { get; private set; }

        internal static AudioClip MenuAudioClip { get; private set; }

        [Init]
        public Plugin(IPALogger logger, PluginMetadata metadata, Zenjector zenjector)
        {
            // Create our paths.
            Directory.CreateDirectory(DataPath);
            Directory.CreateDirectory(AudioPath);

            zenjector.UseLogger(logger);

            zenjector.Install(Location.App, container =>
            {
                container.BindInstance(new UBinder<Plugin, PluginMetadata>(metadata));
            });

            zenjector.Install<MenuInstaller>(Location.Menu);
        }

        [OnEnable]
        public void OnEnable()
        {
#if DEBUG
            Content = AssetBundle.LoadFromFile(@"E:\GameDev\OldschoolBeatSaber.Unity\AssetBundles\content");
#else
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OldschoolBeatSaber.Resources.content");
            Content = AssetBundle.LoadFromStream(stream);
#endif

            MenuCorePrefab = Content.LoadAsset<GameObject>("assets/prefabs/oldschoolmenucore.prefab");
            MenuEnvironmentPrefab = Content.LoadAsset<GameObject>("assets/prefabs/oldschoolmenuenvironment.prefab");

            MenuAudioClip = ResourceUtility.GetEmbeddedResourceAudioClip("Menu.ogg");

            Content.Unload(false);
        }

        [OnDisable]
        public void OnDisable()
        { }
    }
}
