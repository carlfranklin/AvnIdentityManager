using System.ComponentModel.DataAnnotations;

namespace AvnIdentityManager;

public class ResetPasswordModel
{
	[Required]
	public string UserId { get; set; }

	[Required]
	[DataType(DataType.Password)]
	public string NewPassword { get; set; }

	[Required]
	[DataType(DataType.Password)]
	[Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
	public string ConfirmPassword { get; set; }
}