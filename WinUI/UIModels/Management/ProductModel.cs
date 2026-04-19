using System;
using Application.Products;
using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Enums;

namespace WinUI.UIModels.Management;

public sealed class ProductModel : ObservableObject, IProductFilterable
{
    private string _name = string.Empty;
    private decimal _price;
    private ProductType _productType;
    private int _stockQuantity;
    private string _imageUri = "ms-appx:///Assets/Mock.png";

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public decimal Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public ProductType ProductType
    {
        get => _productType;
        set => SetProperty(ref _productType, value);
    }

    public int StockQuantity
    {
        get => _stockQuantity;
        set => SetProperty(ref _stockQuantity, Math.Max(0, value));
    }

    public string ImageUri
    {
        get => _imageUri;
        set => SetProperty(ref _imageUri, string.IsNullOrWhiteSpace(value) ? "ms-appx:///Assets/Mock.png" : value);
    }
}
