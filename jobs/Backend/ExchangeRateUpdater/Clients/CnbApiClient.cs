﻿using ExchangeRateUpdater.Configuration;
using ExchangeRateUpdater.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ExchangeRateUpdater.Clients
{
    public class CnbApiClient : ICnbApiClient
    {
        private readonly CnbApiClientConfiguration _configuration;
        private readonly HttpClient _client;
        private readonly ILogger<CnbApiClient> _logger;

        public CnbApiClient(CnbApiClientConfiguration configuration, HttpClient client, ILogger<CnbApiClient> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _client.BaseAddress = new Uri(_configuration.ApiBaseUri);
            _logger = logger;
        }

        public async Task<GetExchangeRatesResponseDto> GetDailyExchangeRates(DateTime? date, string? lang)
        {
            try
            {
                var httpResponseMessage = await _client.GetAsync("exrates/daily");

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"CNB Api returned Status Code: {httpResponseMessage.StatusCode}");

                    throw new Exception("Request did not return success status code.");
                }

                var response = await httpResponseMessage.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<GetExchangeRatesResponseDto>(response);

                _logger.LogInformation($"CNB Api exrates request successful");

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "A HttpRequestException occurred to the CNB API");
                throw;
            }
        }
    }

    public class GetExchangeRatesResponseDto
    {
        public IEnumerable<ExchangeRateResponseDto> Rates { get; set; }
    }

    public class ExchangeRateResponseDto
    {
        public string ValidFor { get; set; }
        public int Order { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public int Amount { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Rate { get; set; }
    }
}
