namespace HRM_SK.Contracts
{
    public static class UrlNavigation
    {

        public interface IFilterableSortableRoutePageParam
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }

        }


    }
}
