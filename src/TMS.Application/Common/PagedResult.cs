namespace TMS.Application.Common;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
);

/* Items -> The actual data for the current page.
   Total count -> The total number of records, not just the current page.
   PageNumber -> which page you're currently viewing.
   PageSize -> How many items you requested per page.

   PagedResult<T> is a reusable container that holds the current page of data plus the information needed to build pagination.
   And because it uses <T>, you can use it for teachers, subjects, courses, or any other type.
*/
