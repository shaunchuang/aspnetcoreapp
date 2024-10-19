using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using aspnetcoreapp.Converter;

namespace aspnetcoreapp.Pages
{
    public class SearchModel : PageModel
    {
        private readonly ILogger<SearchModel> _logger;
        private readonly HttpClient _httpClient;

        public SearchModel(ILogger<SearchModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        [BindProperty(SupportsGet = true)]
        public string CompanyName { get; set; } // 廠商名稱

        [BindProperty(SupportsGet = true)]
        public string ManagerName { get; set; } // 負責人姓名

        [BindProperty(SupportsGet = true)]
        public string BusinessScope { get; set; } // 執業範圍

        [BindProperty(SupportsGet = true)]
        public string Address { get; set; } // 營業地址

        public List<IriDataItem> IriData { get; set; }

        public async Task OnGet()
        {
            var requestUri = "https://cloudbm.nlma.gov.tw/EIX/RSAPI/V1/opendata/iri?start=1";
            if (!string.IsNullOrEmpty(CompanyName))
            {
                requestUri += $"&compname={CompanyName}";
            }
            // 發送 API 請求
            var response = await _httpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("JSON 回應: {JsonResponse}", jsonResponse); // 用於調試

                // 解析 JSON 結果
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponse>(jsonResponse, options);

                    if (result != null && result.IriData != null)
                    {
                        // 根據每個欄位進行篩選
                        IriData = result.IriData
                            .Where(item =>
                                (string.IsNullOrEmpty(ManagerName) || item.ManagerName.Contains(ManagerName)) &&
                                (string.IsNullOrEmpty(BusinessScope) || item.BusinessScope.Contains(BusinessScope)) &&
                                (string.IsNullOrEmpty(Address) || item.Address.Contains(Address))
                            ).ToList();
                    }
                    else
                    {
                        IriData = new List<IriDataItem>();
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON 反序列化失敗。");
                    IriData = new List<IriDataItem>();
                }
            }
            else
            {
                _logger.LogError("API 請求失敗，狀態碼: {StatusCode}", response.StatusCode);
                IriData = new List<IriDataItem>();
            }
        }
    }

    // 定義用來映射 API 資料的類別
    public class ApiResponse
    {
        [JsonPropertyName("IriData")]
        [JsonConverter(typeof(SingleOrArrayConverter<IriDataItem>))]
        public List<IriDataItem> IriData { get; set; }
    }

    public class IriDataItem
    {
        [JsonPropertyName("申請項目")]
        public string ApplicationType { get; set; }

        [JsonPropertyName("廠商名稱")]
        public string CompanyName { get; set; }

        [JsonPropertyName("登記證書字號")]
        public string RegistrationNumber { get; set; }

        [JsonPropertyName("執業範圍")]
        public string BusinessScope { get; set; }

        [JsonPropertyName("統一編號")]
        public string UnifiedNumber { get; set; }

        [JsonPropertyName("有效期限")]
        public string ExpiryDate { get; set; }

        [JsonPropertyName("負責人姓名")]
        public string ManagerName { get; set; }

        [JsonPropertyName("營業地址")]
        public string Address { get; set; }
    }
}
