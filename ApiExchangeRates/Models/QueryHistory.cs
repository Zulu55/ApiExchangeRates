namespace ApiExchangeRates.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class QueryHistory
    {
        [Key]
        public int QueryHistoryId { get; set; }

        public DateTime DateTime { get; set; }
    }
}