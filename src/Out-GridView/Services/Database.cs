using System.Collections.Generic;
using System.Management.Automation;
using OutGridView.Models;
using System.Linq;

namespace OutGridView.Services
{
    public class Database
    {
        private IEnumerable<PSObject> GetItems()
        {
            var items = new[] {
                new DummyObject { V1 = "Walk the cat" },
                new DummyObject { V1 = "Buy some milk" },
                new DummyObject { V1 = "Learn Avalonia", V2 = "Part2" },
                new DummyObject { V1 = "LearnAvalonia", V2 = "Part2" },
                new DummyObject { V1 = "Test Avalonia", V2 = "Part2" }
            };

            return items.Select(PSObject.AsPSObject);
        }

        public DataSource GetDataSource()
        {
            return new DataSource(GetItems());
        }
    };
}