using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using WebApplication2.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
 // объявл кл. поля приват. // дост. к конфиг. // извлеч. строки подключ. к бд. 
        private readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration) // коструктор класса // парам. - объект и его имя
        {
            _configuration = configuration; // сохраняем обхект в приват. поле
        }
   
        // метод для отображения страницы авторизации
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        // ансихрон или паралл метод. / принимающ. модель класса User
        public async Task<IActionResult> Login(User model)
        {
            if (ModelState.IsValid) // если данные прошли валидацию
            {
                // проверка логина и пароля пользователя
                if (IsValidUser(model.Username, model.Password))
                {
                    // контейн. информ. о пользов. (утвержд)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Username), // создает утвержд о имени пользов.
                        
                    };

                    // создание объекта ClaimsIdentity (идентификацию пользователя) на основе утверждений с использов. схемы аутент. куки
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // создание объекта ClaimsPrincipal - оберка на основе идентификации (содерж идентиф. и утверждение)
                    var principal = new ClaimsPrincipal(identity);

                   // выпол. аутен. в HTTP контексте / схема аутен. CoocieAuten.AutenSchene(на основе куки) /principa-содерж информ. об пользоват. 
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


                    // метод RedirectToAction перенаправляет на другое действие (на защищенную страницу)
                    return RedirectToAction("AuthIndex", "Home");
                }
                else
                {
                    // отображение сообщения об ошибке модели (" " - примен. ко всей модели)
                    ModelState.AddModelError("","Неверное имя пользователя или пароль");
                }
            }
            // при ошибки возвр. модель в View 
            return View(model) ;
        } 


        private bool IsValidUser(string username, string password)
        {
            // получить строку подключения из конфигурации
            string connectionString = _configuration.GetConnectionString("MyConnection");

            // создать подключение к базе данных
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // открыть подключение
                    connection.Open();

                    // создать команду для выполнения запроса
                    string query = "SELECT COUNT(*) FROM user_login WHERE Username = @Username AND Password = @Password";
                    SqlCommand command = new SqlCommand(query, connection);

                    // добавить параметры (@..) в команду command и устанав. значения переменных
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    // выполнить запрос и получить результат (количество записей)
                    int count = (int)command.ExecuteScalar();

                    // если возвращается значение больше 0, то данные пользователя совпадают
                    return count > 0;
                }
                catch (Exception ex)
                {
                   
                    return false;
                }
            }
        }


        public async Task<IActionResult> Logout()
        {
            // выполнение выхода пользователя
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Перенаправление на страницу входа
            return RedirectToAction("Login");

        }


        // защищенная страница
        [Authorize]
        public ActionResult AuthIndex()
        {
            var userNull = ""; // если пользователя нет
            if (User.Identity.IsAuthenticated) //проверка на аутентиф
            {
                var username = User.Identity.Name; // сохр. имя пользоват. котор. прошел. аутент.
                ViewBag.Username = username; //сохр. в объект для передачи в View
                return View(); // возвр. ту же View
            }
            else {
                ViewBag.Username = userNull; 
            }
            return View("Index"); // если польз. не аутент (допол. защита)
        }

        public IActionResult Index()
        {
            var userNull = "";
            if (User.Identity.IsAuthenticated)
            {
                var username = User.Identity.Name;
                // Пример: передача имени пользователя в представление
                ViewBag.Username = username;
            }
            else { ViewBag.Username = userNull; }
            return View();
        }









        private readonly ILogger<HomeController> _logger;

		//public HomeController(ILogger<HomeController> logger)
		//{
		//	_logger = logger;
		//}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}