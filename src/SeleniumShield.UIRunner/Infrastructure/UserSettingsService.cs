namespace SeleniumShield.UIRunner.Infrastructure
{
    public class UserSettingsService
    {
        public string LastFlowAssemblyPath
        {
            get { return Properties.Settings.Default.LastFlowAssemblyPath; }
            set
            {
                Properties.Settings.Default.LastFlowAssemblyPath = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}