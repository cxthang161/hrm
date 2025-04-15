namespace hrm.Common
{
    public class BaseResponse<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; }
        public bool Success { get; set; }

        public BaseResponse(T? data, string message, bool success)
        {
            Data = data;
            Message = message;
            Success = success;
        }
    }
    public class PaginationResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }

        public PaginationResponse(IEnumerable<T> items, int pageIndex, int totalPages)
        {
            Items = items;
            PageIndex = pageIndex;
            TotalPages = totalPages;
        }
    }
}
