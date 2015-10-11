using System;

namespace SeleniumShield.Metadata
{
    public class UIExecutableAttribute : Attribute
    {
        public string DependencyGroup { get; set; }
        public int Order { get; set; }
    }
}