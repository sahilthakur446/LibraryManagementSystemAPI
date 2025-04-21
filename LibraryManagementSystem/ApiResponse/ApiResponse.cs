namespace LibraryManagementSystem.ApiResponse
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; } 

        public ApiResponse(bool isSuccess, string message, T? data)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
        }

        public ApiResponse(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = default;
        }

        // ✅ Success Response - Single Object
        public static ApiResponse<T> Success(T? data, string message)
        {
            return new ApiResponse<T>(true, message, data);
        }

        // ✅ Success Response - No data
        public static ApiResponse<T> Success(string message)
        {
            return new ApiResponse<T>(true, message, default);
        }

        // ✅ Failure Response
        public static ApiResponse<T> Fail(string errorMessage)
        { 
            return new ApiResponse<T>(false, errorMessage, default);
        }
        public static ApiResponse<T> Fail(T? data, string message)
        {
            return new ApiResponse<T>(false, message, data);
        }
    }
}
