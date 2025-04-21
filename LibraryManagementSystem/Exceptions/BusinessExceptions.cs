namespace LibraryManagementSystem.Exceptions
{
    public class BusinessExceptions : Exception
    {
        public int StatusCode { get; }
        public BusinessExceptions(string message) : base(message) { }
        public BusinessExceptions(string message, Exception innerException) : base(message, innerException) { }
        public BusinessExceptions(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
        public BusinessExceptions(string message, int statusCode, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
