using System.Collections.ObjectModel;
using System.ComponentModel;
using static fbnode.UserControl1;

namespace fbnode
{
    public class RecipesViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<fbrecipe> recipes;
        private fbrecipe? selectedRecipe;

        public ObservableCollection<fbrecipe> Recipes
        {
            get => recipes;
            set
            {
                recipes = value;
                OnPropertyChanged(nameof(Recipes));
            }
        }
        public fbrecipe? SelectedRecipe
        {
            get => selectedRecipe;
            set
            {
                selectedRecipe = value;
                OnPropertyChanged(nameof(SelectedRecipe));
            }
        }

        public RecipesViewModel()
        {
            LoadRecipes();
            
        }

        public void LoadRecipes()
        {
            var loadedRecipes = RecipeManager.LoadRecipes();
            Recipes = new ObservableCollection<fbrecipe>(loadedRecipes);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
