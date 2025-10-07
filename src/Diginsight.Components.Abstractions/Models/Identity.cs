namespace Diginsight.Components;

public class Identity
{
    //public Identity(string upn)
    //{
    //    this.Upn = upn;
    //}
    public Identity(string upn, string email)
    {
        Upn = upn;
        Email = email;
        Name = null;
    }
    public Identity(string upn, string email, string name)
    {
        Upn = upn;
        Email = email;
        Name = name;
    }

    public string Upn { get; set; } // upn da clais del token
    public string Email { get; set; }
    public string Name { get; set; }
    public Identity Manager { get; set; } // upn da clais del token
}
