using System;
using System.Collections.Generic;
using System.Linq;

namespace BookLibrary.Storage.Models.Book
{
    public class Book
    {
        public Guid Id { get; }
        public string Name { get; }
        public IEnumerable<string> Authors { get; }
        public DateTime Year { get; }
        public bool? IsAvailable { get; }

        public Book(string name, IEnumerable<string> authors, DateTime year, bool isAvailable) : this(Guid.NewGuid(), name, authors, year, isAvailable) { }

        public static Book FromPersistence(Guid id, string name, IEnumerable<string> authors, DateTime year, bool isAvailable)
        {
            return new Book(id, name, authors, year, isAvailable);
        }

        private Book(Guid id, string name, IEnumerable<string> authors, DateTime year, bool isAvailable)
        {
            Id = id;
            Name = name;
            Authors = authors.Where(author => !string.IsNullOrWhiteSpace(author)).Select(author => author.Trim());
            Year = year;
            IsAvailable = isAvailable;
        }
    }
}
