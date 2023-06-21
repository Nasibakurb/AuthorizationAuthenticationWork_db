using System.ComponentModel.DataAnnotations;



namespace WebApplication2.Models
{
	public class ErrorViewModel
	{
		public string RequestId { get; set; }

		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
	}


    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }    
    }
    public class IpDateMidel
    {
        [Required(ErrorMessage = "Вы не ввели данные")]
        public string ipAddress { get; set; }

        //[Required(ErrorMessage = "Вы не ввели данные")]
        public DateTime Date { get; set; }
        
        public int User_login_id { get; set; }
    }

    //public class CombinedModel
    //{
    //    public List<IpDateMidel> IpDateList { get; set; }
    //    public IpDateMidel SingleIpDate { get; set; }
    //}


    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите имя")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }


}
