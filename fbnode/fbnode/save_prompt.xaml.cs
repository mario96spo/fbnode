using System;
using System.Collections.Generic;
using System.Globalization;
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
    public partial class save_prompt : Window
    {
        public string recipe_name {  get;  private set; }
        int response;
        int scenario = 0;
        private RecipesViewModel _viewModel;
        fbrecipe? selected_recipe;
        public save_prompt()
        {
            InitializeComponent();
        }

        public int save_reg(fbrecipe recipe, int option)
        {
            //option = 0: user saving attempt; 1: overwrite; 2: delete
            //return = 0: succesfully saved; 1: name already exists; 2: succesfully deleted

            if (option == 0 || option == 1 || option == 2)
            {
                if (recipe.type == 0)
                {
                    for (int i = 0; i < recipe_reg_move.Count; i++)
                    {
                        if (recipe_reg_move[i].name == recipe.name)
                        {
                            if (option == 0) return 1;
                            else if (option == 1)
                            {
                                recipe_reg_move[i] = recipe;

                                RecipeManager.SaveRecipes(recipe_reg_move, 1);
                                return 0;
                            }
                            else if (option == 2)
                            {
                                recipe_reg_move.RemoveAt(i);
                                RecipeManager.SaveRecipes(recipe_reg_move, 1);
                                return 2;
                            }
                        }
                    }
                    recipe_reg_move.Add(recipe);

                    RecipeManager.SaveRecipes(recipe_reg_move, 1);
                    return 0;
                }
                else if (recipe.type == 1)
                {
                    for (int i = 0; i < recipe_reg_shake.Count; i++)
                    {
                        if (recipe_reg_shake[i].name == recipe.name)
                        {
                            if (option == 0) return 1;
                            else if (option == 1)
                            {
                                recipe_reg_shake[i] = recipe;

                                RecipeManager.SaveRecipes(recipe_reg_shake, 2);
                                return 0;
                            }
                            else if (option == 2)
                            {
                                recipe_reg_shake.RemoveAt(i);
                                RecipeManager.SaveRecipes(recipe_reg_shake, 2);
                                return 2;
                            }
                        }
                    }
                    recipe_reg_shake.Add(recipe);

                    RecipeManager.SaveRecipes(recipe_reg_shake, 2);
                    return 0;
                }
                else return -1;
            }
            else return -1;
        }

        public void delete(fbrecipe recipe)
        {
            prompt_title.Text = "Are you sure you want to delete the recipe " + recipe.name + " ?";
            save_prompt_textbox.Visibility = Visibility.Collapsed;
            Button1.Visibility = Visibility.Visible;
            Button2.Visibility = Visibility.Visible;
            Button3.Visibility = Visibility.Collapsed;
            save_prompt_textbox.Text = recipe.name;
            Button1.Content = "Yes";
            selected_recipe = recipe;
            scenario = 2;
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if(scenario == 0)
            {
                current_recipe.name = save_prompt_textbox.Text;
                response = save_reg(current_recipe, 0);
                if (response == 0)
                {
                    prompt_title.Text = "Recipe succesfully saved";
                    save_prompt_textbox.Visibility = Visibility.Collapsed;
                    Button1.Visibility = Visibility.Collapsed;
                    Button2.Visibility = Visibility.Collapsed;
                    Button3.Visibility = Visibility.Visible;
                }
                else if (response == 1)
                {
                    prompt_title.Text = "Recipe already exists";
                    save_prompt_textbox.Visibility = Visibility.Collapsed;
                    Button1.Content = "Overwrite";
                    Button3.Visibility = Visibility.Collapsed;
                    scenario = 1;
                }
                else if (response == -1)
                {
                    save_prompt_textbox.Visibility = Visibility.Collapsed;
                    Button1.Visibility = Visibility.Collapsed;
                    Button2.Visibility = Visibility.Collapsed;
                    Button3.Visibility = Visibility.Visible;
                    prompt_title.Text = "Error.";
                }
            }
            else if (scenario == 1)
            {
                response = save_reg(current_recipe, 1);
                save_prompt_textbox.Visibility = Visibility.Collapsed;
                Button1.Visibility = Visibility.Collapsed;
                Button2.Visibility = Visibility.Collapsed;
                Button3.Visibility = Visibility.Visible;

                if (response != -1) prompt_title.Text = "Recipe succesfully saved";
                else if (response == -1) prompt_title.Text = "Error";
            }
            else if (scenario == 2)
            {
                save_reg(selected_recipe.Value, 2);
                prompt_title.Text = "Recipe deleted";
                Button1.Visibility = Visibility.Collapsed;
                Button2.Visibility = Visibility.Collapsed;
                Button3.Visibility = Visibility.Visible;
                if (selected_recipe.Value.type == 0) UserControl1.selectedrecipeMove = null;
                if (selected_recipe.Value.type == 1) UserControl1.selectedrecipeShake = null;
                selected_recipe = null;

                
            }
            ViewModel.LoadRecipes();
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
            save_Prompt.Hide();
            reset();
        }
        
        private void reset()
        {
            prompt_title.Text = "Insert name of the recipe here:";
            save_prompt_textbox.Text = "Type name here";
            save_prompt_textbox.Foreground = Brushes.Gray;
            save_prompt_textbox.FontStyle = FontStyles.Italic;
            scenario = 0;
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
