using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Xml.Linq;
using static fbnode.UserControl1;

namespace fbnode
{
    public static class RecipeManager
    {
        public static void LoadReg()
        {
            recipe_reg.Clear();
            var files = Directory.GetFiles(recipesFolderPath, "*.json");

            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    List<fbcommand> commands = JsonSerializer.Deserialize<List<fbcommand>>(json);
                    if (commands != null)
                    {
                        Recipe recipe = new Recipe
                        {
                            Name = Path.GetFileNameWithoutExtension(file),
                            Commands = commands
                        };
                        recipe_reg.Add(recipe);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading recipe from file {file}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public static Recipe LoadRecipe(string name)
        {
            try
            {
                if (save_Prompt.FileExists(name))
                {
                    string filePath = System.IO.Path.Combine(recipesFolderPath, name + ".json");
                    string json = File.ReadAllText(filePath);
                    Recipe return_recipe = new Recipe();
                    return_recipe.Name = name;
                    return_recipe.Commands = JsonSerializer.Deserialize<List<fbcommand>>(json);
                    return return_recipe;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load recipes: {ex.Message}");
            }
            return null;
        }

        public static bool UpdateRecipe(Recipe recipe)
        {
            try
            {
                string json = JsonSerializer.Serialize(recipe.Commands);
                string filePath = System.IO.Path.Combine(recipesFolderPath, recipe.Name + ".json");
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save recipes: {ex.Message}");
                return false;
            }
        }
    }
}
