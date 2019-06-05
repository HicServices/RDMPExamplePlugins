using MyPipelinePlugin;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System;
using System.Data;

namespace MyPipelinePluginTests
{
    public class TestAnonymisationPlugins
    {
        [Test]
        public void TestBasicDataTableAnonymiser1()
        {
            var dt = new DataTable();
            dt.Columns.Add("Story");
            dt.Rows.Add(new[] {"Thomas went to school regularly"});
            dt.Rows.Add(new[] {"It seems like Wallace went less regularly"});
            dt.Rows.Add(new[] {"Mr Smitty was the teacher"});

            var a = new BasicDataTableAnonymiser1();
            var resultTable = a.ProcessPipelineData(dt,new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());

            Assert.AreEqual(resultTable.Rows.Count,3);
            Assert.AreEqual("REDACTED went to school regularly",resultTable.Rows[0][0]);
            Assert.AreEqual("It seems like REDACTED went less regularly",resultTable.Rows[1][0]);
            Assert.AreEqual("Mr Smitty was the teacher",resultTable.Rows[2][0]);
        }

        [Test]
        public void TestBasicDataTableAnonymiser4_FailConditions()
        {
	        var a = new BasicDataTableAnonymiser4();
	        var ex = Assert.Throws<Exception>(()=>a.Check(new ThrowImmediatelyCheckNotifier()));
	        Assert.IsTrue(ex.Message.Contains("No NamesTable has been set"));
        }
    }
}
