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
        private static readonly string fileName_1 = "recipe_reg_move.json";
        private static readonly string filePath_1 = Path.Combine(Environment.CurrentDirectory, fileName_1);
        private static readonly string fileName_2 = "recipe_reg_shake.json";
        private static readonly string filePath_2 = Path.Combine(Environment.CurrentDirectory, fileName_2);

        public static List<fbrecipe> LoadRecipes(int option)
        {
            //option= 1: move register; 2: shake register
            if (option == 1)
            {
                try
                {
                    if (File.Exists(filePath_1))
                    {
                        string json = File.ReadAllText(filePath_1);
                        return JsonSerializer.Deserialize<List<fbrecipe>>(json);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load recipes: {ex.Message}");
                }
                return new List<fbrecipe>();
            }
            else if (option == 2)
            {
                try
                {
                    if (File.Exists(filePath_2))
                    {
                        string json = File.ReadAllText(filePath_2);
                        return JsonSerializer.Deserialize<List<fbrecipe>>(json);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load recipes: {ex.Message}");
                }
                return new List<fbrecipe>();
            }
            return null;
        }

        public static void SaveRecipes(List<fbrecipe> recipes, int option)
        {
            //option= 1: move register; 2: shake register
            if (option == 1)
            {
                try
                {
                    string json = JsonSerializer.Serialize(recipes);
                    File.WriteAllText(filePath_1, json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save recipes: {ex.Message}");
                }
            }
            else if (option == 2)
            {
                try
                {
                    string json = JsonSerializer.Serialize(recipes);
                    File.WriteAllText(filePath_2, json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save recipes: {ex.Message}");
                }
            }
        }
    }
}
