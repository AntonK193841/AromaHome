using System;

namespace WpfApp6
{
    public static class ProductQuantityCalculator
    {
        public static int CalculateProductQuantity(
            int productTypeId,
            int materialTypeId,
            int rawMaterialQuantity,
            double productParameter1,
            double productParameter2,
            double productTypeCoefficient,
            double materialLossPercent)
        {
            if (productTypeId <= 0
                || materialTypeId <= 0
                || rawMaterialQuantity < 0
                || productParameter1 <= 0
                || productParameter2 <= 0
                || productTypeCoefficient <= 0
                || materialLossPercent < 0
                || materialLossPercent >= 100
                || double.IsNaN(productParameter1)
                || double.IsNaN(productParameter2)
                || double.IsNaN(productTypeCoefficient)
                || double.IsNaN(materialLossPercent)
                || double.IsInfinity(productParameter1)
                || double.IsInfinity(productParameter2)
                || double.IsInfinity(productTypeCoefficient)
                || double.IsInfinity(materialLossPercent))
            {
                return -1;
            }

            double materialPerProduct =
                productParameter1
                * productParameter2
                * productTypeCoefficient;

            if (materialPerProduct <= 0
                || double.IsInfinity(materialPerProduct)
                || double.IsNaN(materialPerProduct))
            {
                return -1;
            }

            double availableMaterial =
                rawMaterialQuantity
                * (1.0 - materialLossPercent / 100.0);

            double productQuantity =
                Math.Floor(availableMaterial / materialPerProduct);

            if (double.IsNaN(productQuantity)
                || double.IsInfinity(productQuantity)
                || productQuantity > int.MaxValue)
            {
                return -1;
            }

            return (int)productQuantity;
        }
    }
}