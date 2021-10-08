using System.Collections.Generic;
using MonetyVerden.Models;
using Jotunn.Utils;

namespace MonetyVerden.Services
{
    internal class ExtendedRecipeManager
    {
        public static List<ExtendedRecipe> LoadRecipesFromJson(string recipesPath)
        {
            var json = AssetUtils.LoadText(recipesPath);
            return SimpleJson.SimpleJson.DeserializeObject<List<ExtendedRecipe>>(json);
        }
    }
}
