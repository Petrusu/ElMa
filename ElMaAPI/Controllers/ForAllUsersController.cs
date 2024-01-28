using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ElMaAPI.Context;
using ElMaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Net.Mail;

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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Проверяем, существует ли пользователь с таким же именем пользователя или email'ом
        if (await _context.Users.AnyAsync(u => u.Username == login || u.Email == email))
        {
            return Conflict("Пользователь с такими данными уже существует");
        }
        
        SendMessage(login, email);
        
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
       
       var booksData = allBooks.Select(book => new
       {
           Title = book.Title,
           SeriesName = book.SeriesName,
           Annotation = book.Annotation,
           Publisher = book.PublisherNavigation.Publishersname,
           Image = GetImageData(book.Image),
           Authors = book.BookAuthors.Select(ba => new {Authorsname = ba.Authors.Authorsname}),
           Editors = book.BookEditors.Select(be => new {Editorname = be.Editors.Editorname}),
           Themes = book.BookThemes.Select(bt => new { Themesname = bt.Themes.Themesname}).ToList()
       }).ToList();

       return Ok(booksData);
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

       var themesData = allthemes.Select(theme => new
       {
            theme.Themesname
       });
       return Ok(themesData);
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
   //сортировка А -> я
   [HttpGet("BooksTitleOrderBy")]
   public async Task<ActionResult<IEnumerable<Book>>> GetBooksOrderBy()
   {
       var books = await _context.Books.OrderBy(b => b.Title).ToListAsync();
       return books;
   }
   //сортировка Я -> а
   [HttpGet("BooksTitleOrderByDescending")]
   public async Task<ActionResult<IEnumerable<Book>>> GetBooksOrderByDescending()
   {
       var books = await _context.Books.OrderByDescending(b => b.Title).ToListAsync();
       return books;
   }
   

   //методы вывода изображения
   //метод возвращающий байты изображения
   private byte[] GetImageData(string imageName)
   {
       //получаем полный путь к изоброажению
       string imagePath = Path.Combine("Upload\\Files", imageName);

       //читаем байты изображения
       return System.IO.File.ReadAllBytes(imagePath);
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