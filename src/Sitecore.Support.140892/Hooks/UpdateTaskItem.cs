namespace Sitecore.Support.Hooks
{
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Events.Hooks;
    using Sitecore.SecurityModel;
    using System;

    public class UpdateTaskItem : IHook
    {
        public void Initialize()
        {
            using (new SecurityDisabler())
            {
                var databaseName = "master";
                var itemPath = "/sitecore/system/Tasks/Commands/Content Testing/Try Finish Test";
                var fieldName = "Type";

                var type = typeof(Sitecore.Support.ContentTesting.Tasks.TryFinishTest);

                var typeName = type.FullName;
                var assemblyName = type.Assembly.GetName().Name;
                var fieldValue = $"{typeName}, {assemblyName}";

                var database = Factory.GetDatabase(databaseName);
                var item = database.GetItem(itemPath);

                if (string.Equals(item[fieldName], fieldValue, StringComparison.Ordinal))
                {
                    // already installed
                    return;
                }

                Log.Audit($"Installing {assemblyName}", this);
                Log.Info($"Updating {item.Paths.FullPath}", this);

                item.Editing.BeginEdit();
                item[fieldName] = fieldValue;
                item.Editing.EndEdit();
            }
        }
    }
}