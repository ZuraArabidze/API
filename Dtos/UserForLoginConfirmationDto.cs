namespace API.Dtos
{
    public partial class UserForLoginConfirmationDto
    {
        byte[] PasswordHash { get; set; } = new byte[0];
        byte[] PasswordSalt { get; set; } = new byte[0];

    }
}
