using Azure;
using DSU23_G5.Models.Dtos;
using Newtonsoft.Json;

namespace DSU23_G5.Infrastrukture
{
    public class CurrentSpotPriceApiClient
    {
        private readonly HttpClient client;
        public CurrentSpotPriceApiClient()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("https://www.elprisetjustnu.se/api/");
        }

        /// <summary>
        /// Gets data from consumptions API.
        /// </summary>
        /// <typeparam name="T">The type of object to return the data</typeparam>
        /// <param name="endpoint">The endpoint to make the GET request</param> 
        /// <returns></returns>
        public async Task<T[]> GetAsync<T>(string endpoint)
        {
            try
            {
                using (var response = await client.GetAsync(endpoint))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<T[]>(responseJson);
                        if (data != null)
                        {
                         return data;
                        }
                        else
                        {
                            throw new Exception($"Error getting data from {endpoint}. Status code: {response.StatusCode}");
                        }
                    }
                    else
                    {
                        throw new Exception($"Error getting data from {endpoint}. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception($"Error getting data from {endpoint}");
            }
        }
    }
}
