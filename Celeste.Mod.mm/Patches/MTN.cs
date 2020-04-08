#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Celeste.Mod;
using Celeste.Mod.Meta;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste {
    class patch_MTN {
        // We don't need an Unload() because the vanilla MTN disposes with the Atlas we use

        public static extern void orig_UnloadData();
        public static void UnloadData() {
            if (MTN.DataLoaded) {
                foreach (KeyValuePair<string, MountainResources> kvp in MTNExt.MountainMappings) {
                    foreach(KeyValuePair<string, ObjModel> modelKvp in kvp.Value.MountainModels) {
                        modelKvp.Value.Dispose();
                    }
                    kvp.Value.MountainModels = null;
                }
            }
            orig_UnloadData();
        }

    }

    public class MountainResources {
        // The key is the name of the model (the file name of the .obj with no extension)
        public Dictionary<string, ObjModel> MountainModels;

        // The key here matches up with the one for the models
        public Dictionary<string, VirtualTexture[]> MountainTextures;

        public VirtualTexture MountainFogTexture;

        public Color[] FogColors;

        public Skybox[] Skyboxes;
    }

    public static class MTNExt {
        /// <summary>
        /// Maps the key to the ModAsset of the map in Everest.Content to the MountainResources for it
        /// </summary>
        public static Dictionary<string, MountainResources> MountainMappings = new Dictionary<string, MountainResources>();

        public static bool ModsLoaded { get; private set; }
        public static bool ModsDataLoaded { get; private set; }

        /// <summary>
        /// Load the custom mountain models for mods.
        /// </summary>
        public static void LoadModData() {
            if (!ModsDataLoaded) {
                Stopwatch stopwatch = Stopwatch.StartNew();
                lock (Everest.Content.Map) {
                    foreach (KeyValuePair<string, ModAsset> kvp in Everest.Content.Map) {
                        MapMeta meta;
                        // Check if the meta for this asset exists and if it has a MountainModelDirectory specified
                        if (kvp.Value != null && (meta = kvp.Value.GetMeta<MapMeta>()) != null && meta.Mountain != null && !string.IsNullOrEmpty(meta.Mountain.MountainModelDirectory)) {
                            // Create the mountain resources for this map if they don't exist already
                            if (!MountainMappings.TryGetValue(kvp.Key, out MountainResources resources)) {
                                resources = new MountainResources();
                                MountainMappings.Add(kvp.Key, resources);
                            }

                            resources.MountainModels = new Dictionary<string, ObjModel>();

                            // Try to use every model in the specified folder
                            // Make sure not to use the folder itself
                            foreach (KeyValuePair<string, ModAsset> modelKvp in Everest.Content.Map.Where((asset) => !asset.Key.Replace('\\', '/').Equals(meta.Mountain.MountainModelDirectory) && asset.Key.Replace('\\', '/').StartsWith(meta.Mountain.MountainModelDirectory))) {
                                resources.MountainModels.Add(Path.GetFileName(modelKvp.Key), ObjModelExt.CreateFromStream(modelKvp.Value.Stream, modelKvp.Key + ".obj"));
                            }
                        }
                    }
                }
                Console.WriteLine(" - MODDED MTN DATA LOAD: " + stopwatch.ElapsedMilliseconds + "ms");
            }

            ModsDataLoaded = true;
        }
        /// <summary>
        /// Load the custom mountain textures for mods.
        /// </summary>
        public static void LoadMod() {
            if (!ModsLoaded) {
                Stopwatch stopwatch = Stopwatch.StartNew();
                lock (Everest.Content.Map) {
                    foreach (KeyValuePair<string, ModAsset> kvp in Everest.Content.Map) {
                        MapMeta meta;
                        // Check if the meta for this asset exists and if it has a MountainTextureDirectory specified
                        if (kvp.Value != null && (meta = kvp.Value.GetMeta<MapMeta>()) != null && meta.Mountain != null && !string.IsNullOrEmpty(meta.Mountain.MountainTextureDirectory)) {
                            // Create the mountain resources for this map if they don't exist already
                            if (!MountainMappings.TryGetValue(kvp.Key, out MountainResources resources)) {
                                resources = new MountainResources();
                                MountainMappings.Add(kvp.Key, resources);
                            }

                            resources.MountainTextures = new Dictionary<string, VirtualTexture[]>();

                            foreach (KeyValuePair<string, ObjModel> modelKvp in resources.MountainModels) {
                                VirtualTexture[] textures = new VirtualTexture[3];
                                for (int i = 0; i < 3; i++) {
                                    if (MTN.Mountain.Has(Path.Combine(meta.Mountain.MountainTextureDirectory, modelKvp.Key + "_" + i).Replace('\\', '/'))) {
                                        textures[i] = MTN.Mountain[Path.Combine(meta.Mountain.MountainTextureDirectory, modelKvp.Key + "_" + i).Replace('\\', '/')].Texture;
                                    }
                                }
                                resources.MountainTextures[modelKvp.Key] = textures;
                            }

                            resources.MountainTextures["skybox"] = new VirtualTexture[3];
                            resources.Skyboxes = new Skybox[3];
                            // Skybox is handled separately
                            for (int i = 0; i < 3; i++) {
                                if (MTN.Mountain.Has(Path.Combine(meta.Mountain.MountainTextureDirectory, "skybox_" + i).Replace('\\', '/'))) {
                                    VirtualTexture texture = MTN.Mountain[Path.Combine(meta.Mountain.MountainTextureDirectory, "skybox_" + i).Replace('\\', '/')].Texture;
                                    resources.MountainTextures["skybox"][i] = texture;
                                    if (texture != null) {
                                        resources.Skyboxes[i] = new Skybox(texture);
                                    }
                                }
                            }

                            // Fog is handled separately
                            if (MTN.Mountain.Has(Path.Combine(meta.Mountain.MountainTextureDirectory, "fog").Replace('\\', '/'))) {
                                resources.MountainFogTexture = MTN.Mountain[Path.Combine(meta.Mountain.MountainTextureDirectory, "fog").Replace('\\', '/')].Texture;
                            }

                            resources.FogColors = new Color[4];

                            // Fog colors currently cannot be configured
                            resources.FogColors[0] = Calc.HexToColor("010817");
                            resources.FogColors[1] = Calc.HexToColor("13203E");
                            resources.FogColors[2] = Calc.HexToColor("281A35");
                            resources.FogColors[3] = Calc.HexToColor("010817");
                        }
                    }
                }
                Console.WriteLine(" - MODDED MTN LOAD: " + stopwatch.ElapsedMilliseconds + "ms");
            }
            ModsLoaded = true;
        }
    }
}
