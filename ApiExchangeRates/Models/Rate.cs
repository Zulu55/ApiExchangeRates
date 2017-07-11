namespace ApiExchangeRates.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Rate
    {
        [Key]
        public int RateId { get; set; }

        public string Code { get; set; }

        public double TaxRate { get; set; }

        public string Name { get; set; }
    }
}