using System;
using System.Collections.Generic;

namespace BookLibrary.UI.Models.BooksModels
{
    public class Book
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public List<string> Authors { get; set; }
        public string Year { get; set; }
        public bool IsAvailable { get; set; }

        public Book(Storage.Models.Book.Book bookitem)
        {
            Id = bookitem.Id;
            Name = bookitem.Name;
            Authors = [.. bookitem.Authors];
            Year = bookitem.Year.Year.ToString();
            IsAvailable = (bool)bookitem.IsAvailable;
        }
    }
}
