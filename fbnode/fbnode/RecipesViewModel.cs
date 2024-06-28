using System.Collections.ObjectModel;
using System.ComponentModel;
using static fbnode.UserControl1;

namespace fbnode
{
    public class RecipesViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<fbrecipe> recipesMove;
        private ObservableCollection<fbrecipe> recipesShake;
        private fbrecipe? selectedRecipeMove;
        private fbrecipe? selectedRecipeShake;

        public ObservableCollection<fbrecipe> Recipes_move
        {
            get => recipesMove;
            set
            {
                recipesMove = value;
                OnPropertyChanged(nameof(Recipes_move));
            }
        }

        public ObservableCollection<fbrecipe> Recipes_shake
        {
            get => recipesShake;
            set
            {
                recipesShake = value;
                OnPropertyChanged(nameof(Recipes_shake));
            }
        }

        public fbrecipe? SelectedRecipeMove
        {
            get => selectedRecipeMove;
            set
            {
                selectedRecipeMove = value;
                OnPropertyChanged(nameof(SelectedRecipeMove));
            }
        }
        public fbrecipe? SelectedRecipeShake
        {
            get => selectedRecipeShake;
            set
            {
                selectedRecipeShake = value;
                OnPropertyChanged(nameof(SelectedRecipeShake));
            }
        }

        public RecipesViewModel()
        {
            LoadRecipes();
        }

        public void LoadRecipes()
        {
            var loadedRecipesMove = RecipeManager.LoadRecipes(1);
            Recipes_move = new ObservableCollection<fbrecipe>(loadedRecipesMove);

            var loadedRecipesShake = RecipeManager.LoadRecipes(2);
            Recipes_shake = new ObservableCollection<fbrecipe>(loadedRecipesShake);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
