using System.Security.Claims;
using Dialysis.Shared.Params;
using Dialysis.Shared.Responses;
using Dialysis.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Constants;
using Dialysis.Shared.Dto.GridPatients;
using Dialysis.Shared.Models.Files;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Dialysis.Server.Domain.Services
{
    public interface IPatientService
    {
        Task<PagedResponseDto<PatientInfoDto>> GetPatientsPagedAsync(
            DateTime? createdOnDateFrom,
            DateTime? createdOnDateTo,
            DateTime? birthDateFrom,
            DateTime? birthDateTo,
            int pageNumber, 
            int pageSize,
            int statusId, 
            long medCenterId,
            string sortBy, 
            bool sortAsc, 
            string searchString);

        Task<FilesResponseDto<MedCenterPatientFile>> GetPatientFiles(long patientId);
        Task<MedCenterPatientFile> GetPatientFile(long fileId);
        Task<IRetResult> DeletePatientFile(long fileId);
        Task<IRetResult> AddEditMedCenterPatient(PatientAddEditDto model);
        Task<IRetResult> DeleteMedCenterPatient(Patient model);
        Task<IRetResult> AddFile(IList<SaveFile> files);
        Task<Result<List<SaveFile>>> GetFiles(long id);
        Task<IRetResult> DeleteProtocolFile(ProtocolFile model);
        Task<IRetResult> AddProtocolFile(IList<ProtocolFile> files);
        Task<Result<List<ProtocolFile>>> GetProtocolFiles(long id);
        Task<Result<List<ProtocolFile>>> GetProtocolFilesByInn(string inn);
        Task<IRetResult> DeleteFile(SaveFile model);
        Task<IRetResult> AddPatient(Patient model);
        Task<IRetResult> DeletePatient(Patient model);
        Task<Result<List<Patient>>> AllPatients();
        Task<Result<List<PatientGridDto>>> AllPatientsWithReason();
        Task<Result<List<Patient>>> AllPatients2(DateTime? fromDate, DateTime? toDate);
        Task<Result<List<PatientGridDto>>> AllPatientsWithReason2(DateTime? fromDate, DateTime? toDate, DateTime? deletedFrom = null, DateTime? deletedTo = null);
        Task<Result<List<PatientBoxDto>>> AllPatientsBox();
        Task<IRetResult> UpdatePatient(Patient model);
        Task<IRetResult> AddGroupAct(GroupProtocolArgs model);

        Task<Result<List<PatientGroupActDto>>> AllGroupActs(DateTime? fromDate, DateTime? toDate);
        Task<Result<PatientGroupActDto>> GroupActsDetail(long id);
        Task<Result<Patient>> SearchByInn(string inn);
        Task<Result<PatientResultDto>> SearchResultByInn(string inn);
        Task<Result<Patient>> SearchByInnAndFio(string inn, string fio);
        Task<Result<Patient>> GetByMedCardId(long medcardId);
        Task<Result<Patient>> GetById(long id);
        Task<Result<PatientInfo>> GetByInn(string inn);
        Task<Result<List<PatientTotalMedCenterDto>>> AllPatients3(DateTime? fromDate, DateTime? toDate);
        Task<Result<List<PatientTotalHdDto>>> AllPatients4(long medCenterId, int totalDays, DateTime? fromDate, DateTime? toDate, string typePage);
        Task<Result<List<PatientTotalHdDto>>> AllPatients5(long medCenterId, DateTime? fromDate, DateTime? toDate);

        Task<Result<List<PatientChangesDto>>> AllReestrChanges();
        Task<Result<PatientChangesDto>> ReestrChangeDetail(long id);


        Task<IRetResult> AddWriting(WritingOut model);
        Task<Result<List<WritingOutDto>>> SearchWritings(string? inn, long mid, DateTime? fromDate, DateTime? toDate);
        Task<Result<WritingOutDto>> GetWritingById(long id);
        Task<IRetResult> AddComplaint(Complaint model);
        Task<Result<List<ComplaintDto>>> SearchComplaint(string? inn, long mid);
        Task<Result<ComplaintDto>> GetComplaintById(long id);

    }

    public class PatientService : IPatientService
    {
        private readonly AppDbContext _dbContext;
        private readonly IActiveUserService _currentUser;
        private readonly IActionLogService _actionLogService;
        
        public PatientService(AppDbContext dbContext, IActiveUserService currentUser, IActionLogService actionLogService)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _actionLogService = actionLogService;
        }
        public async Task<FilesResponseDto<MedCenterPatientFile>> GetPatientFiles(long patientId)
        {
            var result = await _dbContext.MedCenterPatientFiles
                .Where(pf => pf.PatientId == patientId && !pf.IsDeleted)
                .Include(pf => pf.UploadedByUser) 
                .Include(pf => pf.MedCenter)  
                .OrderByDescending(pf => pf.UploadedOn)
                .ToListAsync();
            var patient = _dbContext.Patient.FirstOrDefault(p => p.Id == patientId) ?? new Patient();
            return new FilesResponseDto<MedCenterPatientFile>()
            {
                Items = result,
                MedCenter = _dbContext.MedCenter.FirstOrDefault(m => m.Id == patient.MedCenterId) ?? null,
                User = _dbContext.User.FirstOrDefault(u => u.Id == patient.CreatedBy) ?? null,
            };
        }

        public async Task<MedCenterPatientFile> GetPatientFile(long fileId)
        {
            return await _dbContext.MedCenterPatientFiles
                .FirstOrDefaultAsync(pf => pf.Id == fileId && !pf.IsDeleted) ?? new MedCenterPatientFile();
        }

        public async Task<IRetResult> DeletePatientFile(long fileId)
        {
            var file = await _dbContext.MedCenterPatientFiles.FirstOrDefaultAsync(pf => pf.Id == fileId);
            if (file == null)
                return await Result<int>.FailAsync("Файл не найден");

            file.IsDeleted = true;
    
            // Fiziksel dosyayı sil
            var filePath = Path.Combine("wwwroot", file.FilePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            await _dbContext.SaveChangesAsync();
            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Delete", "MedCenterPatientFile", fileId.ToString(), null, null, "Файл пациента удален");
            return await Result<int>.SuccessAsync();
        }
        private async Task SavePatientFiles(PatientAddEditDto model, long patientId)
        {
            var fileTypes = new Dictionary<string, string>
            {
                { "File1", "passport" },
                { "File2", "government_extract" },
                { "File3", "analysis" },
                { "File4", "application" }
            };

            foreach (var fileType in fileTypes)
            {
                var fileData = GetFileDataFromModel(model, fileType.Key);
                if (!string.IsNullOrEmpty(fileData))
                {
                    await SaveSingleFile(fileData, patientId, fileType.Value, model.FirstName, model.LastName, model.MedCenterId?? 0);
                }
            }
        }

        private string GetFileDataFromModel(PatientAddEditDto model, string propertyName)
        {
            var property = typeof(PatientAddEditDto).GetProperty(propertyName);
            return property?.GetValue(model) as string ?? string.Empty;
        }

        private async Task SaveSingleFile(string base64Data, long patientId, string fileType, string firstName, string lastName, long medCenterId)
        {
            try
            {
                var existingFile = await _dbContext.MedCenterPatientFiles
                    .Where(pf => pf.PatientId == patientId && pf.FileType == fileType && !pf.IsDeleted)
                    .FirstOrDefaultAsync();
                if (existingFile != null)
                {
                    existingFile.IsDeleted = true;
                    var oldFilePath = Path.Combine("wwwroot", existingFile.FilePath);
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }
                var fileName = $"{firstName}_{lastName}_{fileType}_{DateTime.Now:ddMMyyyyHHmmss}";
                var fileExtension = GetFileExtension(base64Data);
                var fullFileName = $"{fileName}.{fileExtension}";
                var relativePath = $"UploadFiles/MedCenterPatientFiles/{fullFileName}";
                var fullPath = Path.Combine("wwwroot", relativePath);
                var directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                var cleanBase64 = CleanBase64String(base64Data);
                var fileBytes = Convert.FromBase64String(cleanBase64);
                await File.WriteAllBytesAsync(fullPath, fileBytes);
                var medCenter = await  _dbContext.MedCenter.Where(m => m.Id.Equals(medCenterId)).FirstOrDefaultAsync(); 
                var patientFile = new MedCenterPatientFile()
                {
                    PatientId = patientId,
                    FileName = fullFileName,
                    FilePath = relativePath,
                    FileType = fileType,
                    OriginalFileName = fullFileName,
                    FileSize = fileBytes.Length,
                    UploadedOn = DateTime.Now,
                    UploadedBy = _currentUser.UserId,
                    MedCenter = medCenter ?? null,
                    UploadedByUser = await _dbContext.User.FindAsync(_currentUser.UserId),
                    IsDeleted = false
                };

                await _dbContext.MedCenterPatientFiles.AddAsync(patientFile);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error
                throw new Exception($"Произошла ошибка при сохранении файла: {ex.Message}");
            }
        }

        private string GetFileExtension(string base64String)
        {
            if (base64String.StartsWith("data:image/jpeg") || base64String.StartsWith("data:image/jpg"))
                return "jpg";
            if (base64String.StartsWith("data:image/png"))
                return "png";
            if (base64String.StartsWith("data:application/pdf"))
                return "pdf";
            if (base64String.StartsWith("data:application/msword"))
                return "doc";
            if (base64String.StartsWith("data:application/vnd.openxmlformats-officedocument.wordprocessingml.document"))
                return "docx";
            
            return "bin"; // Default extension
        }

        private string CleanBase64String(string base64String)
        {
            // Tüm data URL prefix'lerini temizle
            var prefixes = new[]
            {
                "data:image/jpeg;base64,",
                "data:image/jpg;base64,",
                "data:image/png;base64,",
                "data:application/pdf;base64,",
                "data:application/msword;base64,",
                "data:application/vnd.openxmlformats-officedocument.wordprocessingml.document;base64,"
            };

            foreach (var prefix in prefixes)
            {
                if (base64String.StartsWith(prefix))
                {
                    return base64String.Substring(prefix.Length);
                }
            }
            return base64String;
        }

        public async Task<IRetResult> DeleteMedCenterPatient(Patient model)
        {
            if (model != null)
            {

                if (model.Id != null && model.Id != 0)
                {
                    var localModel = await _dbContext.Patient.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        var existingFile = await _dbContext.MedCenterPatientFiles
                            .Where(pf => pf.PatientId == localModel.Id  && !pf.IsDeleted)
                            .ToListAsync();

                        if (existingFile != null && existingFile.Count > 0)
                        {
                            foreach (var file in existingFile)
                            {
                                file.IsDeleted = true;
                                var oldFilePath = Path.Combine("wwwroot", file.FilePath);
                                if (File.Exists(oldFilePath))
                                {
                                    File.Delete(oldFilePath);
                                }
                            }
                            
                        }
                        _dbContext.Remove(localModel);
                        await _dbContext.SaveChangesAsync();
                        if (localModel != null)
                            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Delete", "Patient", localModel.Id.ToString(), new { localModel.Inn, localModel.FirstName, localModel.LastName }, null, "Пациент удален из медцентра");
                    }
                }
            }
            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> AddEditMedCenterPatient(PatientAddEditDto model)
        {
            if(model != null)
            {
               
                    if (model.Id != null && model.Id != 0)
                    {
                        var localModel = await _dbContext.Patient.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                        if (localModel != null)
                        {
                            localModel.FirstName = model.FirstName;
                            localModel.LastName = model.LastName;
                            localModel.MiddleName = model.MiddleName;
                            localModel.BirthDate = model.BirthDate;
                            localModel.RegionId = model.RegionId;
                            localModel.DistrictId = model.DistrictId;
                            localModel.Address = model.Address;
                            localModel.Address2 = model.Address2;
                            localModel.Phone = model.Phone;
                            localModel.Phone2 = model.Phone2;
                            localModel.SpecialStatus = model.SpecialStatus;
                            localModel.LastModifiedBy = _currentUser.UserId;
                            localModel.LastModifiedOn = DateTime.Now;
                            localModel.MedCenterId = model.MedCenterId;
                            if (!String.IsNullOrEmpty(model.Image) )
                            {
                                string imageName = model.FirstName + "_" + model.LastName + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + "_change.png";
                                string imgPath = "wwwroot/UploadImages/" + imageName;
                                string base64 = model.Image;
                                base64 = base64.Replace("data:image/jpeg; base64,", "");
                                base64 = base64.Replace("data:image/jpeg;base64,", "");
                                File.WriteAllBytes(imgPath, Convert.FromBase64String(base64));
                                localModel.Image = imageName;
                            }
                            _dbContext.Attach(localModel);
                            await _dbContext.SaveChangesAsync();
                            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "Patient", localModel.Id.ToString(), null, new { localModel.FirstName, localModel.LastName }, "Пациент обновлен");
                            await SavePatientFiles(model, localModel.Id);
                        }
                    }
                    else
                    {
                        var patientOld = await _dbContext.Patient.Where(p => p.Inn.Equals(model.Inn)).FirstOrDefaultAsync();
                        if (patientOld != null)
                        {
                            return await Result<int>.FailAsync("Пациент с таким ИНН существует");
                        }
                        else
                        {
                            Patient patient = new Patient();
                            patient.Inn = model.Inn;
                            patient.FirstName = model.FirstName;
                            patient.LastName = model.LastName;
                            patient.MiddleName = model.MiddleName;
                            patient.BirthDate = model.BirthDate;
                            patient.RegionId = model.RegionId;
                            patient.DistrictId = model.DistrictId;
                            patient.Address = model.Address;
                            patient.Address2 = model.Address2;
                            patient.Phone = model.Phone;
                            patient.Phone2 = model.Phone2;
                            patient.SpecialStatus = model.SpecialStatus;
                            patient.MedCenterId = model.MedCenterId;
                            patient.GroupId = 8;
                            patient.CreatedBy = _currentUser.UserId;
                            patient.CreatedOn = DateTime.Now;
                            patient.IsDeleted = false;
                            patient.IsActive = true;
                            if (!String.IsNullOrEmpty(model.Image))
                            {
                                string imageName = model.FirstName + "_"+model.LastName + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + "_add.png";
                                string imgPath = "wwwroot/UploadImages/" + imageName;
                                string base64 = model.Image;
                                base64 = base64.Replace("data:image/jpeg; base64,", "");
                                base64 = base64.Replace("data:image/jpeg;base64,", "");
                                File.WriteAllBytes(imgPath, Convert.FromBase64String(base64));
                                patient.Image = imageName;
                            }
                            await _dbContext.AddAsync(patient);
                            await _dbContext.SaveChangesAsync();
                            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "Patient", patient.Id.ToString(), null, new { patient.Inn, patient.FirstName, patient.LastName }, "Пациент создан");
                            await SavePatientFiles(model, patient.Id);
                        }                   
                    }
            }
            return await Result<int>.SuccessAsync();
        }
        public async Task<PagedResponseDto<PatientInfoDto>> GetPatientsPagedAsync(
            DateTime? createdOnDateFrom,
            DateTime? createdOnDateTo,
            DateTime? birthDateFrom,
            DateTime? birthDateTo,
            int pageNumber, 
            int pageSize,
            int statusId,
            long medCenterId,
            string sortBy, 
            bool sortAsc, 
            string searchString)
        {
            var query = _dbContext.Patient
                .Where(p => (medCenterId == 0 || p.MedCenterId == medCenterId))
                .AsQueryable();
            // createdOn filtresi
            if (createdOnDateFrom.HasValue)
                query = query.Where(p => p.CreatedOn >= createdOnDateFrom.Value);
            if (createdOnDateTo.HasValue)
                query = query.Where(p => p.CreatedOn <= createdOnDateTo.Value);

            // birthDate filtresi
            if (birthDateFrom.HasValue)
                query = query.Where(p => p.BirthDate >= birthDateFrom.Value);
            if (birthDateTo.HasValue)
                query = query.Where(p => p.BirthDate <= birthDateTo.Value);
            
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var searchLower = searchString.ToLower();
                query = query.Where(pt =>
                    (!string.IsNullOrEmpty(pt.FirstName) && pt.FirstName.ToLower().Contains(searchLower)) || 
                    (!string.IsNullOrEmpty(pt.LastName) && pt.LastName.ToLower().Contains(searchLower)) || 
                    (!string.IsNullOrEmpty(pt.MiddleName) && pt.MiddleName.ToLower().Contains(searchLower)) || 
                    (!string.IsNullOrEmpty(pt.Inn) && pt.Inn.Contains(searchLower)));
            }
            var patientCount = await query
                .GroupBy(p => 1)
                .Select(g => new PatientCountDto
                {
                    All = g.Count(),
                    NewCount = g.Count(p => p.GroupId == 8),
                    ArchiveCount = g.Count(p => p.GroupId == 2),
                    FomsCount = g.Count(p => p.GroupId == 7),
                    FreseniusCount = g.Count(p=>p.GroupId == 6),
                    OtherCount = g.Count(p => p.GroupId != 8 && p.GroupId != 7 && p.GroupId != 6 && p.GroupId != 2),
                })
                .FirstOrDefaultAsync() ?? new PatientCountDto();
            if (statusId != 0)
            {
                query = query.Where(p => p.GroupId == statusId);
            }
            query = sortBy switch
            {
                "Id" => sortAsc ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id),
                "FirstName" => sortAsc ? query.OrderBy(p => p.FirstName) : query.OrderByDescending(p => p.FirstName),
                "BirthDate" => sortAsc ? query.OrderBy(p => p.BirthDate) : query.OrderByDescending(p => p.BirthDate),
                "Inn" => sortAsc ? query.OrderBy(p => p.Inn) : query.OrderByDescending(p => p.Inn),
                "GroupId" => sortAsc ? query.OrderBy(p => p.GroupId) : query.OrderByDescending(p => p.GroupId),
                "IsDeleted" => sortAsc ? query.OrderBy(p => p.IsDeleted) : query.OrderByDescending(p => p.IsDeleted),
                _ => query.OrderByDescending(p => p.Id) // Varsayılan sıralama
            };
            var totalItems = await query.CountAsync();
            
            // 3. Sayfalama - ID'leri al
            var patientIds = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => p.Id)
                .ToListAsync();

            // Eğer hasta bulunamadıysa erken dön
            if (!patientIds.Any())
            {
                return new PagedResponseDto<PatientInfoDto>
                {
                    Items = new List<PatientInfoDto>(),
                    TotalItems = totalItems,
                    PatientCount = patientCount
                };
            }

            // 4. Ana Veriler - Sıralamayı koru
            var patients = await _dbContext.Patient
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.Id))
                .ToListAsync();
            
            // Orijinal sıralamayı koru
            var patientsOrdered = patientIds
                .Join(patients, id => id, p => p.Id, (id, p) => p)
                .ToList();

            // 5. İlişkili Veriler (Toplu Çekim)
            var patientInns = patientsOrdered
                .Where(p => !string.IsNullOrEmpty(p.Inn))
                .Select(p => p.Inn)
                .Distinct()
                .ToList();

            // İlişkili verileri sıralı olarak çek (DbContext thread safety için)
            var medCards = await _dbContext.MedCard
                .AsNoTracking()
                .Where(mc => patientInns.Contains(mc.Inn))
                .ToListAsync();

            var hdSessions = await _dbContext.HDSession
                .AsNoTracking()
                .Where(hd => patientInns.Contains(hd.Inn))
                .ToListAsync();

            var saveFiles = await _dbContext.SaveFile
                .AsNoTracking()
                .Where(sf => sf.EntityId.HasValue && patientIds.Contains(sf.EntityId.Value))
                .ToListAsync();

            var analysisResults = await _dbContext.AnalysisResult
                .AsNoTracking()
                .Where(ar => patientInns.Contains(ar.Inn))
                .ToListAsync();

            // 6. Verileri Gruplama (Bellek İçi) - Null kontrolü ile
            var medCardsDict = medCards
                .Where(mc => !string.IsNullOrEmpty(mc.Inn))
                .GroupBy(mc => mc.Inn)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var hdSessionsDict = hdSessions
                .Where(hd => !string.IsNullOrEmpty(hd.Inn))
                .GroupBy(hd => hd.Inn)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var saveFilesDict = saveFiles
                .Where(sf => sf.EntityId.HasValue)
                .GroupBy(sf => sf.EntityId.Value)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var analysisResultsDict = analysisResults
                .Where(ar => !string.IsNullOrEmpty(ar.Inn))
                .GroupBy(ar => ar.Inn)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            // 7. DTO Oluşturma
            var patientInfoDtos = patientsOrdered.Select(patient => new PatientInfoDto
            {
                Patient = patient,
                MedCards = !string.IsNullOrEmpty(patient.Inn) && medCardsDict.TryGetValue(patient.Inn, out var mc) 
                    ? mc : Enumerable.Empty<MedCard>(),
                HdSessions = !string.IsNullOrEmpty(patient.Inn) && hdSessionsDict.TryGetValue(patient.Inn, out var hd) 
                    ? hd : Enumerable.Empty<HDSession>(),
                SaveFiles = saveFilesDict.TryGetValue(patient.Id, out var sf) 
                    ? sf : Enumerable.Empty<SaveFile>(),
                AnalysisResults = !string.IsNullOrEmpty(patient.Inn) && analysisResultsDict.TryGetValue(patient.Inn, out var ar) 
                    ? ar : Enumerable.Empty<AnalysisResult>()
            }).ToList();

            return new PagedResponseDto<PatientInfoDto>
            {
                Items = patientInfoDtos,
                TotalItems = totalItems,
                PatientCount = patientCount
            };
        }
        
        public async Task<IRetResult> AddFile(IList<SaveFile> files)
        {
            foreach (var file in files)
            {
                if (file.Id == 0)
                {
                    file.CreatedBy = _currentUser.UserId;
                    file.CreatedOn = DateTime.Now;
                    await _dbContext.SaveFile.AddAsync(file);
                }
            }
            await _dbContext.SaveChangesAsync();
            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "SaveFile", files.FirstOrDefault()?.EntityId.ToString(), null, new { Count = files.Count }, "Файлы добавлены");
            return await Result<int>.SuccessAsync();
        }

        public async Task<Result<List<SaveFile>>> GetFiles(long id)
        {
            var allFiles = (from u in _dbContext.SaveFile where (u.EntityId.Equals(id)) select u).OrderBy(p => p.Id).ToList();
            return await Result<List<SaveFile>>.SuccessAsync(allFiles);
        }

        public async Task<IRetResult> DeleteProtocolFile(ProtocolFile model)
        {
            if (model != null)
            {

                if (model.Id != null && model.Id != 0 && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.ProtocolFile.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        _dbContext.Remove(localModel);
                        await _dbContext.SaveChangesAsync();
                        if (localModel != null)
                            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Delete", "ProtocolFile", localModel.Id.ToString(), null, null, "Файл протокола удален");
                    }
                }
            }
            return await Result<int>.SuccessAsync();
        }

        public async Task<IRetResult> AddProtocolFile(IList<ProtocolFile> files)
        {
            foreach (var file in files)
            {
                if (file.Id == 0)
                {
                    file.CreatedBy = _currentUser.UserId;
                    file.CreatedOn = DateTime.Now;
                    await _dbContext.ProtocolFile.AddAsync(file);
                }
            }
            await _dbContext.SaveChangesAsync();
            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "ProtocolFile", files.FirstOrDefault()?.EntityId.ToString(), null, new { Count = files.Count }, "Файлы протокола добавлены");
            return await Result<int>.SuccessAsync();
        }

        public async Task<Result<List<ProtocolFile>>> GetProtocolFiles(long id)
        {
            var allFiles = (from u in _dbContext.ProtocolFile where (u.EntityId.Equals(id)) select u).OrderBy(p => p.Id).ToList();
            return await Result<List<ProtocolFile>>.SuccessAsync(allFiles);
        }
        public async Task<Result<List<ProtocolFile>>> GetProtocolFilesByInn(string inn)
        {
            var allFiles = (from u in _dbContext.ProtocolFile where (u.Inn.Equals(inn)) select u).OrderBy(p => p.Id).ToList();
            return await Result<List<ProtocolFile>>.SuccessAsync(allFiles);
        }

        public async Task<IRetResult> AddPatient(Patient model)
        {
            if(model != null)
            {
               
                    if (model.Id != null && model.Id != 0 && model.CreatedOn != null)
                    {
                        var localModel = await _dbContext.Patient.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                        if (localModel != null)
                        {
                            localModel.FirstName = model.FirstName;
                            localModel.LastName = model.LastName;
                            localModel.MiddleName = model.MiddleName;
                            localModel.BirthDate = model.BirthDate;
                            localModel.RegionId = model.RegionId;
                            localModel.DistrictId = model.DistrictId;
                            localModel.Address = model.Address;
                            localModel.Address2 = model.Address2;
                            localModel.Phone = model.Phone;
                            localModel.Phone2 = model.Phone2;
                            //localModel.StatusId = model.StatusId;
                            localModel.LastModifiedBy = _currentUser.UserId;
                            localModel.LastModifiedOn = DateTime.Now;
                            if (!String.IsNullOrEmpty(model.Image) )
                            {
                            string img_name = model.FirstName + "_" + model.LastName + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + "_change.png";
                            //var imgPath = Path.GetFullPath("wwwroot\\UploadImages\\")+""+img_name;
                            string imgPath = "wwwroot/UploadImages/" + img_name;
                            string base64 = model.Image;
                            base64 = base64.Replace("data:image/jpeg; base64,", "");
                            base64 = base64.Replace("data:image/jpeg;base64,", "");
                            File.WriteAllBytes(imgPath, Convert.FromBase64String(base64));
                            localModel.Image = img_name;
                            }
                            _dbContext.Attach(localModel);
                            await _dbContext.SaveChangesAsync();
                            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "Patient", localModel.Id.ToString(), null, new { localModel.FirstName, localModel.LastName }, "Пациент обновлен");
                        }
                    }
                    else
                    {
                        var patientOld = await _dbContext.Patient.Where(p => p.Inn.Equals(model.Inn)).FirstOrDefaultAsync();
                        if (patientOld != null)
                        {
                            return await Result<int>.FailAsync("Пациент с таким ИНН существует");
                        }
                        else
                        {
                            model.GroupId = 8;
                            model.CreatedBy = _currentUser.UserId;
                            model.CreatedOn = DateTime.Now;
                            model.IsDeleted = false;
                            if (!String.IsNullOrEmpty(model.Image))
                            {
                                string img_name = model.FirstName + "_"+model.LastName + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + "_add.png";
                                //var imgPath = Path.GetFullPath("wwwroot\\UploadImages\\")+""+img_name;
                                string imgPath = "wwwroot/UploadImages/" + img_name;
                                string base64 = model.Image;
                                base64 = base64.Replace("data:image/jpeg; base64,", "");
                                base64 = base64.Replace("data:image/jpeg;base64,", "");
                                File.WriteAllBytes(imgPath, Convert.FromBase64String(base64));
                                model.Image = img_name;
                            }
                            await _dbContext.AddAsync(model);
                            await _dbContext.SaveChangesAsync();
                            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "Patient", model.Id.ToString(), null, new { model.Inn, model.FirstName, model.LastName }, "Пациент добавлен");
                        }                   
                    }
            }
            return await Result<int>.SuccessAsync();          
        }

        public async Task<IRetResult> DeletePatient(Patient model)
        {
            if (model != null)
            {

                if (model.Id != null && model.Id != 0 && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.Patient.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        _dbContext.Remove(localModel);
                        await _dbContext.SaveChangesAsync();
                        if (localModel != null)
                            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Delete", "Patient", localModel.Id.ToString(), new { localModel.Inn, localModel.FirstName, localModel.LastName }, null, "Пациент удален");
                    }
                }
            }
            return await Result<int>.SuccessAsync();     
        }
        public async Task<IRetResult> DeleteFile(SaveFile model)
        {
            if (model != null)
            {

                if (model.Id != null && model.Id != 0 && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.SaveFile.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        _dbContext.Remove(localModel);
                        await _dbContext.SaveChangesAsync();
                        if (localModel != null)
                            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Delete", "SaveFile", localModel.Id.ToString(), null, null, "Файл удален");
                    }
                }
            }
            return await Result<int>.SuccessAsync();     
        }
        public async Task<IRetResult> UpdatePatient(Patient model)
        {
            var patientUp = await _dbContext.Patient.Where(p => p.Id.Equals(model.Id)).FirstOrDefaultAsync();
           
            if(patientUp != null)
            {
                patientUp = model;
                patientUp.LastModifiedOn = DateTime.Now;
                patientUp.LastModifiedBy = _currentUser.UserId;
                _dbContext.Attach(patientUp);
                await _dbContext.SaveChangesAsync();
                await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "Patient", patientUp.Id.ToString(), null, new { patientUp.FirstName, patientUp.LastName }, "Пациент изменен");
            }
            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> AddGroupAct(GroupProtocolArgs model)
        {
            if (model != null)
            {
                model.PatientGroupTitle.CreatedBy = _currentUser.UserId;
            }
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var protocolTitle = model.PatientGroupTitle;
                var addnew = await _dbContext.AddAsync(protocolTitle);
                await _dbContext.SaveChangesAsync();

                foreach (var item in model.PatientHistory)
                {
                    var patientHistory = new PatientHistory
                    {
                        GroupTitleId = addnew.Entity.Id,
                        CreatedBy = _currentUser.UserId,
                        CreatedOn = model.PatientGroupTitle.CreatedOn,
                        PatientId = item.PatientId,
                        GroupFromId = item.GroupFromId,
                        GroupId = item.GroupId,
                        GroupLPUId = item.GroupLPUId,
                        GroupReasonId = item.GroupReasonId,
                    };
                    var addAct = await _dbContext.AddAsync(patientHistory);
                    await _dbContext.SaveChangesAsync();

                    var patientSel = await _dbContext.Patient.Where(p => p.Id.Equals(item.PatientId)).FirstOrDefaultAsync();
                    patientSel.GroupId = item.GroupId;
                    patientSel.GroupActId = addAct.Entity.Id;
                    if (item.GroupId == 2)
                    {
                        patientSel.IsActive = false;
                        patientSel.IsDeleted = true;
                    }
                    else
                    {
                        patientSel.IsDeleted = false;
                    }
                    //var updateUser = await _dbContext.AddAsync(patientSel);
                    _dbContext.Attach(patientSel);
                    await _dbContext.SaveChangesAsync();
                }

                foreach (var perItem in model.PatientGroupPerson)
                {
                    var patientPerson = new PatientGroupPerson
                    {
                        GroupTitleId = addnew.Entity.Id,
                        CreatedBy = _currentUser.UserId,
                        CreatedOn = model.PatientGroupTitle.CreatedOn,
                        PersonFio = perItem.PersonFio,
                        PersonTitleId = perItem.PersonTitleId,
                    };
                    var addAct = await _dbContext.AddAsync(patientPerson);
                    await _dbContext.SaveChangesAsync();
                }


                transaction.Commit();
                await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "PatientGroupAct", addnew.Entity.Id.ToString(), null, new { PatientCount = model.PatientHistory?.Count }, "Групповой акт создан");
                return await Result<int>.SuccessAsync();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return await Result<int>.FailAsync("Transaction Error");
            }
            //var addnew = await _dbContext.AddAsync(model);
            //await _dbContext.SaveChangesAsync();
            //return await Result<int>.SuccessAsync();
        }
        public async Task<Result<List<PatientGroupActDto>>> AllGroupActs(DateTime? fromDate, DateTime? toDate)
        {
            List<PatientGroupActDto> retList = new List<PatientGroupActDto>();
            var allGroupTitles = await _dbContext.PatientGroupTitle
                .Where(x => (!fromDate.HasValue || x.CreatedOn >= fromDate) && 
                            (!toDate.HasValue || x.CreatedOn <= toDate))
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            if (allGroupTitles != null)
            {
                foreach (var title in allGroupTitles)
                {
                    var addItem = new PatientGroupActDto();
                    addItem.PatientGroupTitle = title;

                    var allPatientActs = await (from u in _dbContext.PatientHistory
                                                join up in _dbContext.Patient on u.PatientId equals up.Id
                                                join fgr in _dbContext.PatientGroup on u.GroupFromId equals fgr.Id
                                                join tgr in _dbContext.PatientGroup on u.GroupId equals tgr.Id
                                                join inRe in _dbContext.GroupReason on u.GroupReasonId equals inRe.Id into inReasons
                                                from re in inReasons.DefaultIfEmpty()
                                                join inLpu in _dbContext.GroupLPU on u.GroupLPUId equals inLpu.Id into inLPUs
                                                from lpu in inLPUs.DefaultIfEmpty()
                                                where u.GroupTitleId.Equals(title.Id)
                                                select new GroupActWithPatient
                                                {
                                                    Id = u.Id,
                                                    GroupTitleId = u.GroupTitleId,
                                                    CreatedOn = u.CreatedOn,
                                                    GroupId = u.GroupId,
                                                    GroupText = tgr.Title,
                                                    GroupFromId = u.GroupFromId,
                                                    GroupFromText = fgr.Title,
                                                    PatientId = up.Id,
                                                    Inn = up.Inn,
                                                    FirstName = up.FirstName,
                                                    LastName = up.LastName,
                                                    MiddleName = up.MiddleName,
                                                    GroupReasonId = u.GroupReasonId,
                                                    GroupLPUId = u.GroupLPUId,
                                                    ReasonText = re.Title,
                                                    LPUText = lpu.Title,
                                                }
                          ).OrderBy(p=>p.Id).ToListAsync();
                    addItem.PatientHistory = allPatientActs;

                    var allComissions = await (from u in _dbContext.PatientGroupAct
                                               join up in _dbContext.GroupPersonTitle on u.PersonTitleId equals up.Id                                               
                                               where u.GroupTitleId.Equals(title.Id)
                                               select new PatientGroupPersonDto
                                               {
                                                   Id = u.Id,
                                                   GroupTitleId = u.GroupTitleId,
                                                   CreatedOn = u.CreatedOn,
                                                   PersonTitleId = u.PersonTitleId,
                                                   PersonTitleText = up.Title,
                                                   PersonFio = u.PersonFio,
                                               }
                          ).OrderBy(p => p.Id).ToListAsync();
                    addItem.PersonGroup = allComissions;

                    retList.Add(addItem);
                }
            }
            return await Result<List<PatientGroupActDto>>.SuccessAsync(retList.OrderByDescending(e => e.PatientGroupTitle.CreatedOn).ToList());

        }

        public async Task<Result<PatientGroupActDto>> GroupActsDetail(long id)
        {

            var retItem = new PatientGroupActDto();
            var selTitle = await _dbContext.PatientGroupTitle.Where(p => p.Id.Equals(id)).FirstOrDefaultAsync();
            if (selTitle != null)
            {

                retItem.PatientGroupTitle = selTitle;

                    var allPatientActs = await (from u in _dbContext.PatientHistory
                                                join up in _dbContext.Patient on u.PatientId equals up.Id
                                                join fgr in _dbContext.PatientGroup on u.GroupFromId equals fgr.Id
                                                join tgr in _dbContext.PatientGroup on u.GroupId equals tgr.Id
                                                join inRe in _dbContext.GroupReason on u.GroupReasonId equals inRe.Id into inReasons
                                                from re in inReasons.DefaultIfEmpty()
                                                join inLpu in _dbContext.GroupLPU on u.GroupLPUId equals inLpu.Id into inLPUs
                                                from lpu in inLPUs.DefaultIfEmpty()
                                                where u.GroupTitleId.Equals(selTitle.Id)
                                                select new GroupActWithPatient
                                                {
                                                    Id = u.Id,
                                                    GroupTitleId = u.GroupTitleId,
                                                    CreatedOn = u.CreatedOn,
                                                    GroupId = u.GroupId,
                                                    GroupText = tgr.Title,
                                                    GroupFromId = u.GroupFromId,
                                                    GroupFromText = fgr.Title,
                                                    PatientId = up.Id,
                                                    Inn = up.Inn,
                                                    FirstName = up.FirstName,
                                                    LastName = up.LastName,
                                                    MiddleName = up.MiddleName,
                                                    GroupReasonId = u.GroupReasonId,
                                                    GroupLPUId = u.GroupLPUId,
                                                    ReasonText = re.Title,
                                                    LPUText = lpu.Title,
                                                }
                          ).OrderBy(p => p.Id).ToListAsync();
                retItem.PatientHistory = allPatientActs;

                    var allComissions = await (from u in _dbContext.PatientGroupAct
                                               join up in _dbContext.GroupPersonTitle on u.PersonTitleId equals up.Id
                                               where u.GroupTitleId.Equals(selTitle.Id)
                                               select new PatientGroupPersonDto
                                               {
                                                   Id = u.Id,
                                                   GroupTitleId = u.GroupTitleId,
                                                   CreatedOn = u.CreatedOn,
                                                   PersonTitleId = u.PersonTitleId,
                                                   PersonTitleText = up.Title,
                                                   PersonFio = u.PersonFio,
                                               }
                          ).OrderBy(p => p.Id).ToListAsync();
                retItem.PersonGroup = allComissions;
            }

            return await Result<PatientGroupActDto>.SuccessAsync(retItem);

        }

        public async Task<Result<List<Patient>>> AllPatients()
        {
            var retData = await _dbContext.Patient.OrderBy(p=>p.LastName).ToListAsync(); 
            
            return await Result<List<Patient>>.SuccessAsync(retData);
        }

        public async Task<Result<List<PatientGridDto>>> AllPatientsWithReason()
        {
            var retData = await (
                from u in _dbContext.Patient
             join r in _dbContext.PatientHistory on u.Id equals r.PatientId into patientHistoryGroup
            from history in patientHistoryGroup.DefaultIfEmpty()
             join t in _dbContext.GroupReason on history.GroupReasonId equals t.Id into groupReasonGroup
            from groupReason in groupReasonGroup.DefaultIfEmpty()
                select new PatientGridDto
                {
                    Id = u.Id,
                    Inn = u.Inn,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    Address = u.Address,
                    Address2 = u.Address2,
                    Phone = u.Phone,
                    Phone2 = u.Phone2,
                    DistrictId = u.DistrictId,
                    RegionId = u.RegionId,
                    MedCenterId = u.MedCenterId,
                    CreatedBy = u.CreatedBy,
                    CreatedOn = u.CreatedOn,
                    LastModifiedBy = u.LastModifiedBy,
                    LastModifiedOn = u.LastModifiedOn,
                    IsDeleted = u.IsDeleted,
                    DeletedOn = u.DeletedOn,
                    GroupId = u.GroupId,
                    StatusId = u.StatusId,
                    GroupText = u.GroupText,
                    Gender = u.Gender,
                    IsActive = u.IsActive,
                    BirthDate = u.BirthDate,
                    GroupActId = u.GroupActId,
                    Image1 = u.Image1,
                    Image2 = u.Image2,
                    Image = u.Image,
                    GroupReason = groupReason != null ? groupReason.Title : null
                }
                ).OrderBy(p => p.Id).ToListAsync();
            var filteredData = retData
                .GroupBy(p => p.Id) // Id alanına göre gruplandırma
                .Select(group =>
                {
                    var nonEmptyReason = group.Where(g => !string.IsNullOrEmpty(g.GroupReason)).FirstOrDefault();
                    if (nonEmptyReason != null)
                        return nonEmptyReason;

                    // Eğer tüm GroupReason alanları boşsa, son nesneyi seç
                    return group.Last();
                })
                .ToList();
            return await Result<List<PatientGridDto>>.SuccessAsync(filteredData);
        }
        public async Task<Result<List<Patient>>> AllPatients2(DateTime? fromDate, DateTime? toDate)
        {
            var retData = await (from u in _dbContext.Patient                                 
                                       where (fromDate==null || u.CreatedOn>=fromDate)
                                       && (toDate == null || u.CreatedOn <= toDate)
                                 select u
                         ).OrderBy(p => p.LastName).ToListAsync();

            return await Result<List<Patient>>.SuccessAsync(retData);
        }
        public async Task<Result<List<PatientGridDto>>> AllPatientsWithReason2(DateTime? fromDate, DateTime? toDate, DateTime? deletedFrom = null, DateTime? deletedTo = null)
        {
            var retData = await (
                from u in _dbContext.Patient
             join r in _dbContext.PatientHistory on u.Id equals r.PatientId into patientHistoryGroup
            from history in patientHistoryGroup.DefaultIfEmpty()
             join t in _dbContext.GroupReason on history.GroupReasonId equals t.Id into groupReasonGroup
            from groupReason in groupReasonGroup.DefaultIfEmpty()
            where (fromDate==null || u.CreatedOn>=fromDate)
                  && (toDate == null || u.CreatedOn <= toDate)
                  && (deletedFrom == null || u.DeletedOn >= deletedFrom)
                  && (deletedTo == null || u.DeletedOn <= deletedTo)
                select new PatientGridDto
                {
                    Id = u.Id,
                    Inn = u.Inn,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    Address = u.Address,
                    Address2 = u.Address2,
                    Phone = u.Phone,
                    Phone2 = u.Phone2,
                    DistrictId = u.DistrictId,
                    RegionId = u.RegionId,
                    MedCenterId = u.MedCenterId,
                    CreatedBy = u.CreatedBy,
                    CreatedOn = u.CreatedOn,
                    LastModifiedBy = u.LastModifiedBy,
                    LastModifiedOn = u.LastModifiedOn,
                    IsDeleted = u.IsDeleted,
                    DeletedOn = u.DeletedOn,
                    GroupId = u.GroupId,
                    StatusId = u.StatusId,
                    GroupText = u.GroupText,
                    Gender = u.Gender,
                    IsActive = u.IsActive,
                    BirthDate = u.BirthDate,
                    GroupActId = u.GroupActId,
                    Image1 = u.Image1,
                    Image2 = u.Image2,
                    Image = u.Image,
                    GroupReason = groupReason != null ? groupReason.Title : null
                }
                ).OrderBy(p => p.Id).ToListAsync();
            var filteredData = retData
                .GroupBy(p => p.Id) // Id alanına göre gruplandırma
                .Select(group =>
                {
                    var nonEmptyReason = group.Where(g => !string.IsNullOrEmpty(g.GroupReason)).FirstOrDefault();
                    if (nonEmptyReason != null)
                        return nonEmptyReason;

                    // Eğer tüm GroupReason alanları boşsa, son nesneyi seç
                    return group.Last();
                })
                .ToList();
            return await Result<List<PatientGridDto>>.SuccessAsync(filteredData);
        }
        public async Task<Result<List<PatientTotalMedCenterDto>>> AllPatients3(DateTime? fromDate, DateTime? toDate)
        {
            var retData = new List<PatientTotalMedCenterDto>();

            var allCenter = await _dbContext.MedCenter.OrderBy(p => p.Title).ToListAsync();
            if (allCenter != null)
            {
                foreach (var cen in allCenter)
                {
                    var allHd = await (from h in _dbContext.HDSession
                                       where (fromDate == null || h.SessionStart >= fromDate)
                                       && (toDate == null || h.SessionStart <= toDate)
                                       && (h.MedCenterId == cen.Id)
                                       select h
                         ).ToListAsync();
                    var allPatient = await (from u in _dbContext.Patient select u).OrderBy(p => p.LastName).ToListAsync();
                    if (allHd != null)
                    {
                        foreach (var item in allPatient)
                        {
                            var total = allHd.Where(p => p.PatientId.Equals(item.Id)).Count();
                            if (total > 0)
                            {
                                var newItem = new PatientTotalMedCenterDto()
                                {
                                    Id = item.Id,
                                    Inn = item.Inn,
                                    Fio = item.LastName + "  " + item.FirstName + " " + item.MiddleName,
                                    Total = total,
                                    Title = cen.Title,
                                    MedCenterId = cen.Id
                                };
                                retData.Add(newItem);
                            }

                        }
                    }
                }
            }

            return await Result<List<PatientTotalMedCenterDto>>.SuccessAsync(retData);
        }

        public async Task<Result<List<PatientTotalHdDto>>> AllPatients4(long MedCenterId, int totalDays, DateTime? fromDate, DateTime? toDate, string typePage)
        {
            bool typePage1 = (typePage == "true") ? true : false;
            toDate = toDate?.AddHours(23).AddMinutes(59);
            var cutoffDate = DateTime.Now.AddDays(-totalDays);

            var patientLastSessions = await (
                from p in _dbContext.Patient
                join h in _dbContext.HDSession on p.Inn equals h.Inn
                join m in _dbContext.MedCenter on h.MedCenterId equals m.Id
                where (MedCenterId == 0 || h.MedCenterId == MedCenterId) 
                    && (p.GroupId == 6 || p.GroupId == 7)
                group new { p, h, m } by new 
                { 
                    p.Id, 
                    p.Inn, 
                    Fio = p.LastName + " " + p.FirstName + " " + (p.MiddleName ?? ""),
                    p.Phone,
                    p.Phone2
                } into g
                select new 
                {
                    PatientInfo = g.Key,
                    LastSession = g.OrderByDescending(x => x.h.SessionStart).First()
                }).ToListAsync();

            // Sadece gerçek en son seansı filtrelerimize uyanları alalım
            var inactivePatients = patientLastSessions
                .Where(x => 
                    (typePage1 || x.LastSession.h.SessionStart <= cutoffDate) && // Devamsızlık kontrolü
                    (fromDate == null || x.LastSession.h.SessionStart >= fromDate) && // Tarih aralığı kontrolü
                    (toDate == null || x.LastSession.h.SessionStart <= toDate)
                )
                .Select(x => new PatientTotalHdDto
                {
                    Id = x.PatientInfo.Id,
                    Inn = x.PatientInfo.Inn,
                    Fio = x.PatientInfo.Fio,
                    LastDate = x.LastSession.h.SessionStart,
                    PhoneNumber1 = x.PatientInfo.Phone,
                    PhoneNumber2 = x.PatientInfo.Phone2,
                    MedCenterTitle = x.LastSession.m.Title, // En son oturumun hastane adı
                    Total = null // Bu branch'te total kullanılmıyor
                })
                .OrderByDescending(x => x.LastDate)
                .ToList();

            return await Result<List<PatientTotalHdDto>>.SuccessAsync(inactivePatients);
        }
        public async Task<Result<List<PatientTotalHdDto>>> AllPatients5(long MedCenterId, DateTime? fromDate, DateTime? toDate)
        {
            toDate = toDate?.AddHours(23).AddMinutes(59);
            var retData = new List<PatientTotalHdDto>();
            var allHd = await (from h in _dbContext.HDSession 
                               where (fromDate == null || h.SessionEnd >= fromDate)
                               && (toDate == null || h.SessionEnd <= toDate)
                               && (MedCenterId == 0 || h.MedCenterId == MedCenterId)
                               select h
                         ).ToListAsync();
            var allPatient = await (from u in _dbContext.Patient select u).OrderBy(p => p.LastName).ToListAsync();
            if (allHd != null)
            {
                foreach (var item in allPatient)
                {
                    var total = allHd.Where(p => p.PatientId.Equals(item.Id)).Count();
                    if (total > 0)
                    {
                        var newItem = new PatientTotalHdDto()
                        {
                            Id = item.Id,
                            Inn = item.Inn,
                            Fio = item.LastName + "  " + item.FirstName + " " + item.MiddleName,
                            Total = total
                        };
                        retData.Add(newItem);
                    }

                }
            }           

            return await Result<List<PatientTotalHdDto>>.SuccessAsync(retData);
        }

        public async Task<Result<List<PatientBoxDto>>> AllPatientsBox()
        {
            var retData = await _dbContext.Patient.OrderBy(p => p.LastName).Select(p=> new PatientBoxDto {Id=p.Id, Inn=p.Inn, GroupId=p.GroupId, TextValue= p.LastName+" "+p.FirstName+" ("+p.Inn+")" }).ToListAsync();

            return await Result<List<PatientBoxDto>>.SuccessAsync(retData);
        }

        public async Task<Result<Patient>> SearchByInn(string inn)
        {
            var retData = await _dbContext.Patient.Where(p => p.Inn.Equals(inn) && p.IsDeleted.Equals(false)).FirstOrDefaultAsync();
            if (retData != null)
            {
                var groupText = await _dbContext.PatientGroup.Where(g => g.Id.Equals(retData.GroupId))
                    .FirstOrDefaultAsync();
                retData.GroupText = groupText?.Title;
                return await Result<Patient>.SuccessAsync(retData);
            }
            else
            {
                Console.WriteLine("Пациент не найден или не активен");
                return await Result<Patient>.FailAsync("Пациент не найден или не активен");
            }
            
        }

       

        public async Task<Result<PatientResultDto>> SearchResultByInn(string Inn)
        {
            var retVal = new PatientResultDto();
            retVal.Status = -1;
            var retData = await _dbContext.Patient.Where(p => p.Inn.Equals(Inn) && p.IsDeleted.Equals(false)).FirstOrDefaultAsync();
            if (retData != null)
            {
                retVal.Status = 1;
                retVal.Patient = retData;

                var oldSession = await _dbContext.HDSession.Where(p => p.PatientId.Equals(retData.Id) && (p.StatusId.Equals((long)HDSessionEnum.Started) || p.StatusId.Equals((long)HDSessionEnum.Paused))).FirstOrDefaultAsync();
                if (oldSession != null)
                {
                    retVal.Status = -1;
                    retVal.Err = "У этого пациента есть активный протокол сеанса";                    
                }

                var identify = await _dbContext.HDSession.Where(p => p.PatientId.Equals(retData.Id) && (p.StatusId.Equals((long)HDSessionEnum.Identification))).FirstOrDefaultAsync();
                if (identify == null)
                {
                    retVal.Status = -1;
                    retVal.Err = "Пациент не идентифицирован";
                }           
            }
            else
            {
                retVal.Status = -1;
                retVal.Err = "Пациент не найден";
            }
            return await Result<PatientResultDto>.SuccessAsync(retVal);
        }

        public async Task<Result<Patient>> SearchByInnAndFio(string inn, string fio)
        {
            var retData = await _dbContext.Patient.Where(p => p.Inn.Equals(inn) && (String.IsNullOrEmpty(fio) || p.LastName.ToLower().Contains(fio)) ).FirstOrDefaultAsync();
            return await Result<Patient>.SuccessAsync(retData);
        }

        public async Task<Result<Patient>> GetByMedCardId(long MedcardId)
        {
            var allVal = (from u in _dbContext.Patient
                          join up in _dbContext.MedCard on u.Inn equals up.Inn                         
                          where up.Id.Equals(MedcardId)
                          select u
                          ).FirstOrDefault();

            return await Result<Patient>.SuccessAsync(allVal);
        }

        public async Task<Result<Patient>> GetById(long Id)
        {
            var retData = await _dbContext.Patient.Where(p => p.Id.Equals(Id)).FirstOrDefaultAsync();
            return await Result<Patient>.SuccessAsync(retData);
        }
        public async Task<Result<PatientInfo>> GetByInn(string Inn)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    string url = "http://192.168.0.49:8000/api/v1/mhi/" + Inn;
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    PatientInfo patientInfo = JsonConvert.DeserializeObject<PatientInfo>(jsonResponse);
                    return await Result<PatientInfo>.SuccessAsync(patientInfo,"Пациент найден");
                }
                catch (HttpRequestException ex)
                {
                    return Result<PatientInfo>.Fail("Пациент не найден");
                }
            }
        }
        public async Task<Result<List<PatientChangesDto>>> AllReestrChanges()
        {
            List<PatientChangesDto> retList = new List<PatientChangesDto>();
            var allGroupTitles = await _dbContext.PatientGroupTitle.OrderByDescending(p => p.Id).ToListAsync();
            if (allGroupTitles != null)
            {
                foreach (var title in allGroupTitles)
                {
                    var dateAct = title.CreatedOn;
                    var totalPatients = await _dbContext.Patient.Where(p => p.CreatedOn <= dateAct && p.IsDeleted.Equals(false)).CountAsync();

                    var addItem = new PatientChangesDto();
                    addItem.Id = title.Id;
                    addItem.Before = totalPatients;
                    addItem.CreatedOn = dateAct;




                    var gonePatients = await (from u in _dbContext.PatientHistory                                                
                                                where u.GroupTitleId.Equals(title.Id) && u.GroupId == 2
                                                select u ).CountAsync();
                    var comePatients = await (from u in _dbContext.PatientHistory
                                              where u.GroupTitleId.Equals(title.Id) && u.GroupId != 2
                                              select u).CountAsync();

                    addItem.Come = comePatients;
                    addItem.Gone = gonePatients;
                    addItem.After = (totalPatients- gonePatients) + comePatients;

                    retList.Add(addItem);
                }
            }
            return await Result<List<PatientChangesDto>>.SuccessAsync(retList);
        }

        public async Task<Result<PatientChangesDto>> ReestrChangeDetail(long Id)
        {
            var retList = new PatientChangesDto();

            retList.Id = Id;
            retList.Before = 0;

                    var allGonePatientActs = await (from u in _dbContext.PatientHistory
                                        join up in _dbContext.Patient on u.PatientId equals up.Id
                                        join fgr in _dbContext.PatientGroup on u.GroupFromId equals fgr.Id
                                        join tgr in _dbContext.PatientGroup on u.GroupId equals tgr.Id
                                        join inRe in _dbContext.GroupReason on u.GroupReasonId equals inRe.Id into inReasons
                                        from re in inReasons.DefaultIfEmpty()
                                        join inLpu in _dbContext.GroupLPU on u.GroupLPUId equals inLpu.Id into inLPUs
                                        from lpu in inLPUs.DefaultIfEmpty()
                                        where u.GroupTitleId.Equals(Id) && u.GroupId == 2
                                                select new GroupActWithPatient
                                        {
                                            Id = u.Id,
                                            GroupTitleId = u.GroupTitleId,
                                            CreatedOn = u.CreatedOn,
                                            GroupId = u.GroupId,
                                            GroupText = tgr.Title,
                                            GroupFromId = u.GroupFromId,
                                            GroupFromText = fgr.Title,
                                            PatientId = up.Id,
                                            Inn = up.Inn,
                                            FirstName = up.FirstName,
                                            LastName = up.LastName,
                                            MiddleName = up.MiddleName,
                                            GroupReasonId = u.GroupReasonId,
                                            GroupLPUId = u.GroupLPUId,
                                            ReasonText = re.Title,
                                            LPUText = lpu.Title,
                                        }
                          ).OrderBy(p => p.Id).ToListAsync();
            retList.GoneList = allGonePatientActs;

            var allComePatientActs = await (from u in _dbContext.PatientHistory
                                            join up in _dbContext.Patient on u.PatientId equals up.Id
                                            join fgr in _dbContext.PatientGroup on u.GroupFromId equals fgr.Id
                                            join tgr in _dbContext.PatientGroup on u.GroupId equals tgr.Id
                                            join inRe in _dbContext.GroupReason on u.GroupReasonId equals inRe.Id into inReasons
                                            from re in inReasons.DefaultIfEmpty()
                                            join inLpu in _dbContext.GroupLPU on u.GroupLPUId equals inLpu.Id into inLPUs
                                            from lpu in inLPUs.DefaultIfEmpty()
                                            where u.GroupTitleId.Equals(Id) && u.GroupId != 2
                                            select new GroupActWithPatient
                                            {
                                                Id = u.Id,
                                                GroupTitleId = u.GroupTitleId,
                                                CreatedOn = u.CreatedOn,
                                                GroupId = u.GroupId,
                                                GroupText = tgr.Title,
                                                GroupFromId = u.GroupFromId,
                                                GroupFromText = fgr.Title,
                                                PatientId = up.Id,
                                                Inn = up.Inn,
                                                FirstName = up.FirstName,
                                                LastName = up.LastName,
                                                MiddleName = up.MiddleName,
                                                GroupReasonId = u.GroupReasonId,
                                                GroupLPUId = u.GroupLPUId,
                                                ReasonText = re.Title,
                                                LPUText = lpu.Title,
                                            }
                          ).OrderBy(p => p.Id).ToListAsync();
            retList.ComeList = allComePatientActs;
            retList.Come = 0;
            retList.Gone = 0;
            retList.After = 0;


            return await Result<PatientChangesDto>.SuccessAsync(retList);
        }

        public async Task<IRetResult> AddWriting(WritingOut model)
        {
            if (model != null)
            {

                if (model.Id != null && model.Id != 0 && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.WritingOut.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Fio = model.Fio;
                        localModel.FIODoctor = model.FIODoctor;
                        localModel.FIODepartmentHead = model.FIODepartmentHead;
                        localModel.Familiarized = model.Familiarized;
                        localModel.Address = model.Address;
                        localModel.ReceiptDate = model.ReceiptDate;
                        localModel.LeaveDate = model.LeaveDate;
                        localModel.TotalHDSession = model.TotalHDSession;
                        localModel.BloodResus = model.BloodResus;
                        localModel.MainDiagnosis = model.MainDiagnosis;
                        localModel.SoputDiagnosis = model.SoputDiagnosis;
                        localModel.Complication = model.Complication;
                        localModel.Anamnez = model.Anamnez;
                        localModel.AnamnezZabol = model.AnamnezZabol;
                        localModel.Allergo = model.Allergo;
                        localModel.Gemotrans = model.Gemotrans;
                        localModel.GepatitB = model.GepatitB;
                        localModel.Sosud = model.Sosud;
                        localModel.Objectivus = model.Objectivus;
                        localModel.AllResults = model.AllResults;
                        localModel.ExamFor = model.ExamFor;
                        localModel.VirusBC = model.VirusBC;
                        localModel.RW = model.RW;
                        localModel.Vich = model.Vich;
                        localModel.Instrumental = model.Instrumental;
                        localModel.Uzi = model.Uzi;
                        localModel.Ekg = model.Ekg;
                        localModel.Eho = model.Eho;
                        localModel.Rentgen = model.Rentgen;
                        localModel.Consulting = model.Consulting;
                        localModel.Otchet = model.Otchet;
                        localModel.TimeProcedure = model.TimeProcedure;
                        localModel.Gemo = model.Gemo;
                        localModel.Sostoyania = model.Sostoyania;
                        localModel.Medikamentoz = model.Medikamentoz;
                        localModel.Recommend = model.Recommend;
                        localModel.LastModifiedBy = _currentUser.UserId;
                        localModel.LastModifiedOn = DateTime.Now;
                            _dbContext.Attach(localModel);
                            await _dbContext.SaveChangesAsync();
                            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "WritingOut", localModel.Id.ToString(), null, new { localModel.Fio }, "Выписка обновлена");
                        }
                    }
                    else
                {
                                        
                        model.CreatedBy = _currentUser.UserId;
                        model.CreatedOn = DateTime.Now;
                        model.IsDeleted = false;
                        await _dbContext.AddAsync(model);
                        await _dbContext.SaveChangesAsync();                    
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "WritingOut", model.Id.ToString(), null, new { model.Inn, model.Fio }, "Выписка создана");
                }
            }
            return await Result<int>.SuccessAsync();
        }

        public async  Task<Result<List<WritingOutDto>>> SearchWritings(string? inn, long mid, DateTime? fromDate, DateTime? toDate)
        {
             var  allHd = await (from h in _dbContext.WritingOut
                               join p in _dbContext.Patient on h.PatientId equals p.Id
                               where (fromDate == null || h.LeaveDate >= fromDate)
                               && (toDate == null || h.LeaveDate <= toDate)
                               && (mid == 0 || h.MedCenterId == mid)
                               && (String.IsNullOrEmpty(inn) || h.Inn.Contains(inn))
                               select new WritingOutDto()
                               {
                                   WritingOut = h,
                                   Patient = p,
                                   MedCenter = null
                               }
                         ).OrderByDescending(t => t.WritingOut.Id).ToListAsync();
            return await Result<List<WritingOutDto>>.SuccessAsync(allHd);
        }
        public async Task<Result<WritingOutDto>> GetWritingById(long id)
        {
            var allHd = await (from h in _dbContext.WritingOut
                               join p in _dbContext.Patient on h.PatientId equals p.Id
                               where h.Id.Equals(id)
                               select new WritingOutDto()
                               {
                                   WritingOut = h,
                                   Patient = p,
                                   MedCenter = null
                               }
                         ).FirstOrDefaultAsync();
            return await Result<WritingOutDto>.SuccessAsync(allHd ?? new WritingOutDto());
        }
        public async Task<IRetResult> AddComplaint(Complaint model)
        {
            if (model != null)
            {

                if (model.Id != null && model.Id != 0 && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.Complaint.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.TextComplaint = model.TextComplaint;
                        _dbContext.Attach(localModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "Complaint", localModel.Id.ToString(), null, new { localModel.TextComplaint }, "Жалоба обновлена");
                    }
                }
                else
                {
                                        
                        model.CreatedBy = _currentUser.UserId;
                        model.CreatedOn = DateTime.Now;
                        model.IsDeleted = false;
                        await _dbContext.AddAsync(model);
                        await _dbContext.SaveChangesAsync();                    
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "Complaint", model.Id.ToString(), null, new { model.Inn }, "Жалоба создана");
                }
            }
            return await Result<int>.SuccessAsync();
        }
        public async  Task<Result<List<ComplaintDto>>> SearchComplaint(string? inn, long mid)
        {
            var  allHd = await (from h in _dbContext.Complaint
                               join p in _dbContext.Patient on h.PatientId equals p.Id
                               where (mid == 0 || h.MedCenterId == mid)
                                     && (String.IsNullOrEmpty(inn) || h.Inn.Contains(inn))
                               select new ComplaintDto()
                               {
                                   Complaint = h,
                                   Patient = p,
                                   MedCenter = null
                               }).OrderByDescending(t => t.Complaint.Id).ToListAsync();
            return await Result<List<ComplaintDto>>.SuccessAsync(allHd);
        }
        public async Task<Result<ComplaintDto>> GetComplaintById(long id)
        {
            var allHd = await (from h in _dbContext.Complaint
                               join p in _dbContext.Patient on h.PatientId equals p.Id
                               where h.Id.Equals(id)
                               select new ComplaintDto()
                               {
                                   Complaint = h,
                                   Patient = p,
                                   MedCenter = null
                               }
                         ).FirstOrDefaultAsync();
            return await Result<ComplaintDto>.SuccessAsync(allHd ?? new ComplaintDto());
        }
    }
}

