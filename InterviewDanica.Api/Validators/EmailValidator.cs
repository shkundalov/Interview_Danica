using System.Text.RegularExpressions;
namespace InterviewDanica.Api.Validators;
public static class EmailValidator{
    public static bool IsValid(string email){
        if (string.IsNullOrWhiteSpace(email)){
            return false;
        }
        /*try{
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch(FormatException){
            return false;
        }*/
        return Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
    }
}
