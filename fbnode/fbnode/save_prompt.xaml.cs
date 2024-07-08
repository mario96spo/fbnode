using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TMCraftProtos;
using static fbnode.UserControl1;

namespace fbnode
{
    /// <summary>
    /// Logica di interazione per Window1.xaml
    /// </summary>
    /// 
    public class RecipeAddedEventArgs : EventArgs
    {
        public Recipe NewRecipe { get; }

        public RecipeAddedEventArgs(Recipe newRecipe)
        {
            NewRecipe = newRecipe;
        }
    }
    public class RecipeDeletedEventArgs : EventArgs
    {
    }
    public partial class save_prompt : Window
    {
        public event EventHandler<RecipeAddedEventArgs> RecipeAdded;
        public event EventHandler<RecipeDeletedEventArgs> RecipeDeleted;
        public string recipe_name {  get;  private set; }
        int scenario = 0;
        private RecipesViewModel _viewModel;
        Recipe? selected_recipe;
        public save_prompt()
        {
            InitializeComponent();
        }
        public bool FileExists(string name)
        {
            string filePath = System.IO.Path.Combine(recipesFolderPath, name + ".json");
            if (File.Exists(filePath)) return true;
            else return false;
        }

        public void delete(Recipe recipe)
        {
            prompt_title.Text = "Are you sure you want to delete the recipe " + recipe.Name + " ?";
            save_prompt_textbox.Visibility = Visibility.Collapsed;
            Button1.Visibility = Visibility.Visible;
            Button2.Visibility = Visibility.Visible;
            Button3.Visibility = Visibility.Collapsed;
            Button1.IsEnabled = true;
            Button1.Opacity = 1;
            Button1.Content = "Yes";
            selected_recipe = recipe;
            scenario = 2;
        }
        public void create_json(string name)
        {
            string filePath = System.IO.Path.Combine(recipesFolderPath, name + ".json");
            Recipe newRecipe = new Recipe { Name = name };
            string json = System.Text.Json.JsonSerializer.Serialize(newRecipe.Commands);
            File.WriteAllText(filePath, json);
            RecipeAdded?.Invoke(this, new RecipeAddedEventArgs(newRecipe));
        }
        public void delete_json(string name)
        {
            string filePath = System.IO.Path.Combine(recipesFolderPath, name + ".json");
            File.Delete(filePath);

            RecipeManager.LoadReg();
            RecipeDeleted?.Invoke(this, new RecipeDeletedEventArgs());
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if(scenario == 0)
            {
                bool response = FileExists(save_prompt_textbox.Text);
                if (!response)
                {
                    create_json(save_prompt_textbox.Text);
                    RecipeManager.LoadReg();
                    prompt_title.Text = "Recipe succesfully saved";
                    save_prompt_textbox.Visibility = Visibility.Collapsed;
                    Button1.Visibility = Visibility.Collapsed;
                    Button2.Visibility = Visibility.Collapsed;
                    Button3.Visibility = Visibility.Visible;
                }
                else if (response)
                {
                    prompt_title.Text = "Recipe already exists";
                    Button1.Visibility = Visibility.Collapsed;
                    Button2.Visibility = Visibility.Collapsed;
                    Button3.Visibility = Visibility.Visible;
                    save_prompt_textbox.Visibility = Visibility.Collapsed;
                    scenario = 3;
                }
            }
            else if (scenario == 2)
            {
                delete_json(selected_recipe.Name);
                selected_recipe = null;
                prompt_title.Text = "Recipe deleted";
                Button1.Visibility = Visibility.Collapsed;
                Button2.Visibility = Visibility.Collapsed;
                Button3.Visibility = Visibility.Visible;
            }
            //ViewModel.LoadRecipes();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (scenario == 0 || scenario == 2)
            {
                save_Prompt.Hide();
                reset();
            }
            else if (scenario == 1)
            {
                reset();
            }
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (scenario == 0 || scenario == 2)
            {
                save_Prompt.Hide();
                reset();
            }
            else if(scenario == 3) reset();
        }
        
        public void reset()
        {
            scenario = 0;
            prompt_title.Text = "Insert name of the recipe here:";
            save_prompt_textbox.Text = "Type name here";
            save_prompt_textbox.Foreground = Brushes.Gray;
            save_prompt_textbox.FontStyle = FontStyles.Italic;
            save_prompt_textbox.Visibility = Visibility.Visible;
            Button1.Content = "Save";
            Button1.Visibility = Visibility.Visible;
            Button2.Visibility = Visibility.Visible;
            Button3.Visibility = Visibility.Collapsed;
        }
        private void save_prompt_focus(object sender, RoutedEventArgs e)
        {
            if(save_prompt_textbox.Text == "Type name here")
            {
                save_prompt_textbox.Text = "";
                save_prompt_textbox.Foreground = Brushes.Black;
                save_prompt_textbox.FontStyle = FontStyles.Normal;
            }
        }
        private void save_prompt_nofocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(save_prompt_textbox.Text))
            {
                save_prompt_textbox.Text = "Type name here";
                save_prompt_textbox.Foreground = Brushes.Gray;
                save_prompt_textbox.FontStyle = FontStyles.Italic;
            }
        }

    }
    public class EmptyText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            string initialText = parameter as string;

            // Return false if the textbox is empty or contains the initial text, true otherwise
            if (string.IsNullOrWhiteSpace(text) || text == "Type name here")
            {
                return false;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
