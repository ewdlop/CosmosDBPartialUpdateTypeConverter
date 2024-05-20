namespace ConsoleApp;

using System.Collections.Generic;

//public record Book(int BookId, string Title, string ISBN, DateTime PublicationDate, decimal Price, int AuthorId, Author Author, int PublisherId, Publisher Publisher, ICollection<BookCategory> BookCategories);

//public record Author(int AuthorId, string Name, ICollection<Book> Books);

//public record Publisher(int PublisherId, string Name, ICollection<Book> Books);

//public record Category(int CategoryId, string Name, ICollection<BookCategory> BookCategories);

//public record BookCategory(int BookId, Book Book, int CategoryId, Category Category);

public class Book
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Title { get; set; }
    public string? ISBN { get; set; }
    public DateTime? PublicationDate { get; set; }
    public decimal? Price { get; set; }

    // Embedded documents for one-to-many relationships
    public Author? Author { get; set; }
    public Publisher? Publisher { get; set; }
    public List<Category> Categories { get; set; } = [];
}

public class Author
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
}

public class Publisher
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
}

public class Category
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
}