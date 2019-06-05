using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using System.Windows.Forms;

namespace MyPlugin
{
    public class MyPluginUserInterface:Rdmp.UI.PluginChildProvision.PluginUserInterface
    {
        public MyPluginUserInterface(IActivateItems activator):base(activator)
        {

        }
        
        public override ToolStripMenuItem[] GetAdditionalRightClickMenuItems(object o)
        {
            if (o is Catalogue)
                return new[]
                {
                    new ToolStripMenuItem("Hello World", null, (s, e) => MessageBox.Show("Hello World")),

                    GetMenuItem(new ExecuteCommandRenameCatalogueToBunnies(ItemActivator,(Catalogue)o))
                };

            return null;
        }
    }
}
