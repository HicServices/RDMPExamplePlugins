﻿using FAnsi;
using FAnsi.Discovery;
using MyPipelinePlugin;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging.Listeners;
using ReusableLibraryCode.Progress;
using System;
using System.Data;
using Tests.Common;

namespace MyPipelinePluginTests
{
    class TestAnonymisationPluginsDatabaseTests: DatabaseTests
    {
        [Test]
        public void Test()
        {
            Assert.Pass();
        }

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.Oracle)]
        [TestCase(DatabaseType.MySql)]
        public void TestBasicDataTableAnonymiser3(DatabaseType type)
        {
            DiscoveredDatabase database = GetCleanedServer(type);

            //Create a names table that will go into the database
            var dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Rows.Add(new[] {"Thomas"});
            dt.Rows.Add(new[] {"Wallace"});
            dt.Rows.Add(new[] {"Frank"});

            DiscoveredTable table = database.CreateTable("ForbiddenNames",dt);
            
            TableInfo tableInfo;
            Import(table,out tableInfo,out _);

            //Create the test dataset chunk that will be anonymised
            var dtStories = new DataTable();
            dtStories.Columns.Add("Story");
            dtStories.Rows.Add(new[] { "Thomas went to school regularly" });
            dtStories.Rows.Add(new[] { "It seems like Wallace went less regularly" });
            dtStories.Rows.Add(new[] { "Mr Smitty was the teacher" });

            //Create the anonymiser
            var a = new BasicDataTableAnonymiser3();

            //Tell it about the database table
            a.NamesTable = tableInfo;

            //run the anonymisation
            var resultTable = a.ProcessPipelineData(dtStories, new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());

            //check the results
            Assert.AreEqual(resultTable.Rows.Count, 3);
            Assert.AreEqual("REDACTED went to school regularly", resultTable.Rows[0][0]);
            Assert.AreEqual("It seems like REDACTED went less regularly", resultTable.Rows[1][0]);
            Assert.AreEqual("Mr Smitty was the teacher", resultTable.Rows[2][0]);

            //finally drop the database table
            table.Drop();
        }


        public enum LoggerTestCase
        {
	        ToConsole,
	        ToMemory,
	        ToDatabase
        }

        [Test]
        [TestCase(LoggerTestCase.ToConsole)]
        [TestCase(LoggerTestCase.ToMemory)]
        [TestCase(LoggerTestCase.ToDatabase)]
        public void TestBasicDataTableAnonymiser5(LoggerTestCase testCase)
        {
	        //Create a names table that will go into the database
	        var dt = new DataTable();
	        dt.Columns.Add("Name");
	        dt.Rows.Add(new[] { "Thomas" });
	        dt.Rows.Add(new[] { "Wallace" });
	        dt.Rows.Add(new[] { "Frank" });

	        //upload the DataTable from memory into the database
	        var discoveredTable = GetCleanedServer(DatabaseType.MicrosoftSQLServer).CreateTable("ForbiddenNames", dt);
	        try
	        {
                TableInfo tableInfo;

		        //import the persistent TableInfo reference
		        var importer = Import(discoveredTable,out tableInfo ,out _);
                
		        //Create the test dataset chunks that will be anonymised
		        var dtStories1 = new DataTable();
		        dtStories1.Columns.Add("Story");
		        dtStories1.Rows.Add(new[] { "Thomas went to school regularly" }); //1st redact
		        dtStories1.Rows.Add(new[] { "It seems like Wallace went less regularly" }); //2nd redact
		        dtStories1.Rows.Add(new[] { "Mr Smitty was the teacher" });

		        var dtStories2 = new DataTable();
		        dtStories2.Columns.Add("Story");
		        dtStories2.Rows.Add(new[] { "Things were going so well" });
		        dtStories2.Rows.Add(new[] { "And then it all turned bad for Wallace" }); //3rd redact
	
		        var dtStories3 = new DataTable();
		        dtStories3.Columns.Add("Story");
		        dtStories3.Rows.Add(new[] { "There were things creeping in the dark" });
		        dtStories3.Rows.Add(new[] { "Surely Frank would know what to do.  Frank was a genius" }); //4th redact
		        dtStories3.Rows.Add(new[] { "Mr Smitty was the teacher" });
	
		        //Create the anonymiser
		        var a = new BasicDataTableAnonymiser5();

		        //Tell it about the database table
		        a.NamesTable = tableInfo;

		        //Create a listener according to the test case
		        IDataLoadEventListener listener = null;               

		        switch (testCase)
		        {
			        case LoggerTestCase.ToConsole:
				        listener = new ThrowImmediatelyDataLoadEventListener();
				        break;
			        case LoggerTestCase.ToMemory:
				        listener = new ToMemoryDataLoadEventListener(true);
				        break;
			        case LoggerTestCase.ToDatabase:
			
				        //get the default logging server
				        var logManager = CatalogueRepository.GetDefaultLogManager();

				        //create a new super task Anonymising Data Tables
				        logManager.CreateNewLoggingTaskIfNotExists("Anonymising Data Tables");

				        //setup a listener that goes to this logging database 
				        listener = new ToLoggingDatabaseDataLoadEventListener(this,logManager ,"Anonymising Data Tables","Run on " + DateTime.Now);
				        break;
			        default:
				        throw new ArgumentOutOfRangeException("testCase");
		        }

		        //run the anonymisation
		        //process all 3 batches
		        a.ProcessPipelineData(dtStories1, listener, new GracefulCancellationToken());
		        a.ProcessPipelineData(dtStories2, listener, new GracefulCancellationToken());
		        a.ProcessPipelineData(dtStories3, listener, new GracefulCancellationToken());

		        //check the results
		        switch (testCase)
		        {
			        case LoggerTestCase.ToMemory:
				        Assert.AreEqual(4, ((ToMemoryDataLoadEventListener)listener).LastProgressRecieivedByTaskName["REDACTING Names"].Progress.Value);
				        break;
			        case LoggerTestCase.ToDatabase:
				        ((ToLoggingDatabaseDataLoadEventListener)listener).FinalizeTableLoadInfos();
				        break;
		        }
	        }
	        finally
	        {
		        //finally drop the database table
		        discoveredTable.Drop();
	        }
        }
    }
}
