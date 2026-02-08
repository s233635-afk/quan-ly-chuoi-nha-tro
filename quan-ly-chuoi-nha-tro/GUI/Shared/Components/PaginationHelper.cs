using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Helper class for data pagination
    /// Supports DataTable and IEnumerable collections
    /// </summary>
    public class PaginationHelper<T>
    {
        public int PageSize { get; set; }
        public int CurrentPage { get; private set; }
        public int TotalRecords { get; private set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 1;
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        private IEnumerable<T> _allData;

        public PaginationHelper(int pageSize = 50)
        {
            PageSize = pageSize;
            CurrentPage = 1;
        }

        /// <summary>
        /// Set the data source
        /// </summary>
        public void SetData(IEnumerable<T> data)
        {
            _allData = data ?? Enumerable.Empty<T>();
            TotalRecords = _allData.Count();
            CurrentPage = 1;
        }

        /// <summary>
        /// Get current page data
        /// </summary>
        public List<T> GetCurrentPageData()
        {
            if (_allData == null) return new List<T>();
            if (PageSize <= 0) return _allData.ToList();

            return _allData
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        /// <summary>
        /// Go to specific page
        /// </summary>
        public bool GoToPage(int page)
        {
            if (page < 1 || page > TotalPages) return false;
            CurrentPage = page;
            return true;
        }

        /// <summary>
        /// Go to next page
        /// </summary>
        public bool NextPage()
        {
            return GoToPage(CurrentPage + 1);
        }

        /// <summary>
        /// Go to previous page
        /// </summary>
        public bool PreviousPage()
        {
            return GoToPage(CurrentPage - 1);
        }

        /// <summary>
        /// Go to first page
        /// </summary>
        public void FirstPage()
        {
            CurrentPage = 1;
        }

        /// <summary>
        /// Go to last page
        /// </summary>
        public void LastPage()
        {
            CurrentPage = TotalPages > 0 ? TotalPages : 1;
        }

        /// <summary>
        /// Get page info string
        /// </summary>
        public string GetPageInfo()
        {
            if (TotalRecords == 0) return "Không có dữ liệu";
            
            int start = (CurrentPage - 1) * PageSize + 1;
            int end = Math.Min(CurrentPage * PageSize, TotalRecords);
            return $"Hiển thị {start}-{end} / {TotalRecords} bản ghi";
        }
    }

    /// <summary>
    /// DataTable-specific pagination helper
    /// </summary>
    public class DataTablePaginator
    {
        public int PageSize { get; set; }
        public int CurrentPage { get; private set; }
        public int TotalRecords { get; private set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 1;
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        private DataTable _sourceTable;

        public DataTablePaginator(int pageSize = 50)
        {
            PageSize = pageSize;
            CurrentPage = 1;
        }

        /// <summary>
        /// Set the data source
        /// </summary>
        public void SetData(DataTable table)
        {
            _sourceTable = table;
            TotalRecords = table?.Rows.Count ?? 0;
            CurrentPage = 1;
        }

        /// <summary>
        /// Get current page as DataTable
        /// </summary>
        public DataTable GetCurrentPageData()
        {
            if (_sourceTable == null) return new DataTable();
            if (PageSize <= 0) return _sourceTable.Copy();

            var result = _sourceTable.Clone();
            int start = (CurrentPage - 1) * PageSize;
            int count = Math.Min(PageSize, TotalRecords - start);

            for (int i = start; i < start + count && i < _sourceTable.Rows.Count; i++)
            {
                result.ImportRow(_sourceTable.Rows[i]);
            }

            return result;
        }

        /// <summary>
        /// Go to specific page
        /// </summary>
        public bool GoToPage(int page)
        {
            if (page < 1 || page > TotalPages) return false;
            CurrentPage = page;
            return true;
        }

        public bool NextPage() => GoToPage(CurrentPage + 1);
        public bool PreviousPage() => GoToPage(CurrentPage - 1);
        public void FirstPage() => CurrentPage = 1;
        public void LastPage() => CurrentPage = TotalPages > 0 ? TotalPages : 1;

        public string GetPageInfo()
        {
            if (TotalRecords == 0) return "Không có dữ liệu";
            int start = (CurrentPage - 1) * PageSize + 1;
            int end = Math.Min(CurrentPage * PageSize, TotalRecords);
            return $"Hiển thị {start}-{end} / {TotalRecords} bản ghi";
        }
    }
}
