using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Passsport.Models;
using Passsport.Helper;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Security.Claims;
using System.Security.Principal;
using System.Net;


namespace Passsport.Controllers
{


    [Authorize]
    [RoutePrefix("api/project")]
    public class ProjectController : ApiController
    {
        private PassportEntities db = new PassportEntities();
        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            byte[] salt = GenerateSalt();
            byte[] hash = HashPassword(model.Password, salt);

            string token = JwtTokenHelper.GenerateToken(0, model.FullName, "Citizen", model.LoginID);

            var result = db.Database.SqlQuery<AuthResponseDto>(
                @"EXEC SP_RegisterOrLoginUser 
          @Mode,
          @LoginID,
          @PasswordHash,
          @Salt,
          @FullName,
          @EmailAddress,
          @RegistrationType,
          @JwtToken,
          @TokenExpiryMinutes",

                new SqlParameter("@Mode", "REGISTER"),
                new SqlParameter("@LoginID", model.LoginID),
                new SqlParameter("@PasswordHash", hash),
                new SqlParameter("@Salt", salt),
                new SqlParameter("@FullName", model.FullName),
                new SqlParameter("@EmailAddress", model.EmailAddress),
                new SqlParameter("@RegistrationType", model.RegistrationType),
                new SqlParameter("@JwtToken", token),
                new SqlParameter("@TokenExpiryMinutes", 60)
            ).FirstOrDefault();

            if (result == null || result.Status == "FAILED")
                return BadRequest(result?.Message);

            return Ok(result);
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginDto model)
        {
            // 1️⃣ Get salt first
            byte[] salt = GetSaltByLoginID(model.LoginID);
            if (salt == null)
                return Unauthorized();

            // 2️⃣ Hash using stored salt
            byte[] hash = HashPassword(model.Password, salt);
            int Userid= model.UserID;

            string token = JwtTokenHelper.GenerateToken(Userid, model.LoginID, "Citizen", model.LoginID);

            var result = db.Database.SqlQuery<AuthResponseDto>(
                @"EXEC SP_RegisterOrLoginUser @Mode, @LoginID, @PasswordHash, @Salt, NULL, NULL, NULL, @JwtToken, @TokenExpiryMinutes",
                new SqlParameter("@Mode", "LOGIN"),
                new SqlParameter("@LoginID", model.LoginID),
                new SqlParameter("@PasswordHash", SqlDbType.VarBinary, 256) { Value = hash },
                new SqlParameter("@Salt", SqlDbType.VarBinary, 128) { Value = salt },
                new SqlParameter("@JwtToken", token),
                new SqlParameter("@TokenExpiryMinutes", 60)).FirstOrDefault();

            // 5️⃣ Validate result if (result == null || result.Status == "FAILED") return Unauthorized();

            return Ok(new
            {
                token,
                userId = result.UserID,
                fullName = model.LoginID,
                status = result.Status


            });
        }

        private byte[] GenerateSalt()
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);
                return salt;
            }
        }
        private byte[] GetSaltByLoginID(string loginId)
        {
            return db.Database.SqlQuery<byte[]>(
                "SELECT Salt FROM AppUser WHERE LoginID = @LoginID",
                new SqlParameter("@LoginID", loginId)
            ).FirstOrDefault();
        }
        private byte[] HashPassword(string password, byte[] salt)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var combined = salt.Concat(System.Text.Encoding.UTF8.GetBytes(password)).ToArray();
                return sha256.ComputeHash(combined);
            }
        }
       
        [HttpGet]
        [Route("passport-types")]
        public IHttpActionResult GetPassportTypes()
        {
            try
            {
                var types = db.Database
                    .SqlQuery<PassportTypeDto>("EXEC SP_GetActivePassportTypes")
                    .ToList();

                return Ok(types);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

       
        [HttpGet]
        [Route("offices")]
        public IHttpActionResult GetOffices()
        {
            try
            {
                
                var offices = db.Database
          .SqlQuery<OfficeDto>("EXEC SP_GetActiveOffices")
          .ToList();

                return Ok(offices);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("available-dates/{officeId}")]
        public IHttpActionResult GetAvailableDates(int officeId)
        {
            var startDate = DateTime.Today.AddDays(1);
            var endDate = DateTime.Today.AddDays(30);

            var allDates = Enumerable.Range(0, 30)
                .Select(i => startDate.AddDays(i))
                .Where(d => d.DayOfWeek != DayOfWeek.Sunday)
                .ToList();

            var param1 = new SqlParameter("@OfficeID", officeId);
            var param2 = new SqlParameter("@StartDate", startDate);
            var param3 = new SqlParameter("@EndDate", endDate);

            var bookedDates = db.Database
            .SqlQuery<BookedDateDto>(
                "EXEC SP_GetFullyBookedDates_ByOffice @OfficeID, @StartDate, @EndDate",
                param1, param2, param3)
            .Select(x => x.AppointmentDate.Date)
            .ToList();
            var availableDates = allDates.Except(bookedDates).ToList();

            return Ok(new
            {
                availableDates = availableDates.Select(d => d.ToString("yyyy-MM-dd")),
                bookedDates = bookedDates.Select(d => d.ToString("yyyy-MM-dd"))
            });
        }
        [HttpGet]
        [Route("time-slots/{officeId}/{date}")]
        public IHttpActionResult GetTimeSlots(int officeId, string date)
        {
            var appointmentDate = DateTime.Parse(date);
            var slots = new[] { "09:00 AM", "09:30 AM", "10:00 AM", "10:30 AM", "11:00 AM",
                           "11:30 AM", "02:00 PM", "02:30 PM", "03:00 PM", "03:30 PM", "04:00 PM" };

            var p1 = new SqlParameter("@OfficeID", officeId);
            var p2 = new SqlParameter("@AppointmentDate", appointmentDate);

            var booked = db.Database
                .SqlQuery<SlotCountDto>(
                    "EXEC SP_GetBookedSlotCounts @OfficeID, @AppointmentDate",
                    p1, p2)
                .ToList();

            var bookedDict = booked.ToDictionary(x => x.TimeSlot, x => x.BookedCount);

            var result = slots.Select(s => new
            {
                TimeSlot = s,
                IsAvailable = !bookedDict.ContainsKey(s) || bookedDict[s] < 5,
                AvailableCount = 5 - (bookedDict.ContainsKey(s) ? bookedDict[s] : 0)
            });

            return Ok(result);
        }

        [HttpPost]
        [Route("schedule-appointment")]
        public async Task<IHttpActionResult> ScheduleAppointment(Appointment model)
        {
            try
            {
                var p1 = new SqlParameter("@ApplicationID", model.ApplicationID);
                var p2 = new SqlParameter("@OfficeID", model.OfficeID);
                var p3 = new SqlParameter("@AppointmentDate", model.AppointmentDate);
                var p4 = new SqlParameter("@TimeSlot", model.TimeSlot);
                var p5 = new SqlParameter("@Purpose", model.Purpose);

                var resultParam = new SqlParameter("@Result", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                var idParam = new SqlParameter("@NewAppointmentID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                await db.Database.ExecuteSqlCommandAsync(
                    "EXEC SP_BookAppointment_FCFS @ApplicationID, @OfficeID, @AppointmentDate, @TimeSlot, @Purpose, @Result OUTPUT, @NewAppointmentID OUTPUT",
                    p1, p2, p3, p4, p5, resultParam, idParam
                );

                int result = (int)resultParam.Value;

                if (result == 0)
                {
                    return Ok(new { Status = 0, Message = "Time slot is already full. Please choose another slot." });
                }

                return Ok(new
                {
                    Status = 1,
                    Message = "Appointment scheduled successfully",
                    AppointmentID = idParam.Value
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        //[HttpPost]
        //[Route("process-payment")]
        //public async Task<IHttpActionResult> ProcessPayment(Payment model)
        //{
        //    var transactionId = $"TXN{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";

        //    var application = db.Applications.Find(model.ApplicationID);
        //    if (application == null)
        //        return NotFound();

        //    application.AmountPaid = model.Amount;
        //    application.PaymentDate = DateTime.Now;
        //    application.PaymentReference = transactionId;
        //    application.StatusID = 2; // Payment Completed

        //    db.ApplicationHistories.Add(new ApplicationHistory
        //    {
        //        ApplicationID = model.ApplicationID,
        //        StatusID = 2,
        //        Comments = $"Payment completed. Transaction: {transactionId}",
        //        CreatedAt = DateTime.Now
        //    });

        //    await db.SaveChangesAsync();

        //    return Ok(new { Status = 1, Message = "Payment successful", TransactionID = transactionId });
        //}

        //private string GenerateARN(int officeId)
        //{
        //    var today = DateTime.Today;
        //    var tomorrow = today.AddDays(1); // ✅ move outside LINQ

        //    var dateStr = today.ToString("yyyyMMdd");

        //    var officeCode = db.Offices
        //        .Where(o => o.OfficeID == officeId)
        //        .Select(o => o.OfficeName.Substring(0, 3).ToUpper())
        //        .FirstOrDefault() ?? "GEN";

        //    var todayCount = db.Applications
        //        .Count(a => a.CreatedAt >= today && a.CreatedAt < tomorrow) + 1;

        //    return $"ARN-{dateStr}-{officeCode}-{todayCount:D4}";
        //}
        private string GenerateARN(int officeId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            string datePart = today.ToString("yyyyMMdd");

            string officeCode = db.Offices
                .Where(o => o.OfficeID == officeId)
                .Select(o => o.OfficeName.Substring(0, 3).ToUpper())
                .FirstOrDefault() ?? "GEN";

            int todayCount = db.Applications
                .Count(a => a.CreatedAt >= today && a.CreatedAt < tomorrow) + 1;

            string sequence = todayCount.ToString("D5"); // 00001

            // EXACTLY 20 characters
            return $"ARN{datePart}{officeCode}{sequence}";
        }



        [HttpGet]
        [Route("document-types")]
        public IHttpActionResult GetDocumentTypes()
        {
            try
            {
                var documentTypes = db.Database
                    .SqlQuery<DocumentTypeDto>("EXEC SP_GetDocumentTypes")
                    .ToList();

                return Ok(documentTypes);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        [Authorize]
        [HttpPost]
        [Route("submit")]
        public async Task<IHttpActionResult> SubmitApplication()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    return BadRequest("Invalid request format");

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                ApplicationDto appData = null;
                var documents = new List<(int DocTypeId, string FileName, byte[] Data)>();

                foreach (var content in provider.Contents)
                {
                    var name = content.Headers.ContentDisposition.Name?.Trim('"');

                    if (name == "ApplicationData")
                    {
                        var json = await content.ReadAsStringAsync();
                        appData = JsonConvert.DeserializeObject<ApplicationDto>(json);
                    }
                    else if (name.StartsWith("Documents_"))
                    {
                        var docId = int.Parse(name.Replace("Documents_", ""));
                        var fileName = content.Headers.ContentDisposition.FileName?.Trim('"');
                        var bytes = await content.ReadAsByteArrayAsync();
                        documents.Add((docId, fileName, bytes));
                    }
                }

                if (appData == null)
                    return BadRequest("Application data missing");

                string arn = GenerateARN(appData.OfficeID ?? 0);

                var newIdParam = new SqlParameter("@NewApplicationID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                var ErrNum = new SqlParameter("@ErrorNumber", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var ErrMessage = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 2000) { Direction = ParameterDirection.Output };
                

                await db.Database.ExecuteSqlCommandAsync(
                    @"EXEC SP_SubmitPassportApplication
              @NationalID, @FirstName, @MiddleName, @LastName, @DateOfBirth, @Gender,
              @PlaceOfBirth, @MaritalStatus, @Occupation, @Email, @Phone, @Address,
              @City, @State, @PostalCode,
              @PassportTypeID, @OfficeID, @ApplicationType, @PreviousPassportNumber,
              @EmergencyContactName, @EmergencyContactPhone, @TravelPurpose,
              @ExpectedTravelDate, @StatusID, @ApplicationNumber,
              @NewApplicationID OUTPUT,@ErrorNumber OUTPUT,@ErrorMessage OUTPUT",

                    new SqlParameter("@NationalID", appData.NationalID),
                    new SqlParameter("@FirstName", appData.FirstName),
                    new SqlParameter("@MiddleName", appData.MiddleName ?? (object)DBNull.Value),
                    new SqlParameter("@LastName", appData.LastName),
                    new SqlParameter("@DateOfBirth", appData.DateOfBirth),
                    new SqlParameter("@Gender", appData.Gender.Substring(0, 1).ToUpper()),
                    new SqlParameter("@PlaceOfBirth", appData.PlaceOfBirth),
                    new SqlParameter("@MaritalStatus", appData.MaritalStatus),
                    new SqlParameter("@Occupation", appData.Occupation),
                    new SqlParameter("@Email", appData.Email),
                    new SqlParameter("@Phone", appData.Phone),
                    new SqlParameter("@Address", appData.Address),
                    new SqlParameter("@City", appData.City),
                    new SqlParameter("@State", appData.State),
                    new SqlParameter("@PostalCode", appData.PostalCode),

                    new SqlParameter("@PassportTypeID", appData.PassportTypeID),
                    new SqlParameter("@OfficeID", appData.OfficeID),
                    new SqlParameter("@ApplicationType", appData.ApplicationType),
                    new SqlParameter("@PreviousPassportNumber", appData.PreviousPassportNumber ?? (object)DBNull.Value),
                    new SqlParameter("@EmergencyContactName", appData.EmergencyContactName),
                    new SqlParameter("@EmergencyContactPhone", appData.EmergencyContactPhone),
                    new SqlParameter("@TravelPurpose", appData.TravelPurpose),
                    new SqlParameter("@ExpectedTravelDate", appData.ExpectedTravelDate ?? (object)DBNull.Value),

                    new SqlParameter("@StatusID", 1), // Submitted
                    new SqlParameter("@ApplicationNumber", arn),
                    newIdParam,
                     ErrNum,
                     ErrMessage
                    
                );
                

                int appId = (int)newIdParam.Value;
                int errorNumber = (int)ErrNum.Value;
                string errorMessage = ErrMessage.Value?.ToString();

                // ================= FILE SAVE =================
                var uploadPath = HttpContext.Current.Server.MapPath("~/Uploads/Documents/");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var doc in documents)
                {
                    var unique = $"{appId}_{doc.DocTypeId}_{Guid.NewGuid()}{Path.GetExtension(doc.FileName)}";
                    var path = Path.Combine(uploadPath, unique);
                    File.WriteAllBytes(path, doc.Data);

                    db.Documents.Add(new Document
                    {
                        ApplicationID = appId,
                        DocumentTypeID = doc.DocTypeId,
                        FileName = doc.FileName,
                        FilePath = $"/Uploads/Documents/{unique}",
                        FileSize = doc.Data.Length,
                        MimeType = MimeMapping.GetMimeMapping(doc.FileName),
                        UploadedAt = DateTime.Now
                    });
                }

                await db.SaveChangesAsync();

                return Ok(new
                {
                    Status = 1,
                    Message = "Application submitted successfully",
                    ARN = arn,
                    ApplicationID = appId
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }




        //private async Task SaveDocumentsEf(int applicationId,
        //      Dictionary<string, byte[]> documents,
        //       Dictionary<string, string> documentNames)

        //{
        //    // Ensure upload directory exists
        //    if (!Directory.Exists(_uploadPath))
        //        Directory.CreateDirectory(_uploadPath);

        //    foreach (var doc in documents)
        //    {
        //        var docType = doc.Key;
        //        var fileBytes = doc.Value;
        //        var originalFileName = documentNames[docType];
        //        var extension = Path.GetExtension(originalFileName);
        //        var newFileName = string.Format("{0}_{1}_{2}{3}", applicationId, docType, Guid.NewGuid(), extension);
        //        var filePath = Path.Combine(_uploadPath, newFileName);

        //        // Save file to disk
        //        File.WriteAllBytes(filePath, fileBytes);

        //        // Get DocumentTypeID from DB using EF
        //        var documentType = await _dbContext.DocumentTypes
        //            .FirstOrDefaultAsync(d => d.TypeName == docType);

        //        if (documentType == null)
        //        {
        //            throw new Exception($"Document type '{docType}' not found in database.");
        //        }

        //        // Create Document entity
        //        var documentEntity = new Document
        //        {
        //            ApplicationID = applicationId,
        //            DocumentTypeID = documentType.DocumentTypeID,
        //            FileName = originalFileName,
        //            FilePath = filePath,
        //            FileSize = fileBytes.Length,
        //            MimeType = GetMimeType(extension),
        //            UploadedAt = DateTime.Now
        //        };

        //        _dbContext.Documents.Add(documentEntity);
        //    }

        //    await _dbContext.SaveChangesAsync();
        //}


        private int GetDocumentTypeId(string docType, SqlConnection con)
        {
            var mapping = new Dictionary<string, string>
            {
                { "Photo", "Passport Photo" },
                { "IDProof", "ID Proof" },
                { "AddressProof", "Address Proof" },
                { "BirthCertificate", "Birth Certificate" }
            };

            var typeName = mapping.ContainsKey(docType) ? mapping[docType] : docType;

            using (var cmd = new SqlCommand(
                "SELECT DocumentTypeID FROM DocumentTypes WHERE TypeName = @TypeName", con))
            {
                cmd.Parameters.AddWithValue("@TypeName", typeName);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 1;
            }
        }

        private string GetMimeType(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return "application/octet-stream";

            extension = extension.ToLower();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".pdf":
                    return "application/pdf";
                default:
                    return "application/octet-stream";
            }
        }




        [HttpGet]
        [Route("status/{applicationNumber}")]
        public IHttpActionResult GetApplicationStatus(string applicationNumber)
        {
            try
            {
                var application = db.Applications
                    .Where(a => a.ApplicationNumber == applicationNumber)
                    .Select(a => new
                    {
                        a.ApplicationID,
                        a.ApplicationNumber,
                        a.ApplicationType,
                        Status = a.ApplicationStatus.StatusName,
                        StatusColor = a.ApplicationStatus.ColorCode,
                        PassportType = a.PassportType.TypeName,
                        Office = a.Office.OfficeName,
                        OfficeCity = a.Office.City,
                        ApplicantName = a.Citizen.FirstName + " " + a.Citizen.LastName,
                        a.SubmittedAt,
                        ApprovedAt = a.ApprovedAt,
                        RejectedAt = a.RejectedAt,
                        RejectionReason = a.RejectionReason
                    })
                    .FirstOrDefault();

                if (application == null)
                    return NotFound();

                return Ok(application);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("my-applications")]
        public IHttpActionResult GetMyApplications()
        {
            try
            {
                // Get current user ID from JWT claims
                var identity = User.Identity as ClaimsIdentity;
                var userIdClaim = identity?.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized();

                string userId = userIdClaim.ToString();


                // Query applications via EF
                var applications = db.Applications
                    .Where(a => a.Citizen.CitizenID.ToString() == userId) // Assuming Citizen has UserID
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new
                    {
                        a.ApplicationNumber,
                        a.ApplicationType,
                        Status = a.ApplicationStatus.StatusName,
                        StatusColor = a.ApplicationStatus.ColorCode,
                        PassportType = a.PassportType.TypeName,
                        a.SubmittedAt,
                        a.CreatedAt
                    })
                    .ToList();

                return Ok(applications);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("stats")]
        public IHttpActionResult GetDashboardStats() 
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                // Extract UserID from JWT token claims
                var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized();

                int userId = int.Parse(userIdClaim);

                // Fetch the user from AppUser table
                var user = db.AppUsers
                    .Where(u => u.UserID == userId)
                    .Select(u => new
                    {
                        u.UserID,
                        u.LoginID,
                        u.EmailAddress,
                        u.FullName,
                        u.RegistrationType,
                        u.AccountStatus,
                        u.DateRegistered,
                        u.LastLogin
                    })
                    .FirstOrDefault();

                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}