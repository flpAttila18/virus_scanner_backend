using Cloudmersive.APIClient.NETCore.VirusScan.Api;
using Cloudmersive.APIClient.NETCore.VirusScan.Client; // Új using az új konfigurációhoz
using Cloudmersive.APIClient.NETCore.VirusScan.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.models; // Javítva nagy M-re, ha a mappa is nagybetűs

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api")]
    public class UploadController : ControllerBase
    {
        private readonly ScanApi _scanApi;

        // AZ ÚJ KONSTRUKTOR: Ez adja át az appsettings.json-ből a kulcsot a Cloudmersive-nek
        public UploadController(IConfiguration configuration)
        {
            var config = new Configuration();
            var apiKey = configuration["Cloudmersive:ApiKey"];

            config.ApiKey.Add("Apikey", apiKey);

            _scanApi = new ScanApi(config);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> uploadAndScan(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest(new { error = "Nem érkezett érvényes file a szerverre." });
            }

            try
            {
                VirusScanResult cloudmersiveResult;

                using (var stream = file.OpenReadStream())
                {
                    cloudmersiveResult = await Task.Run(() => _scanApi.ScanFile(stream));
                }

                var response = new ScanResponse
                {
                    scanResult = new scanresultDto
                    {
                        IsClean = false,
                        FoundViruses = new List<VirusDetails>()
                    },
                    Message = "Figyelem! A rendszer veszélyes filet észlelt vagy hiba történt."
                };

                if (cloudmersiveResult != null && cloudmersiveResult.CleanResult == true)
                {
                    response.scanResult.IsClean = true;
                    response.Message = "A file sikeresen ellenőrizve , tiszta";
                }
                // Javítva: Kivettem a felesleges zárójelet a sor végéről
                else if (cloudmersiveResult != null && cloudmersiveResult.FoundViruses != null && cloudmersiveResult.FoundViruses.Count > 0)
                {
                    foreach (var virus in cloudmersiveResult.FoundViruses)
                    {
                        response.scanResult.FoundViruses.Add(new VirusDetails
                        {
                            VirusName = virus.VirusName
                        });
                    }
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"szerver hiba {ex.Message}" });
            }
        }
    }
}