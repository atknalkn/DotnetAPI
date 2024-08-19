namespace DotnetAPI.Dtos
{
    public partial class UserForLoginConfirmationDto
    {
        public byte[] password1 {get; set;}
        public byte[] password2 {get; set;}
        UserForLoginConfirmationDto()
        {
            if (password1 == null)
            {
                password1 = new byte[0];
            }
            if (password2 == null)
            {
                password2 = new byte[0];
            }
        }
    }
}