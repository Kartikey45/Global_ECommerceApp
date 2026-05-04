namespace ECommerceApp.Shared.Wrappers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> Ok(
            T data,
            string message = "Success") => new()
            {
                Success = true,
                Message = message,
                Data = data
            };

        public static ApiResponse<T> Fail(
            string message,
            List<string>? errors = null) => new()
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
    }

    // Non-generic version for responses with no data
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Ok(string message = "Success") =>
            new() { Success = true, Message = message };

        public static new ApiResponse Fail(
            string message,
            List<string>? errors = null) => new()
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
    }
}
