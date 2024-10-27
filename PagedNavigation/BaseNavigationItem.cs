using Common.Lib.HelperClasses;
using System.Windows.Input;

namespace Common.Lib.PagedNavigation
{
    public abstract class BaseNavigationItem : ObservableObject, INavigationItem
    {
        public string Name { get; protected set; }

        public virtual string Description { get; protected set; }

        public bool IsHomeView { get; protected set; }

        public int Index { get; set; }

        public INavigationContext ParentNavigationContext { get; private set; }

        private ICommand _navigateCommand;
        public ICommand NavigateCommand
        {
            get
            {
                return _navigateCommand ?? (_navigateCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => CanNavigate(x is string ? x.ToString() : null),
                    ExecuteDelegate = x => Navigate(x is string ? x.ToString() : null)
                });
            }
        }

        public BaseNavigationItem(string name, bool isHomeView) : base()
        {
            Name = name;
            IsHomeView = isHomeView;
        }

        public BaseNavigationItem(string name, string description, bool isHomeView) : this(name, isHomeView)
        {
            Description = description;
        }

        protected virtual bool CanNavigate(string itemName) => true;

        protected virtual void Navigate(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
                NavigationManager.CurrentNavContext.NavigateHome();
            else
                NavigationManager.CurrentNavContext.Navigate(itemName);
        }

        public virtual void ResetViewContent() { }
    }
}
