using Common.Lib.HelperClasses;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Common.Lib.PagedNavigation
{
    public class NavigationManager : ObservableObject
    {
        private static NavigationManager _instance;
        public static NavigationManager Instance
        {
            get { return _instance ?? (_instance = new NavigationManager()); }
            private set
            {
                if (_instance != value)
                    _instance = value;
            }
        }

        private List<INavigationContext> _navigationContexts = new List<INavigationContext>();
        public IReadOnlyList<INavigationContext> NavigationContexts
        {
            get { return _navigationContexts.AsReadOnly(); }
        }

        private INavigationContext _currentNavigationContext;
        public INavigationContext CurrentNavigationContext
        {
            get { return _currentNavigationContext; }
            private set
            {
                if (_currentNavigationContext != value)
                {
                    _currentNavigationContext = value;
                    RaisePropertyChanged();
                }
            }
        }

        public static INavigationContext CurrentNavContext { get { return Instance.CurrentNavigationContext; } }

        private NavigationManager() { }

        public static void ChangeNavigationContext(string contextName)
        {
            if (contextName != Instance.CurrentNavigationContext.Name && Instance.NavigationContexts.Any(c => c.Name == contextName))
            {
                Instance.CurrentNavigationContext = Instance.NavigationContexts.FirstOrDefault(c => c.Name == contextName);
                Instance.CurrentNavigationContext.NavigateHome();
            }
        }

        public static void ChangeNavigationContext(INavigationContext navigationContext)
        {
            if (!Instance.NavigationContexts.Any(c => c.Name == navigationContext.Name))
                Instance._navigationContexts.Add(navigationContext);

            Instance.CurrentNavigationContext = navigationContext;
            Instance.CurrentNavigationContext.NavigateHome();
        }

        public static void AddNavigationContext(INavigationContext navigationContext, bool navigate)
        {
            if (!Instance.NavigationContexts.Any(c => c.Name == navigationContext.Name))
                Instance._navigationContexts.Add(navigationContext);

            if (Instance.CurrentNavigationContext == null || navigate)
            {
                Instance.CurrentNavigationContext = navigationContext;
                Instance.CurrentNavigationContext.NavigateHome();
            }
        }

        public static void RemoveNavigationContext(string contextName)
        {
            if (Instance.NavigationContexts.Any(c => c.Name == contextName))
            {
                INavigationContext context = Instance.NavigationContexts.FirstOrDefault(c => c.Name == contextName);
                Instance._navigationContexts.Remove(context);
                if (Instance.CurrentNavigationContext.Name == contextName)
                {
                    Instance.CurrentNavigationContext = Instance.NavigationContexts.FirstOrDefault();
                    if (Instance.CurrentNavigationContext != null)
                        Instance.CurrentNavigationContext.NavigateHome();
                }
            }
        }

        public static void ClearNavigationContexts()
        {
            if (Instance.NavigationContexts.Any())
                Instance._navigationContexts.Clear();
        }
    }

    public interface INavigationContext
    {
        string Name { get; }
        string Description { get; }

        IReadOnlyList<INavigationItem> NavigationItems { get; }
        INavigationItem CurrentNavigationItem { get; }

        ICommand NavigateCommand { get; }
        ICommand NavigateHomeCommand { get; }
        ICommand NavigateForwardCommand { get; }
        ICommand NavigateBackCommand { get; }

        bool ResetViewContentOnChange { get; set; }

        void Navigate(string itemName);
        void NavigateHome();

        void AddNavigationItem(INavigationItem item);
        void AddNavigationItems(IEnumerable<INavigationItem> items);
        void RemoveNavigationItem(string navigationItemName);
        void RemoveNavigationItem(INavigationItem item);
    }

    public interface INavigationItem
    {
        string Name { get; }
        string Description { get; }
        int Index { get; set; }
        bool IsHomeView { get; }

        void ResetViewContent();
    }
}
