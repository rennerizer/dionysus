using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using WorldCities.Data;
using WorldCities.Data.Models;

namespace WorldCities.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private static bool _isRunning = false;

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SeedController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        private ActionResult Import()
        {
            if (_isRunning)
                return Ok();

            _isRunning = true;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var path = Path.Combine(
                _environment.ContentRootPath,
                "Data/Source/worldcities.xlsx");

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var excel = new ExcelPackage(stream))
                {
                    var sheet = excel.Workbook.Worksheets[0];

                    var countCountries = 0;
                    var countCities = 0;

                    var listCountries = _context.Countries.ToList();

                    // Import Countries
                    for (int indexRow = 2; indexRow <= sheet.Dimension.End.Row; indexRow++)
                    {
                        var row = sheet.Cells[indexRow, 1, indexRow, sheet.Dimension.End.Column];

                        var name = row[indexRow, 5].GetValue<string>();

                        // Create the country if it doesn't exist
                        if (listCountries.Where(c => c.Name == name).Count() == 0)
                        {
                            var country = new Country();
                            country.Name = name;
                            country.ISO2 = row[indexRow, 6].GetValue<string>();
                            country.ISO3 = row[indexRow, 7].GetValue<string>();

                            _context.Countries.Add(country);

                            _context.SaveChanges();

                            listCountries.Add(country);

                            countCountries++;
                        }
                    }

                    // Import Cities
                    for (int indexRow = 2; indexRow <= sheet.Dimension.End.Row; indexRow++)
                    {
                        var row = sheet.Cells[indexRow, 1, indexRow, sheet.Dimension.End.Column];

                        var city = new City();
                        city.Name = row[indexRow, 1].GetValue<string>();
                        city.Name_ASCII = row[indexRow, 2].GetValue<string>();
                        city.Lat = row[indexRow, 3].GetValue<decimal>();
                        city.Lon = row[indexRow, 4].GetValue<decimal>();

                        var countryName = row[indexRow, 5].GetValue<string>();

                        var country = listCountries
                            .Where(c => c.Name == countryName)
                            .FirstOrDefault();

                        city.CountryId = country.Id;

                        _context.Cities.Add(city);

                        _context.SaveChanges();

                        countCities++;
                    }

                    return new JsonResult(new
                    {
                        Cities = countCities,
                        Countries = countCountries
                    });
                }
            }
        }
    }
}