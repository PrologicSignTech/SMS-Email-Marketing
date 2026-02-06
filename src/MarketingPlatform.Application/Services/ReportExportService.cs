using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;
using System.Globalization;
using System.Text;
using MarketingPlatform.Application.Interfaces;

namespace MarketingPlatform.Application.Services
{
    public class ReportExportService : IReportExportService
    {
        public async Task<byte[]> ExportToCsvAsync<T>(List<T> data) where T : class
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });

            await csvWriter.WriteRecordsAsync(data);
            await streamWriter.FlushAsync();
            
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ExportToExcelAsync<T>(List<T> data, string sheetName) where T : class
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            // Get properties
            var properties = typeof(T).GetProperties();

            if (data.Count == 0 || properties.Length == 0)
                return package.GetAsByteArray();

            // Add headers
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = properties[i].Name;
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Add data
            for (int row = 0; row < data.Count; row++)
            {
                var item = data[row];
                for (int col = 0; col < properties.Length; col++)
                {
                    var value = properties[col].GetValue(item);
                    worksheet.Cells[row + 2, col + 1].Value = value;
                }
            }

            // Auto-fit columns
            if (worksheet.Dimension != null)
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return await Task.FromResult(package.GetAsByteArray());
        }
    }
}
