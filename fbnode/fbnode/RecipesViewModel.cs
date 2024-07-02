using System.Collections.ObjectModel;
using System.ComponentModel;
using static fbnode.UserControl1;

namespace fbnode
{
    public class RecipesViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<fbcommand> commandView;
        private fbcommand? selectedCommand;

        public ObservableCollection<fbcommand> command_view
        {
            get => commandView;
            set
            {
                commandView = value;
                OnPropertyChanged(nameof(command_view));
            }
        }

        public fbcommand? SelectedCommand
        {
            get => selectedCommand;
            set
            {
                selectedCommand = value;
                OnPropertyChanged(nameof(SelectedCommand));
            }
        }

        public RecipesViewModel()
        {
            LoadRecipe();
        }

        public void LoadRecipe()
        {
            //var loadedRecipe = RecipeManager.LoadRecipe(current_recipe.Name);
            //command_view = new ObservableCollection<fbcommand>(loadedRecipe.Commands);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
