using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;
using System.Drawing;
using Rdmp.Core.CommandExecution.AtomicCommands;

namespace MyPlugin
{
    public class ExecuteCommandRenameCatalogueToBunnies:BasicUICommandExecution, IAtomicCommand
    {
        private readonly Catalogue _catalogue;

        public ExecuteCommandRenameCatalogueToBunnies(IActivateItems activator,Catalogue catalogue) : base(activator)
        {
            _catalogue = catalogue;

		    if(catalogue.Name == "Bunny")
                SetImpossible("Catalogue is already called Bunny");
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
		    //icon to use for the right click menu (return null if you don't want one)
            return Resources.Bunny;
        }

        public override void Execute()
        {
            base.Execute();

		    //change the name
            _catalogue.Name = "Bunny";
			
		    //save the change
            _catalogue.SaveToDatabase();

		    //Lets the rest of the application know that a change has happened
            Publish(_catalogue);
        }
    }
}
