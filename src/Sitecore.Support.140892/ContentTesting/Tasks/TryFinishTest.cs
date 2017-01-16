using Sitecore.Analytics.Data.Items;
using Sitecore.ContentTesting;
using Sitecore.ContentTesting.Configuration;
using Sitecore.ContentTesting.ContentSearch;
using Sitecore.ContentTesting.Data;
using Sitecore.ContentTesting.Diagnostics;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Tasks;
using System;
using System.Linq;

namespace Sitecore.Support.ContentTesting.Tasks
{
  public class TryFinishTest
  {
    private readonly IContentTestStore contentTestStore;

    public TryFinishTest() : this(null)
    {
    }

    public TryFinishTest(IContentTestStore contentTestStore)
    {
      this.contentTestStore = contentTestStore ?? ContentTestingFactory.Instance.ContentTestStore;
    }

    public void Execute(Item[] items, CommandItem command, ScheduleItem schedule)
    {
      if (Settings.IsAutomaticContentTestingEnabled)
      {
        #region changed code
        ITestingSearch testingSearch = ContentTestingFactory.Instance.TestingSearch;
        testingSearch.Start = DateTimeOffset.MinValue.UtcDateTime;
        testingSearch.End = DateTimeOffset.MaxValue.UtcDateTime;
        Database db = schedule.Database;
        foreach (Item item in from x in testingSearch.GetRunningTests() select db.GetItem(x.Uri.ToDataUri()))
        {
          try
          {
            #endregion
            TestDefinitionItem testDefinitionItem = new TestDefinitionItem(item);
            if (string.IsNullOrEmpty(testDefinitionItem.WinnerCombination))
            {
              Database database = item.Database;
              Item hostItem = database.GetItem(new DataUri(testDefinitionItem.ContentItem));
              if (hostItem != null)
              {
                DeviceItem[] all = database.Resources.Devices.GetAll();
                int count = 0;
                ITestConfiguration test = null;
                foreach (DeviceItem item4 in all)
                {
                  ITestConfiguration configuration2 = this.contentTestStore.LoadTestForItem(hostItem, item4.ID, testDefinitionItem);
                  if (((configuration2 != null) && (configuration2.TestDefinitionItem != null)) && (count < configuration2.TestSet.Variables.Count))
                  {
                    count = configuration2.TestSet.Variables.Count;
                    test = configuration2;
                  }
                }
                if (test != null)
                {
                  this.contentTestStore.TestDeploymentManager.RunFinishTestBehavior(test);
                }
              }
            }

          }
          catch (Exception exception)
          {
            Logger.Error("Error while trying to finish tests: ", exception);
          }
        }
      }
    }
  }
}