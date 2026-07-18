using BookLibrary.Storage.Repositories;
using BookLibrary.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Globalization;
using System.Linq;

namespace BookLibrary.UI.Pages
{
    /// <summary>
    /// Interaction logic for BookLibraryMainPage.xaml
    /// </summary>
    public partial class BookLibraryMainPage : Page
    {
        private readonly IBooksRepository booksRepository = new BooksRepository();

        public BookLibraryMainPage()
        {
            InitializeComponent();

            DataContext = pageModel = new MainPageViewModel();

            BooksGrid.SelectionChanged += DataGrid_Row_Click;
            BooksGrid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
            tbSearch.TextChanged += Search_Text_Changed;
            tbFilter.TextChanged += Filter_Text_Changed;
            BtnAddBook.Click += BtnAddBook_Click;
            BtnEditBook.Click += BtnEditBook_Click;
            BtnTrackBook.Click += BtnTrackBook_Click;
            BtnDeleteBook.Click += BtnDeleteBook_Click;
            BtnFirstPage.Click += BtnFirstPage_Click;
            BtnPreviousPage.Click += BtnPreviousPage_Click;
            tbCurrentPage.TextChanged += tbCurrentPage_Text_Changed;
            BtnNextPage.Click += BtnNextPage_Click;
            BtnLastPage.Click += BtnLastPage_Click;
            cbRecordsPerPage.SelectionChanged += cbRecordsPerPage_SelectionChanged;
            BtnRefreshBooksGrid.Click += BtnRefreshBooksGrid_Click;
            Loaded += OnLoaded;

        }

        private class AuthorsToStringConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null) return string.Empty;

                if (value is string s) return s;

                if (value is System.Collections.IEnumerable enumerable)
                {
                    try
                    {
                        var items = enumerable.Cast<object>().Select(o => o?.ToString()).Where(str => !string.IsNullOrEmpty(str));
                        return string.Join(", ", items);
                    }
                    catch
                    {
                        return value.ToString();
                    }
                }

                return value.ToString();
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        public MainPageViewModel pageModel { get; set; }

        private void BtnAddBook_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddBookPage());
        }

        private void BtnEditBook_Click(object sender, RoutedEventArgs e)
        {
            var book = BooksGrid.SelectedItem as Models.BooksModels.Book;
            if (book == null) return;
            var editBook = Storage.Models.Book.Book.FromPersistence(book.Id ?? Guid.NewGuid(), book.Name, book.Authors, DateTime.Parse($"01/01/{book.Year}"), book.IsAvailable);
            NavigationService.Navigate(new EditBookPage(editBook));
        }

        private async void BtnTrackBook_Click(object sender, RoutedEventArgs e)
        {
            var book = BooksGrid.SelectedItem as Models.BooksModels.Book;
            if (book == null) return;
            var trackBook = await booksRepository.GetBookTrack(AppUser.GetInstance().AccountId, book.Id ?? Guid.NewGuid());
            NavigationService.Navigate(new BookTrackPage(trackBook));
        }


        private async void BtnDeleteBook_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var booksGridEnumerator = BooksGrid.SelectedItems.GetEnumerator();
                while (booksGridEnumerator.MoveNext())
                {
                    var bookId = (booksGridEnumerator.Current as Models.BooksModels.Book)?.Id;
                    if (bookId != null)
                    {
                        await booksRepository.DeleteBook((Guid)bookId);
                    }
                }
                LoadBooks();
            }
        }

        private void BtnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            pageModel.CurrentPage = 1;
        }

        private void BtnPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            var currentPage = pageModel.CurrentPage - 1;
            if (currentPage > 0)
            {
                pageModel.CurrentPage = currentPage;
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            var currentPage = pageModel.CurrentPage + 1;
            if (currentPage <= pageModel.NumberOfPages)
            {
                pageModel.CurrentPage = currentPage;
            }
        }

        private void BtnLastPage_Click(object sender, RoutedEventArgs e)
        {
            pageModel.CurrentPage = pageModel.NumberOfPages;
        }

        private void BtnRefreshBooksGrid_Click(object sender, RoutedEventArgs e)
        {
            CalculateNumberOfPages();
            LoadBooks();
        }

        private void tbCurrentPage_Text_Changed(object sender, TextChangedEventArgs e)
        {
            LoadBooks();
        }

        private void cbRecordsPerPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pageModel.CurrentPage = 1;
            LoadBooks();
            CalculateNumberOfPages();
        }

        private void DataGrid_Row_Click(object sender, SelectionChangedEventArgs e)
        {
            if (BtnEditBook.IsEnabled == false || BtnDeleteBook.IsEnabled == false || BtnTrackBook.IsEnabled == false)
            {
                BtnEditBook.IsEnabled = true;
                BtnTrackBook.IsEnabled = true;
                BtnDeleteBook.IsEnabled = true;
            }
            if (BooksGrid.SelectedItems.Count == 0)
            {
                BtnEditBook.IsEnabled = false;
                BtnTrackBook.IsEnabled = false;
                BtnDeleteBook.IsEnabled = false;
            }
            if (BooksGrid.SelectedItems.Count > 1)
            {
                BtnEditBook.IsEnabled = false;
                BtnTrackBook.IsEnabled = false;
            }
        }

        private void Search_Text_Changed(object sender, TextChangedEventArgs e)
        {
            CalculateNumberOfPages();
            pageModel.CurrentPage = 1;
            LoadBooks();
        }

        private void Filter_Text_Changed(object sender, TextChangedEventArgs e)
        {
            pageModel.FilterString = tbFilter.Text;
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "Id")
            {
                e.Cancel = true;
            }
            if (e.Column.Header.ToString() == "Name")
            {
                e.Column.Width = 730;
                var col = e.Column as DataGridTextColumn;

                var style = new Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));

                col.ElementStyle = style;
            }
            if (e.Column.Header.ToString() == "Authors")
            {
                // Display Authors collection as a comma-separated string
                var style = new Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));

                e.Column = new DataGridTextColumn
                {
                    Header = "Authors",
                    Width = 220,
                    Binding = new Binding("Authors") { Converter = new AuthorsToStringConverter() },
                    ElementStyle = style
                };
            }
            if (e.Column.Header.ToString() == "Year")
            {
                var col = e.Column as DataGridTextColumn;

                var style = new Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));

                col.ElementStyle = style;
            }
            if (e.Column.Header.ToString() == "IsAvailable")
            {
                var col = e.Column as DataGridCheckBoxColumn;

                var style = new Style(typeof(CheckBox));
                style.Setters.Add(new Setter(CheckBox.VerticalAlignmentProperty, VerticalAlignment.Center));
                style.Setters.Add(new Setter(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center));
                style.Setters.Add(new Setter(CheckBox.IsHitTestVisibleProperty, false));

                col.ElementStyle = style;
                col.Header = "Is available";
            }
        }

        public void LoadBooks()
        {
            LoadBooksPage(pageModel.CurrentPage);
        }

        public async void LoadBooksPage(int pageNumber)
        {
            var searchText = tbSearch.Text;
            var recordsPerPage = pageModel.RecordsPerPage;
            var currentPage = pageNumber;
            var from = recordsPerPage * (currentPage - 1);
            await pageModel.LoadBooks(searchText, from, recordsPerPage, pageModel.Filter);
        }

        private void CalculateNumberOfPages()
        {
            var pageCountDouble = (double)pageModel.BooksTotalCount / (double)pageModel.RecordsPerPage;
            pageModel.NumberOfPages = (int)Math.Ceiling(pageCountDouble);
        }

        private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pageModel.CurrentPage = 1;
            LoadBooks();
            CalculateNumberOfPages();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadBooks();
            CalculateNumberOfPages();
        }
    }
}
