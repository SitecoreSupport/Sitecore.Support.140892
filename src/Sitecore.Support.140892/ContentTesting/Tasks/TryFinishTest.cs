namespace Sitecore.Support.ContentTesting.Tasks
{
    using Sitecore.ContentTesting;
    using Sitecore.ContentTesting.Data;
    using Sitecore.ContentTesting.Diagnostics;
    using Sitecore.ContentTesting.Intelligence;
    using Sitecore.ContentTesting.Model.Data.Items;
    using System;

    public class TryFinishTest
    {
        private readonly IContentTestStore contentTestStore;

        public bool UpdateTestStatistics
        {
            get;
            set;
        }

        public TryFinishTest() : this(null)
        {
        }

        public TryFinishTest(IContentTestStore contentTestStore)
        {
            this.contentTestStore = (contentTestStore ?? ContentTestingFactory.Instance.ContentTestStore);
            this.UpdateTestStatistics = true;
        }

        public void Execute(Sitecore.Data.Items.Item[] items, Sitecore.Tasks.CommandItem command, Sitecore.Tasks.ScheduleItem schedule)
        {
            if (!Sitecore.ContentTesting.Configuration.Settings.IsAutomaticContentTestingEnabled)
            {
                return;
            }
            this.SetPropertiesFromItem(command);
            IntelligenceService intelligenceService = new IntelligenceService();
            for (int i = 0; i < items.Length; i++)
            {
                Sitecore.Data.Items.Item item = items[i];
                try
                {
                    TestDefinitionItem testDefinitionItem = new TestDefinitionItem(item);
                    if (string.IsNullOrEmpty(testDefinitionItem.WinnerCombination))
                    {
                        Sitecore.Data.Database database = item.Database;
                        if (database.GetItem(new Sitecore.Data.DataUri(testDefinitionItem.ContentItem)) != null)
                        {
                            if (this.UpdateTestStatistics)
                            {
                                intelligenceService.CalculateStatisticalRelevancy(testDefinitionItem);
                            }
                            Sitecore.Data.ID deviceId = testDefinitionItem.Device.TargetID ?? Sitecore.Data.Items.DeviceItem.ResolveDevice(testDefinitionItem.Database).ID;
                            ITestConfiguration testConfiguration = this.contentTestStore.LoadTest(testDefinitionItem, deviceId);
                            if (testConfiguration != null)
                            {
                                this.contentTestStore.TestDeploymentManager.RunFinishTestBehavior(testConfiguration);
                            }
                        }
                    }
                }
                catch (Exception owner)
                {
                    Logger.Error("Error while trying to finish tests: ", owner);
                }
            }
        }

        protected virtual void SetPropertiesFromItem(Sitecore.Tasks.CommandItem command)
        {
            TryFinishTestsItem tryFinishTestsItem = new TryFinishTestsItem(command.InnerItem);
            this.UpdateTestStatistics = tryFinishTestsItem.UpdateTestStatistics;
        }
    }
}