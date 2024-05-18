using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ElMaAPI.Context;
using ElMaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;
using ElMaDesktop.Classes;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ElMaAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ForAllUsersController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly JvwaskwsContext _context;
    private int TokenTimeoutMinutes = 5; // Время истечения срока действия токена в минутах
    private DateTime _tokenCreationTime;
    
    public ForAllUsersController(JvwaskwsContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    
    //регистрация
    [HttpPost("registration")]
    public async Task<IActionResult> RegisterUser(string login, string email, string password)
    {

        // Проверяем, существует ли пользователь с таким же именем пользователя или email'ом
        if (await _context.Users.AnyAsync(u => u.Username == login || u.Email == email))
        {
            return Conflict("Пользователь с такими данными уже существует");
        }
        
        // Шифрование пароля
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        // Создаем нового пользователя
        var user = new User
        {
            Username = login,
            Email = email,
            Userpassword = hashedPassword,
            Userrole = 2  
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        SendMessage(login, email);
        return Ok("Пользователь добавлен");
        
    }
    
    //авторизация
    [HttpPost("login")]
    public IActionResult Authenticate(string login, string password)
    {
        // Проверяем, существует ли пользователь
        var user = _context.Users.FirstOrDefault(u => u.Username == login);
        if (user == null)
        {
            return Unauthorized(); // Пользователь не найден
        }

        var loginResponse = new LoginResponse();

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Userpassword);

        // Если пароль действителен
        if (isPasswordValid)
        {
            string token = CreateToken(user.UserId);

            loginResponse.Token = token;
            loginResponse.ResponseMsg = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK
            };

            // Возвращаем токен
            return Ok(new { loginResponse });
        }
        else
        {
            // Если имя пользователя или пароль недействительны, отправляем статус-код "BadRequest" в ответе
            return BadRequest("Имя пользователя илм пароль не верен!");
        }
    }
    
    //проверка почты
    [HttpPost("verify_email")]
    public IActionResult ChekEmail(int _code)
    {
        int userCode = _code;
        bool codeExists = _context.Verifycodes.Any(vc => vc.Code == userCode);

        if (codeExists)
        {
            return Ok("Код подтвержден");
        }
        else
        {
            return BadRequest("Неверный код подтверждения");
        }
    }
    
   //вывод книг 
   [HttpGet("fillbook")]
   public IActionResult GetAllBooks()
   {
       var allBooks = _context.Books
           .Include(b => b.BbkCodeNavigation)
           .Include(b => b.PlaceOfPublicationNavigation)
           .Include(b => b.PublisherNavigation)
           .Include(b => b.BookAuthors)
           .ThenInclude(ba => ba.Authors)
           .Include(b => b.BookEditors)
           .ThenInclude(be => be.Editors)
           .Include(b => b.BookThemes)
           .ThenInclude(bt => bt.Themes)
           .ToList();

       if (allBooks.Count == 0)
       {
           return NotFound(); // Книги не найдены
       }
       
       var booksData = allBooks.Select(book => new BooksCard()
       {
           BookId = book.BookId,
           BBK = book.BbkCodeNavigation.BbkCode,
           Title = book.Title,
           SeriesName = book.SeriesName,
           Publisher = book.PublisherNavigation.Publishersname,
           Image = GetImageData(book.Image),
           PlaceOfPublication = book.PlaceOfPublicationNavigation.Publicationplasesname,
           YearOfPublication = book.YearOfPublication.ToString(),
           Authors = book.BookAuthors.Select(ba => ba.Authors.Authorsname).ToList(),
           Editors = book.BookEditors.Select(be => be.Editors.Editorname).ToList()
       }).ToList();

       return Ok(JsonSerializer.Serialize(booksData));
   }
   //вывод тем
   [HttpGet("fillthemes")]
   public IActionResult GetAllThemes()
   {
       var allthemes = _context.Themes.ToList();
       if (allthemes.Count == null)
       {
           return NotFound(); //темы не найдены
       }

       var themesData = allthemes.ToList();
       return Ok(JsonSerializer.Serialize(themesData, new JsonSerializerOptions(){ReferenceHandler = ReferenceHandler.IgnoreCycles}));
   }
   //вывод авторов
   [HttpGet("fillauthors")]
   public IActionResult GetAllAuthors()
   {
       var allauthors = _context.Authors.ToList();
       if (allauthors.Count == null)
       {
           return NotFound(); //авторы не найдены
       }

       var authorsData = allauthors.Select(author => new
       {
           author.Authorsname
       });
       return Ok(authorsData);
   }
   //вывод редакторов
   [HttpGet("filleditors")]
   public IActionResult GetAllEditors()
   {
       var alleditors = _context.Editors.ToList();
       if (alleditors.Count == null)
       {
           return NotFound(); //редакторы не найдены
       }

       var editorData = alleditors.Select(editor => new
       {
           editor.Editorname
       });
       return Ok(editorData);
   }
   // запрос на информацию о конкретной книге
   [HttpGet("getinformationaboutbook")]
   public async Task<ActionResult<BookRequest>> GetInformationAboutBook(int bookId)
   {
       // запрос к бд для нахождения книги соответствующей введенному id
       var currentBook = await _context.Books
           .Include(b => b.BbkCodeNavigation)
           .Include(b => b.PlaceOfPublicationNavigation)
           .Include(b => b.PublisherNavigation)
           .Include(b => b.BookAuthors)
           .ThenInclude(ba => ba.Authors)
           .Include(b => b.BookEditors)
           .ThenInclude(be => be.Editors)
           .Include(b => b.BookThemes)
           .ThenInclude(bt => bt.Themes)
           .FirstOrDefaultAsync(b => b.BookId == bookId); // исправлено здесь, добавлен FirstOrDefaultAsync

       if (currentBook == null)
       {
           return NotFound("Книга не найдена");
       }

       var bookInformation = new BookRequest()
       {
           Id = currentBook.BookId,
           Title = currentBook.Title,
           SeriesName = currentBook.SeriesName,
           Annotation = currentBook.Annotation,
           Publisher = currentBook.PublisherNavigation.Publishersname,
           PlaceOfPublication = currentBook.PlaceOfPublicationNavigation.Publicationplasesname,
           YearOfPublication = currentBook.YearOfPublication,
           BBK = currentBook.BbkCodeNavigation.BbkCode,
           Themes = _context.BookThemes.Where(b => b.BookId == bookId).Select(b => b.ThemesId).ToList(),
       };
       string fileInfo = currentBook.Image == "" || currentBook.Image == null ? "picture.png" : currentBook.Image;
       bookInformation.Image = System.IO.File.ReadAllBytes(
           $"Upload/Files/{fileInfo}");
       bookInformation.ImageName = currentBook.Image;

       var authorsList = _context.BookAuthors.Where(a => a.BookId == bookId).ToList();
       string[] authorsArr = new string[authorsList.Count()];
       for (int i = 0; i < authorsArr.Length; i++)
       {
           authorsArr[i] = authorsList[i].Authors.Authorsname;
       }

       bookInformation.AuthorBook = authorsArr;

       var editorsList = _context.BookEditors.Where(a => a.BookId == bookId).ToList();
       string[] editorsArr = new string[editorsList.Count()];
       for (int i = 0; i < editorsArr.Length; i++)
       {
           editorsArr[i] = editorsList[i].Editors.Editorname;
       }

       bookInformation.Editor = editorsArr;

       return Ok(JsonSerializer.Serialize(bookInformation)); // возвращаем информацию о книге
   }


   //вывод избранного
   [HttpGet("Favorite")]
   public OkObjectResult GetFavoriteBooksByUserId()
   {
       int userId = GetUserIdFromToken();

       var favoriteBooks = _context.Favorites
           .Where(f => f.UserId == userId) // Фильтрация по UserId
           .Include(f => f.Book) // Предзагрузка связанной сущности Book
           .Select(f => new
           {
               IdBook = f.Book.BookId,
               Title = f.Book.Title,
           })
           .ToList();

       return Ok(favoriteBooks);
   }
   //добавление книги в избранное
   [HttpPost("addbookforfavorite")]
   public IActionResult AddBookForFavorite(int idBook)
   {

       int idUser = GetUserIdFromToken(); //получаем id пользователя из токена
       Favorite favoriteModel = new Favorite //экземпляр класса избранного
       {
           UserId = idUser,
           BookId = idBook
       };

       _context.Favorites.Add(favoriteModel); //добавляем 
       _context.SaveChanges(); //сохраняем

       return Ok("Книга добавлена в избранное.");
   }
   //удаляем книгу из избранного
   [HttpDelete("removebookfromfavorites")]
   public async Task<IActionResult> DeleteBookFromFavarite(int idBook)
   {
       var book =  _context.Favorites.FirstOrDefault(b => b.BookId == idBook); //находим книгу по id

       if (book == null)
       {
           return NotFound("Book not found"); // Если пользователь с указанным id не найден, возвращаем 404 Not Found
       }

       _context.Favorites.Remove(book); //удаляем
       await _context.SaveChangesAsync(); //сохраняем

       return Ok("Book delited"); 
   }

   //методы вывода изображения
   //метод возвращающий байты изображения
   private byte[] GetImageData(string imageName)
   {
       //получаем полный путь к изоброажению
       string imagePath = Path.Combine("Upload//Files", imageName == "" ? "picture.png" : imageName);

       //читаем байты изображения
       return System.IO.File.ReadAllBytes(imagePath);
   }
   //запрос на изменения пароля
   [HttpPut("changepassword")]
   public IActionResult ChangePassword(string password)
   {
       int id = GetUserIdFromToken();
       // Проверяем, существует ли пользователь
       var user = _context.Users.FirstOrDefault(u => u.UserId == id);
       if (user == null)
       {
           return Unauthorized(); // Пользователь не найден
       }

       // Шифрование пароля
       string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
       user.Userpassword = hashedPassword;
       _context.SaveChanges(); //сохранение нового пароля

       return Ok("Пароль изменен");
   }

   [HttpGet("getLogin")]
   public string GetLogin()
   {
       int id = GetUserIdFromToken();
       var user = _context.Users.FirstOrDefault(u => u.UserId == id);
       if (user == null)
       {
           return null; // Пользователь не найден
       }

       return (user.Username);
   }

    //изменения email
   [HttpPut("changeemail")]
   public IActionResult ChangeEmail(string email)
   {
       int id = GetUserIdFromToken();
       // Проверяем, существует ли пользователь
       var user = _context.Users.FirstOrDefault(u => u.UserId == id);
       if (user == null)
       {
           return Unauthorized(); // Пользователь не найден
       }

       user.Email = email;

       _context.SaveChanges(); //сохранение нового мыла

       // Отправка сообщения с кодом на почту
       SendMessage(user.Username, email);

       return Ok("Почта изменена");
   }

    //изменение логина
   [HttpPut("changelogin")]
   public IActionResult ChangeLogin(string login)
   {
       int id = GetUserIdFromToken();
       // Проверяем, существует ли пользователь
       var user = _context.Users.FirstOrDefault(u => u.UserId == id);
       if (user == null)
       {
           return Unauthorized(); // Пользователь не найден
       }

       // Проверяем, что новый логин не совпадает ни с одним из существующих логинов
       var existingUser = _context.Users.FirstOrDefault(u => u.Username == login);
       if (existingUser != null)
       {
           return BadRequest("Такой логин уже существует"); // Логин уже существует
       }

       user.Username = login;
       _context.SaveChanges(); //сохранение нового логина

       // Отправка сообщения с котом на почту
       SendMessage(login, user.Email);

       return Ok("Логин изменен");
   }
   
   
    //методы для почты
    //метод отправки кода на почту
    private void SendMessage(string login, string email)
    {
        //отправка сообщения на почту
        MailAddress fromAddress = new MailAddress("elma.rtk.email@mail.ru", "Elma");
        MailAddress toAddress = new MailAddress(email, login);
        
        MailMessage message = new MailMessage(fromAddress, toAddress);

        //генерация и сохранения кода в бд
        int code = GenerateRandomCode();
        _context.Verifycodes.Add(new Verifycode{Code = code});
        _context.SaveChanges();
        
        
        message.Body = $"Спасибо за регистрацию, {login}! Ваш код: {code}";
        message.Subject = "Подтвержение почты";
        
        using (SmtpClient smtpClient = new SmtpClient("smtp.mail.ru"))
        {
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(fromAddress.Address, "iLCdjgb0qMhW1WBhhPRC");
        
            try
            {
                smtpClient.Send(message);
            }
            catch (SmtpException ex)
            {
                BadRequest($"Ошибка отправки почты: {ex.Message}");
            }
        }
    }
    //метод генерации кода
    private int GenerateRandomCode()
    {
        return new Random().Next(1000, 10000);
    }
    
    
    //Методы связанные с токенами
    
    //создание токена
    private string CreateToken(int userId)
    {
        var claims = new List<Claim>()
        {
            // Список претензий (claims) - мы проверяем только id пользователя.
            new Claim("userId", Convert.ToString(userId)),
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddMinutes(TokenTimeoutMinutes),
            signingCredentials: cred
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }
    //получение id пользователя из токена
    private int GetUserIdFromToken()
    {
        var token = GetTokenFromAuthorizationHeader(); //получаем токен
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        //полчение срока действия токена
        var now = DateTime.UtcNow;
        if (jwtToken.ValidTo < now)
        {
            // Токен истек, выполните необходимые действия, например, вызовите исключение
            throw new Exception("Expired token.");
        }
        // Извлечение идентификатора пользователя из полезной нагрузки токена
        var userId = int.Parse(jwtToken.Claims.First(c => c.Type == "userId").Value);

        return userId;
    }

    //получение токена из запроса
    private string GetTokenFromAuthorizationHeader()
    {
        var autorizationHeader = Request.Headers["Authorization"].FirstOrDefault();

        if (autorizationHeader != null && autorizationHeader.StartsWith("Bearer "))
        {
            var token = autorizationHeader.Substring("Bearer ".Length).Trim();
            return token;
        }

        return null;
    }
}