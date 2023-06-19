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

}