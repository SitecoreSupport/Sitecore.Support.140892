namespace Sitecore.Support.ContentTesting.Tasks
{
    using Sitecore.ContentTesting;
    using Sitecore.ContentTesting.ContentSearch;
    using Sitecore.ContentTesting.Data;
    using Sitecore.ContentTesting.Diagnostics;
    using Sitecore.ContentTesting.Intelligence;
    using Sitecore.ContentTesting.Model.Data.Items;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using System;
    using System.Linq;

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

            #region Added code
            ITestingSearch testingSearch = ContentTestingFactory.Instance.TestingSearch;
            testingSearch.Start = DateTimeOffset.MinValue.UtcDateTime;
            testingSearch.End = DateTimeOffset.MaxValue.UtcDateTime;
            Database db = schedule.Database;
            #endregion

            this.SetPropertiesFromItem(command);
            IntelligenceService intelligenceService = new IntelligenceService();

            #region Modified code
            foreach (Item item in from x in testingSearch.GetRunningTests() select db.GetItem(x.Uri.ToDataUri()))
            #endregion
            {
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