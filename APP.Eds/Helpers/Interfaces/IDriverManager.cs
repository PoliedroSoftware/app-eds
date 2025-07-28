using Helpers.Enums;
using Helpers.UIElements.Mobile;

namespace Helpers.Interfaces
{
    public interface IDriverManager
    {
        void Close();

        byte[] TakeScreenshot();

        MobileElement GetElement(ElementType elementType, FindsBy findsby, string locator);
    }
}
