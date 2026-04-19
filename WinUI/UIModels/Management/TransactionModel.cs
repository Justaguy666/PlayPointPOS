using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace WinUI.UIModels.Management;

public sealed partial class TransactionModel : ObservableObject
{
    private string _code = string.Empty;
    private string? _memberId;
    private string _customerName = string.Empty;
    private PaymentMethod _paymentMethod;
    private decimal _subtotalAmount;
    private decimal _discountAmount;
    private decimal _depositRefund;
    private decimal _totalAmount;
    private DateTime _createdAt;
    private List<TransactionLine> _lines = [];

    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value);
    }

    public string? MemberId
    {
        get => _memberId;
        set => SetProperty(ref _memberId, value);
    }

    public string CustomerName
    {
        get => _customerName;
        set => SetProperty(ref _customerName, value);
    }

    public PaymentMethod PaymentMethod
    {
        get => _paymentMethod;
        set => SetProperty(ref _paymentMethod, value);
    }

    public decimal SubtotalAmount
    {
        get => _subtotalAmount;
        set => SetProperty(ref _subtotalAmount, value);
    }

    public decimal DiscountAmount
    {
        get => _discountAmount;
        set => SetProperty(ref _discountAmount, value);
    }

    public decimal DepositRefund
    {
        get => _depositRefund;
        set => SetProperty(ref _depositRefund, value);
    }

    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetProperty(ref _totalAmount, value);
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }

    public List<TransactionLine> Lines
    {
        get => _lines;
        set => SetProperty(ref _lines, value);
    }
}
