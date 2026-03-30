namespace Shipment.Entities.Shared;


public class PaginationResponse<T>(List<T> data, int page, int pageSize, int totalCount)
{
    public List<T> Data { get; } = data;
    public int Page { get; } = page;
    public int PageSize { get; } = pageSize;
    public int TotalCount { get; } = totalCount;

    public int TotalPages => (int)Math.Ceiling((decimal)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

}