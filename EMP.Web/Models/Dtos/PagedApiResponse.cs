namespace EMP.Web.Models.Dtos
{
    public class PagedApiResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

}
