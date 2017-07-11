namespace ApiExchangeRates.Controllers
{
    using Models;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class RatesController : ApiController
    {
        #region Attributes
        DataContext db;
        ApiService apiService;
        #endregion

        #region Constructor
        public RatesController()
        {
            db = new DataContext();
            apiService = new ApiService();
        }
        #endregion

        public async Task<IHttpActionResult> GetRates()
        {
            var queryHistory = await db.QueryHistories.FirstOrDefaultAsync();
            Response response = null;
            if (queryHistory == null)
            {
                response = await GetRatesFromApi();
            }
            else
            {
                if ((DateTime.Now - queryHistory.DateTime).TotalMinutes > 120)
                {
                    response = await GetRatesFromApi();
                }
                else
                {
                    return Ok(db.Rates);
                }
            }

            if (response.IsSuccess)
            {
                return Ok((List<Rate>)response.Result);
            }

            return BadRequest(response.Message);
        }

        async Task<Response> GetRatesFromApi()
        {
            var response1 = await apiService.Get<ExchangeRates>(
                "https://openexchangerates.org",
                "/api/latest.json?app_id=f490efbcd52d48ee98fd62cf33c47b9e");

            if (!response1.IsSuccess)
            {
                return response1;
            }

            var response2 = await apiService.Get<ExchangeNames>(
                "https://gist.githubusercontent.com",
                "/picodotdev/88512f73b61bc11a2da4/raw/9407514be22a2f1d569e75d6b5a58bd5f0ebbad8");

            if (!response2.IsSuccess)
            {
                return response2;
            }

            var exchangeRates = (ExchangeRates)response1.Result;
            var exchangeNames = (ExchangeNames)response2.Result;
            return await LoadRates(exchangeRates, exchangeNames);
        }

        async Task<Response> LoadRates(ExchangeRates exchangeRates, ExchangeNames exchangeNames)
        {
            // Get values
            var rateValues = new List<RateValue>();
            var type = typeof(Rates);
            var properties = type.GetRuntimeFields();

            foreach (var property in properties)
            {
                var code = property.Name.Substring(1, 3);
                rateValues.Add(new RateValue
                {
                    Code = code,
                    TaxRate = (double)property.GetValue(exchangeRates.Rates),
                });
            }

            // Get names
            var rateNames = new List<RateName>();
            type = typeof(ExchangeNames);
            properties = type.GetRuntimeFields();

            foreach (var property in properties)
            {
                var code = property.Name.Substring(1, 3);
                rateNames.Add(new RateName
                {
                    Code = code,
                    Name = (string)property.GetValue(exchangeNames),
                });
            }

            // Join the complete list
            var qry = (from v in rateValues
                       join n in rateNames on v.Code equals n.Code
                       select new { v, n }).ToList();

            var rates = new List<Rate>();
            foreach (var item in qry)
            {
                rates.Add(new Rate
                {
                    Code = item.v.Code,
                    Name = item.n.Name,
                    TaxRate = item.v.TaxRate,
                });
            }

            await SaveData(rates);

            return new Response
            {
                IsSuccess = true,
                Message = "Ok",
                Result = rates,
            };
        }

        async Task SaveData(List<Rate> rates)
        {
            var queryHistory = await db.QueryHistories.FirstOrDefaultAsync();
            if (queryHistory == null)
            {
                queryHistory = new QueryHistory
                {
                    DateTime = DateTime.Now,
                };

                db.QueryHistories.Add(queryHistory);
            }
            else
            {
                queryHistory.DateTime = DateTime.Now;
                db.Entry(queryHistory).State = EntityState.Modified;
            }

            await db.Database.ExecuteSqlCommandAsync("TRUNCATE TABLE Rates");
            db.Rates.AddRange(rates);
            await db.SaveChangesAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}