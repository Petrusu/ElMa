using ElMaAPI.Context;
using ElMaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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

    //добавление книги
    [HttpPost("AddNewBook")]
    public async Task<IActionResult> AddNewBook([FromForm] BookRequest bookRequest)
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
                Image = await WriteFile(bookRequest.Image)
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
            // Найти или добавить темы
            List<Theme> bookThemes = new List<Theme>();
            foreach (string themeName in bookRequest.Themes)
            {
                Theme existingTheme = _context.Themes.FirstOrDefault(t => t.Themesname == themeName);

                if (existingTheme == null)
                {
                    existingTheme = new Theme { Themesname = themeName };
                    _context.Themes.Add(existingTheme);
                    _context.SaveChanges();
                }
        
                bookThemes.Add(existingTheme);
                _context.SaveChanges();
            }
            //Добавить связь тема и книги
            foreach (var theme in bookThemes)
            {
                BookTheme bookTheme = new BookTheme
                {
                    BookId = newBook.BookId,
                    ThemesId = theme.ThemesId
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

    //метод для сохранения изображения, возвращает имя изображения
    private async Task<string> WriteFile(IFormFile file)
    {
        string filename = "";
        try
        {
            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            filename = DateTime.Now.Ticks.ToString() + extension;

            var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files");

            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }

            var exactpath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files", filename);
            using (var stream = new FileStream(exactpath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }
        catch (Exception e)
        {
        }

        return filename;
    }
   
}