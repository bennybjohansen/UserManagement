using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserManagement.Models
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UseCase
    {
        SaxoCreatingClient,
        IBCreatingUser,
        WLCCreatingClient,
        Default
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IdentificationType
    {
        None,
        DriversLicense,
        Passport
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum UserRole
    {
        RetailUser,
        TradeSupervisor,
        TradeManager,
        ClientSupervisor,
        ClientManager
    }


    public class User
    {
        [Required]
        public long  UserId { get; set; }
        public string UserName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Address { get; set; }
        public string ZipCode { get; set; }
        
        public IdentificationType IdentificationType { get; set; }

        public  UserRole[] Roles { get; set; }

    }

    [ValidateNever]
    public class UserNotValidated: User
    {

    }
}
