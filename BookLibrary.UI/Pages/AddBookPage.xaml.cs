using BookLibrary.Storage.Models.Book;
using BookLibrary.Storage.Repositories;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BookLibrary.UI.Pages
{
    /// <summary>
    /// Interaction logic for AddBookPage.xaml
    /// </summary>
    public partial class AddBookPage : Page
    {
        private readonly BooksRepository booksRepository = new();

        public AddBookPage()
        {
            InitializeComponent();

            BtnBackward.Background = PagesPropertiesProvider.BackwardImage;

            BtnBackward.Click += BtnBackward_Click;
            BtnAddBook.Click += BtnAddBook_Click;
        }

        private async Task AddBook()
        {
            if (!string.IsNullOrEmpty(bookView.BookName) && bookView.BookAuthors.Count > 0)
            {
                var book = new Book(bookView.BookName, bookView.BookAuthors, bookView.BookYear, true);
                await booksRepository.AddBook(book);
                bookView.BookName = string.Empty;
                bookView.BookAuthors = [];
                NavigationService.Navigated += NavigationService_Navigated;
                NavigationService.GoBack();
            }
        }

        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            if (sender is Frame frame)
            {
                if (frame.NavigationService.Content is BookLibraryMainPage page)
                {
                    page.LoadBooksPage(1);
                }
                frame.NavigationService.Navigated -= NavigationService_Navigated;
            }
        }

        private void BtnBackward_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private async void BtnAddBook_Click(object sender, RoutedEventArgs e)
        {
            await AddBook();
        }
    }
}
