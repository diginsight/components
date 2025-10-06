namespace Diginsight.Components
{
    public class Identity
    {
        //public Identity(string upn)
        //{
        //    this.Upn = upn;
        //}
        public Identity(string upn, string email)
        {
            this.Upn = upn;
            this.Email = email;
            this.Name = null;
        }
        public Identity(string upn, string email, string name)
        {
            this.Upn = upn;
            this.Email = email;
            this.Name = name;
        }

        public string Upn { get; set; } // upn da clais del token
        public string Email { get; set; }
        public string Name { get; set; }
        public Identity Manager { get; set; } // upn da clais del token
    }
}
