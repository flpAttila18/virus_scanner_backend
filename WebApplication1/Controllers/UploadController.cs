using Cloudmersive.APIClient.NETCore.VirusScan.Api;
using Cloudmersive.APIClient.NETCore.VirusScan.Client; // Új using az új konfigurációhoz
using Cloudmersive.APIClient.NETCore.VirusScan.Model;
using Microsoft.AspNetCore.Authorization;
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
        private readonly AppDbContext _context;




        // AZ ÚJ KONSTRUKTOR: Ez adja át az appsettings.json-ből a kulcsot a Cloudmersive-nek
        public UploadController(IConfiguration configuration , AppDbContext context)
        {
            _context = context;
            var config = new Configuration();
            var apiKey = configuration["Cloudmersive:ApiKey"];

            config.ApiKey.Add("Apikey", apiKey);

            _scanApi = new ScanApi(config);
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> uploadAndScan(IFormFile file)
        {

            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return StatusCode(401, new { error = "kérlek jelentkezz be a szolgáltatás használatáért" });
            }

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

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var dbresult = new Viruses
                {
                    FileName = file.FileName,
                    userId = int.Parse(userIdClaim),
                    VirusName = "",
                    virusType = ""
                };


                if (cloudmersiveResult != null && cloudmersiveResult.CleanResult == true)
                {
                    response.scanResult.IsClean = true;
                    response.Message = "A file sikeresen ellenőrizve , tiszta";
                }
                // Javítva: Kivettem a felesleges zárójelet a sor végéről
                else if (cloudmersiveResult != null && cloudmersiveResult.FoundViruses != null && cloudmersiveResult.FoundViruses.Count > 0)
                {
                    var virusNames = cloudmersiveResult.FoundViruses[0];
                    dbresult.VirusName = cloudmersiveResult.FoundViruses[0].VirusName;
                    dbresult.virusType = "Malware";
                    foreach (var virus in cloudmersiveResult.FoundViruses)
                    {
                        response.scanResult.FoundViruses.Add(new VirusDetails
                        {
                            VirusName = virus.VirusName
                        });
                    }
                }
                _context.viruses.Add(dbresult);
                await _context.SaveChangesAsync();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"szerver hiba {ex.Message}" });
            }
        }
    }
}