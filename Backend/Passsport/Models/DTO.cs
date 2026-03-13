using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Passsport.Models
{
    public class ApplicationDto
    {

        // =========================
        // Application Info
        // =========================

        [Required]
        public string ApplicationType { get; set; }

        [Required]
        public int PassportTypeID { get; set; }

        public string PreviousPassportNumber { get; set; }

        public int? OfficeID { get; set; }

        // =========================
        // Citizen / Personal Info
        // =========================

        [Required]
        public string NationalID { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; }

        public string PlaceOfBirth { get; set; }

        public string MaritalStatus { get; set; }

        public string Occupation { get; set; }

        // =========================
        // Contact Info
        // =========================

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PostalCode { get; set; }

        // =========================
        // Emergency Contact
        // =========================

        public string    EmergencyContactName { get; set; }

        public string EmergencyContactPhone { get; set; }

        // =========================
        // Travel Info
        // =========================

        public string TravelPurpose { get; set; }

        public DateTime? ExpectedTravelDate { get; set; }

        public int ErrNum { get; set; }
        public string ErrMessage { get; set; }
    }

    public class PassportTypeDto
    {
        public int PassportTypeID { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public int ValidityYears { get; set; }
        public decimal Fee { get; set; }
    }
    public class DocumentTypeDto
    {
        public int DocumentTypeID { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public int MaxFileSizeMB { get; set; }
    }


    public class OfficeDto
    {
        public int OfficeID { get; set; }
        public string OfficeName { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string State { get; set; }       
        public string PostalCode { get; set; }
    }

    public class AuthResponseDto
    {

        
        public string Status { get; set; }
        public string Message { get; set; }
        public int UserID { get; set; }
        public string Token { get; set; }
    }

    public class LoginDto
    {
        public int UserID { get; set; }
        public byte[] PasswordHash { get; set; } // was string
        public byte[] Salt { get; set; }

        public string LoginID { get; set; }
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string LoginID { get; set; }
        public string Password { get; set; }
        public string RegistrationType { get; set; }
    }
    public class BookedDateDto
    {
        public DateTime AppointmentDate { get; set; }
    }
    public class SlotCountDto
    {
        public string TimeSlot { get; set; }
        public int BookedCount { get; set; }
    }
    public class DTO
    {
    }

}