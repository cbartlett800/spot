namespace TestLibrary
{
    public class TestClass
    {
        public string PublicProperty { get; set; }
        private string PrivateProperty { get; set; }
        public List<string> PublicProperties { get; set; }
        protected string ProtectedProperty { get; set; }
        internal string InternalProperty { get; set; }
        const string DefaultProperty = "bla";
    }
}