namespace MarketingPlatform.Application.Interfaces
{
    public interface IReportExportService
    {
        Task<byte[]> ExportToCsvAsync<T>(List<T> data) where T : class;
        Task<byte[]> ExportToExcelAsync<T>(List<T> data, string sheetName) where T : class;
    }
}
