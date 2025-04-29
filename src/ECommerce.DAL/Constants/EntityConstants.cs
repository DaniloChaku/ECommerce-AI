namespace ECommerce.DAL.Constants;

public static class EntityConstants
{
    public static class Product
    {
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 1000;
        public const int ImageUrlMaxLength = 500;
        public const string PriceColumnType = "decimal(18,2)";
    }

    public static class Category
    {
        public const int NameMaxLength = 50;
        public const int DescriptionMaxLength = 500;
    }

    public static class User
    {
        public const int NameMaxLength = 100;
        public const int EmailMaxLength = 100;
        public const int PasswordMinLength = 6;
        public const int PasswordMaxLength = 100;
        public const string CustomerRole = "Customer";
    }

    public static class Cart
    {
        public const int MaxCartItems = 20;
    }

    public static class Order
    {
        public const int AddressMaxLength = 500;
    }
}
