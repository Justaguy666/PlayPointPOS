using Application.Members;
using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Entities;
using System;

namespace WinUI.UIModels.Management;

public sealed partial class MemberModel : ObservableObject, IMemberFilterable
{
    private string _code = string.Empty;
    private string _fullName = string.Empty;
    private string _phoneNumber = string.Empty;
    private decimal _totalSpentAmount;
    private int _progressPercentage;
    private MembershipRank? _membershipRank;
    private MembershipRank? _nextMembershipRank;

    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    public decimal TotalSpentAmount
    {
        get => _totalSpentAmount;
        set => SetProperty(ref _totalSpentAmount, Math.Max(0m, value));
    }

    public int ProgressPercentage
    {
        get => _progressPercentage;
        set => SetProperty(ref _progressPercentage, Math.Clamp(value, 0, 100));
    }

    public MembershipRank? MembershipRank
    {
        get => _membershipRank;
        set => SetProperty(ref _membershipRank, value);
    }

    public MembershipRank? NextMembershipRank
    {
        get => _nextMembershipRank;
        set => SetProperty(ref _nextMembershipRank, value);
    }
}
