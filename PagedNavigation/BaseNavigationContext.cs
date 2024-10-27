using Common.Lib.HelperClasses;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Common.Lib.PagedNavigation
{
    public abstract class BaseNavigationContext : ObservableObject, INavigationContext
    {
        public virtual string Name { get; protected set; }
        public virtual string Description { get; protected set; }

        private ICommand _navigateCommand;
        public ICommand NavigateCommand
        {
            get
            {
                return _navigateCommand ?? (_navigateCommand = new SimpleCommand()
                {
                    CanExecuteDelegate = p => p is string,
                    ExecuteDelegate = p =>
                    {
                        if (!string.IsNullOrEmpty(p.ToString()))
                            Navigate(p.ToString());
                    }
                });
            }
        }

        private ICommand _navigateHomeCommand;
        public ICommand NavigateHomeCommand
        {
            get
            {
                return _navigateHomeCommand ?? (_navigateHomeCommand = new SimpleCommand()
                {
                    CanExecuteDelegate = p => NavigationItems.Count > 1,
                    ExecuteDelegate = p => NavigateHome()
                });
            }
        }

        private ICommand _navigateForwardCommand;
        public ICommand NavigateForwardCommand
        {
            get
            {
                return _navigateForwardCommand ?? (_navigateForwardCommand = new SimpleCommand()
                {
                    CanExecuteDelegate = p => NavigationItems.Count > 1,
                    ExecuteDelegate = p => NavigateForward()
                });
            }
        }

        private ICommand _navigateBackCommand;
        public ICommand NavigateBackCommand
        {
            get
            {
                return _navigateBackCommand ?? (_navigateBackCommand = new SimpleCommand()
                {
                    CanExecuteDelegate = p => NavigationItems.Count > 1,
                    ExecuteDelegate = p => NavigateBack()
                });
            }
        }

        protected List<INavigationItem> _navigationItems = new List<INavigationItem>();
        public IReadOnlyList<INavigationItem> NavigationItems { get { return _navigationItems.AsReadOnly(); } }

        private INavigationItem _currentNavigationItem;
        public INavigationItem CurrentNavigationItem
        {
            get { return _currentNavigationItem; }
            protected set
            {
                if (_currentNavigationItem != value)
                {
                    _currentNavigationItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _resetViewOnChange;
        public bool ResetViewContentOnChange
        {
            get { return _resetViewOnChange; }
            set
            {
                if (_resetViewOnChange != value)
                {
                    _resetViewOnChange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BaseNavigationContext() { }

        public BaseNavigationContext(string name) : base() { Name = name; }

        public BaseNavigationContext(string name, string description) : this(name)
        {
            Description = description;
        }

        public void Navigate(string itemName)
        {
            INavigationItem item = NavigationItems.FirstOrDefault(i => i.Name == itemName);
            if (item != null)
            {
                if (ResetViewContentOnChange)
                    item.ResetViewContent();
                CurrentNavigationItem = NavigationItems.FirstOrDefault(i => i.Name == itemName);
            }
        }

        public void NavigateHome()
        {
            INavigationItem homeItem = NavigationItems.FirstOrDefault(i => i.IsHomeView);
            if (homeItem == null)
                homeItem = NavigationItems[0];
            if (ResetViewContentOnChange)
                homeItem.ResetViewContent();
            CurrentNavigationItem = homeItem;
        }

        private void NavigateForward()
        {
            int currentIndex = _navigationItems.FindIndex(i => i == CurrentNavigationItem);
            if (currentIndex == NavigationItems.Count - 1)
                currentIndex = -1;

            CurrentNavigationItem = NavigationItems[++currentIndex];
        }

        private void NavigateBack()
        {
            int currentIndex = _navigationItems.FindIndex(vm => vm == CurrentNavigationItem);
            if (currentIndex == 0)
                currentIndex = NavigationItems.Count;

            CurrentNavigationItem = NavigationItems[--currentIndex];
        }

        public void AddNavigationItem(INavigationItem item)
        {
            if (!NavigationItems.Contains(item))
                _navigationItems.Add(item);
        }

        public void AddNavigationItems(IEnumerable<INavigationItem> items)
        {
            foreach (INavigationItem item in items)
                AddNavigationItem(item);
        }

        public void RemoveNavigationItem(INavigationItem item)
        {
            if (item != null)
                _navigationItems.Remove(item);
        }

        public void RemoveNavigationItem(string navigationItemName)
        {
            INavigationItem item = NavigationItems.FirstOrDefault(i => i.Name == navigationItemName);
            RemoveNavigationItem(item);
        }
    }
}
