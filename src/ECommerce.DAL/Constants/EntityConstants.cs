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
}
