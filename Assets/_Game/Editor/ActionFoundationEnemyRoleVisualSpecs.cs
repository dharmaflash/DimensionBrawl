using System;
using System.Collections.Generic;
using System.Linq;
using DimensionBrawl.Enemies;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    internal sealed class EnemyRoleVisualSpec
    {
        public string RoleId;
        public string RoleTag;
        public string VisualName;
        public string SourceModelPath;
        public string SourcePrefabPath;
        public string TargetModelPath;
        public string MaterialRoot;
        public string TextureRoot;
        public string AnimationRoot;
        public string ControllerPath;
        public string[] DefaultMaterialPaths;
        public RoleAnimationClipSpec[] Clips;
        public RoleWeaponSpec[] Weapons;
        public Vector3 VisualScale;
        public string VisualRead;
        public string AnimationRead;
        public RoleTelegraphSpec Telegraph;
        public bool CreateSummonIntentAnchor;
    }

    internal sealed class RoleAnimationClipSpec
    {
        public string Key;
        public string SourceRoot;
        public string SourceFileName;
        public string TargetClipName;
        public bool LoopTime;
        public float Speed;
        public bool HeightFromFeet;
    }

    internal sealed class RoleWeaponSpec
    {
        public RoleWeaponSpec(string name, string sourceModelPath, string sourceMaterialPath, string socketName)
        {
            Name = name;
            SourceModelPath = sourceModelPath;
            SourceMaterialPath = sourceMaterialPath;
            SocketName = socketName;
            TargetRoot = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/" + name;
            TargetModelPath = TargetRoot + "/Models/" + name + ".fbx";
            TargetMaterialRoot = TargetRoot + "/Materials";
            TargetTextureRoot = TargetRoot + "/Textures";
        }

        public string Name;
        public string SourceModelPath;
        public string SourceMaterialPath;
        public string SocketName;
        public string TargetRoot;
        public string TargetModelPath;
        public string TargetMaterialRoot;
        public string TargetTextureRoot;
    }

    internal sealed class RoleTelegraphSpec
    {
        public Vector3 WindupStartScale;
        public Vector3 WindupEndScale;
        public Vector3 ActiveScale;
        public Color WindupStartColor;
        public Color WindupEndColor;
        public Color ActiveColor;
    }
}
