using BookLibrary.Storage.Repositories;
using BookLibrary.UI.HelperClasses;
using BookLibrary.UI.HelperClasses.Commands.MenuCommands;
using BookLibrary.UI.Models.BooksModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace BookLibrary.UI.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly BooksRepository booksRepository = new();

        private string _userName;
        private ICollectionView _booksView;
        private string _filterString = string.Empty;
        private bool _panelLoading;
        private string _panelMainMessage = "Loading";
        private string _panelSubMessage = "Please wait...";
        private int _currentPage = 1;
        private int _numberOfPages = 2;
        private int _recordsPerPage = 4;
        private int _booksTotalCount = 0;
        private string _filter = "all";

        public MainPageViewModel()
        {
            NavigateUserCabinet = new NavigateUserCabinetCommand();
        }

        private bool BooksFilter(object item)
        {
            var book = item as Book;
            return book.Name.ToString().Contains(_filterString, StringComparison.CurrentCultureIgnoreCase)
                ||
                book.Authors.Any(author => author.Contains(_filterString, StringComparison.CurrentCultureIgnoreCase))
                ||
                book.Year.Contains(_filterString)
                ||
                book.IsAvailable.ToString().Contains(_filterString, StringComparison.CurrentCultureIgnoreCase)
                ||
                book.Id.ToString().Contains(_filterString, StringComparison.CurrentCultureIgnoreCase);
        }

        public async Task LoadBooks(string searchString = "", int from = 0, int count = 10, string filter = "all")
        {
            ShowPanelCommand.Execute(null);
            UserName = AppUser.GetInstance().Login;
            var books = new List<Book>();
            switch (filter)
            {
                case "all":
                    books = (await booksRepository.GetBooks(searchString, false, null, from, count)).Select(book => new Book(book)).ToList();
                    BooksTotalCount = await booksRepository.GetBooksTotalCount(searchString);
                    break;
                case "available":
                    books = (await booksRepository.GetAvailableBooks(searchString, from, count)).Select(book => new Book(book)).ToList();
                    BooksTotalCount = await booksRepository.GetAvailableBooksTotalCount(searchString);
                    break;
                case "taken by user":
                    books = (await booksRepository.GetBooksByUser(AppUser.GetInstance().AccountId, searchString, from, count)).Select(book => new Book(book)).ToList();
                    BooksTotalCount = await booksRepository.GetBooksByUserTotalCount(AppUser.GetInstance().AccountId, searchString);
                    break;
            }

            _booksView = CollectionViewSource.GetDefaultView(books);
            _booksView.Filter = BooksFilter;
            _booksView.SortDescriptions.Add(
                new SortDescription("Id", ListSortDirection.Descending));
            _booksView.Refresh();
            OnPropertyChanged(nameof(Books));
            HidePanelCommand.Execute(null);
        }

        public string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
                OnPropertyChanged(nameof(Filter));
            }
        }

        public int BooksTotalCount
        {
            get
            {
                return _booksTotalCount;
            }
            set
            {
                _booksTotalCount = value;
                OnPropertyChanged(nameof(BooksTotalCount));
            }
        }

        public int CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public int NumberOfPages
        {
            get
            {
                return _numberOfPages;
            }
            set
            {
                _numberOfPages = value;
                OnPropertyChanged(nameof(NumberOfPages));
            }
        }

        public int RecordsPerPage
        {
            get
            {
                return _recordsPerPage;
            }
            set
            {
                _recordsPerPage = value;
                OnPropertyChanged(nameof(RecordsPerPage));
            }
        }

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public ICollectionView Books
        {
            get
            {
                return _booksView;
            }
            set
            {
                _booksView = value;
                OnPropertyChanged(nameof(Books));
            }
        }

        public string FilterString
        {
            get { return _filterString; }
            set
            {
                _filterString = value;
                OnPropertyChanged(nameof(FilterString));
                _booksView.Refresh();
            }
        }

        public bool PanelLoading
        {
            get
            {
                return _panelLoading;
            }
            set
            {
                _panelLoading = value;
                OnPropertyChanged(nameof(PanelLoading));
            }
        }

        public string PanelMainMessage
        {
            get
            {
                return _panelMainMessage;
            }
            set
            {
                _panelMainMessage = value;
                OnPropertyChanged(nameof(PanelMainMessage));
            }
        }

        public string PanelSubMessage
        {
            get
            {
                return _panelSubMessage;
            }
            set
            {
                _panelSubMessage = value;
                OnPropertyChanged(nameof(PanelSubMessage));
            }
        }

        public ICommand PanelCloseCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    PanelLoading = false;
                });
            }
        }

        public ICommand ShowPanelCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    PanelLoading = true;
                });
            }
        }

        public ICommand HidePanelCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    PanelLoading = false;
                });
            }
        }

        public ICommand ChangeSubMessageCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    PanelSubMessage = string.Format("Message: {0}", DateTime.Now);
                });
            }
        }

        public ICommand NavigateUserCabinet { get; }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
