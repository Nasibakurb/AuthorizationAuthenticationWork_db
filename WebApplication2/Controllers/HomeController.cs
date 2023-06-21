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
using System.Net;
using System.Runtime.CompilerServices;

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

                    //////
                    string ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();

                    ////
                    if (!string.IsNullOrEmpty(ipAddress))
                    {
                        InsertIPAndDate(model.Username, ipAddress);
                    }

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


        public void InsertIPAndDate(string username, string ipAddress)
        {
            string connectionString = _configuration.GetConnectionString("MyConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // получаем Id пользователя по его имени
                string getUserIdQuery = "SELECT Id FROM user_login WHERE Username = @Username";
                SqlCommand getUserIdCommand = new SqlCommand(getUserIdQuery, connection);
                getUserIdCommand.Parameters.AddWithValue("@Username", username);
                int userId = (int)getUserIdCommand.ExecuteScalar();

                // вставляем запись IP и даты, связанную с пользователем
                string insertQuery = "INSERT INTO user_IpDate (IP, Date, User_login_id) VALUES (@IP, @Date, @UserId)";
                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@IP", ipAddress);
                insertCommand.Parameters.AddWithValue("@Date", DateTime.Now);
                insertCommand.Parameters.AddWithValue("@UserId", userId);
                insertCommand.ExecuteNonQuery();
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

                // Получение данных таблицы из базы данных
                List<IpDateMidel> rows = GetTableDataFromDatabase();

                // Передача данных таблицы в представление
                return View(rows);
            }
            else
            {
                ViewBag.Username = userNull;
                return View();
            }
        }




        private List<IpDateMidel> GetTableDataFromDatabase()
        {
            List<IpDateMidel> tableRows = new List<IpDateMidel>();

            string connectionString = _configuration.GetConnectionString("MyConnection");
            string query = "SELECT IP, [Date], User_login_id FROM user_IpDate";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
 
                    IpDateMidel row = new IpDateMidel
                    {
                        ipAddress = reader.GetString(0),
                        Date = reader.GetDateTime(1),
                        User_login_id = reader.GetInt32(2)
                    };

                    // добавление строки в список
                    tableRows.Add(row);
                }

                reader.Close();
            }

            return tableRows;
        }


        //.////
        [HttpPost]
        public ActionResult Search(string ip, DateTime date, List<IpDateMidel> model)
        {
            AuthIndex();
            if (ModelState.IsValid)
            {
                List<IpDateMidel> searchResults = SearchInDatabase(ip, date);

                return View("AuthIndex", searchResults); ;
            }
            else
            {
                return View("AuthIndex", new List<IpDateMidel>());
                
            }
            
        }

        private List<IpDateMidel> SearchInDatabase(string ip, DateTime date)
        {
            List<IpDateMidel> searchResults = new List<IpDateMidel>();

            string connectionString = _configuration.GetConnectionString("MyConnection");
            string query = "SELECT IP, Date, User_login_id FROM user_IpDate WHERE IP LIKE @IP AND Date = @Date";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@IP", "%" + ip + "%");
                command.Parameters.AddWithValue("@Date", date);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    IpDateMidel row = new IpDateMidel
                    {
                        ipAddress = reader.GetString(0),
                        Date = reader.GetDateTime(1),
                        User_login_id = reader.GetInt32(2)
                    };

                    searchResults.Add(row);
                }

                reader.Close();
            }

            return searchResults;
        }


        [HttpPost]
        public ActionResult ResetFilter() // сбросить фильтрацию
        {

            return RedirectToAction("AuthIndex");
        }


        ///.../


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



        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password, RegisterViewModel model)
        {
            // Проверка модели данных
            if (ModelState.IsValid)
            {
                string connectionString = _configuration.GetConnectionString("MyConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string insertQuery = "INSERT INTO user_login (Username, Password) VALUES (@Username, @Password)";
                    SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@Username", username);
                    insertCommand.Parameters.AddWithValue("@Password", password);
                    insertCommand.ExecuteNonQuery();
                }

                // После успешной регистрации перенаправление на страницу логина
                return RedirectToAction("Login");
            }

            return View(model);
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