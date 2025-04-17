using DisputeReconciliation.App.Data;
using DisputeReconciliation.App.Services;
using DisputeReconciliation.Core;
using DisputeReconciliation.Parsers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(opt => 
    opt.UseInMemoryDatabase("InMemoryDb"));
builder.Services.AddScoped<DisputeDAO>();
builder.Services.AddScoped<ExchangeRateService>();
builder.Services.AddScoped<DisputeFileParser>();
builder.Services.AddScoped<DisputeService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Seed Mock Data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!dbContext.Disputes.Any())
    {
        dbContext.Disputes.AddRange(new List<Dispute>
        {
            new Dispute { DisputeId = "case_001", TransactionId = "txn_001", Amount = 100.00m, Currency = "USD", Status = "Open", Reason = "Fraud" },
            new Dispute { DisputeId = "case_002", TransactionId = "txn_005", Amount = 150.00m, Currency = "USD", Status = "Lost", Reason = "Product Not Received" },
            new Dispute { DisputeId = "case_004", TransactionId = "txn_007", Amount = 90.00m, Currency = "USD", Status = "Open", Reason = "Unauthorized" },
        });
        dbContext.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
