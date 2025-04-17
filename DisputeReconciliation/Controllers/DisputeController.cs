using DisputeReconciliation.Core;
using Microsoft.AspNetCore.Mvc;

namespace DisputeReconciliation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisputeController : ControllerBase
    {
        private readonly DisputeService _service;
        private readonly ILogger<DisputeController> _logger;

        public DisputeController(DisputeService service, ILogger<DisputeController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("internal")]
        public async Task<IActionResult> GetInternal([FromQuery] int page = 1)
        {
            var data = await _service.GetPagedInternalDataAsync(page, 50);
            return Ok(data);
        }

        [HttpPost("compare/file")]
        public async Task<IActionResult> CompareFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                ms.Position = 0;

                string reportName = await _service.CompareDisputeFileAsync(ms, file.FileName);
                string reportsDir = Path.Combine(AppContext.BaseDirectory, "Reports");
                string fullPath = Path.Combine(reportsDir, reportName);

                if (!System.IO.File.Exists(fullPath))
                    return NotFound("Report not found");

                byte[] bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                return File(bytes, "text/plain", reportName);
            }
        }

        [HttpPost("compare/json")]
        public async Task<IActionResult> CompareJson([FromBody] List<Dispute> disputes)
        {
            if (disputes == null || !disputes.Any())
                return BadRequest("No disputes in JSON.");

            string reportName = await _service.CompareDisputesAsync(disputes);
            string reportsDir = Path.Combine(AppContext.BaseDirectory, "Reports");
            string fullPath = Path.Combine(reportsDir, reportName);

            if (!System.IO.File.Exists(fullPath))
                return NotFound("Report not found");

            byte[] bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, "text/plain", reportName);
        }
    }
}
