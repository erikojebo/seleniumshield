using System;

namespace SeleniumShield.Metadata
{
    public class UIExecutableAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string DependencyGroup { get; set; }
        public int Order { get; set; }
    }
}