﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Celeste.Mod;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using System.Collections.Generic;

namespace Celeste {
    class patch_Level : Level {

        // We're effectively in GameLoader, but still need to "expose" private fields to our mod.
        private static EventInstance PauseSnapshot;
        public static EventInstance _PauseSnapshot => PauseSnapshot;

        public extern void orig_Pause(int startIndex = 0, bool minimal = false, bool quickReset = false);
        public new void Pause(int startIndex = 0, bool minimal = false, bool quickReset = false) {
            orig_Pause(startIndex, minimal, quickReset);

            if (!quickReset) {
                // Iterate over the added Entities and grab the first TextMenu.
                List<Entity> added = Entities.GetToAdd();
                foreach (Entity entity in added) {
                    if (!(entity is TextMenu))
                        continue;
                    TextMenu menu = (TextMenu) entity;
                    Everest.Events.Level.CreatePauseMenuButtons(this, menu, minimal);
                    break;
                }
            }

            Everest.Events.Level.Pause(this, startIndex, minimal, quickReset);
        }

        public extern void orig_TransitionTo(LevelData next, Vector2 direction);
        public new void TransitionTo(LevelData next, Vector2 direction) {
            orig_TransitionTo(next, direction);
            Everest.Events.Level.TransitionTo(next, direction);
        }

        [MonoModIgnore] // We don't want to change anything about the method...
        [PatchLevelLoader] // ... except for manually manipulating the method via MonoModRules
        public extern new void LoadLevel(Player.IntroTypes playerIntro, bool isFromLoader = false);

        // Called from LoadLevel, patched via MonoModRules.PatchLevelLoader
        public static bool LoadCustomEntity(EntityData entityData, Level level) {
            LevelData levelData = level.Session.LevelData;
            Vector2 offset = new Vector2(levelData.Bounds.Left, levelData.Bounds.Top);

            // Everest comes with a few core utility entities out of the box.
            if (entityData.Name == $@"levelFlagTrigger") {
                level.Add(new LevelFlagTrigger(entityData, offset));
                return true;
            }

            return Everest.Events.Level.LoadEntity(level, levelData, offset, entityData);
        }

    }
    public static class LevelExt {

        // Mods can't access patch_ classes directly.
        // We thus expose any new members through extensions.

        internal static EventInstance PauseSnapshot => patch_Level._PauseSnapshot;

    }
}
