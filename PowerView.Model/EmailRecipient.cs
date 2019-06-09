using System;
using System.Text.RegularExpressions;

namespace PowerView.Model
{
  public class EmailRecipient
  {
    public EmailRecipient(string name, string emailAddress)
    {
      if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
      if (string.IsNullOrEmpty(emailAddress)) throw new ArgumentNullException("emailAddress");
      ValidateEmailAddress(emailAddress);

      Name = name;
      EmailAddress = emailAddress;
    }

    public string Name { get; private set; }
    public string EmailAddress { get; private set; }

    // From: http://emailregex.com/
    private const string emailAddressPattern = "^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-\\w]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$";
    private static readonly Regex isEmailAddress = new Regex(emailAddressPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static void ValidateEmailAddress(string emailAddress)
    {
      var isValid = isEmailAddress.IsMatch(emailAddress);

      if (!isValid)
      {
        throw new FormatException("Invalid email address:" + emailAddress);
      }
    }

  }
}

