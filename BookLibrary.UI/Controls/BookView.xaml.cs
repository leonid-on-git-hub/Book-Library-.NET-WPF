using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BookLibrary.UI.Controls
{
    /// <summary>
    /// Interaction logic for BookView.xaml
    /// </summary>
    public partial class BookView : UserControl
    {
        private int AuthorsCount = 0;

        public static readonly DependencyProperty BookNameProperty =
            DependencyProperty.Register("BookName", typeof(string), typeof(BookView), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty BookYearProperty =
            DependencyProperty.Register("BookYear", typeof(DateTime), typeof(BookView), new UIPropertyMetadata(DateTime.Now));

        public BookView()
        {
            InitializeComponent();

            if (BookAuthors.Count == 0)
            {
                AddAuthor(string.Empty);
            }
        }

        public string BookName
        {
            get { return (string)GetValue(BookNameProperty); }
            set { SetValue(BookNameProperty, value); }
        }

        public ObservableCollection<string> BookAuthors
        {
            get;
            set
            {
                var bookAuthorsCount = BookAuthors.Count;
                for (var i = 0; i < bookAuthorsCount; i++)
                {
                    RemoveTextBox(i);
                }

                foreach (var author in value)
                {
                    AddAuthor(author);
                }
            }
        } = [];

        public DateTime BookYear
        {
            get { return (DateTime)GetValue(BookYearProperty); }
            set { SetValue(BookYearProperty, value); }
        }

        private void btnAddAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (AuthorsCount > 4)
            {
                return;
            }

            AddAuthor(string.Empty);
        }

        private void AddAuthor(string author)
        {
            DockPanel authorPanel = new DockPanel();
            TextBox newTextBox = new TextBox
            {
                Style = (Style)FindResource("MyTextBox"),
                TextWrapping = TextWrapping.Wrap,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Height = 25,
                Width = 375,
                Margin = new Thickness(5)
            };
            BookAuthors.Add(author);
            int index = BookAuthors.Count - 1;
            var binding = new Binding($"BookAuthors[{index}]")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };


            newTextBox.SetBinding(TextBox.TextProperty, binding);

            Button removeButton = new Button
            {
                Content = "Remove",
                Style = (Style)FindResource("ActionButton"),
                Height = 25,
                Margin = new Thickness(5),
            };

            DockPanel.SetDock(removeButton, Dock.Right);

            removeButton.Click += (s, args) =>
            {
                int currentIndex = spAuthors.Children.IndexOf(authorPanel);
                if (currentIndex >= 0)
                    RemoveTextBox(currentIndex);
                spAuthors.Children.Remove(authorPanel);
                AuthorsCount--;
            };

            authorPanel.Children.Add(newTextBox);
            authorPanel.Children.Add(removeButton);

            spAuthors.Children.Add(authorPanel);
            AuthorsCount++;
        }

        private void RemoveTextBox(int index)
        {
            if (index < 0 || index >= spAuthors.Children.Count || index >= BookAuthors.Count)
                return;

            spAuthors.Children.RemoveAt(index);

            BookAuthors.RemoveAt(index);

            RebindTextBoxes();
        }

        private void RebindTextBoxes()
        {
            for (int i = 0; i < spAuthors.Children.Count; i++)
            {
                if (spAuthors.Children[i] is Panel row)
                {
                    TextBox tb = null;
                    foreach (var child in row.Children)
                    {
                        if (child is TextBox t)
                        {
                            tb = t;
                            break;
                        }
                    }

                    if (tb == null)
                        continue;

                    BindingOperations.ClearBinding(tb, TextBox.TextProperty);
                    var binding = new Binding($"BookAuthors[{i}]")
                    {
                        Source = this,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    tb.SetBinding(TextBox.TextProperty, binding);
                }
            }
        }
    }
}
