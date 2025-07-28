using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Helpers.Enums;
using Helpers.Interfaces;
using Helpers.UIElements.Mobile;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Mobile_views
{
    public class CourtView : MainMenuView
    {
        public CourtView(IDriverManager driver) : base(driver) { }
        public IButton CourtButton => (IButton)_driver.GetElement(ElementType.Button, FindsBy.XPath,
            "//android.widget.Button[@resource-id=\"com.companyname.app.eds:id/ButtonMenuItem\" and @text=\"Corte\"]");

        public TextField SelectBussines => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/BusinessPicker");

        public TextField ClickBussines => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "android:id/text1");

        public TextField SelectEds => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/EdsPicker");

        public TextField ClickEds => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "android:id/text1");

        public TextField SelectIslander => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/IslanderPicker");

        public TextField ClickIslander => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "android:id/text1");

        public IButton NewSale => (IButton)_driver.GetElement(ElementType.Button, FindsBy.Id,
            "com.companyname.app.eds:id/AddSaleButton");

        public TextField SelectHose => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/HosePicker");

        public TextField ClickHose => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.XPath,
            "//android.widget.TextView[@resource-id=\'android:id/text1\'][2]");

        public TextField ClickAmmountAcumulate => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/AccumulatedAmountEntry");

        public TextField Ammount => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/AccumulatedAmountEntry");

        public Text AcumulatedValue => (Text)_driver.GetElement(ElementType.Text, FindsBy.XPath,
            "(//android.widget.TextView[starts-with(@text,'$')])[2]");

        public async Task ChangeTextToInt(int value)
        {
            string valueRaw = AcumulatedValue.Element.Text;
            string valueClear = valueRaw.Replace("$", "").Replace(",", "").Trim();

            if (int.TryParse(valueClear, out int valueNumber))
            {
                int valueNew = valueNumber + value;
                Ammount.SetNumber(valueNew);
            }
            else
            {
                Assert.Fail($"{valueRaw} Can not be resolver.");
                return;
            }
        }

        public IButton AddButton => (IButton)_driver.GetElement(ElementType.Button, FindsBy.Id,
            "com.companyname.app.eds:id/AddButton");

        public IButton AddCollectionButton => (IButton)_driver.GetElement(ElementType.Button, FindsBy.Id,
            "com.companyname.app.eds:id/AddCollectionTypeButton");

        public TextField TypeCollectionClick => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/PickerTypeOfCollection");

        public TextField CashSelect => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.XPath,
            "//android.widget.TextView[@resource-id=\"android:id/text1\" and @text=\"Efectivo\"]");

        public TextField ValueCollection => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/EntryCourtTypeOfCollectionAmount");

        public TextField ValueDescription => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/EntryCourtTypeOfCollectionDescription");

        public IButton SendCollectionButton => (IButton)_driver.GetElement(ElementType.Button, FindsBy.Id,
            "com.companyname.app.eds:id/ButtonAddTypeOfCollection");

        public IButton NewEgressButton => (IButton)_driver.GetElement(ElementType.Button, FindsBy.Id,
            "com.companyname.app.eds:id/AddExpenditureButton");

        public TextField ExpendiSelectClick => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/ExpenditurePicker");

        public TextField FleteSelectClick => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.XPath,
            "//android.widget.TextView[@resource-id=\"android:id/text1\" and @text=\"Flete\"]");

        public TextField AmmountValue => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/AmountEntry");

        public TextField DescriptionValue => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/DescriptionEntry");

        public IButton AddExpenseButton => (IButton)_driver.GetElement(ElementType.Button, FindsBy.Id,
            "com.companyname.app.eds:id/AddButton");

        public IButton SendDataButton => (IButton)_driver.GetElement(ElementType.Button, FindsBy.Id,
            "com.companyname.app.eds:id/SendDataButton");

        public IButton OpenListButton => (IButton)_driver.GetElement(ElementType.Button, FindsBy.Id,
            "com.companyname.app.eds:id/QuestionsButton");

    }
}
