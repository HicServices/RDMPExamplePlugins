using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System;
using System.Data;
using System.Text.RegularExpressions;

namespace MyPipelinePlugin
{
    class BasicDataTableAnonymiser1 : IPluginDataFlowComponent<DataTable>
    {
        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public void Check(ICheckNotifier notifier)
        {
            
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            //Go through each row in the table
		    foreach (DataRow row in toProcess.Rows)
		    {
			    //for each cell in current row
			    for (int i = 0; i < row.ItemArray.Length; i++)
			    {
				    //if it is a string
				    var stringValue = row[i] as string;

				    if(stringValue != null)
				    {
					    //replace any common names with REDACTED
					    foreach (var name in CommonNames)
						    stringValue =  Regex.Replace(stringValue, name, "REDACTED",RegexOptions.IgnoreCase);

					    row[i] = stringValue;
				    }
			    }
            }

		    return toProcess;
        }

        string[] CommonNames = new string[]
        { 
            "Dave","Frank","Bob","Pete","Daisy","Marley","Lucy","Tabitha"
        };

    }
}
