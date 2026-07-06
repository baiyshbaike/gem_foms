using System.Security.Cryptography;
using System.Text;
using Dialysis.Shared.Constants;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Responses;
using DocumentFormat.OpenXml.Math;
using Microsoft.EntityFrameworkCore;

namespace Dialysis.Server.Domain.Services;

public interface IVerifyService
{
    Task<IRetResult<bool>> GetTundukStatus();
    Task<Result<List<RequestPatientSessionDto>>> GetPatientSessions(string secretKey);
    Task<IRetResult<bool>> SetTundukStatus(bool status);
}

public class VerifyService : IVerifyService
{
    private readonly AppDbContext _dbContext;
    public VerifyService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IRetResult<bool>> GetTundukStatus()
    {
        var tundukStatus = await _dbContext.IdentifyTunduks.FirstOrDefaultAsync();
        return await Result<bool>.SuccessAsync(tundukStatus?.Status ?? false);
    }

    public async Task<IRetResult<bool>> SetTundukStatus(bool status)
    {
        var tundukStatus = await _dbContext.IdentifyTunduks.FirstOrDefaultAsync();
        if (tundukStatus != null)
        {
            tundukStatus.Status = status;
            await _dbContext.SaveChangesAsync();
            return await Result<bool>.SuccessAsync(true);
        }
        return await Result<bool>.SuccessAsync(false);
    }

    public async Task<Result<List<RequestPatientSessionDto>>> GetPatientSessions(string secretKey)
    {
        var allVal = await (from h in _dbContext.HDSession
                join c in _dbContext.MedCenter on h.MedCenterId equals c.Id
                where h.StatusId == (long)HDSessionEnum.Finished || h.StatusId == (long)HDSessionEnum.SendToPay|| h.StatusId == (long)HDSessionEnum.Payed
                select new RequestPatientSessionDto
                {
                    Inn = h.Inn,
                    MedCenterTitle = c.Title,
                    Date = h.CreatedOn.Value.Date,
                    StartDateTime = h.SessionStart,
                    EndDateTime = h.SessionEnd,
                }
            ).OrderByDescending(p => p.Date).Take(100).ToListAsync();
        
        // INN alanlarını hash'le
        foreach (var item in allVal)
        {
            item.Inn = HashInn(item.Inn, secretKey);
        }
        
        return await Result<List<RequestPatientSessionDto>>.SuccessAsync(allVal);
    }
    
    private string HashInn(string inn, string secretKey)
    {
        if (string.IsNullOrEmpty(inn))
            return string.Empty;
            
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(inn));
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}