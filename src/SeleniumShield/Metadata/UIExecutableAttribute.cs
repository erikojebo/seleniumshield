using System;

namespace SeleniumShield.Metadata
{
    public class UIExecutableAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string DependencyGroup { get; set; }

        public int DependencyGroupOrder
        {
            get { return OptionalDependencyGroupOrder.GetValueOrDefault(); }
            set { OptionalDependencyGroupOrder = value; }
        }

        public int? OptionalDependencyGroupOrder { get; set; }
    }
}