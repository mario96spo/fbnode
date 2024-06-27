using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using static fbnode.UserControl1;

namespace fbnode
{
    public static class RecipeManager
    {
        private static readonly string fileName = "recipe_reg.json";
        private static readonly string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

        public static List<fbrecipe> LoadRecipes()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<List<fbrecipe>>(json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load recipes: {ex.Message}");
            }
            return new List<fbrecipe>();
        }

        public static void SaveRecipes(List<fbrecipe> recipes)
        {
            try
            {
                string json = JsonSerializer.Serialize(recipes);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save recipes: {ex.Message}");
            }
        }
    }
}
