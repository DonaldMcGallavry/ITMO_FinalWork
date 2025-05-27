using Microsoft.EntityFrameworkCore;

namespace ITMO_FinalWork.Utilities
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public string CurrentFilter { get; private set; }
        public string CurrentSort { get; private set; }
        public string CurrentEntityType { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize,
                           string currentFilter = "", string currentSort = "", string entityType = "")
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            PageSize = pageSize;
            TotalCount = count;
            CurrentFilter = currentFilter;
            CurrentSort = currentSort;
            CurrentEntityType = entityType;

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source,
            int pageIndex,
            int pageSize,
            string currentFilter = "",
            string currentSort = "",
            string entityType = "")
        {
            var count = await source.CountAsync();
            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedList<T>(items, count, pageIndex, pageSize,
                                      currentFilter, currentSort, entityType);
        }

        public Dictionary<string, string> GetSortParams()
        {
            return CurrentEntityType switch
            {
                "MigrationReg" => new Dictionary<string, string>
                {
                    { "NameSortParam", CurrentSort == "name" ? "name_desc" : "name" },
                    { "DateSortParam", CurrentSort == "date" ? "date_desc" : "date" },
                    { "StatusSortParam", CurrentSort == "status" ? "status_desc" : "status" }
                },
                "RegistrationReg" => new Dictionary<string, string>
                {
                    { "NameSortParam", CurrentSort == "name" ? "name_desc" : "name" },
                    { "DateSortParam", CurrentSort == "date" ? "date_desc" : "date" }
                },
                "Passport" => new Dictionary<string, string>
                {
                    { "SeriesSortParam", CurrentSort == "series" ? "series_desc" : "series" },
                    { "NumberSortParam", CurrentSort == "number" ? "number_desc" : "number" },
                    { "DateSortParam", CurrentSort == "date" ? "date_desc" : "date" }
                },
                _ => new Dictionary<string, string>()
            };
        }

        public string GetEntitySpecificInfo()
        {
            return CurrentEntityType switch
            {
                "MigrationReg" => $"Миграционные записи: {TotalCount}",
                "RegistrationReg" => $"Регистрационные записи: {TotalCount}",
                "Passport" => $"Паспорта: {TotalCount}",
                _ => $"Всего записей: {TotalCount}"
            };
        }

        public IEnumerable<int> GetPageRange(int maxPages = 5)
        {
            int startPage = Math.Max(1, PageIndex - maxPages / 2);
            int endPage = Math.Min(TotalPages, startPage + maxPages - 1);

            if (endPage - startPage + 1 < maxPages)
            {
                startPage = Math.Max(1, endPage - maxPages + 1);
            }

            return Enumerable.Range(startPage, endPage - startPage + 1);
        }
    }
}
