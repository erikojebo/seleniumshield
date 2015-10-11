using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SeleniumShield.Metadata;

namespace SeleniumShield.Loader
{
    public class FlowAssemblyLoader
    {
        public async Task<IEnumerable<Type>> LoadFlowTypes(string assemblyPath)
        {
            return await Task.Factory.StartNew(() =>
            {
                var assembly = Assembly.LoadFile(assemblyPath);

                return assembly.GetTypes()
                    .Where(x => typeof(IAutomationFlow).IsAssignableFrom(x))
                    .Where(x => x.GetCustomAttributes(typeof(UIExecutableAttribute), true).Any());
            });
        }
    }
}