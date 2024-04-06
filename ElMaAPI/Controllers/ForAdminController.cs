using ElMaAPI.Context;
using ElMaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ElMaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForAdminController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly JvwaskwsContext _context;
    private readonly IWebHostEnvironment _environment;

    public ForAdminController(IConfiguration configuration, JvwaskwsContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _configuration = configuration;
        _environment = environment;
    }
    public class AddThemeRequestModel
    {
        public string Theme { get; set; }
    }

    [HttpPost("AddTheme")]
    public async Task<IActionResult> AddTheme([FromBody] AddThemeRequestModel model)
    {
        try
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Theme))
            {
                return BadRequest("Не указана тема");
            }

            // Найти или добавить темы
            List<Theme> bookThemes = new List<Theme>();
    
            Theme existingTheme = _context.Themes.FirstOrDefault(t => t.Themesname == model.Theme);

            if (existingTheme == null)
            {
                existingTheme = new Theme { Themesname =  model.Theme };
                _context.Themes.Add(existingTheme);
                _context.SaveChanges();
            }
    
            bookThemes.Add(existingTheme);
            _context.SaveChanges();
            return Ok("Тема добавлена!");
        }
        catch (Exception e)
        {
            return BadRequest("Произошла ошибка:" + e);
        }
    }
    //добавление книги
    [HttpPost("AddNewBook")]
    public async Task<IActionResult> AddNewBook([FromBody] BookRequest bookRequest)
    {
        try
        {
            //Заполнение книги
            Book newBook = new Book
            {
                Title = bookRequest.Title,
                SeriesName = bookRequest.SeriesName,
                Annotation = bookRequest.Annotation,
                YearOfPublication = bookRequest.YearOfPublication,
                Image = await WriteFile(bookRequest.Image, bookRequest.ImageName)
            };

            // Найти или добавить место публикации
            Publicationplase place = _context.Publicationplases
                .FirstOrDefault(p => p.Publicationplasesname == bookRequest.PlaceOfPublication);
            if (place == null)
            {
                place = new Publicationplase { Publicationplasesname = bookRequest.PlaceOfPublication };
                _context.Publicationplases.Add(place);
                _context.SaveChanges();
            }
            newBook.PlaceOfPublication = place.PublicationplaseId;

            // Найти или добавить издателя
            Publisher publisherObj = _context.Publishers
                .FirstOrDefault(p => p.Publishersname == bookRequest.Publisher);
            if (publisherObj == null)
            {
                publisherObj = new Publisher { Publishersname = bookRequest.Publisher };
                _context.Publishers.Add(publisherObj);
                _context.SaveChanges();
            }
            newBook.Publisher = publisherObj.PublishersId;

            // Найти или добавить ББК
            Bbk bbk = _context.Bbks
                .FirstOrDefault(b => b.BbkCode == bookRequest.BBK);
            if (bbk == null)
            {
                bbk = new Bbk { BbkCode = bookRequest.BBK };
                _context.Bbks.Add(bbk);
                _context.SaveChanges();
            }
            newBook.BbkCode = bbk.BbkId;
            
            // Добавить книгу в контекст данных
            _context.Books.Add(newBook);

            // Сохранить изменения в базе данных
            _context.SaveChanges();
            
            //У книги может быть или редактор или автор
            if (bookRequest.AuthorBook != null)
            {
                //Найти или добавить автора
                Author author = _context.Authors.FirstOrDefault(a => a.Authorsname == bookRequest.AuthorBook);
                if (author == null)
                {
                    author = new Author { Authorsname = bookRequest.AuthorBook };
                    _context.Authors.Add(author);
                    _context.SaveChanges();
                }

                //Добавить связь между автором и книгой
                BookAuthor bookAuthor = new BookAuthor
                {
                    BookId = newBook.BookId,
                    AuthorsId = author.AuthorsId
                };
                _context.BookAuthors.Add(bookAuthor);
                _context.SaveChanges();
            }
            else if (bookRequest.Editor != null)
            {
                //Найти или добавить редактора
                Editor editorObj = _context.Editors.FirstOrDefault(e => e.Editorname == bookRequest.Editor);
                if (editorObj == null)
                {
                    editorObj = new Editor { Editorname = bookRequest.Editor };
                    _context.Editors.Add(editorObj);
                    _context.SaveChanges();
                }
            
                //Добавить связь между редактором и книгой
                BookEditor bookEditor = new BookEditor
                {
                    BookId = newBook.BookId,
                    EditorsId = editorObj.EditorsId
                };
                _context.BookEditors.Add(bookEditor);
                _context.SaveChanges();
            }
            //Добавить связь тема и книги
            foreach (var theme in bookRequest.Themes)
            {
                BookTheme bookTheme = new BookTheme
                {
                    BookId = newBook.BookId,
                    ThemesId = theme
                };
                _context.BookThemes.Add(bookTheme);
                _context.SaveChanges();
            }

            return Ok("Книга добавлена!");
        }
        catch (Exception e)
        {
            return BadRequest("Произошла ошибка:" + e);
        }
    }
    
    [HttpPost("EditBook")]
public async Task<IActionResult> EditBook(BookRequest bookRequest)
{
    using (var transaction = _context.Database.BeginTransaction())
    {
        try
        {
            // Найти книгу по идентификатору
            Book existingBook = _context.Books.Find(bookRequest.Id);

            if (existingBook == null)
            {
                return NotFound("Книга не найдена");
            }

            // Обновить только те параметры, которые предоставлены пользователем
            if (!string.IsNullOrEmpty(bookRequest.Title))
            {
                existingBook.Title = bookRequest.Title;
            }

            if (!string.IsNullOrEmpty(bookRequest.SeriesName))
            {
                existingBook.SeriesName = bookRequest.SeriesName;
            }

            if (!string.IsNullOrEmpty(bookRequest.Annotation))
            {
                existingBook.Annotation = bookRequest.Annotation;
            }

            if (bookRequest.YearOfPublication != null)
            {
                existingBook.YearOfPublication = bookRequest.YearOfPublication;
            }

            // Обновить изображение, если оно указано
            if (bookRequest.Image != null)
            {
                existingBook.Image = await WriteFile(bookRequest.Image, bookRequest.ImageName);
            }

            // Обновить место публикации, если оно указано
            if (!string.IsNullOrEmpty(bookRequest.PlaceOfPublication))
            {
                Publicationplase place = _context.Publicationplases
                    .FirstOrDefault(p => p.Publicationplasesname == bookRequest.PlaceOfPublication);

                if (place == null)
                {
                    place = new Publicationplase { Publicationplasesname = bookRequest.PlaceOfPublication };
                    _context.Publicationplases.Add(place);
                    _context.SaveChanges();
                }

                existingBook.PlaceOfPublication = place.PublicationplaseId;
            }

            // Обновить издателя, если он указан
            if (!string.IsNullOrEmpty(bookRequest.Publisher))
            {
                Publisher publisherObj = _context.Publishers
                    .FirstOrDefault(p => p.Publishersname == bookRequest.Publisher);

                if (publisherObj == null)
                {
                    publisherObj = new Publisher { Publishersname = bookRequest.Publisher };
                    _context.Publishers.Add(publisherObj);
                    _context.SaveChanges();
                }

                existingBook.Publisher = publisherObj.PublishersId;
            }

            // Обновить автора или редактора, если указаны
            if (!string.IsNullOrEmpty(bookRequest.AuthorBook))
            {
                UpdateBookAuthor(existingBook, bookRequest.AuthorBook);
            }
            else if (!string.IsNullOrEmpty(bookRequest.Editor))
            {
                UpdateBookEditor(existingBook, bookRequest.Editor);
            }

            // Обновить темы, если указаны
            if (bookRequest.Themes != null && bookRequest.Themes.Any())
            {
                UpdateBookThemes(existingBook, bookRequest.Themes);
            }

            // Сохранить изменения в базе данных
            _context.SaveChanges();

            // Завершить транзакцию
            transaction.Commit();

            return Ok("Книга отредактирована!");
        }
        catch (Exception e)
        {
            // Откатить транзакцию в случае ошибки
            transaction.Rollback();
            return BadRequest("Произошла ошибка:" + e);
        }
    }
}
    //удаление книги
    [HttpPost("DeleteBook")]
    public IActionResult DeleteBook([FromQuery] int bookId)
    {
        try
        {
            // Найти книгу по идентификатору
            Book existingBook = _context.Books.Where(b => b.BookId == bookId).FirstOrDefault();

            if (existingBook == null)
            {
                return NotFound("Книга не найдена");
            }

            // Удалить связи с авторами
            var bookAuthors = _context.BookAuthors.Where(ba => ba.BookId == existingBook.BookId).ToList();
            foreach (var bookAuthor in bookAuthors)
            {
                _context.BookAuthors.Remove(bookAuthor);
            }

            // Удалить связи с редакторами
            var bookEditors = _context.BookEditors.Where(be => be.BookId == existingBook.BookId).ToList();
            foreach (var bookEditor in bookEditors)
            {
                _context.BookEditors.Remove(bookEditor);
            }

            // Удалить связи с темами
            var bookThemes = _context.BookThemes.Where(bt => bt.BookId == existingBook.BookId).ToList();
            foreach (var bookTheme in bookThemes)
            {
                _context.BookThemes.Remove(bookTheme);
            }

            try
            {
                System.IO.File.Delete($"Upload/Files/{existingBook.Image}");
            }
            catch (Exception e)
            {
                BadRequest("Не удалось удалить книгу.");
            }
            
            // Удалить книгу
            _context.Books.Remove(existingBook);

            // Сохранить изменения в базе данных
            _context.SaveChanges();

            return Ok("Книга удалена!");
        }
        catch (Exception e)
        {
            return BadRequest("Произошла ошибка:" + e);
        }
    }
    private byte[] GetImageData(string imageName)
    {
        //получаем полный путь к изоброажению
        string imagePath = Path.Combine("Upload\\Files", imageName == "" ? "picture.png" : imageName);
        
        

        //читаем байты изображения
        return System.IO.File.ReadAllBytes(imagePath);
    }
    private void UpdateBookAuthor(Book existingBook, string authorName)
{
    Author author = _context.Authors.FirstOrDefault(a => a.Authorsname == authorName);

    if (author == null)
    {
        author = new Author { Authorsname = authorName };
        _context.Authors.Add(author);
        _context.SaveChanges();
    }

    // Удалить существующую связь
    var bookAuthor = _context.BookAuthors.FirstOrDefault(ba => ba.BookId == existingBook.BookId);
    if (bookAuthor != null)
    {
        _context.BookAuthors.Remove(bookAuthor);
        _context.SaveChanges();
    }

    // Создать новую связь
    bookAuthor = new BookAuthor
    {
        BookId = existingBook.BookId,
        AuthorsId = author.AuthorsId
    };
    _context.BookAuthors.Add(bookAuthor);
}

    private void UpdateBookEditor(Book existingBook, string editorName)
    {
        Editor editorObj = _context.Editors.FirstOrDefault(e => e.Editorname == editorName);

        if (editorObj == null)
        {
            editorObj = new Editor { Editorname = editorName };
            _context.Editors.Add(editorObj);
            _context.SaveChanges();
        }

        // Удалить существующую связь
        var bookEditor = _context.BookEditors.FirstOrDefault(be => be.BookId == existingBook.BookId);
        if (bookEditor != null)
        {
            _context.BookEditors.Remove(bookEditor);
            _context.SaveChanges();
        }

        // Создать новую связь
        bookEditor = new BookEditor
        {
            BookId = existingBook.BookId,
            EditorsId = editorObj.EditorsId
        };
        _context.BookEditors.Add(bookEditor);
    }

    private void UpdateBookThemes(Book existingBook, List<int> themeId)
    {
        var oldThemes = _context.BookThemes.Where(t => t.BookId == existingBook.BookId).ToList();
        _context.RemoveRange(oldThemes);
        _context.SaveChanges();
        List<Theme> bookThemes = new List<Theme>();
        foreach (int themeID in themeId)
        {
            Theme existingTheme = _context.Themes.FirstOrDefault(t => t.ThemesId == themeID);

            if (existingTheme == null)
            {
                existingTheme = new Theme { ThemesId = themeID };
                _context.Themes.Add(existingTheme);
                _context.SaveChanges();
            }

            bookThemes.Add(existingTheme);
        }

        // Удалить существующие связи
        var existingBookThemes = _context.BookThemes
            .Where(bt => bt.BookId == existingBook.BookId)
            .ToList();

        foreach (var oldBookTheme in existingBookThemes)
        {
            _context.BookThemes.Remove(oldBookTheme);
        }

        // Добавить новые связи
        foreach (var newTheme in bookThemes)
        {
            BookTheme bookTheme = new BookTheme
            {
                BookId = existingBook.BookId,
                ThemesId = newTheme.ThemesId
            };
            _context.BookThemes.Add(bookTheme);
        }
    }
        //метод для сохранения изображения, возвращает имя изображения
        private async Task<string> WriteFile(byte[] file, string fileName)
        {
            try
            {
                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files");

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }

                var exactpath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files", fileName);
                if (!System.IO.File.Exists(exactpath))
                {
                    System.IO.File.WriteAllBytes(exactpath, file);
                }
                else
                {
                        
                }
            }
            catch (Exception e)
            {
            }

            return fileName;
        }
       
    }