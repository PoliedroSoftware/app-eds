using Helpers.Interfaces;

namespace Mobile_views
{
    public class BaseView
    {
        protected IDriverManager _driver;
        protected BaseView(IDriverManager driver)
        {
            _driver = driver;
        }
    }
}
