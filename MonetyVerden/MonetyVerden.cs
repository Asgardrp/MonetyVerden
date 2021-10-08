﻿//MontyVyrden by kubson
//Wszelka Edycja zabroniona
using System.IO;
using BepInEx;
using MonetyVerden.Models;
using MonetyVerden.Services;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace MonetyVerden
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Mod : BaseUnityPlugin
    {
        public const string PluginGUID = "kubson.monetyverden";
        public const string PluginName = "MonetyVerden";
        public const string PluginVersion = "1.0.0";

        private AssetBundle _embeddedResourceBundle;

        [System.Obsolete]
        private void Awake()
        {
            LoadAssetBundle();
            AddRecipes();
            UnloadAssetBundle();

            // Listen to event to know when all prefabs are registered
            PrefabManager.OnPrefabsRegistered += () =>
            {
                // The null check is necessary in case users remove the item from the config
                var monetyverden = PrefabManager.Instance.GetPrefab("monetyverden");
                if (monetyverden != null)
                {
                    // Add fire damage to the funky sword
                    monetyverden.GetComponent<ItemDrop>().m_itemData.m_shared.m_damages.m_fire = 1000;

                    // Add funky sword to skeleton drops with 100% drop chance
                    var skeletonDrop = PrefabManager.Instance.GetPrefab("Skeleton").GetComponent<CharacterDrop>();
                    skeletonDrop.m_drops.Add(new CharacterDrop.Drop
                    {
                        m_amountMax = 1,
                        m_amountMin = 1,
                        m_chance = 1f,
                        m_levelMultiplier = false,
                        m_onePerPlayer = false,
                        m_prefab = monetyverden
                    });

                    var segullDrop = PrefabManager.Instance.GetPrefab("Seagal").GetComponent<DropOnDestroyed>().m_dropWhenDestroyed;
                    segullDrop.m_drops.Add(new DropTable.DropData
                    {
                        m_item = monetyverden,
                        m_stackMin = 1,
                        m_stackMax = 1,
                        m_weight = 1f
                    });
                }
            };
        }

        private void LoadAssetBundle()
        {
            // Load asset bundle from embedded resources
            Jotunn.Logger.LogInfo($"Embedded resources: {string.Join(",", typeof(Mod).Assembly.GetManifestResourceNames())}");
            _embeddedResourceBundle = AssetUtils.LoadAssetBundleFromResources("monetyverden", typeof(Mod).Assembly);
        }

        private void UnloadAssetBundle()
        {
            _embeddedResourceBundle.Unload(false);
        }

        [System.Obsolete]
        private void AddRecipes()
        {
            var extendedRecipes = ExtendedRecipeManager.LoadRecipesFromJson($"{Path.GetDirectoryName(typeof(Mod).Assembly.Location)}/Assets/recipes.json");

            extendedRecipes.ForEach(extendedRecipe =>
            {
                // Load prefab from asset bundle
                var prefab = _embeddedResourceBundle.LoadAsset<GameObject>(extendedRecipe.prefabPath);

                // Create custom item
                var customItem = new CustomItem(prefab, true);

                // Edit item drop to set name and description
                var itemDrop = customItem.ItemDrop;
                itemDrop.m_itemData.m_shared.m_name = extendedRecipe.name;
                itemDrop.m_itemData.m_shared.m_description = extendedRecipe.description;

                // Add localizations for name and description
                LocalizationManager.Instance.AddToken(extendedRecipe.name, extendedRecipe.nameValue, false);
                LocalizationManager.Instance.AddToken(extendedRecipe.descriptionToken, extendedRecipe.description, false);

                // Add item with recipe
                customItem.Recipe = ExtendedRecipe.Convert(extendedRecipe);
                ItemManager.Instance.AddItem(customItem);
            });
        }
    }
}