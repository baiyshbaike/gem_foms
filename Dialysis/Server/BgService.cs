using Dialysis.Server.Domain.Services;
using Dialysis.Server.Domain;
using Dialysis.Shared.Constants;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Models;
using Dialysis.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Office.CustomUI;

namespace Dialysis.Server
{
    public class BgService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public BgService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var allVal = await (from u in dbContext.HDSession
                                        join up in dbContext.Patient on u.PatientId equals up.Id
                                        join c in dbContext.MedCenter on u.MedCenterId equals c.Id
                                        join cm in dbContext.MedCenterMachine on u.MachineId equals cm.Id
                                        where (u.StatusId.Equals((long)HDSessionEnum.Started) || u.StatusId.Equals((long)HDSessionEnum.Paused))
                                        select new HDSessionDto
                                        {
                                            HDSession = u,
                                            Patient = up,
                                            MedCenter = c,
                                            MedCenterMachine = cm
                                        }
                                  ).OrderByDescending(p => p.HDSession.Id).ToListAsync();

                    if (allVal != null)
                    {
                        foreach (var item in allVal)
                        {
                            item.HDSessionHours = await dbContext.HDSessionHour.Where(p => p.HDSessionId.Equals(item.HDSession.Id)).ToListAsync();
                        }
                    }
                    foreach (var session in allVal)
                    {
                        if (session.HDSession.SessionStart != null && session.HDSession.StatusId.Equals((long)HDSessionEnum.Started))
                        {
                            if ((int)(DateTime.Now - (DateTime)session.HDSession.SessionStart).TotalMinutes > 270)
                            {
                                var activeSession = await dbContext.HDSession.Where(p => p.Id.Equals(session.HDSession.Id)).FirstOrDefaultAsync();
                                if (activeSession != null)
                                {
                                    activeSession.SessionEnd = DateTime.Now;
                                    var totalHours = ((DateTime)activeSession.SessionEnd - (DateTime)activeSession.SessionStart).TotalHours;
                                    var totalMinutes = ((DateTime)activeSession.SessionEnd - (DateTime)activeSession.SessionStart).TotalMinutes;

                                    var totalPause = 0.0;
                                    var totalPauseMinutes = 0.0;

                                    var activePrice = await dbContext.ActivePrice.Where(p => p.IsDeleted.Equals(false)).OrderByDescending(p => p.CreatedOn).FirstOrDefaultAsync();
                                    if (activePrice != null) activeSession.ActivePrice = activePrice.Price;

                                    var allPauses = await dbContext.HDSessionPause.Where(p => p.HDSessionId.Equals(activeSession.Id)).ToListAsync();
                                    if (allPauses.Any())
                                    {
                                        foreach (var item in allPauses)
                                        {
                                            totalPause = totalPause + (((DateTime)item.PauseEnd - (DateTime)item.PauseStart).TotalHours);
                                            totalPauseMinutes = totalPauseMinutes + (((DateTime)item.PauseEnd - (DateTime)item.PauseStart).TotalMinutes);
                                        }
                                    }
                                    if (totalPause > 0)
                                    {
                                        totalHours = totalHours - totalPause;
                                        totalMinutes = totalMinutes - totalPauseMinutes;
                                    }
                                    activeSession.TotalHours = MinutsTohHours((int)totalMinutes);
                                    activeSession.TotalMinutes = totalMinutes;
                                    activeSession.LastModifiedOn = DateTime.Now;
                                    activeSession.StatusId = (long)HDSessionEnum.Finished;
                                    Console.WriteLine(activeSession.Inn);
                                    dbContext.Attach(activeSession);
                                    await dbContext.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }
                await Task.Delay(60000);
            }
        }
        private double MinutsTohHours(int totalMinutes)
        {
            double totalHours = 0;
            totalHours = ((double)(totalMinutes % 60) / 100);
            return Math.Floor((double)totalMinutes / 60) + totalHours;
        }
    }
}
